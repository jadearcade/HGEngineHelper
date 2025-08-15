using CsvConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static HgEngineCsvConverter.Code.HgEngineMonDataCsvExporter;

namespace HgEngineCsvConverter.Code
{
    public class HgEngineItemDataParser
    {
        //public class ItemData
        //{
        //    public string speciesKey;
        //    public int price;
        //    public int holdEffect;
        //    public int holdEffectParam;
        //    public int pluckEffect;
        //    public int flingEffect;
        //    public int flingPower;
        //    public int naturalGiftPower;
        //    public int naturalGiftType;
        //    public bool prevent_toss;
        //    public bool selectable;
        //    public string fieldPocket;
        //    public string battlePocket;
        //    public int fieldUseFunc;
        //    public int battleUseFunc;
        //    public int partyUse;
        //    //party use param
        //    public bool slp_heal;
        //    public bool psn_heal;
        //    public bool brn_heal;
        //    public bool frz_heal;
        //    public bool prz_heal;
        //    public bool cfs_heal;
        //    public bool inf_heal;
        //    public bool guard_spec;
        //    public bool revive;
        //    public bool revive_all;
        //    public bool level_up;
        //    public bool evolve;
        //    public int atk_stages;
        //    public int def_stages;
        //    public int spatk_stages;
        //    public int spdef_stages;
        //    public int speed_stages;
        //    public int accuracy_stages;
        //    public int critrate_stages;
        //    public bool pp_up;
        //    public bool pp_max = false;
        //    public bool pp_restore = false;
        //    public bool pp_restore_all = false;
        //    public bool hp_restore = false;
        //    public bool hp_ev_up = false;
        //    public bool atk_ev_up = false;
        //    public bool def_ev_up = false;
        //    public bool speed_ev_up = false;
        //    public bool spatk_ev_up = false;
        //    public bool spdef_ev_up = false;
        //    public bool friendship_mod_lo = false;
        //    public bool friendship_mod_med = false;
        //    public bool friendship_mod_hi = false;
        //    public int hp_ev_up_param = 0;
        //    public int atk_ev_up_param = 0;
        //    public int def_ev_up_param = 0;
        //    public int speed_ev_up_param = 0;
        //    public int spatk_ev_up_param = 0;
        //    public int spdef_ev_up_param = 0;
        //    public int hp_restore_param = 0;
        //    public int pp_restore_param = 0;
        //    public int friendship_mod_lo_param = 0;
        //    public int friendship_mod_med_param = 0;
        //}

        public List<string> BoolAttrs = new List<string>()
        {
            "prevent_toss","selectable","slp_heal","psn_heal","brn_heal","frz_heal","prz_heal","cfs_heal","inf_heal","guard_spec","revive","revive_all","level_up","evolve","pp_up","pp_max","pp_restore","pp_restore_all","hp_restore","hp_ev_up","atk_ev_up","def_ev_up","speed_ev_up","spatk_ev_up","spdef_ev_up","friendship_mod_lo","friendship_mod_med","friendship_mod_hi"
        };

        public static string ItemKeyString = "itemKey";
        public static string HasExplorerKitIndexString = "hasExplorerKitUnknownSlotsOffset";
        public static string HasNormalUnknownSlotIndexString = "hasNormalUnknownSlotsOffset";
        private static string NumUnknownSlotsExplorerKitStringNoSpaces = "-NUM_UNKNOWN_SLOTS_EXPLORER_KIT";
        private static string NumUnknownSlotsStringNoSpaces = "-NUM_UNKNOWN_SLOTS";

        public List<Dictionary<string, string>> ReadItemData(string filePath, out List<string> attrsInOrder)
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            string currentItemKey = "";
            attrsInOrder = new List<string>();
            HashSet<string> attrsHash = new HashSet<string>();
            attrsHash.Add(ItemKeyString);
            attrsHash.Add(HasExplorerKitIndexString);
            attrsHash.Add(HasNormalUnknownSlotIndexString);
            attrsInOrder.Add(ItemKeyString);
            attrsInOrder.Add(HasExplorerKitIndexString);
            attrsInOrder.Add(HasNormalUnknownSlotIndexString);
            Dictionary<string, string> attributeDataToValue = new Dictionary<string, string>();
            while (line != null)
            {
                if (line.StartsWith("},"))
                {
                    if (currentItemKey != "")
                    {
                        result.Add(attributeDataToValue);
                        currentItemKey = "";
                        attributeDataToValue = new Dictionary<string, string>();
                    }
                }else if (currentItemKey == "")
                {
                    if (line.Contains("ITEM_"))
                    {
                        var itemKeyAlmostReady = line.Replace("[", "").Replace("]", "").Replace(" ", "").Replace("=", "");
                        bool hasExplorerKitString = false;
                        bool HasNormalUnknownSlotString = false;
                        if (itemKeyAlmostReady.Contains(NumUnknownSlotsExplorerKitStringNoSpaces))
                        {
                            itemKeyAlmostReady = itemKeyAlmostReady.Replace(NumUnknownSlotsExplorerKitStringNoSpaces, "");
                            hasExplorerKitString = true;
                        }
                        else if (itemKeyAlmostReady.Contains(NumUnknownSlotsStringNoSpaces))
                        {
                            itemKeyAlmostReady = itemKeyAlmostReady.Replace(NumUnknownSlotsStringNoSpaces, "");
                            HasNormalUnknownSlotString = true;
                        }
                        currentItemKey = itemKeyAlmostReady;
                        attributeDataToValue[ItemKeyString] = currentItemKey;
                        attributeDataToValue[HasExplorerKitIndexString] = hasExplorerKitString ? "TRUE" : "FALSE";
                        attributeDataToValue[HasNormalUnknownSlotIndexString] = HasNormalUnknownSlotString ? "TRUE" : "FALSE";
                    }
                }
                else
                {
                    List<string> pieces = line.Replace(".","").Replace(" ","").Replace(",","").Replace("=",",").Split(",").ToList();
                    if (pieces.Count >= 2)
                    {
                        var attr = pieces[0];
                        if (!attrsHash.Contains(attr))
                        {
                            attrsInOrder.Add(attr);
                            attrsHash.Add(attr);
                        }
                        if (!attr.Contains( "partyUseParam"))
                        {
                            attributeDataToValue[attr] = pieces[1];
                        }
                    }
                }
                 line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }

