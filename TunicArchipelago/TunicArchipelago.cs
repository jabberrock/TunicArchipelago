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

            Logger.LogInfo("Plugin loaded");
        }

        private void SetupArchipelago()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ArchipelagoBehavior>();

            var harmony = new Harmony(PluginInfo.Guid);

            harmony.Patch(
                AccessTools.Method(typeof(SceneLoader), "OnSceneLoaded"),
                new HarmonyMethod(AccessTools.Method(typeof(SceneLoaderPatches), "SceneLoader_OnSceneLoaded_Patch")));

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

            // TODO: Replace well pickups
        }
    }
}
