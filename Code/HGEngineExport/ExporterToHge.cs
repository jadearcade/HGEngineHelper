using HGEngineHelper.Code.CsvProcessing;
using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using Newtonsoft.Json;
using System.IO;
using static HgEngineCsvConverter.Code.HgEngineEvoDataParser;
using static HGEngineHelper.Code.HGEngineExport.HgEngineCodeWriter;
using static HGEngineHelper.Code.HGEngineImport.HgEngineFormDataParser;

namespace HGEngineHelper.Code.HGEngineExport
{
    public class ExporterToHge
    {
        private HgEngineCodeWriter codeWriter = new HgEngineCodeWriter();
        public void ExportPokemonDataToHgeTestFiles()
        {

            //var babyMonCodeInfo = GetCodeInfo(HgEnginePaths.BabyMonDataFileName);
            //var heightCodeInfo = GetCodeInfo(HgEnginePaths.HeightTableFileName);
            //var spriteOffsetCodeInfo = GetCodeInfo(HgEnginePaths.SpriteOffsetFileName);
            //var hiddenAbilityCodeInfo = GetCodeInfo(HgEnginePaths.HiddenAbilityFileName);
            //var baseExpCodeInfo = GetCodeInfo(HgEnginePaths.BaseExperienceFileName);
            //var speciesCodeInfo = GetCodeInfo(HgEnginePaths.SpeciesFileName);

            List<MonInfo> pokemon = (new HGEHelperCsvReader()).GetPokemonForProject(App.ProjectInfo);
            List<FormesForSpeciesInfoRow> formes = (new HGEHelperCsvReader()).GetFormesForProject(App.ProjectInfo);
            WriteModelFile(HgEnginePaths.MonDataFileName, GetWriteInfoMonData(pokemon));
            WriteDataEntryFile<MonInfo>(HgEnginePaths.HeightTableFileName, GetHeightTableWriteInfo<MonInfo>(pokemon));
            WriteDataEntryFile<MonInfo>(HgEnginePaths.BabyMonDataFileName, GetBabyMonWriteEntryInfo<MonInfo>(pokemon));
            WriteDataEntryFile<MonInfo>(HgEnginePaths.SpriteOffsetFileName, GetSpriteOffsetsWriteInfo<MonInfo>(pokemon));
            WriteConstantsFile<MonInfo>(HgEnginePaths.HiddenAbilityFileName, GetHiddenAbilityWriteInfo(pokemon));
            WriteConstantsFile<MonInfo>(HgEnginePaths.BaseExperienceFileName, GetBaseExperienceWriteInfo(pokemon));
            (new SpeciesFormFileWriter()).WriteSpeciesFormFile(App.ProjectInfo.hgeTestOutputFolder + "/" + HgEnginePaths.SpeciesFormFileName, formes, GetCodeInfo(HgEnginePaths.SpeciesFormFileName) );
            (new SpeciesFormFileWriter()).WriteFormSpeciesMappingFile(App.ProjectInfo.hgeTestOutputFolder + "/" + HgEnginePaths.FormToSpeciesMappingFileName, formes);
            (new SpeciesFormFileWriter()).WriteFormReversionMappingFile(App.ProjectInfo.hgeTestOutputFolder + "/" + HgEnginePaths.FormReversionMappingFileName, formes, GetCodeInfo(HgEnginePaths.FormReversionMappingFileName));
            (new SpeciesFormFileWriter()).WriteMegaFormFile(App.ProjectInfo.hgeTestOutputFolder + "/" + HgEnginePaths.MegaFormTableFileName, formes, GetCodeInfo(HgEnginePaths.MegaFormTableFileName));
            (new SpeciesHeaderFileWriter()).WriteSpeciesConstantFiles(HgEnginePaths.SpeciesFileName, HgEnginePaths.SpeciesIncFileName, GetCodeInfo(HgEnginePaths.SpeciesFileName), pokemon);
        }

