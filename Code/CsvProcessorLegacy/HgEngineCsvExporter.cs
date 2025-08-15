using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using CsvConverter;
using System.IO;


namespace HgEngineCsvConverter.Code
{
    public class HgEngineMonDataCsvExporter
    {
        private HgEngineDataParser __parser = new HgEngineDataParser();

        public class HgEngineMonDataRow
        {
            public HgEngineMonDataRow()
            {

            }
            //defined in mondata.s
            [CsvConverter(ColumnIndex = 1, ColumnName = "Key")]
            public string speciesKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Name")]
            public string speciesName { get; set; }

            [CsvConverter(ColumnIndex = 3, ColumnName = "Type 1")]
            public string Type1 { get; set; }
            [CsvConverter(ColumnIndex = 4, ColumnName = "Type 2")]
            public string Type2 { get; set; }
            [CsvConverter(ColumnIndex = 5, ColumnName = "Ability 1")]
            public string Ability1 { get; set; }
            [CsvConverter(ColumnIndex = 6, ColumnName = "Ability 2")]
            public string Ability2 { get; set; }
            //HiddenAbilityTable.c
            [CsvConverter(ColumnIndex = 7, ColumnName = "Hidden Ability")]
            public string HiddenAbility { get; set; }
            //Back tonormal PokemonData

            public List<string> baseStats { get; set; }
            [CsvConverter(ColumnIndex = 8)]
            public int HP => baseStats[0].ToInt();
            [CsvConverter(ColumnIndex = 9)]
            public int Atk => baseStats[1].ToInt();
            [CsvConverter(ColumnIndex = 10)]
            public int Def => baseStats[2].ToInt();
            [CsvConverter(ColumnIndex = 11)]
            public int SpA => baseStats[3].ToInt();
            [CsvConverter(ColumnIndex = 12)]
            public int SpD => baseStats[4].ToInt();
            [CsvConverter(ColumnIndex = 13)]
            public int Spe => baseStats[5].ToInt();
            [CsvConverter(ColumnIndex = 14, ColumnName = "Dex Entry")]
            public string DexEntry { get; set; }
            [CsvConverter(ColumnIndex = 15, ColumnName = "Dex Classification")]
            public string DexClassification { get; set; }
            [CsvConverter(ColumnIndex = 16, ColumnName = "Dex Height")]
            public string DexHeight { get; set; }
            [CsvConverter(ColumnIndex = 17, ColumnName = "Dex Weight")]
            public string DexWeight { get; set; }
            //babymon.s
            [CsvConverter(ColumnIndex = 18, ColumnName = "Baby Mon")]
            public string BabyMonSpecies { get; set; }
            [CsvConverter(ColumnIndex = 19, ColumnName = "Growth Rate")]
            public string GrowthRate { get; set; }
            [CsvConverter(ColumnIndex = 20, ColumnName = "Egg Group 1")]
            public string EggGroup1 { get; set; }
            [CsvConverter(ColumnIndex = 21, ColumnName = "Egg Group 2")]
            public string EggGroup2 { get; set; }
            [CsvConverter(ColumnIndex = 22, ColumnName = "Egg Cycles")]
            public int EggCycles { get; set; }
            [CsvConverter(ColumnIndex = 23, ColumnName = "Catch Rate")]
            public int CatchRate { get; set; }
            [CsvConverter(ColumnIndex = 24, ColumnName = "Gender Ratio")]
            public int GenderRatio { get; set; }
            [CsvConverter(ColumnIndex = 25, ColumnName = "Base Friendship")]
            public int BaseFriendship { get; set; }
            [CsvConverter(ColumnIndex = 26, ColumnName = "Item 1")]
            public string Item1 { get; set; }
            [CsvConverter(ColumnIndex = 27, ColumnName = "Item 2")]
            public string Item2 { get; set; }
            public List<string> evyields { get; set; }
            [CsvConverter(ColumnIndex = 28)]
            public int EV_HP => baseStats[0].ToInt();
            [CsvConverter(ColumnIndex = 29)]
            public int EV_Atk => baseStats[1].ToInt();
            [CsvConverter(ColumnIndex = 30)]
            public int EV_Def => baseStats[2].ToInt();
            [CsvConverter(ColumnIndex = 31)]
            public int EV_Spa => baseStats[3].ToInt();
            [CsvConverter(ColumnIndex = 32)]
            public int EV_Spd => baseStats[4].ToInt();
            [CsvConverter(ColumnIndex = 33)]
            public int EV_Spe => baseStats[5].ToInt();
            [CsvConverter(ColumnIndex = 34, ColumnName = "Run Chance")]
            public int RunChance { get; set; }
            [CsvConverter(ColumnIndex = 35, ColumnName = "Color Flip")]
            public string ColorFlip { get; set; }
            [CsvConverter(ColumnIndex = 36, ColumnName = "Color Flip Int")]
            public int ColorFlipInteger { get; set; }
            
