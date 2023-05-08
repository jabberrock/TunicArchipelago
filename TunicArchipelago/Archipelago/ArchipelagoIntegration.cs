using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunicArchipelago
{
    internal class ArchipelagoIntegration
    {
        private const string SaveFileLastProcessedItemIndexKey = "ARCHIPELAGO - Last Processed Item Index";
        private const string SaveFileCheckActivatedKeyPrefix = "ARCHIPELAGO - Check Activated ";

        private readonly ArchipelagoItemHandler itemHandler = new ArchipelagoItemHandler();
        private readonly ArchipelagoLocationHandler locationHandler = new ArchipelagoLocationHandler();

        private string hostname = "localhost";
        private int port = 38281;

        private ArchipelagoSession session;
        private bool connected;
        private CancellationTokenSource cancellationTokenSource;
        private IEnumerator<bool> processIncomingItemsStateMachine;
        private IEnumerator<bool> processOutgoingItemsStateMachine;
        private int lastProcessedItemIndex = -1;
        private ConcurrentQueue<(NetworkItem NetworkItem, int ItemIndex)> incomingItems;
        private ConcurrentQueue<NetworkItem> outgoingItems;

        public void Connect()
        {
            if (this.connected)
            {
                Logger.LogInfo("Already connected to Archipelago");
                return;
            }

            var settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
            if (File.Exists(settingsPath))
            {
                using (var fileStream = File.OpenText(settingsPath))
                using (var jsonReader = new JsonTextReader(fileStream))
                {
                    var settings = (JObject)JToken.ReadFrom(jsonReader);

                    if (settings.TryGetValue("hostname", out var hostnameField))
                    {
                        var hostname = hostnameField.Value<string>();
                        if (!string.IsNullOrEmpty(hostname))
                        {
                            this.hostname = hostname;
                        }
                    }

                    if (settings.TryGetValue("port", out var portField))
                    {
                        this.port = portField.Value<int>();
                    }
                }
            }

            this.session = ArchipelagoSessionFactory.CreateSession(this.hostname, this.port);
            this.connected = false;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.processIncomingItemsStateMachine = this.ProcessIncomingItemsStateMachine();
            this.processOutgoingItemsStateMachine = this.ProcessOutgoingItemsStateMachine();
            this.lastProcessedItemIndex = LoadLastProcessedItemIndex();
            this.incomingItems = new ConcurrentQueue<(NetworkItem NetworkItem, int Index)>();
            this.outgoingItems = new ConcurrentQueue<NetworkItem>();

            Logger.LogInfo("Connecting to Archipelago at " + this.hostname + ":" + this.port);

            this.session.Items.ItemReceived += (receivedItemsHelper) =>
            {
                var item = receivedItemsHelper.DequeueItem();

                var itemName = receivedItemsHelper.GetItemName(item.Item);
                var itemIndex = receivedItemsHelper.Index;

                Logger.LogInfo("Received item " + itemName + " (" + item.Item + ") at index " + itemIndex);

                this.incomingItems.Enqueue((item, itemIndex));
            };

            var loginResult = this.session.TryConnectAndLogin(
                "Tunic",
                "PlayerTunc",
                Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems);
            if (loginResult.Successful)
            {
                Logger.LogInfo("Connected to Archipelago");
                this.connected = true;
            }
            else
            {
                Logger.LogError("Failed to connect to Archipelago");
            }
        }

        public void Disconnect()
        {
            Logger.LogInfo("Disconnecting from Archipelago");

            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
            }

            if (this.session != null)
            {
                this.session.Socket.DisconnectAsync();
                this.session = null;
            }

            this.connected = false;
            this.cancellationTokenSource = null;
            this.processIncomingItemsStateMachine = null;
            this.processOutgoingItemsStateMachine = null;
            this.lastProcessedItemIndex = -1;
            this.incomingItems = new ConcurrentQueue<(NetworkItem NetworkItem, int ItemIndex)>();
            this.outgoingItems = new ConcurrentQueue<NetworkItem>();
        }

        public void Update()
        {
            if (!this.connected)
            {
                return;
            }

            if (this.processIncomingItemsStateMachine != null)
            {
                this.processIncomingItemsStateMachine.MoveNext();
            }

            if (this.processOutgoingItemsStateMachine != null)
            {
                this.processOutgoingItemsStateMachine.MoveNext();
            }
        }

        private IEnumerator<bool> ProcessIncomingItemsStateMachine()
        {
            while (!this.cancellationTokenSource.IsCancellationRequested)
            {
                // FIXME: Don't process items within the first few seconds
                // of transitioning into a scene because the screen is too
                // dark or too bright.

                if (!incomingItems.TryPeek(out var pendingItem))
                {
                    yield return true;
                    continue;
                }

                var networkItem = pendingItem.NetworkItem;
                var itemName = session.Items.GetItemName(networkItem.Item);
                var itemDisplayName = itemName + " (" + networkItem.Item + ") at index " + pendingItem.ItemIndex;

                if (pendingItem.ItemIndex <= this.lastProcessedItemIndex)
                {
                    Logger.LogWarning("Skipping item " + itemDisplayName + " because it has already been processed");
                    incomingItems.TryDequeue(out _);
                    yield return true;
                    continue;
                }

                if (networkItem.Player != this.session.ConnectionInfo.Slot)
                {
                    var sender = this.session.Players.GetPlayerName(networkItem.Player);
                    ShowNotification(sender + " sent you " + itemName + "!", "Aren't they nice?");
                    yield return true;
                }

                var handleResult = this.itemHandler.Handle(itemName, networkItem);
                switch (handleResult)
                {
                    case ArchipelagoItemHandler.HandleResult.Success:
                        Logger.LogInfo("Successfully handled item " + itemDisplayName);

                        incomingItems.TryDequeue(out _);
                        this.lastProcessedItemIndex = pendingItem.ItemIndex;
                        SaveLastProcessedItemIndex(this.lastProcessedItemIndex);

                        // Wait for animation to finish
                        var preInteractionStart = DateTime.Now;
                        while (DateTime.Now < preInteractionStart + TimeSpan.FromSeconds(1.0))
                        {
                            yield return true;
                        }

                        // Wait for all interactions to finish
                        while (
                            GenericMessage.instance.isActiveAndEnabled ||
                            ItemPresentation.instance.isActiveAndEnabled ||
                            PageDisplay.instance.isActiveAndEnabled)
                        {
                            yield return true;
                        }

                        // Pause before processing next item
                        var postInteractionStart = DateTime.Now;
                        while (DateTime.Now < postInteractionStart + TimeSpan.FromSeconds(5.0))
                        {
                            yield return true;
                        }

                        // FIXME: If there is a queue of incoming items,
                        // opening your own item adds it to the end of the
                        // queue rather than processing it immediately

                        break;

                    case ArchipelagoItemHandler.HandleResult.TemporaryFailure:
                        Logger.LogDebug("Will retrying processing item " + itemDisplayName);
                        break;

                    case ArchipelagoItemHandler.HandleResult.PermanentFailure:
                        Logger.LogWarning("Failed to process item " + itemDisplayName);
                        incomingItems.TryDequeue(out _);
                        this.lastProcessedItemIndex = pendingItem.ItemIndex;
                        SaveLastProcessedItemIndex(this.lastProcessedItemIndex);
                        break;
                }

                yield return true;
            }
        }

        private IEnumerator<bool> ProcessOutgoingItemsStateMachine()
        {
            while (!this.cancellationTokenSource.IsCancellationRequested)
            {
                if (!this.outgoingItems.TryDequeue(out var networkItem))
                {
                    yield return true;
                    continue;
                }

                var itemName = this.session.Items.GetItemName(networkItem.Item);
                var location = this.session.Locations.GetLocationNameFromId(networkItem.Location);
                var receiver = this.session.Players.GetPlayerName(networkItem.Player);

                Logger.LogInfo("Scouted item " + itemName + " at " + location + " for " + receiver);

                if (networkItem.Player != this.session.ConnectionInfo.Slot)
                {
                    ShowNotification("You sent " + itemName + " to " + receiver, "Nice!");
                }
                else
                {
                    ShowNotification("You found " + itemName + "!", "Nice!");
                }

                yield return true;
            }
        }

        public enum CheckState
        {
            Unknown,
            NotChecked,
            AlreadyChecked
        }

        public CheckState GetCheckState(GameObject gameObject)
        {
            var uniqueName =
                this.locationHandler.GetUniqueName(
                    SceneManager.GetActiveScene().name, gameObject);
            if (uniqueName == null)
            {
                return CheckState.Unknown;
            }

            if (SaveFile.GetInt(SaveFileCheckActivatedKeyPrefix + uniqueName) == 1)
            {
                return CheckState.AlreadyChecked;
            }
            else
            {
                return CheckState.NotChecked;
            }
        }

        public void ActivateCheck(GameObject gameObject)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            var uniqueName = locationHandler.GetUniqueName(sceneName, gameObject);
            if (uniqueName != null)
            {
                Logger.LogInfo("Activating check " + uniqueName);

                var locationId =
                    this.session.Locations.GetLocationIdFromName(
                        this.session.ConnectionInfo.Game,
                        uniqueName);

                this.session.Locations.CompleteLocationChecks(locationId);

                this.session.Locations.ScoutLocationsAsync(locationId)
                    .ContinueWith(locationInfoPacket =>
                        this.outgoingItems.Enqueue(locationInfoPacket.Result.Locations[0]));

                SaveFile.SetInt(
                    SaveFileCheckActivatedKeyPrefix +
                        this.locationHandler.GetUniqueName(
                            SceneManager.GetActiveScene().name, gameObject),
                    1);
            }
            else
            {
                Logger.LogWarning(
                    "Failed to get unique name for check " + gameObject.name +
                    " at " + gameObject.transform.position);

                ShowNotification("INTERACTED WITH UNKNOWN CHEST", "Please file a bug!");
            }
        }

        private int LoadLastProcessedItemIndex()
        {
            return SaveFile.GetInt(SaveFileLastProcessedItemIndexKey) - 1;
        }

        private void SaveLastProcessedItemIndex(int lastProcessedItemIndex)
        {
            SaveFile.SetInt(SaveFileLastProcessedItemIndexKey, lastProcessedItemIndex + 1);
        }

        private void ShowNotification(string topLine, string bottomLine)
        {
            var topLineObject = ScriptableObject.CreateInstance<LanguageLine>();
            topLineObject.text = "\"" + topLine + "\"";

            var bottomLineObject = ScriptableObject.CreateInstance<LanguageLine>();
            bottomLineObject.text = "\"" + bottomLine + "\"";

            var areaData = ScriptableObject.CreateInstance<AreaData>();
            areaData.topLine = topLineObject;
            areaData.bottomLine = bottomLineObject;

            AreaLabel.ShowLabel(areaData);
        }
    }
}
