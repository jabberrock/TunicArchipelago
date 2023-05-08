using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunicArchipelago
{
    internal class SceneLoaderPatches
    {
        public static bool SceneLoader_OnSceneLoaded_Patch(Scene loadingScene, LoadSceneMode mode, SceneLoader __instance)
        {
            Logger.LogInfo("Scene loaded: " + loadingScene.name);

            if (ArchipelagoBehavior.instance == null)
            {
                var gameObject = new GameObject();
                gameObject.name = "Archipelago Integration";
                ArchipelagoBehavior.instance = gameObject.AddComponent<ArchipelagoBehavior>();
                GameObject.DontDestroyOnLoad(gameObject);
            }

            ChestPatches.ResetAllChests();

            return true;
        }

        public static void PauseMenu___button_ReturnToTitle_PostfixPatch(PauseMenu __instance)
        {
            Logger.LogInfo("Returned to title screen");

            ArchipelagoBehavior.instance.Disconnect();
        }
    }
}
