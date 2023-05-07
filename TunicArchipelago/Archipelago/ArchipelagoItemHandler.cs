using Archipelago.MultiClient.Net.Models;
using NReco.Csv;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TunicArchipelago
{
    internal class ArchipelagoItemHandler
    {
        public enum HandleResult
        {
            Success,
            TemporaryFailure, // Can't accept right now, but can accept in the future
            PermanentFailure // Can never accept it
        }

        private class ItemTemplate
        {
            public string Name { get; set; }
            public string Classification { get; set; }
            public string Handler { get; set; }
            public string Parameter1 { get; set; }
            public string Parameter2 { get; set; }
            public int NumChecks { get; set; }
            public int? MinAmount { get; set; }
            public int? MaxAmount { get; set; }
            public string Comment { get; set; }
        }

        private interface IItemHandler
        {
            HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem);
        }

        private class UnknownItemHandler : IItemHandler
        {
            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                return HandleResult.PermanentFailure;
            }
        }

        private class AddHexagonHandler : IItemHandler
        {
            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                Logger.LogInfo("Collected " + itemTemplate.Parameter1);

                // TODO: Is this the same as SaveFile.SetInt?
                var inventoryItem = Inventory.GetItemByName(itemTemplate.Parameter1);
                inventoryItem.Quantity = 1;

                SaveFile.SetInt("Got " + itemTemplate.Parameter2, 1);

                ItemPresentation.PresentItem(inventoryItem);

                return HandleResult.Success;
            }
        }

        private class AddUniqueItemToInventoryHandler : IItemHandler
        {
            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                Logger.LogInfo("Adding " + itemTemplate.Parameter1 + " to inventory");

                var inventoryItem = Inventory.GetItemByName(itemTemplate.Parameter1);
                inventoryItem.Quantity = 1;

                ItemPresentation.PresentItem(inventoryItem);

                return HandleResult.Success;
            }
        }

        private class AddQuantityItemToInventoryHandler : IItemHandler
        {
            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                var amount = new System.Random().Next(itemTemplate.MinAmount.Value, itemTemplate.MaxAmount.Value);

                Logger.LogInfo("Adding " + itemTemplate.Parameter1 + " x " + amount + " to inventory");

                var inventoryItem = Inventory.GetItemByName(itemTemplate.Parameter1);
                inventoryItem.Quantity += amount;

                ItemPresentation.PresentItem(inventoryItem);

                return HandleResult.Success;
            }
        }

        private class AddRandomFairyHandler : IItemHandler
        {
            private static readonly List<string> Fairies = new List<string>()
            {
                "SV_Fairy_1_Overworld_Flowers_Upper_Opened",
                "SV_Fairy_2_Overworld_Flowers_Lower_Opened",
                "SV_Fairy_3_Overworld_Moss_Opened",
                "SV_Fairy_4_Caustics_Opened",
                "SV_Fairy_5_Waterfall_Opened",
                "SV_Fairy_6_Temple_Opened",
                "SV_Fairy_7_Quarry_Opened",
                "SV_Fairy_8_Dancer_Opened",
                "SV_Fairy_9_Library_Rug_Opened",
                "SV_Fairy_10_3DPillar_Opened",
                "SV_Fairy_11_WeatherVane_Opened",
                "SV_Fairy_12_House_Opened",
                "SV_Fairy_13_Patrol_Opened",
                "SV_Fairy_14_Cube_Opened",
                "SV_Fairy_15_Maze_Opened",
                "SV_Fairy_16_Fountain_Opened",
                "SV_Fairy_17_GardenTree_Opened",
                "SV_Fairy_18_GardenCourtyard_Opened",
                "SV_Fairy_19_FortressCandles_Opened",
                "SV_Fairy_20_ForestMonolith_Opened",
            };

            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                var uncollectedFairies = new List<string>();
                foreach (var fairy in Fairies)
                {
                    if (SaveFile.GetInt(fairy) != 1)
                    {
                        uncollectedFairies.Add(fairy);
                    }
                }

                var collectFairyIndex = new System.Random().Next(uncollectedFairies.Count);

                Logger.LogInfo("Collected fairy " + collectFairyIndex);

                SaveFile.SetInt(uncollectedFairies[collectFairyIndex], 1);

                // TODO: Fairy animation

                return HandleResult.Success;
            }
        }

        private class AddRandomManualPageHandler : IItemHandler
        {
            private const int NumPages = 28;

            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                var collectedPages = new HashSet<int>();
                for (int i = 0; i < NumPages; ++i)
                {
                    if (SaveFile.GetInt("unlocked page " + i) == 1)
                    {
                        collectedPages.Add(i);
                    }
                }

                var uncollectedPages = new List<int>();
                for (int i = 0; i < NumPages; ++i)
                {
                    if (!collectedPages.Contains(i))
                    {
                        uncollectedPages.Add(i);
                    }
                }

                var collectPageIndex = new System.Random().Next(uncollectedPages.Count);
                var collectPage = uncollectedPages[collectPageIndex];

                Logger.LogInfo("Collected page " + (collectPage * 2) + "/" + (collectPage * 2 + 1));

                SaveFile.SetInt("unlocked page " + collectPage, 1);

                PageDisplay.ShowPage(collectPage);

                return HandleResult.Success;
            }
        }

        private class AddRandomGoldenTrophyHandler : IItemHandler
        {
            private const int NumTrophies = 12;

            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                var uncollectedTrophies = new List<int>();
                for (int i = 1; i <= NumTrophies; ++i)
                {
                    if (SaveFile.GetInt("GoldenTrophy_" + i) == 0)
                    {
                        uncollectedTrophies.Add(i);
                    }
                }

                var collectTrophyIndex = new System.Random().Next(uncollectedTrophies.Count);
                var collectTrophy = uncollectedTrophies[collectTrophyIndex];

                Logger.LogInfo("Collected Golden Trophy " + collectTrophy);

                SaveFile.SetInt("inventory quantity GoldenTrophy_" + collectTrophy, 1);

                return HandleResult.Success;
            }
        }

        private class AddMoneyHandler : IItemHandler
        {
            public HandleResult Handle(ItemTemplate itemTemplate, NetworkItem networkItem)
            {
                var amount = new System.Random().Next(itemTemplate.MinAmount.Value, itemTemplate.MaxAmount.Value);

                Logger.LogInfo("Spawning " + amount + " money");
                CoinSpawner.SpawnCoins(amount, PlayerCharacter.Transform.position);

                return HandleResult.Success;
            }
        }

        private static readonly Dictionary<string, IItemHandler> ItemHandlers =
            new Dictionary<string, IItemHandler>()
            {
                { "AddHexagon",                 new AddHexagonHandler() },
                { "AddUniqueItemToInventory",   new AddUniqueItemToInventoryHandler() },
                { "AddQuantityItemToInventory", new AddQuantityItemToInventoryHandler() },
                { "AddRandomFairy",             new AddRandomFairyHandler() },
                { "AddRandomManualPage",        new AddRandomManualPageHandler() },
                { "AddRandomGoldenTrophy",      new AddRandomGoldenTrophyHandler() },
                { "AddMoney",                   new AddMoneyHandler() },
            };

        private readonly Dictionary<string, ItemTemplate> itemTemplates;

        public ArchipelagoItemHandler()
        {
            var itemTemplates = new Dictionary<string, ItemTemplate>();
            using (var inputStream =
                Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("TunicArchipelago.data.items.csv"))
            using (var streamReader = new StreamReader(inputStream))
            {
                var csvReader = new CsvReader(streamReader, ",");
                csvReader.Read(); // Skip header

                while (csvReader.Read())
                {
                    var itemTemplate = new ItemTemplate()
                    {
                        Name = csvReader[0],
                        Classification = csvReader[1],
                        Handler = csvReader[2],
                        Parameter1 = csvReader[3],
                        Parameter2 = csvReader[4],
                        NumChecks = int.Parse(csvReader[5]),
                        MinAmount = string.IsNullOrEmpty(csvReader[6]) ? (int?)null : int.Parse(csvReader[6]),
                        MaxAmount = string.IsNullOrEmpty(csvReader[7]) ? (int?)null : int.Parse(csvReader[7]),
                        Comment = csvReader[8]
                    };

                    itemTemplates[itemTemplate.Name] = itemTemplate;
                }
            }

            this.itemTemplates = itemTemplates;
        }

        public HandleResult Handle(string itemName, NetworkItem networkItem)
        {
            if (ItemPresentation.instance.isActiveAndEnabled)
            {
                return HandleResult.TemporaryFailure;
            }

            if (!this.itemTemplates.TryGetValue(itemName, out var itemTemplate))
            {
                return HandleResult.PermanentFailure;
            }

            if (!ItemHandlers.TryGetValue(itemTemplate.Handler, out var itemHandler))
            {
                itemHandler = new UnknownItemHandler();
            }

            return itemHandler.Handle(itemTemplate, networkItem);
        }
    }
}