        public void ExportEvoDataToHgeTestFiles()
        {
            string basePath = App.ProjectInfo.hgeTestOutputFolder;
            Directory.CreateDirectory(basePath);
            List<MonInfo> pokemon = (new HGEHelperCsvReader()).GetPokemonForProject(App.ProjectInfo);
            List<MonEvo> evolutions = (new HGEHelperCsvReader()).GetEvosForProject(App.ProjectInfo);
            codeWriter.WriteHgeTableFile(basePath + HgEnginePaths.EvoDataFileName, GetEvoDataWriteInfo(pokemon, evolutions));
        }

        public HgEngineObjectWriteInfo<MonInfo> GetWriteInfoMonData(List<MonInfo> pokemon)
        {
            var monCodeInfo = GetCodeInfo(HgEnginePaths.MonDataFileName);
            return new HgEngineObjectWriteInfo<MonInfo>()
            {
                codeInfo = monCodeInfo,
                objectsToWrite = pokemon,
                getHeaderRowFunc = pokemon => "mondata " + pokemon.Key + ", " + pokemon.Name.Wrap("\""),
                attributes = new List<HgeAttr<MonInfo>>()
                {
                    new HgeAttr<MonInfo>(){ attr = "basestats"
                    , valGet = mon => String.Join(", ", new List<string>(){mon.HP.ToString(), mon.Atk.ToString()
                    , mon.Def.ToString(), mon.Spe.ToString(), mon.SpA.ToString(), mon.SpD.ToString()}) },
                    new HgeAttr<MonInfo>(){ attr = "types", valGet = mon => mon.GetTypeLine(App.ProjectInfo.fairyTypeImplemented)},
                    new HgeAttr<MonInfo>() { attr = "catchrate", valGet = mon => mon.CatchRate.ToString()},
                    new HgeAttr<MonInfo>() { attr = "baseexp", valGet = mon => mon.hasBaseExpComment ? "0 // defined in baseexp.s" : mon.BaseExp.ToString()},
                    new HgeAttr<MonInfo>(){ attr = "evyields"
                    , valGet = mon => String.Join(", ", new List<string>(){mon.EV_HP.ToString(), mon.EV_Atk.ToString()
                    , mon.EV_Def.ToString(), mon.EV_Spe.ToString(), mon.EV_SpA.ToString(), mon.EV_SpD.ToString()}) },
                    new HgeAttr<MonInfo>() { attr = "items", valGet = mon => mon.Item1 + ", " + mon.Item2 },
                    new HgeAttr<MonInfo>() { attr = "genderratio", valGet = mon => mon.GenderRatio.ToString()},
                    new HgeAttr<MonInfo>() { attr = "eggcycles", valGet = mon => mon.EggCycles.ToString() },
                    new HgeAttr<MonInfo>() { attr = "basefriendship", valGet = mon => mon.BaseFriendship.ToString()},
                    new HgeAttr<MonInfo>() { attr = "growthrate", valGet = mon => mon.GrowthRate.ToString()},
                    new HgeAttr<MonInfo>() { attr = "egggroups", valGet = mon => mon.EggGroup1 + ", " + mon.EggGroup2},
                    new HgeAttr<MonInfo>() { attr = "abilities", valGet = mon => mon.Ability1 + ", " + mon.Ability2 },
                    new HgeAttr<MonInfo>() { attr = "runchance", valGet = mon => mon.RunChance.ToString()},
                    new HgeAttr<MonInfo>() { attr = "colorflip", valGet = mon => mon.ColorFlip.ToString() + ", " + mon.ColorFlipInteger.ToString()},
                    new HgeAttr<MonInfo>() { attr = "mondexentry", valGet = mon =>
                        mon.Key != "SPECIES_NONE" && mon.DexEntry == "" ? "" :
                        mon.Key + ", " + mon.DexEntry.ToString().Wrap("\"")},

                    new HgeAttr<MonInfo>() { attr = "mondexclassification", valGet = mon =>
                        mon.Key != "SPECIES_NONE" && mon.DexClassification == "" ? "" :
                        mon.Key + ", " + mon.DexClassification.ToString().Wrap("\"")},

                    new HgeAttr<MonInfo>() { attr = "mondexheight", valGet = mon =>
                        mon.Key != "SPECIES_NONE" && mon.DexHeight == "" ? "" :
                        mon.Key + ", " + mon.DexHeight.ToString().Wrap("\"")},

                    new HgeAttr<MonInfo>() { attr = "mondexweight", valGet = mon =>
                        mon.Key != "SPECIES_NONE" && mon.DexWeight == "" ? "" :
                        mon.Key + ", " + mon.DexWeight.ToString().Wrap("\"")},
                },
                getNumSpacingLinesFunc = mon => mon.SpeciesIndexValue1.Contains("_") ? 1 : 2
            };
        }