            //heightTable.s
            public List<string> heightTableEntry;
            [CsvConverter(ColumnIndex = 37)]
            public int FBackHeightOffset => heightTableEntry?[0].ToInt() ?? 0;
            [CsvConverter(ColumnIndex = 38)]
            public int MBackHeightOffset => heightTableEntry?[1].ToInt() ?? 0;
            [CsvConverter(ColumnIndex = 39)]
            public int FFrontHeightOffset => heightTableEntry?[2].ToInt() ?? 0;
            [CsvConverter(ColumnIndex = 40)]
            public int MFrontHeightOffset => heightTableEntry?[3].ToInt() ?? 0;
            //spriteoffsets.s
            public List<string> spriteOffsetEntries;
            [CsvConverter(ColumnIndex = 41)]
            public int FrontAnim => spriteOffsetEntries?[0].ToInt() ?? 0;
            [CsvConverter(ColumnIndex = 42)]
            public int BackAnim => spriteOffsetEntries?[1].ToInt() ?? 0;
            [CsvConverter(ColumnIndex = 43)]
            public int MonOffy => spriteOffsetEntries?[2].ToInt() ?? 0;
            [CsvConverter(ColumnIndex = 44)]
            public int ShadowOffx => spriteOffsetEntries?[3].ToInt() ?? 0;
            [CsvConverter(ColumnIndex = 44)]
            public string ShadowSize => spriteOffsetEntries?[4] ?? "";
            //BaseExperienceTable.c
            [CsvConverter(ColumnIndex = 45)]
            public int BaseExp { get; set; }
        }

        public static string MonDataFileName = "mondata.s";
        public static string BabyMonDataFileName = "babymons.s";
        public static string HeightTableFileName = "heightTable.s";
        public static string SpriteOffsetFileName = "spriteOffsets.s";
        public static string BaseExperienceFileName = "BaseExperienceTable.c";
        public static string HiddenAbilityFileName = "HiddenAbilityTable.c";

        public static List<string> FileNames = new List<string>()
        {
            MonDataFileName, BabyMonDataFileName, HeightTableFileName, SpriteOffsetFileName, BaseExperienceFileName, HiddenAbilityFileName,
        };

        public Dictionary<string, List<string>> ConvertToDictionaryOfLists(List<List<string>> input)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            foreach(var inputList in input)
            {
                string speciesKey = inputList[0];
                inputList.RemoveAt(0);
                result[speciesKey] = inputList;
            }

            return result;
        }

        public BoolResultWithMessage CreateJoinedMonDataCsv(string basePath, string outputPath)
        {
            var result = new BoolResultWithMessage();
            var fileNameToPathDict = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath, FileNames);
            List<string> missingFileNames = new List<string>();
            foreach(var fileName in FileNames)
            {
                if (fileNameToPathDict.ContainsKey(fileName))
                {
                    continue;
                }
                missingFileNames.Add(fileName);
            }
            if (missingFileNames.Count > 0)
            {
                return new BoolResultWithMessage(false, "Could not find the following files: " + String.Join(", ", missingFileNames));
            }
            List<HgEngineObject> monDataList = __parser.ReadEngineObjects(fileNameToPathDict[MonDataFileName], "mondata");
            Dictionary<string, string> babymons = __parser.ReadDataEntries(fileNameToPathDict[BabyMonDataFileName], "babymon").ToDictBasedOnFirst() ;
            Dictionary<string, List< string>> heightTableEntries = ConvertToDictionaryOfLists(__parser.ReadDataEntries(fileNameToPathDict[HeightTableFileName], "heightentry"));
            var spriteOffsetsList = __parser.ReadDataEntries(fileNameToPathDict[SpriteOffsetFileName], "dataentry");
            Dictionary<string, List<string>> spriteOffsets = ConvertToDictionaryOfLists(spriteOffsetsList);
            Dictionary<string, string> baseExperienceTableEntries = __parser.ReadSpeciesDictionary(fileNameToPathDict[BaseExperienceFileName]);
            Dictionary<string, string> hiddenAbilityTableEntries = __parser.ReadSpeciesDictionary(fileNameToPathDict[HiddenAbilityFileName]);

