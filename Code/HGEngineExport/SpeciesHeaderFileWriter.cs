using HgEngineCsvConverter.Code;
using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using System;
using System.IO;

namespace HGEngineHelper.Code.HGEngineExport
{
    public class SpeciesHeaderFileWriter
    {
        public static HgEngineCodeInfo IncFileCodeInfo = new HgEngineCodeInfo()
        {
            codeSections = new Dictionary<CodeSectionType, List<CodeSection>>()
            {
                { 
                    CodeSectionType.BEGINNING, new List<CodeSection>()
                    {
                        new CodeSection() { lines = new List<string>()
                        {
                            ".ifndef GUARD_SPECIES",".definelabel GUARD_SPECIES, 0","",""
                        }}
                    }
                },
            }
        };

        public void WriteSpeciesConstantFiles(string subPath, string subPathInc, HgEngineCodeInfo codeInfo, List<MonInfo> pokemon)
        {
            string basePath = App.ProjectInfo.hgeTestOutputFolder;
            Directory.CreateDirectory(basePath);
            string pathHeader = basePath + subPath;
            string pathInc = basePath + subPathInc;

            string directoryHeader = System.IO.Path.GetDirectoryName(pathHeader);
            Directory.CreateDirectory(directoryHeader);

            string directoryInc = System.IO.Path.GetDirectoryName(pathInc);
            Directory.CreateDirectory(directoryInc);
            string lastKey = "";
            int currentIndex = 0;
            using (StreamWriter incFile = new StreamWriter(pathInc))
            using (StreamWriter headerFile = new StreamWriter(pathHeader))
            {
                HgEngineCodeWriter.WriteCodeSectionIfItExists(headerFile, ref codeInfo.codeSections, CodeSectionType.BEGINNING);
                HgEngineCodeWriter.WriteCodeSectionIfItExists(incFile, ref IncFileCodeInfo.codeSections, CodeSectionType.BEGINNING);

                Dictionary<string, List<MonInfo>> monsByGroupKey = HelperFunctions.ConvertListToDictionaryOfLists(pokemon, i => i.GetGroupKey());
                foreach (var group in MonInfo.PokemonGroups)
                {
                    var monsThisGroup = monsByGroupKey.GetValueOrDefault(group.groupKey, new List<MonInfo>())
                        .OrderBy(i => i.SpeciesIndexValue1.ToInt()).ThenBy(i => i.SpeciesIndexValue2.ToInt()).ToList() ;
                    MonInfo prevMon = null;
                    foreach (var mon in monsThisGroup)
                    {
                        AddSubtractPair indexValue = mon.GetSpeciesIndex();
                        string key = mon.Key;
                        if (key == "SPECIES_MIMEJR")
                        {
                            key = "SPECIES_MIME_JR";
                        }
                        string keyDisplay = mon.SpeciesIndexValue1.Contains("_") ? (key) + " " 
                            : (mon.SpeciesIndexValue1.ToInt() > MonInfo.GenFour ? (key + " ") : key.PadRight(23));
                        string specialLineEnding = SpecialLineEndingsByKey.GetValueOrDefault(mon.Key, "");
                        string headerLine = "#define " + keyDisplay + indexValue.GetString() + specialLineEnding;
                        string incLine = ".equ " + mon.Key + ", " + indexValue.GetString();

                        WriteExtraSpacingLinesPerMon(group, mon, prevMon, headerFile, false);
                        WriteExtraSpacingLinesPerMon(group, mon, prevMon, incFile, true);

                        //Write main line
                        headerFile.WriteLine(headerLine);
                        incFile.WriteLine(incLine);
                        prevMon = mon;
                    }

                    var lastMon = monsThisGroup.Count > 0 ? monsThisGroup.Last() : null;
                    currentIndex = currentIndex + monsThisGroup.Count;
                    List<string> afterCodeSectionsHeader = GetAfterCodeSectionsHeaderFile(group, monsThisGroup, lastKey, currentIndex, lastMon);
                    afterCodeSectionsHeader.ForEach(i => headerFile.WriteLine(i));

                    List<string> afterCodeSectionsInc = GetAfterCodeSectionsIncFile(group, monsThisGroup, lastKey, currentIndex, lastMon);
                    afterCodeSectionsInc.ForEach(i => incFile.WriteLine(i));

                    lastKey = lastMon?.Key ?? lastKey;
                }

                //HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref codeInfo.codeSections, CodeSectionType.END);
            }
        }

