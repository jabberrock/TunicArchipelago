using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace TunicArchipelago
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class TunicArchipelago : BasePlugin
    {
        public override void Load()
        {
            Logger.SetLogger(this.Log);

            Logger.LogInfo("Loading plugin...");

            Application.runInBackground = true;

            SetupArchipelago();
            //SetupCheckDetector();

            Logger.LogInfo("Plugin loaded");
        }

        private void SetupArchipelago()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ArchipelagoBehavior>();

            var harmony = new Harmony(PluginInfo.Guid);

            harmony.Patch(
                AccessTools.Method(typeof(SceneLoader), "OnSceneLoaded"),
                new HarmonyMethod(AccessTools.Method(typeof(SceneLoaderPatches), "SceneLoader_OnSceneLoaded_Patch")));

            harmony.Patch(AccessTools.Method(typeof(PauseMenu), "__button_ReturnToTitle"),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(SceneLoaderPatches), "PauseMenu___button_ReturnToTitle_PostfixPatch")));

            harmony.Patch(
                AccessTools.Method(typeof(PlayerCharacter), "Start"),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(PlayerCharacterPatches), "PlayerCharacter_Start_PostfixPatch")));
            harmony.Patch(
                AccessTools.Method(typeof(PlayerCharacter), "Update"),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(PlayerCharacterPatches), "PlayerCharacter_Update_PostfixPatch")));
            harmony.Patch(
                AccessTools.Method(typeof(PlayerCharacter), "OnDestroy"),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(PlayerCharacterPatches), "PlayerCharacter_OnDestroy_PostfixPatch")));

            harmony.Patch(
                AccessTools.Method(typeof(Chest), "IInteractionReceiver_Interact"),
                new HarmonyMethod(AccessTools.Method(typeof(ChestPatches), "Chest_IInteractionReceiver_InteractPatch")));
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Chest), "shouldShowAsOpen"),
                new HarmonyMethod(AccessTools.Method(typeof(ChestPatches), "Chest_shouldShowAsOpen_GetterPatch")));
            harmony.Patch(
                AccessTools.Method(typeof(Chest), "InterruptOpening"),
                new HarmonyMethod(AccessTools.Method(typeof(ChestPatches), "Chest_InterruptOpening_Patch")));
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Chest), "itemContentsfromDatabase"),
                new HarmonyMethod(AccessTools.Method(typeof(ChestPatches), "Chest_itemContentsfromDatabase_GetterPatch")));
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Chest), "itemQuantityFromDatabase"),
                new HarmonyMethod(AccessTools.Method(typeof(ChestPatches), "Chest_itemQuantityFromDatabase_GetterPatch")));
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Chest), "moneySprayQuantityFromDatabase"),
                new HarmonyMethod(AccessTools.Method(typeof(ChestPatches), "Chest_moneySprayQuantityFromDatabase_GetterPatch")));

            harmony.Patch(AccessTools.Method(typeof(ItemPickup), "onGetIt"),
                new HarmonyMethod(AccessTools.Method(typeof(ItemPickupPatches), "ItemPickup_onGetIt_PrefixPatch")));
            harmony.Patch(AccessTools.Method(typeof(ItemPickup), "alreadyPickedUp"),
                new HarmonyMethod(AccessTools.Method(typeof(ItemPickupPatches), "ItemPickup_alreadyPickedUp_PrefixPatch")));

            harmony.Patch(AccessTools.Method(typeof(PagePickup), "onGetIt"),
                new HarmonyMethod(AccessTools.Method(typeof(PagePickupPatches), "PagePickup_onGetIt_PrefixPatch")));
            harmony.Patch(AccessTools.Method(typeof(PagePickup), "alreadyPickedUp"),
                new HarmonyMethod(AccessTools.Method(typeof(PagePickupPatches), "PagePickup_alreadyPickedUp_PrefixPatch")));

            harmony.Patch(AccessTools.Method(typeof(HeroRelicPickup), "onGetIt"),
                new HarmonyMethod(AccessTools.Method(typeof(HeroRelicPickupPatches), "HeroRelicPickup_onGetIt_PrefixPatch")));
            harmony.Patch(AccessTools.Method(typeof(HeroRelicPickup), "alreadyPickedUp"),
                new HarmonyMethod(AccessTools.Method(typeof(HeroRelicPickupPatches), "HeroRelicPickup_alreadyPickedUp_PrefixPatch")));

            // TODO: Replace Hero Relic pickups
            // TODO: Replace well pickups
        }

        private void SetupCheckDetector()
        {
            var harmony = new Harmony(PluginInfo.Guid);

            harmony.Patch(
                AccessTools.Method(typeof(Chest), "IInteractionReceiver_Interact"),
                new HarmonyMethod(AccessTools.Method(typeof(CheckDetectorPatches), "Chest_IInteractionReceiver_InteractPatch")));

            harmony.Patch(AccessTools.Method(typeof(ItemPickup), "onGetIt"),
                new HarmonyMethod(AccessTools.Method(typeof(CheckDetectorPatches), "ItemPickup_onGetIt_Patch")));

            harmony.Patch(AccessTools.Method(typeof(PagePickup), "onGetIt"),
                new HarmonyMethod(AccessTools.Method(typeof(CheckDetectorPatches), "PagePickup_onGetIt_Patch")));

            harmony.Patch(AccessTools.Method(typeof(HeroRelicPickup), "onGetIt"),
                new HarmonyMethod(AccessTools.Method(typeof(CheckDetectorPatches), "HeroRelicPickup_onGetIt_Patch")));
        }
    }
}
