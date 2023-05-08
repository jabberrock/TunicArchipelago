using UnityEngine;

namespace TunicArchipelago
{
    internal class ChestPatches
    {
        public static bool Chest_IInteractionReceiver_InteractPatch(Item i, Chest __instance)
        {
            ArchipelagoBehavior.instance.ActivateCheck(__instance.gameObject);

            return true;
        }

        public static bool Chest_shouldShowAsOpen_GetterPatch(Chest __instance, ref bool __result)
        {
            var checkState = ArchipelagoBehavior.instance.GetCheckState(__instance.gameObject);

            Logger.LogDebug("Chest " + __instance.name + " is " + checkState);

            switch (checkState)
            {
                case ArchipelagoIntegration.CheckState.NotChecked:
                    __result = false;
                    return false;

                case ArchipelagoIntegration.CheckState.AlreadyChecked:
                    __result = true;
                    return false;

                default:
                    // Assume that non-interactable chests are open (which
                    // doesn't matter if they are hidden)
                    __result = true;
                    return false;
            }
        }

        public static bool Chest_InterruptOpening_Patch(Chest __instance)
        {
            // Chests can not be interrupted
            //
            // If we want chests to be interruptable, we need to wait until the
            // chest is fully opened before rewarding the contents
            return false;
        }

        public static bool Chest_itemContentsfromDatabase_GetterPatch(Chest __instance, ref Item __result)
        {
            __result = null;

            return false;
        }

        public static bool Chest_itemQuantityFromDatabase_GetterPatch(Chest __instance, ref int __result)
        {
            __result = 0;

            return false;
        }

        public static bool Chest_moneySprayQuantityFromDatabase_GetterPatch(Chest __instance, ref int __result)
        {
            __result = 0;

            return false;
        }

        public static void ResetAllChests()
        {
            foreach (var chestBehavior in Resources.FindObjectsOfTypeAll<Chest>())
            {
                // TODO: Prevent fairies from spawning
                // TODO: Remove special material for golden trophy chests
                // TODO: Reset sound effects
            }
        }
    }
}