            List< HgEngineMonDataRow > monDataRows = new List< HgEngineMonDataRow >();
            foreach(var monData in monDataList)
            {
                string speciesKey = monData.headerInfo[0];
                bool hasMonDexEntry = monData.attributeInfoByLine.ContainsKey("mondexentry");
                monDataRows.Add(new HgEngineMonDataRow()
                {
                    speciesKey = speciesKey,
                    speciesName = HgEngineCsvConverterHelperFunctions.FixDescriptionColumn(monData.headerInfo),
                    Type1 = monData.attributeInfoByLine["types"][0].Replace(" ", "").Replace("(", "").Replace(")", "").Replace("FAIRY_TYPE_IMPLEMENTED?", "").Split(":").FirstOrDefault(""),
                    Type2 = monData.attributeInfoByLine["types"][1].Replace(" ", "").Replace("(", "").Replace(")", "").Replace("FAIRY_TYPE_IMPLEMENTED?", "").Split(":").FirstOrDefault(""),
                    CatchRate = monData.attributeInfoByLine["catchrate"][0].ToInt(),
                    baseStats = monData.attributeInfoByLine["basestats"],
                    BaseFriendship = monData.attributeInfoByLine["basefriendship"][0].ToInt(),
                    Item1 = monData.attributeInfoByLine["items"][0],
                    Item2 = monData.attributeInfoByLine["items"][1],
                    GenderRatio = monData.attributeInfoByLine["genderratio"][0].ToInt(),
                    EggCycles = monData.attributeInfoByLine["eggcycles"][0].ToInt(),
                    GrowthRate = monData.attributeInfoByLine["growthrate"][0],
                    EggGroup1 = monData.attributeInfoByLine["egggroups"][0],
                    EggGroup2 = monData.attributeInfoByLine["egggroups"][1],
                    Ability1 = monData.attributeInfoByLine["abilities"][0],
                    Ability2 = monData.attributeInfoByLine["abilities"][1],
                    RunChance = monData.attributeInfoByLine["runchance"][0].ToInt(),
                    //DexEntrySpecies = hasMonDexEntry ? monData.attributeInfoByLine["mondexentry"][0] : "",
                    DexEntry = hasMonDexEntry ? HgEngineCsvConverterHelperFunctions.FixDescriptionColumn(monData.attributeInfoByLine["mondexentry"]) : "",
                    //DexClassificationSpecies = hasMonDexEntry ? monData.attributeInfoByLine["mondexclassification"][0] : "",
                    DexClassification = hasMonDexEntry ? HgEngineCsvConverterHelperFunctions.FixDescriptionColumn(monData.attributeInfoByLine["mondexclassification"]) : "",
                    //DexHeightSpecies = hasMonDexEntry ? monData.attributeInfoByLine["mondexheight"][0] : "",
                    DexHeight = hasMonDexEntry ? HgEngineCsvConverterHelperFunctions.FixDescriptionColumn(monData.attributeInfoByLine["mondexheight"]) : "",
                    //DexWeightSpecies = hasMonDexEntry ? monData.attributeInfoByLine["mondexweight"][0] : "",
                    DexWeight = hasMonDexEntry ? HgEngineCsvConverterHelperFunctions.FixDescriptionColumn(monData.attributeInfoByLine["mondexweight"]) : "",
                    ColorFlip = monData.attributeInfoByLine["colorflip"][0],
                    ColorFlipInteger = monData.attributeInfoByLine["colorflip"][1].ToInt(),
                    evyields = monData.attributeInfoByLine["evyields"],
                    HiddenAbility = hiddenAbilityTableEntries.GetValueOrDefault(speciesKey) ?? "",
                    BaseExp = (baseExperienceTableEntries.GetValueOrDefault(speciesKey) ?? "").ToInt(),
                    BabyMonSpecies = babymons.GetValueOrDefault(speciesKey) ?? "",
                    heightTableEntry = heightTableEntries.GetValueOrDefault(speciesKey),
                    spriteOffsetEntries = spriteOffsets.GetValueOrDefault(speciesKey),
                });
            }
            using (var fs = File.Create(outputPath))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<HgEngineMonDataRow>(sw);

                foreach(var monDataRow in monDataRows)
                {
                    service.WriteRecord(monDataRow);
                }
            }

            return new BoolResultWithMessage(true, "Success");
        }
    }
}
