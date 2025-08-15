using CsvHelper;
using HgEngineCsvConverter.Code;
using HGEngineHelper.Code.CsvProcessing;
using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static HgEngineCsvConverter.Code.HgEngineEvoDataParser;
using static HGEngineHelper.Code.HGEngineImport.HGEngineCodeParser;

namespace HGEngineHelper.Code.HGEngineImport
{
    public class ImporterFromHge
    {
        public HGEngineCodeParser parser = new HGEngineCodeParser();

        public void ImportPokemonDataAndCodeFromHge()
        {
            string basePath = App.ProjectInfo.hgEnginePath + "/";
            var monReadResult = parser.ReadEngineObjects(basePath + HgEnginePaths.MonDataFileName, new HashSet<string>() { "mondata", "mondatanoname" });
            var babyMonsReadResult = parser.ReadDataEntries(basePath + HgEnginePaths.BabyMonDataFileName, "babymon");
            var heightEntriesReadResult = parser.ReadDataEntries(basePath + HgEnginePaths.HeightTableFileName, "heightentry");
            var spriteOffsetsReadResult = parser.ReadDataEntries(basePath + HgEnginePaths.SpriteOffsetFileName, "dataentry");
            var hiddenAbilityDictReadResult = parser.ReadSpeciesDictionary(basePath + HgEnginePaths.HiddenAbilityFileName);
            var baseExpDictReadResult = parser.ReadSpeciesDictionary(basePath + HgEnginePaths.BaseExperienceFileName);
            var speciesDictReadResult = parser.ReadConstantsHeaderFile(basePath + HgEnginePaths.SpeciesFileName, "SPECIES_");

            Directory.CreateDirectory(App.ProjectInfo.hgeCodeInfoFolder);
            SaveCodeInfo(monReadResult.codeInfo, HgEnginePaths.MonDataFileName);
            SaveCodeInfo(babyMonsReadResult.codeInfo, HgEnginePaths.BabyMonDataFileName);
            SaveCodeInfo(heightEntriesReadResult.codeInfo, HgEnginePaths.HeightTableFileName);
            SaveCodeInfo(spriteOffsetsReadResult.codeInfo, HgEnginePaths.SpriteOffsetFileName);
            SaveCodeInfo(hiddenAbilityDictReadResult.codeInfo, HgEnginePaths.HiddenAbilityFileName);
            SaveCodeInfo(baseExpDictReadResult.codeInfo, HgEnginePaths.BaseExperienceFileName);
            SaveCodeInfo(speciesDictReadResult.codeInfo, HgEnginePaths.SpeciesFileName);
            var speciesIndexDict = speciesDictReadResult.constants.ConvertListToDictionaryOfValues(i => i.key.part1, i => i.value);

            List<MonInfo> pokemonInfo = new List<MonInfo>();
            List<string> distinctAttributes = new List<string>();
            foreach (var monData in monReadResult.hgeObjects)
            {
                string speciesKey = monData.headerInfo[1];
                bool hasMonDexEntry = monData.attributeInfoByLine.ContainsKey("mondexentry");
                var types = GetTypes(monData.attributeInfoByLine["types"], App.ProjectInfo.fairyTypeImplemented);
                var poke = new MonInfo()
                {
                    Key = speciesKey,
                    Name = monData.headerInfo[2],
                    Type1 = types[0],
                    Type2 = types[1],
                    CatchRate = monData.attributeInfoByLine["catchrate"][0].ToInt(),
                    //baseStats = monData.attributeInfoByLine["basestats"],
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
                    ////evyields = monData.attributeInfoByLine["evyields"],
                    HiddenAbility = hiddenAbilityDictReadResult.dictionaryValues.GetValueOrDefault(speciesKey) ?? "",
                    BaseExp = (baseExpDictReadResult.dictionaryValues.GetValueOrDefault(speciesKey) ?? "").ToInt(),
                    BabyMonSpecies = babyMonsReadResult.dataEntries.GetValueOrDefault(speciesKey, new List<string>()).FirstOrDefault("") ?? "",
                    hasBaseExpComment = monData.attributeInfoByLine.ContainsKey("hasBaseExpComment"),
                };
                poke.SetBaseStats(monData.attributeInfoByLine["basestats"]);
                poke.SetEvYields(monData.attributeInfoByLine["evyields"]);
                poke.SetAddSubtractPairIndexValue(speciesIndexDict.GetValueOrDefault(speciesKey, null));
                poke.SetHeightTableEntries(heightEntriesReadResult.dataEntries.GetValueOrDefault(speciesKey, new List<string>()));
                poke.SetSpriteOffsetEntries(spriteOffsetsReadResult.dataEntries.GetValueOrDefault(speciesKey, new List<string>()));

                pokemonInfo.Add(poke);
            }

            using (var writer = new StreamWriter(App.ProjectInfo.dataFolder + CsvFileNames.PokemonCsvFileName))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(pokemonInfo);
            }
        }

