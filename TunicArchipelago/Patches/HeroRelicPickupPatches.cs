using System.Collections.Generic;

namespace TunicArchipelago
{
    internal class HeroRelicPickupPatches
    {
        private static readonly Dictionary<string, string> NameToStateVariable =
            new Dictionary<string, string>()
            {
                { "Relic PIckup (1) (SP)",      "SV_RelicVoid_Got_Pendant_SP" },
                { "Relic PIckup (2) (Crown)",   "SV_RelicVoid_Got_Crown_DEF" },
                { "Relic PIckup (3) (HP)",      "SV_RelicVoid_Got_Pendant_HP" },
                { "Relic PIckup (4) (water)",   "SV_RelicVoid_Got_Water_POT" },
                { "Relic PIckup (5) (MP)",      "SV_RelicVoid_Got_Pendant_MP" },
                { "Relic PIckup (6) Sword)",    "SV_RelicVoid_Got_Sword_ATT" },
            };

        public static bool HeroRelicPickup_onGetIt_PrefixPatch(HeroRelicPickup __instance)
        {
            var checkState = ArchipelagoBehavior.instance.GetCheckState(__instance.gameObject);
            switch (checkState)
            {
                case ArchipelagoIntegration.CheckState.NotChecked:
                    ArchipelagoBehavior.instance.ActivateCheck(__instance.gameObject);
                    SaveFile.SetInt(NameToStateVariable[__instance.name], 1);
                    // TODO: Do we need to update the state variable too?
                    __instance.timelineDirector.Play();
                    return false;

                case ArchipelagoIntegration.CheckState.AlreadyChecked:
                    Logger.LogWarning("Attempted to activate hero relic pickup that has already been picked up");
                    return false;

                default:
                    return true;
            }
        }

        public static bool HeroRelicPickup_alreadyPickedUp_PrefixPatch(HeroRelicPickup __instance, ref bool __result)
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
