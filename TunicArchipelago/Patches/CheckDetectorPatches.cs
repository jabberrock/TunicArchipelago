using NReco.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunicArchipelago
{
    internal class CheckDetectorPatches
    {
        private class Check
        {
            public string Scene { get; set; }
            public string CheckType { get; set; }
            public string CheckName { get; set; }
            public Vector3 CheckPosition { get; set; }
            public int? ChestID { get; set; }
            public string ItemType { get; set; }
            public string ItemName { get; set; }
            public int? ItemQuantity { get; set; }
            public bool? IsFairy { get; set; }
        }

        public static void Chest_IInteractionReceiver_InteractPatch(Item i, Chest __instance)
        {
            var check = new Check()
            {
                Scene = SceneManager.GetActiveScene().name,
                CheckType = "Chest",
                CheckName = __instance.name,
                CheckPosition = __instance.transform.position,
                ChestID = __instance.chestID,
                ItemType = __instance.itemContents != null ? __instance.itemContents.Type.ToString() : null,
                ItemName = __instance.itemContents != null ? __instance.itemContents.CachedInventoryString : null,
                ItemQuantity = __instance.itemContentsQuantity,
                IsFairy = __instance.isFairy
            };

            SaveCheck(check);
        }

        public static void ItemPickup_onGetIt_Patch(ItemPickup __instance)
        {
            if (__instance.name.StartsWith("Coin Pickup"))
            {
                return;
            }

            var check = new Check()
            {
                Scene = SceneManager.GetActiveScene().name,
                CheckType = "Item",
                CheckName = __instance.name,
                CheckPosition = __instance.transform.position,
                ItemType = __instance.itemToGive != null ? __instance.itemToGive.Type.ToString() : null,
                ItemName = __instance.itemToGive != null ? __instance.itemToGive.CachedInventoryString : null,
                ItemQuantity = __instance.quantityToGive,
            };

            SaveCheck(check);
        }

        public static void PagePickup_onGetIt_Patch(PagePickup __instance)
        {
            var check = new Check()
            {
                Scene = SceneManager.GetActiveScene().name,
                CheckType = "Page",
                CheckName = __instance.name,
                CheckPosition = __instance.transform.position,
                ItemName = __instance.pageName,
                ItemQuantity = 1
            };

            SaveCheck(check);
        }

        public static void HeroRelicPickup_onGetIt_Patch(HeroRelicPickup __instance)
        {
            var check = new Check()
            {
                CheckType = "HeroRelic",
                CheckName = __instance.name,
                CheckPosition = __instance.transform.position,
                ItemType = __instance.relicItem != null ? __instance.relicItem.Type.ToString() : null,
                ItemName = __instance.relicItem != null ? __instance.relicItem.CachedInventoryString : null,
                ItemQuantity = 1,
            };

            SaveCheck(check);
        }

        private static void SaveCheck(Check check)
        {
            var path = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Desktop\tunic-checks.csv");
            var alreadyExists = File.Exists(path);

            using (var streamWriter = new StreamWriter(path, append: true))
            {
                var csvWriter = new CsvWriter(streamWriter);

                if (!alreadyExists)
                {
                    csvWriter.WriteField("Scene");
                    csvWriter.WriteField("CheckType");
                    csvWriter.WriteField("CheckName");
                    csvWriter.WriteField("CheckPosition");
                    csvWriter.WriteField("ChestID");
                    csvWriter.WriteField("ItemType");
                    csvWriter.WriteField("ItemName");
                    csvWriter.WriteField("ItemQuantity");
                    csvWriter.WriteField("IsFairy");
                    csvWriter.NextRecord();
                }

                csvWriter.WriteField(check.Scene);
                csvWriter.WriteField(check.CheckType);
                csvWriter.WriteField(check.CheckName);
                csvWriter.WriteField(check.CheckPosition.ToString());
                csvWriter.WriteField(check.ChestID.HasValue ? check.ChestID.Value.ToString() : null);
                csvWriter.WriteField(check.ItemType);
                csvWriter.WriteField(check.ItemName);
                csvWriter.WriteField(check.ItemQuantity.HasValue ? check.ItemQuantity.Value.ToString() : null);
                csvWriter.WriteField(check.IsFairy.HasValue ? (check.IsFairy.Value ? "1" : "0") : null);
                csvWriter.NextRecord();
            }
        }
    }
}