        public void ImportEvolutionData( )
        {
            string basePath = App.ProjectInfo.hgEnginePath + "/";
            HgeTableReadResult readResult = parser.ReadHgeTableFile(basePath + HgEnginePaths.EvoDataFileName, "evodata", "evolution", "terminateevodata");
            List<MonEvo> evos = ConvertEvoTableReadToEvoDataList(readResult.evosByKey);
            SaveCodeInfo(readResult.codeInfo, HgEnginePaths.EvoDataFileName);
            using (var writer = new StreamWriter(App.ProjectInfo.dataFolder + CsvFileNames.EvolutionsCsvFileName))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(evos);
            }
        }

        public void ImportFormAndMegaData()
        {
            new HgEngineFormDataParser().ImportFormData();
        }

        private List<MonEvo> ConvertEvoTableReadToEvoDataList(Dictionary<string, List<List<string>>> evoTable)
        {
            List<MonEvo> result = new List<MonEvo>();
            foreach(var kvPair in evoTable)
            {
                foreach(var evoPieces in kvPair.Value)
                {
                    if (evoPieces.Count < 4)
                    {
                        continue;
                    }
                    string evoKey = evoPieces[1];
                    EvoType evoType = (EvoType)Enum.Parse(typeof(EvoType), evoKey);
                    if (evoType == EvoType.EVO_NONE)
                    {
                        break;
                    }
                    bool hasForm = evoPieces[0].Contains("withform");
                    string evoDataStr = evoPieces[2];
                    var evoData = new MonEvo()
                    {
                        HasForm = hasForm,
                        FromSpecies = kvPair.Key,
                        ToSpecies = evoPieces[3],
                        EvoType = evoKey,
                        FormIndex = hasForm ? evoPieces[4].ToInt() : 0,
                    };
                    if (evoDataStr.Contains(ItemKeyStart))
                    {
                        evoData.Item = evoDataStr;
                    }
                    else if (evoDataStr.Contains(MoveKeyStart))
                    {
                        evoData.Move = evoDataStr;
                    }
                    else if (evoDataStr.Contains(SpeciesKeyStart))
                    {
                        evoData.OtherMonSpecies = evoDataStr;
                    }
                    else if (evoDataStr.Contains(TypeKeyStart))
                    {
                        evoData.MoveType = evoDataStr;
                    }
                    else
                    {
                        evoData.Level = evoDataStr.ToInt();
                    }
                    result.Add(evoData);
                }
            }
            return result;
        }

        private List<string> GetTypes(List<string> list, bool useFairyType)
        {
            if (list.Count == 2)
            {
                return list;
            }
            List<string> result = new List<string>();
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Contains("FAIRY_TYPE_IMPLEMENTED"))
                {
                    result.Add(useFairyType ? list[i+2] : list[i+4]);
                    i += 4;
                }
                else
                {
                    result.Add(list[i]);
                }
            }
            return result;
        }

        public static void SaveCodeInfo(HgEngineCodeInfo codeInfo, string nameInHge)
        {
            string json = JsonConvert.SerializeObject(codeInfo, Formatting.Indented);
            File.WriteAllText(App.ProjectInfo.hgeCodeInfoFolder + System.IO.Path.GetFileNameWithoutExtension(nameInHge) + ".json", json);
        }

        internal void ImportOverworldData()
        {
            (new HGEngineOverworldDataParser()).ReadAllOverworldData(App.ProjectInfo.hgEnginePath + "/");
        }
    }
}
