using NReco.Csv;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TunicArchipelago
{
    internal class ArchipelagoLocationHandler
    {
        private Dictionary<string, string> snpToUniqueName = new Dictionary<string, string>();
        private Dictionary<string, string> snToUniqueName = new Dictionary<string, string>();

        public ArchipelagoLocationHandler()
        {
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
                    var name = csvReader[2];
                    var position = csvReader[3];
                    var isPositionAccurate = csvReader[4] == "1";
                    var uniqueName = csvReader[6];

                    if (isPositionAccurate)
                    {
                        this.snpToUniqueName[sceneName + "|" + name + "|" + position] = uniqueName;
                    }
                    else
                    {
                        this.snToUniqueName[sceneName + "|" + name] = uniqueName;
                    }
                }
            }

            foreach (var k in this.snpToUniqueName)
            {
                Logger.LogInfo("Location " + k.Key + " -> " + k.Value);
            }

            foreach (var k in this.snToUniqueName)
            {
                Logger.LogInfo("Location " + k.Key + " -> " + k.Value);
            }
        }

        public string GetUniqueName(string sceneName, GameObject gameObject)
        {
            var name = gameObject.name;
            var position = gameObject.transform.position;

            string uniqueName;
            if (this.snpToUniqueName.TryGetValue(sceneName + "|" + name + "|" + position, out uniqueName))
            {
                return uniqueName;
            }
            else if (this.snToUniqueName.TryGetValue(sceneName + "|" + name, out uniqueName))
            {
                return uniqueName;
            }
            else
            {
                return null;
            }
        }
    }
}