        private static HashSet<string> MiscMonFormsToIgnoreSpacing = new HashSet<string>() { "MORPEKO", "ZARUDE", "URSHIFU", "LYCANROC", "MELOETTA", "SHELLOS" };

        public void WriteExtraSpacingLinesPerMon(MonInfo.GroupStartIndexNamePair group, MonInfo mon, MonInfo prevMon, StreamWriter outputFile, bool isIncFile)
        {
            //Special Spacing lines
            if (group.groupKey == MonInfo.MISC_FORM_GROUP_KEY)
            {
                if (prevMon != null)
                {
                    string firstPartOfPrevKey = prevMon.GetFirstPartOfKey();
                    string firstPartOfKey = mon.GetFirstPartOfKey();
                    string lastPartOfPrevKey = prevMon.GetLastPartOfKey();
                    string lastPartOfKey = mon.GetLastPartOfKey();

                    if ((firstPartOfPrevKey != firstPartOfKey || (firstPartOfKey == "PIKACHU" && lastPartOfKey == "CAP") )
                        && (lastPartOfKey != lastPartOfPrevKey || lastPartOfPrevKey == "FLOWER")
                        && !MiscMonFormsToIgnoreSpacing.Contains(firstPartOfKey)) 
                    {
                        outputFile.WriteLine("");
                    }
                }
            }
            else if (group.groupKey == MonInfo.CANONICAL_GROUP_KEY)
            {
                if (isIncFile)
                {
                    if ((prevMon?.Key ?? "") == "SPECIES_ENAMORUS")
                    {
                        outputFile.WriteLine("");
                    }
                }
                else
                {
                    var prevGen = prevMon?.GetPokemonGenerationForCanonicalMon();
                    var curGen = mon.GetPokemonGenerationForCanonicalMon();
                    if (prevGen != null && prevGen != curGen && curGen < MonInfo.PokemonGeneration.GEN_FIVE)
                    {
                        outputFile.WriteLine("");
                    }
                }
            }
            else if (group.groupKey == MonInfo.ALOLAN_GROUP_KEY)
            {
                string lastPartOfPrevKey = prevMon?.GetLastPartOfKey() ?? "";
                string lastPartOfKey = mon.GetLastPartOfKey();
                if (lastPartOfKey == "LARGE" && lastPartOfKey != lastPartOfPrevKey)
                {
                    outputFile.WriteLine("");
                    if (!isIncFile)
                    {
                        outputFile.WriteLine( "//totems");
                    }
                }
            }
            else if (group.groupKey == MonInfo.HISUIAN_GROUP_KEY)
            {
                string lastPartOfPrevKey = prevMon?.GetLastPartOfKey() ?? "";
                bool isPrevNoble = IsNoble(lastPartOfPrevKey);
                string lastPartOfKey = mon.GetLastPartOfKey();
                bool isNoble = IsNoble(lastPartOfKey);
                if (isNoble && isNoble != isPrevNoble)
                {
                    outputFile.WriteLine("");
                    if (!isIncFile)
                    {
                        outputFile.WriteLine("//nobles");
                    }
                }
            }
        }

        private static Dictionary<string, string> SpecialLineEndingsByKey = new Dictionary<string, string>()
        {
            { "SPECIES_VIVILLON", "  // Icy Snow Pattern" },
            { "SPECIES_FLABEBE", "  // Red Flower" },
            { "SPECIES_FLOETTE", "  // Red Flower" },
            { "SPECIES_FLORGES", "  // Red Flower" },
        };

        public bool IsNoble(string lastPartOfKey)
        {
            string fixedKey = lastPartOfKey.Replace("_", "");
            return fixedKey.Contains("LORD") || fixedKey.Contains("LADY");
        }