        private void WriteModelFile<T>(string subPathName, HgEngineObjectWriteInfo<T> writeInfo)
        {
            string basePath = App.ProjectInfo.hgeTestOutputFolder;
            Directory.CreateDirectory(basePath);
            codeWriter.WriteModelFile(basePath + subPathName, writeInfo);
        }

        private void WriteDataEntryFile<T>(string subPathName, HgEngineDataEntryWriteInfo<T> writeInfo)
        {
            string basePath = App.ProjectInfo.hgeTestOutputFolder;
            Directory.CreateDirectory(basePath);
            codeWriter.WriteHgEngineDataEntryFile<T>(basePath + subPathName, writeInfo);
        }

        private void WriteConstantsFile<T>(string subPathName, HgEngineConstantsDictionaryWriteInfo<T> writeInfo)
        {
            string basePath = App.ProjectInfo.hgeTestOutputFolder;
            Directory.CreateDirectory(basePath);
            codeWriter.WriteConstantsDictionary<T>(basePath + subPathName, writeInfo);
        }

        public HgEngineCodeInfo GetCodeInfo( string nameInHge)
        {
            string jsonContent = System.IO.File.ReadAllText(App.ProjectInfo.hgeCodeInfoFolder + Path.GetFileNameWithoutExtension(nameInHge) + ".json");
            return JsonConvert.DeserializeObject<HgEngineCodeInfo>(jsonContent);
        }

        public HgEngineDataEntryWriteInfo<MonInfo> GetBabyMonWriteEntryInfo<T>(List<MonInfo> pokemon)
        {
            var codeInfo = GetCodeInfo(HgEnginePaths.BabyMonDataFileName);
            return new HgEngineDataEntryWriteInfo<MonInfo>()
            {
                codeInfo = codeInfo,
                GetDataEntryKey = mon => mon.Key,
                HasDataEntry = mon => mon.BabyMonSpecies != "",
                GetDataEntryClassName = mon => "babymon",
                GetDataEntryLineListValue = mon => new List<string>() { mon.BabyMonSpecies },
                objectsToWrite = pokemon,
            };
        }

        public HgEngineDataEntryWriteInfo<MonInfo> GetHeightTableWriteInfo<T>(List<MonInfo> pokemon)
        {
            var codeInfo = GetCodeInfo(HgEnginePaths.HeightTableFileName);
            return new HgEngineDataEntryWriteInfo<MonInfo>()
            {
                codeInfo = codeInfo,
                GetDataEntryKey = mon => mon.Key,
                HasDataEntry = mon => true,
                GetDataEntryClassName = mon => "heightentry",
                GetDataEntryLineListValue = mon => new List<string>() { NullIntToStr(mon.FBackHeightOffset), NullIntToStr(mon.MBackHeightOffset)
                , NullIntToStr(mon.FFrontHeightOffset), NullIntToStr(mon.MFrontHeightOffset) },
                objectsToWrite = pokemon,
            };
        }

        public string NullIntToStr(int? intVal)
        {
            if (!intVal.HasValue)
            {
                return "null".Wrap("\"");
            }
            return intVal.Value.ToString();
        }

