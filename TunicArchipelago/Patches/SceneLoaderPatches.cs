using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunicArchipelago
{
    internal class SceneLoaderPatches
    {
        public static bool SceneLoader_OnSceneLoaded_Patch(Scene loadingScene, LoadSceneMode mode, SceneLoader __instance)
        {
            if (ArchipelagoBehavior.instance == null)
            {
                var gameObject = new GameObject();
                gameObject.name = "Archipelago Integration";
                ArchipelagoBehavior.instance = gameObject.AddComponent<ArchipelagoBehavior>();
                GameObject.DontDestroyOnLoad(gameObject);
            }

            return true;
        }
    }
}
