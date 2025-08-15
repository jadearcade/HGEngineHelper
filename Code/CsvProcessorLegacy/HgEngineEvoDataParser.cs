using CsvConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static HgEngineCsvConverter.Code.HgEngineEvoDataParser;
using static HgEngineCsvConverter.Code.HgEngineMonDataCsvExporter;

namespace HgEngineCsvConverter.Code
{
    public class HgEngineEvoDataParser
    {
        public class MonEvo
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "Evolve From")]
            public string FromSpecies { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Evolve To")]
            public string ToSpecies { get; set; }
            [CsvConverter(ColumnIndex = 3, ColumnName = "Has Form?")]
            public bool HasForm { get; set; }
            [CsvConverter(ColumnIndex = 4, ColumnName = "Form Index")]
            public int FormIndex { get; set; } = 0;
            [CsvConverter(ColumnIndex = 5, ColumnName = "Evolution Type")]
            public string EvoType { get; set; }
            [CsvConverter(ColumnIndex = 6, ColumnName = "Level")]
            public int Level { get; set; } = 0;//and beauty level
            [CsvConverter(ColumnIndex = 7, ColumnName = "Move Key")]
            public string Move { get; set; } = "";
            [CsvConverter(ColumnIndex = 8, ColumnName = "Item Key")]
            public string Item { get; set; } = "";
            [CsvConverter(ColumnIndex = 9, ColumnName = "Move Type")]
            public string MoveType { get; set; } = "";
            [CsvConverter(ColumnIndex = 10, ColumnName = "Other Mon")]
            public string OtherMonSpecies { get; set; } = "";
            public EvoType evoType => (EvoType) Enum.Parse(typeof(EvoType), EvoType);

            public List<string> GetEvoTableEntry()
            {
                List<string> result = new List<string>();
                result.Add(evoType.ToString());
                if (ItemEvoTypes.Contains(evoType)){
                    result.Add(Item);
                }else if (LevelEvoTypes.Contains(evoType) || NonStandardLevelEvoTypes.Contains(evoType))
                {
                    result.Add(Level.ToString());
                }else if (MoveEvoTypes.Contains(evoType)) {
                    result.Add(Move);
                }else if (MoveTypeEvoTypes.Contains(evoType))
                {
                    result.Add(MoveType);
                }else if (OtherMonEvoTypes.Contains(evoType))
                {
                    result.Add(OtherMonSpecies);
                }
                else
                {
                    result.Add("0");
                }
                result.Add(ToSpecies);
                if (HasForm)
                {
                    result.Add(FormIndex.ToString());
                }
                return result;
            }
        }

        public static HashSet<EvoType> ItemEvoTypes = new HashSet<EvoType>() { EvoType.EVO_ITEM_DAY, EvoType.EVO_ITEM_NIGHT, EvoType.EVO_TRADE_ITEM
            , EvoType.EVO_STONE, EvoType.EVO_STONE_MALE, EvoType.EVO_STONE_FEMALE};

        public static HashSet<EvoType> LevelEvoTypes = new HashSet<EvoType>()
        {
            EvoType.EVO_LEVEL, EvoType.EVO_LEVEL_MALE, EvoType.EVO_LEVEL_FEMALE
            ,  EvoType.EVO_LEVEL_ATK_EQ_DEF, EvoType.EVO_LEVEL_ATK_GT_DEF, EvoType.EVO_LEVEL_ATK_LT_DEF
            , EvoType.EVO_LEVEL_DARK_TYPE_MON_IN_PARTY, EvoType.EVO_LEVEL_DAY, EvoType.EVO_LEVEL_DUSK,
            EvoType.EVO_LEVEL_DUSK, EvoType.EVO_LEVEL_FEMALE, EvoType.EVO_LEVEL_MALE, EvoType.EVO_LEVEL_NATURE_AMPED
            , EvoType.EVO_LEVEL_NATURE_LOW_KEY, EvoType.EVO_LEVEL_NIGHT, EvoType.EVO_LEVEL_NINJASK
            , EvoType.EVO_LEVEL_PID_HI, EvoType.EVO_LEVEL_PID_LO, EvoType.EVO_LEVEL_RAIN, EvoType.EVO_LEVEL_SHEDINJA 
        };

        public static HashSet<EvoType> NonStandardLevelEvoTypes = new HashSet<EvoType>()
        {
            EvoType.EVO_BEAUTY, EvoType.EVO_AMOUNT_OF_CRITICAL_HITS, EvoType.EVO_HURT_IN_BATTLE_AMOUNT,
        };

        public static HashSet<EvoType> OtherMonEvoTypes = new HashSet<EvoType>()
        {
            EvoType.EVO_OTHER_PARTY_MON, EvoType.EVO_TRADE_SPECIFIC_MON,
        };

        public static HashSet<EvoType> MoveEvoTypes = new HashSet<EvoType>()
        {
            EvoType.EVO_HAS_MOVE,
        };

        public static HashSet<EvoType> MoveTypeEvoTypes = new HashSet<EvoType>() { EvoType.EVO_HAS_MOVE_TYPE };

        public enum EvoType
        {
            EVO_NONE = 0,
            EVO_FRIENDSHIP = 1,
            EVO_FRIENDSHIP_DAY = 2,
            EVO_FRIENDSHIP_NIGHT = 3,
            EVO_LEVEL = 4,
            EVO_TRADE = 5,
            EVO_TRADE_ITEM = 6,
            EVO_STONE = 7,
            EVO_LEVEL_ATK_GT_DEF = 8,
            EVO_LEVEL_ATK_EQ_DEF = 9,
            EVO_LEVEL_ATK_LT_DEF = 10,
            EVO_LEVEL_PID_LO = 11,
            EVO_LEVEL_PID_HI = 12,
            EVO_LEVEL_NINJASK = 13,
            EVO_LEVEL_SHEDINJA = 14,
            EVO_BEAUTY = 15,
            EVO_STONE_MALE = 16,
            EVO_STONE_FEMALE = 17,
            EVO_ITEM_DAY = 18,
            EVO_ITEM_NIGHT = 19,
            EVO_HAS_MOVE = 20,
            EVO_OTHER_PARTY_MON = 21,
            EVO_LEVEL_MALE = 22,
            EVO_LEVEL_FEMALE = 23,
            EVO_LEVEL_ELECTRIC_FIELD = 24,
            EVO_LEVEL_MOSSY_STONE = 25,
            EVO_LEVEL_ICY_STONE = 26,
            EVO_LEVEL_DAY = 27,
            EVO_LEVEL_NIGHT = 28,
            EVO_LEVEL_DUSK = 29,
            EVO_LEVEL_RAIN = 30,
            EVO_HAS_MOVE_TYPE = 31,
            EVO_LEVEL_DARK_TYPE_MON_IN_PARTY = 32,
            EVO_TRADE_SPECIFIC_MON = 33,
            EVO_LEVEL_NATURE_AMPED = 34,
            EVO_LEVEL_NATURE_LOW_KEY = 35,
            EVO_AMOUNT_OF_CRITICAL_HITS = 36,
            EVO_HURT_IN_BATTLE_AMOUNT = 37,
        }
        public static string SpeciesKeyStart = "SPECIES_";
        public static string ItemKeyStart = "ITEM_";
        public static string MoveKeyStart = "MOVE_";
        public static string TypeKeyStart = "TYPE_";

        public List<MonEvo> ReadEvoDataFormTable(string filePath)
        {
            List<MonEvo> result = new List<MonEvo>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            string currentEvoFromSpecies = "";
            while (line != null)
            {
                if (line.Contains("evodata ") || line.Contains("terminateevodata"))
                {
                    currentEvoFromSpecies = "";
                    var pieces = line.Split(" ");
                    if (pieces.Length > 1)
                    {
                        currentEvoFromSpecies = pieces[1];
                    }
                }else if (currentEvoFromSpecies != "")
                {
                    List<string> listOfData = line.Replace("evolution ", "evolution,").Replace("evolutionwithform ", "evolutionwithform,").Replace(" ", "").Split(",").ToList();
                    if (listOfData.Count < 4)
                    {
                        line = sr.ReadLine();
                        continue;
                    }
                    string evoKey = listOfData[1];
                    EvoType evoType = (EvoType)Enum.Parse(typeof(EvoType), evoKey);
                    if (evoType == EvoType.EVO_NONE)
                    {
                        currentEvoFromSpecies = "";//stop searching
                        line = sr.ReadLine();
                        continue;
                    }
                    bool hasForm = listOfData[0].Contains("withform");
                    string evoDataStr = listOfData[2];
                    var evoData = new MonEvo()
                    {
                        HasForm = hasForm,
                        FromSpecies = currentEvoFromSpecies,
                        ToSpecies = listOfData[3],
                        EvoType = evoKey,
                        FormIndex = hasForm ? listOfData[4].ToInt() : 0,
                    };
                    if (evoDataStr.Contains(ItemKeyStart))
                    {
                        evoData.Item = evoDataStr;
                    }else if (evoDataStr.Contains(MoveKeyStart))
                    {
                        evoData.Move = evoDataStr;
                    }else if (evoDataStr.Contains(SpeciesKeyStart))
                    {
                        evoData.OtherMonSpecies = evoDataStr;
                    }else if (evoDataStr.Contains(TypeKeyStart))
                    {
                        evoData.MoveType = evoDataStr;
                    }
                    else
                    {
                        evoData.Level = evoDataStr.ToInt();
                    }
                    result.Add(evoData);
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }

        public static string FilePathEvoData = "evodata.s";

        internal BoolResultWithMessage CreateEvoDataFormTableCsv(string basePath, string outputPath)
        {
            var filePath = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath, new List<string>() { FilePathEvoData }).GetValueOrDefault(FilePathEvoData);
            if ((filePath ?? "") == "")
            {
                return new BoolResultWithMessage(false, "Missing " + FilePathEvoData);
            }
            var evoData = ReadEvoDataFormTable(filePath);

            using (var fs = File.Create(outputPath))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<MonEvo>(sw);

                foreach (var evoDataRow in evoData)
                {
                    service.WriteRecord(evoDataRow);
                }
            }
            return new BoolResultWithMessage(true, "") ;
        }
    }
}