        public HgEngineDataEntryWriteInfo<MonInfo> GetSpriteOffsetsWriteInfo<T>(List<MonInfo> pokemon)
        {
            var codeInfo = GetCodeInfo(HgEnginePaths.SpriteOffsetFileName);
            return new HgEngineDataEntryWriteInfo<MonInfo>()
            {
                codeInfo = codeInfo,
                GetDataEntryKey = mon => mon.Key,
                HasDataEntry = mon => true,
                GetDataEntryClassName = mon => "dataentry",
                GetDataEntryLineListValue = mon => new List<string>() { mon.FrontAnim.ToString(), mon.BackAnim.ToString()
                , mon.MonOffy.ToString(), mon.ShadowOffx.ToString(), mon.ShadowSize },
                objectsToWrite = pokemon,
            };
        }

        public HgEngineConstantsDictionaryWriteInfo<MonInfo> GetHiddenAbilityWriteInfo(List<MonInfo> pokemon)
        {
            var codeInfo = GetCodeInfo(HgEnginePaths.HiddenAbilityFileName);
            return new HgEngineConstantsDictionaryWriteInfo<MonInfo>()
            {
                codeInfo = codeInfo,
                objectsToWrite = pokemon,
                HasEntryFunc = mon => true,
                GetKeyFunc = mon => mon.Key,
                GetValueFunc = mon => mon.HiddenAbility,
            };
        }

        public HgEngineConstantsDictionaryWriteInfo<MonInfo> GetBaseExperienceWriteInfo(List<MonInfo> pokemon)
        {
            var codeInfo = GetCodeInfo(HgEnginePaths.BaseExperienceFileName);
            return new HgEngineConstantsDictionaryWriteInfo<MonInfo>()
            {
                codeInfo = codeInfo,
                objectsToWrite = pokemon,
                HasEntryFunc = mon => true,
                GetKeyFunc = mon => mon.Key,
                GetValueFunc = mon => mon.BaseExp.ToString(),
            };
        }

        public static int MAX_EVOLUTIONS = 9;
        public static MonEvo EvoNone = new MonEvo()
        {
            EvoType = EvoType.EVO_NONE.ToString(),
            ToSpecies = "SPECIES_NONE",
        };

        public HgEngineTableWriteInfo<MonInfo, MonEvo> GetEvoDataWriteInfo(List<MonInfo> pokemon, List<MonEvo> evos)
        {
            var codeInfo = GetCodeInfo(HgEnginePaths.EvoDataFileName);
            //pokemon = pokemon.Where(i => i.Key != "SPECIES_NONE").ToList();
            Dictionary<string, List<MonEvo>> evosByMonFromKey = HelperFunctions.ConvertListToDictionaryOfLists(evos, i => i.FromSpecies);
            foreach(var mon in pokemon)
            {
                var evosThisMon = evosByMonFromKey.GetValueOrDefault(mon.Key, new List<MonEvo>());
                while(evosThisMon.Count < MAX_EVOLUTIONS)
                {
                    evosThisMon.Add(new MonEvo()
                    {
                        EvoType = EvoType.EVO_NONE.ToString(),
                        ToSpecies = "SPECIES_NONE",
                    });
                }
                evosByMonFromKey[mon.Key] = evosThisMon;
            }
            return new HgEngineTableWriteInfo<MonInfo, MonEvo>()
            {
                className = "evodata",
                codeInfo = codeInfo,
                getAttributeNameFunc = evo => !evo.HasForm ? "evolution" : "evolutionwithform",
                objectsToWrite = pokemon,
                getExtraSpacingLinesFunc = mon => mon.GetGroupKey() == MonInfo.PALDEAN_GROUP_KEY ? new List<string>() { "", "" } : new List<string>() {  ""},
                getKeyFunc = mon => mon.Key,
                terminateClassName = "terminateevodata",
                entriesByKey = evosByMonFromKey,
                doesTerminateClassHaveSpacing = mon => mon.GetGroupKey() == MonInfo.MEGA_GROUP_KEY ? true 
                : ((mon?.GetPokemonGenerationForCanonicalMon() ?? MonInfo.PokemonGeneration.GEN_SIX_PLUS) < MonInfo.PokemonGeneration.GEN_SIX_PLUS),
                getValuesForTableEntryFunc = evo => evo.GetEvoTableEntry(),
            };
        }
    }
}
