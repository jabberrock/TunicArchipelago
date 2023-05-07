using System.Linq;
using System.Threading;
using UnityEngine;

namespace TunicArchipelago
{
    internal class PlayerCharacterPatches
    {
        public static void PlayerCharacter_Start_PostfixPatch(PlayerCharacter __instance)
        {
            ArchipelagoBehavior.instance.Connect();
        }

        public static void PlayerCharacter_Update_PostfixPatch(PlayerCharacter __instance)
        {
        }

        public static void PlayerCharacter_OnDestroy_PostfixPatch(PlayerCharacter __instance)
        {
            ArchipelagoBehavior.instance.Disconnect();
        }
    }
}