        public List<string> GetAfterCodeSectionsHeaderFile(MonInfo.GroupStartIndexNamePair group, List<MonInfo> monsThisGroup, string lastKey, int curIndex, MonInfo lastMon)
        {
            if (group.groupKey == MonInfo.CANONICAL_GROUP_KEY)
            {
                return new List<string>() { "", "#define {0} ({1})".FormatStr(MonInfo.FAKEMON_GROUP_KEY, lastMon.Key), "",
                    "// define your fakemons below like this","// #define SPECIES_FAKEMON_NAME1 ({0} + 1)".FormatStr(MonInfo.FAKEMON_GROUP_KEY), "" };
            }
            else if (group.groupKey == MonInfo.FAKEMON_GROUP_KEY)
            {
                return new List<string>() { "#define MUM_OF_FAKEMONS {0}".FormatStr(monsThisGroup.Count.ToString()), ""
                    , "#define MAX_MON_NUM ({0} + MUM_OF_FAKEMONS)".FormatStr(lastKey)
                    , "", "//mega", "#define {0} (MAX_MON_NUM + 1) // {1}".FormatStr(MonInfo.MEGA_GROUP_KEY, curIndex), "" };
            }
            else if (group.groupKey == MonInfo.MEGA_GROUP_KEY)
            {
                return new List<string>() {  "","#define MAX_MEGA_NUM ({0})".FormatStr(lastMon.Key),"",""
                ,"//primals","#define {0} (MAX_MEGA_NUM + 1) // {1}".FormatStr(MonInfo.PRIMAL_GROUP_KEY, curIndex), ""};
            }
            else if (group.groupKey == MonInfo.PRIMAL_GROUP_KEY)
            {
                return new List<string>() { "","#define MAX_PRIMAL_NUM ({0})".FormatStr(lastMon.Key),"","",
                    "//alolans", "#define {0} (MAX_PRIMAL_NUM + 1) // {1}".FormatStr(MonInfo.ALOLAN_GROUP_KEY, curIndex), ""};
            }
            else if (group.groupKey == MonInfo.ALOLAN_GROUP_KEY)
            {
                return new List<string>() {"","","#define MAX_ALOLAN_REGIONAL_NUM ({0})".FormatStr(lastMon.Key),
"","","//galarians","#define {0} (MAX_ALOLAN_REGIONAL_NUM + 1) // {1}".FormatStr(MonInfo.GALARIAN_GROUP_KEY, curIndex),"" };
            }
            else if (group.groupKey == MonInfo.GALARIAN_GROUP_KEY)
            {
                return new List<string>() { "","#define MAX_GALARIAN_REGIONAL_NUM ({0})".FormatStr(lastMon.Key),"","",
                    "//misc forms","#define {0} (MAX_GALARIAN_REGIONAL_NUM + 1) // {1}".FormatStr(MonInfo.MISC_FORM_GROUP_KEY, curIndex),"",};
            }
            else if (group.groupKey == MonInfo.MISC_FORM_GROUP_KEY)
            {
                return new List<string>() {"","#define MAX_MISC_FORM_NUM ({0})".FormatStr(lastMon.Key),"",
                    "//hisuian","#define {0} (MAX_MISC_FORM_NUM + 1) // {1}".FormatStr(MonInfo.HISUIAN_GROUP_KEY, curIndex),""};
            }
            else if (group.groupKey == MonInfo.HISUIAN_GROUP_KEY)
            {
                return new List<string>(){ "","#define MAX_HISUIAN_REGIONAL_NUM ({0})".FormatStr( lastMon.Key),""
                ,"#define {0} (MAX_HISUIAN_REGIONAL_NUM + 1) // {1}".FormatStr(MonInfo.GENDER_DIFF_GROUP_KEY, curIndex), "",};
            }
            else if (group.groupKey == MonInfo.GENDER_DIFF_GROUP_KEY)
            {
                return new List<string>(){"","#define MAX_SPECIES_SIGNIFICANT_GENDER_DIFFERENCE_NUM ({0})".FormatStr(lastMon.Key),""
                    ,"#define {0} (MAX_SPECIES_SIGNIFICANT_GENDER_DIFFERENCE_NUM + 1) // {1}".FormatStr(MonInfo.PALDEAN_GROUP_KEY, curIndex ),"",};
            }
            else if (group.groupKey == MonInfo.PALDEAN_GROUP_KEY)
            {
                return new List<string>(){"","#define MAX_SPECIES_INCLUDING_FORMS ({0})  // {1}".FormatStr(lastMon.Key, curIndex), 
                    "", "","#endif",};
            }
            return new List<string>();
        }