        //public Dictionary<string, int> ReadItemHeaders(string filePath)
        //{
        //    Dictionary<string, int> result = new Dictionary<string, int>();
        //    String line = "";
        //    StreamReader sr = new StreamReader(filePath);
        //    line = sr.ReadLine();
        //    while (line != null)
        //    {
        //        if (!line.Contains("#define ITEM_") || line.StartsWith("//"))
        //        {
        //            line = sr.ReadLine();
        //            continue;
        //        }
        //        List<string> pieces = line.Replace(" + ", ",").Replace("(", ",").Replace(")", "")
        //                .Replace("#define ", "").Replace(" ", ",").Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        //        if (pieces.Count < 2)
        //        {
        //            line = sr.ReadLine();
        //            continue;
        //        }
        //        string itemKey = pieces[0].Replace(" ", "");
        //        if (pieces[1].Contains("ITEM_"))//is addition
        //        {
        //            int indexToAddTo = result.GetValueOrDefault(pieces[1], 0);
        //            int amtToAdd = pieces.Count == 2 ? 0 : pieces[2].ToInt();
        //            if (indexToAddTo == 0)
        //            {
        //                throw new Exception("Error parsing items");
        //            }
        //            result[pieces[0]] = indexToAddTo + amtToAdd;
        //        }
        //        else
        //        {
        //            result[pieces[0]] = pieces[1].ToInt();
        //        }
        //        line = sr.ReadLine();
        //    }
        //    //close the file
        //    sr.Close();
        //    return result;
        //}

        public Dictionary<int, string> ReadTextArchive(string path)
        {
            int index = 0;
            Dictionary<int, string> result = new Dictionary<int, string>();
            String line = "";
            StreamReader sr = new StreamReader(path);
            line = sr.ReadLine();
            while (line != null)
            {
                result[index] = line;
                index++;
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }

        public static string FileNameItemData = "itemdata.c";
        public static string FileNameItemHeader = "include/constants/item.h";
        public static string FileSubPathItemDescriptionTextArchive = "data/text/221.txt";
        public static string FileSubPathItemNameTextArchive = "data/text/222.txt";
        private static string ItemIndexString = "itemIndex_";
        private static string ItemNameString = "itemName_";
        private static string ItemDescriptionString = "itemDescription_";
        public static List<string> FileNames = new List<string>() { FileNameItemData, FileNameItemHeader, FileSubPathItemDescriptionTextArchive, FileSubPathItemNameTextArchive };

        internal BoolResultWithMessage OutputItemDataToCsv(string basePath, string outputPath)
        {
            var fileNameToPathDict = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath, FileNames); 
            if (FileNames.Any(i => !fileNameToPathDict.ContainsKey(i)))
            {
                return new BoolResultWithMessage(false, "Missing " + FileNameItemData);
            }
            List<Dictionary<string, string>> itemData = ReadItemData(fileNameToPathDict[FileNameItemData], out List<string> attrsInorder);
            Dictionary<string, int> itemHeaderDict = HgEngineCsvConverterHelperFunctions.ReadItemHeaders(fileNameToPathDict[FileNameItemHeader], "ITEM_");
            Dictionary<int, string> itemNameDict = ReadTextArchive(fileNameToPathDict[FileSubPathItemNameTextArchive]);
            Dictionary<int, string> itemDescDict = ReadTextArchive(fileNameToPathDict[FileSubPathItemDescriptionTextArchive]);
            attrsInorder.InsertRange(1, ItemIndexString, ItemNameString, ItemDescriptionString);
            foreach(var item in itemData)
            {
                string itemKey = item.GetValueOrDefault(ItemKeyString, "");
                int itemIndex = itemHeaderDict.GetValueOrDefault(itemKey, 0);
                item[ItemIndexString] = itemIndex.ToString();
                item[ItemNameString] = itemNameDict.GetValueOrDefault(itemIndex, "");
                item[ItemDescriptionString] = "\"" + itemDescDict.GetValueOrDefault(itemIndex, "") + "\""; ;
            }

            using (var fs = File.Create(outputPath))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                sw.WriteLine(String.Join(",", attrsInorder));
                foreach(var item in itemData)
                {
                    sw.WriteLine(String.Join(",", attrsInorder.Select(i => item.GetValueOrDefault(i, "NULL"))));
                }
            }
            return new BoolResultWithMessage(true, "") ;
        }
    }
}
