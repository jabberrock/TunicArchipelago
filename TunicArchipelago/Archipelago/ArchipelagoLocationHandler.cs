using NReco.Csv;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TunicArchipelago
{
    internal class ArchipelagoLocationHandler
    {
        private Dictionary<string, string> gameObjectIdToUniqueName;

        public ArchipelagoLocationHandler()
        {
            var gameObjectIdToUniqueName = new Dictionary<string, string>();

            using (var inputStream =
                Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("TunicArchipelago.data.locations.csv"))
            using (var streamReader = new StreamReader(inputStream))
            {
                var csvReader = new CsvReader(streamReader, ",");
                csvReader.Read(); // Skip header

                while (csvReader.Read())
                {
                    var sceneName = csvReader[0];
                    var gameObjectName = csvReader[1];
                    var position = csvReader[2];
                    var isValidCheck = csvReader[3] == "1";
                    var uniqueName = csvReader[4];

                    if (!isValidCheck)
                    {
                        continue;
                    }

                    var gameObjectId = GetGameObjectId(sceneName, gameObjectName, position);
                    gameObjectIdToUniqueName[gameObjectId] = uniqueName;
                }
            }

            this.gameObjectIdToUniqueName = gameObjectIdToUniqueName;
        }

        public string GetUniqueName(string sceneName, GameObject gameObject)
        {
            var gameObjectId = GetGameObjectId(sceneName, gameObject.name, gameObject.transform.position.ToString());

            if (!this.gameObjectIdToUniqueName.TryGetValue(gameObjectId, out var uniqueName))
            {
                return null;
            }
            
            return uniqueName;
        }

        private static string GetGameObjectId(string sceneName, string gameObjectName, string position)
        {
            return "[" + sceneName + "] " + gameObjectName + " " + position;
        }
    }
}
