namespace TunicArchipelago
{
    internal class ItemPickupPatches
    {
        public static bool ItemPickup_onGetIt_PrefixPatch(ItemPickup __instance)
        {
            var checkState = ArchipelagoBehavior.instance.GetCheckState(__instance.gameObject);
            switch (checkState)
            {
                case ArchipelagoIntegration.CheckState.NotChecked:
                    ArchipelagoBehavior.instance.ActivateCheck(__instance.gameObject);
                    return false;

                case ArchipelagoIntegration.CheckState.AlreadyChecked:
                    Logger.LogWarning("Attempted to activate item pickup that has already been picked up");
                    return false;

                default:
                    return true;
            }
        }

        public static bool ItemPickup_alreadyPickedUp_PrefixPatch(ItemPickup __instance, ref bool __result)
        {
            var checkState = ArchipelagoBehavior.instance.GetCheckState(__instance.gameObject);

            Logger.LogDebug("Checked already picked up " + __instance.name + " is " + checkState);

            switch (checkState)
            {
                case ArchipelagoIntegration.CheckState.NotChecked:
                    __result = false;
                    break;

                case ArchipelagoIntegration.CheckState.AlreadyChecked:
                    __result = true;
                    break;

                default:
                    __result = true;
                    break;
            }

            return false;
        }
    }
}