        public List<string> GetAfterCodeSectionsIncFile(MonInfo.GroupStartIndexNamePair group, List<MonInfo> monsThisGroup, string lastKey, int curIndex, MonInfo lastMon)
        {
            if (group.groupKey == MonInfo.CANONICAL_GROUP_KEY)
            {
                return new List<string>() { "", ".equ {0}, ({1})".FormatStr(MonInfo.FAKEMON_GROUP_KEY, lastMon.Key), "",
                    "// define your fakemons below like this","//.equ SPECIES_FAKEMON_NAME1, ({0} + 1)".FormatStr(MonInfo.FAKEMON_GROUP_KEY), "" };
            }
            else if (group.groupKey == MonInfo.FAKEMON_GROUP_KEY)
            {
                return new List<string>() {  ".equ MUM_OF_FAKEMONS, {0}".FormatStr(monsThisGroup.Count), ""
                    , ".equ NUM_OF_MONS, ({0} + MUM_OF_FAKEMONS)".FormatStr(lastKey)
                    , "", "", ".equ {0}, (SPECIES_NONE + NUM_OF_MONS + 1) // {1}".FormatStr(MonInfo.MEGA_GROUP_KEY, curIndex), "" };
            }
            else if (group.groupKey == MonInfo.MEGA_GROUP_KEY)
            {
                return new List<string>() {  "",".equ NUM_OF_MEGAS, ({0})".FormatStr(monsThisGroup.Count),"",""
                ,".equ {0}, ({1} + NUM_OF_MEGAS) // {2}".FormatStr(MonInfo.PRIMAL_GROUP_KEY, MonInfo.MEGA_GROUP_KEY, curIndex), ""};
            }
            else if (group.groupKey == MonInfo.PRIMAL_GROUP_KEY)
            {
                return new List<string>() { "",".equ NUM_OF_PRIMALS, ({0})".FormatStr(monsThisGroup.Count),"",
                    "//alolans", ".equ {0}, ({1} + NUM_OF_PRIMALS) // {2}".FormatStr(MonInfo.ALOLAN_GROUP_KEY, MonInfo.PRIMAL_GROUP_KEY, curIndex), ""};
            }
            else if (group.groupKey == MonInfo.ALOLAN_GROUP_KEY)
            {
                return new List<string>() {"",".equ NUM_OF_ALOLAN_FORMS, ({0})".FormatStr(monsThisGroup.Count),
"","","//galarians",".equ {0}, ({1} + NUM_OF_ALOLAN_FORMS) // {2}".FormatStr(MonInfo.GALARIAN_GROUP_KEY, MonInfo.ALOLAN_GROUP_KEY, curIndex),"" };
            }
            else if (group.groupKey == MonInfo.GALARIAN_GROUP_KEY)
            {
                return new List<string>() { "",".equ NUM_OF_GALARIAN_FORMS, ({0})".FormatStr(monsThisGroup.Count),"","",
                    "//misc forms",".equ {0}, ({1} + NUM_OF_GALARIAN_FORMS) // {2}".FormatStr(MonInfo.MISC_FORM_GROUP_KEY, MonInfo.GALARIAN_GROUP_KEY, curIndex),"",};
            }
            else if (group.groupKey == MonInfo.MISC_FORM_GROUP_KEY)
            {
                return new List<string>() {"",".equ NUM_OF_MISC_FORMS, ({0})".FormatStr(monsThisGroup.Count),"",
                    "","",".equ {0}, ({1} + NUM_OF_MISC_FORMS) // {2}".FormatStr(MonInfo.HISUIAN_GROUP_KEY, MonInfo.MISC_FORM_GROUP_KEY, curIndex),""};
            }
            else if (group.groupKey == MonInfo.HISUIAN_GROUP_KEY)
            {
                return new List<string>(){ "",".equ NUM_OF_HISUIAN_FORMS, ({0})".FormatStr( monsThisGroup.Count),"","",""
                ,".equ {0}, ({1} + NUM_OF_HISUIAN_FORMS) // {2}".FormatStr(MonInfo.GENDER_DIFF_GROUP_KEY, MonInfo.HISUIAN_GROUP_KEY, curIndex), "",};
            }
            else if (group.groupKey == MonInfo.GENDER_DIFF_GROUP_KEY)
            {
                return new List<string>(){"",".equ NUM_OF_SIGNIFICANT_GENDER_DIFFERENCES, ({0})".FormatStr(monsThisGroup.Count),"","",""
                    ,".equ {0}, ({1} + NUM_OF_SIGNIFICANT_GENDER_DIFFERENCES) // {2}".FormatStr(MonInfo.PALDEAN_GROUP_KEY, MonInfo.GENDER_DIFF_GROUP_KEY, curIndex ),"",};
            }
            else if (group.groupKey == MonInfo.PALDEAN_GROUP_KEY)
            {
                return new List<string>(){"",".equ NUM_OF_PALDEAN_FORMS, ({0})".FormatStr(monsThisGroup.Count),"",
                    ".equ NUM_OF_TOTAL_MONS_PLUS_FORMS, ({0} + NUM_OF_PALDEAN_FORMS) // {1}".FormatStr(MonInfo.PALDEAN_GROUP_KEY, curIndex)
                    , "",".endif",};
            }
            return new List<string>();
        }
    }
}
