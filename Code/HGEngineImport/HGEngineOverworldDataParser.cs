using CsvHelper;
using HgEngineCsvConverter.Code;
using HGEngineHelper.Code.CsvProcessing;
using HGEngineHelper.Code.HGECodeHelper;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HGEngineHelper.Code.HGEngineImport.HGEngineCodeParser;

namespace HGEngineHelper.Code.HGEngineImport
{
    public class HGEngineOverworldDataParser
    {
        public class OverworldAndIconData
        {
            public Dictionary<string, string> speciesToOwGfxDict = new Dictionary<string, string>();//SPECIES -> OWGfx start
            public List<ByteTableEntry> iconPaletteTableEntries = new List<ByteTableEntry>();//One for each species
            public List<HgEngineOverworldTableEntry> followerEntries = new List<HgEngineOverworldTableEntry>();
            public List<ByteTableEntry> gDimorphismEntries = new List<ByteTableEntry>();//Mon Index Order
            public List<ByteTableEntry> numOwFormsPerMonEntries = new List<ByteTableEntry>();//Mon Index Order
            public List<HgeMonOwDataEntry> monOwDataEntries = new List<HgeMonOwDataEntry>();//GFX Tag Order
        }

        public void ReadAllOverworldData(string basePath)
        {
            var parser = new HGEngineCodeParser();
            var speciesToOwgfxReadResult = parser.ReadSpeciesDictionary(basePath + HgEnginePaths.SpeciesToOwGfxTableFileName);
            var iconPaletteTableReadResult = parser.ReadByteTablesFile(basePath + HgEnginePaths.IconPaletteTableFileName, new List<string>() {"gIconPalTable"} );
            var readOwTableResult = ReadOverworldTableFile(basePath + HgEnginePaths.OverworldTableFileName);
            var readMonOwFileResult = ReadMonOverworldsFile(basePath + HgEnginePaths.MonOverworldsFileName);
            var x = 1;
            var owAndIconData = new OverworldAndIconData()
            {
                speciesToOwGfxDict = speciesToOwgfxReadResult.dictionaryValues,
                followerEntries = readOwTableResult.followerEntries,
                gDimorphismEntries = readMonOwFileResult.byteTables.FirstOrDefault(i => i.tableName == GDimorphismTableVarName)?.entries ?? new List<ByteTableEntry>(),
                iconPaletteTableEntries = iconPaletteTableReadResult.byteTables.FirstOrDefault()?.entries ?? new List<ByteTableEntry>(),
                monOwDataEntries = readMonOwFileResult.owDataTable.dataEntries,
                numOwFormsPerMonEntries = readMonOwFileResult.byteTables.First(i => i.tableName == NumOfOwFormsPerMonVarName)?.entries ?? new List<ByteTableEntry>(),
            };

            string json = JsonConvert.SerializeObject(owAndIconData, Formatting.Indented);

            //write string to file
            System.IO.File.WriteAllText(App.ProjectInfo.dataFolder + CsvFileNames.OverworldJsonInfo, json);

            var csvNumFormsPerMon = owAndIconData.numOwFormsPerMonEntries.Select(i => new CsvNumFormsPerMon()
            {
                amount = i.value.ToInt(),
                key = i.key,
            }).ToList();

            using (var writer = new StreamWriter(App.ProjectInfo.dataFolder + CsvFileNames.OverworldAnalysisInfo))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(csvNumFormsPerMon);
            }
        }

        public class CsvNumFormsPerMon
        {
            public string key { get; set; }
            public int amount { get; set; }
        }

        private HgeMonOwFileReadResult ReadMonOverworldsFile(string path)
        {
            var parser = new HGEngineCodeParser();
            var byteTableFileRead = parser.ReadByteTablesFile(path, MonOverworldsVarNames);
            var owDataTable = ReadMonDataEntriesFromCodeSection(byteTableFileRead.lastCodeSection.lines);

            return new HgeMonOwFileReadResult()
            {
                byteTables = byteTableFileRead.byteTables,
                owDataTable = owDataTable,
            };
        }

        public class HgEngineOverworldTableEntry
        {
            public int tag;
            public int gfx;
            public string owType;
            public string comment;
        }

        public class HgEngineOverworldTableReadResult
        {
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public List<HgEngineOverworldTableEntry> followerEntries = new List<HgEngineOverworldTableEntry>();
        }

        public static string GDimorphismTableVarName = "gDimorphismTable";
        public static string NumOfOwFormsPerMonVarName = "NumOfOWFormsPerMon";

        public static List<string> MonOverworldsVarNames = new List<string>()
        { GDimorphismTableVarName, NumOfOwFormsPerMonVarName };

        public HgEngineOverworldTableReadResult ReadOverworldTableFile(string path)
        {
            HgEngineOverworldTableReadResult result = new HgEngineOverworldTableReadResult();
            StreamReader sr = new StreamReader(path);
            CodeSection currentCodeSection = new CodeSection();
            CodeSectionType curCodeType = CodeSectionType.BEGINNING;
            bool readingCode = true;
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.END, currentCodeSection);
                    }
                    break;
                }
                if (readingCode && line.TrimStart().StartsWith("// pokémon follower specific overworlds"))
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        currentCodeSection.lines.Add(line);
                        currentCodeSection.lines.Add("");
                        result.codeInfo.codeSections.AddValueToCorrespondingList(curCodeType, currentCodeSection);
                        currentCodeSection = new CodeSection();
                        if (curCodeType == CodeSectionType.BEGINNING) { curCodeType = CodeSectionType.MIDDLE; }
                    }
                    readingCode = false;
                }else if (!readingCode && line.TrimStart().StartsWith("{ 0xFFFF"))
                {
                    readingCode = true;
                    currentCodeSection.lines.Add(line);
                }else if (!readingCode && line.Contains("{ .tag"))
                {
                    var fixedLine = line.Replace(" = ", "=").Replace("}", "").Replace("{", "").Trim();
                    List<string> pieces = fixedLine.Split(",").ToList();
                    if (pieces.Count < 3)
                    {
                        continue;
                    }
                    result.followerEntries.Add(new HgEngineOverworldTableEntry()
                    {
                        tag = pieces[0].Trim().Replace(".tag=", "").Trim().ToInt(),
                        gfx = pieces[1].Trim().Replace(".gfx=", "").Trim().ToInt(),
                        owType = pieces[2].Trim().Replace(".callback_params=", "").Trim(),
                        comment = pieces.Count > 3 ? pieces[3] : "",
                    });
                }
                else
                {
                    currentCodeSection.lines.Add(line);
                }
            }
            //close the file
            sr.Close();
            return result;
        }

        public class HgeMonOwFileReadResult
        {
            public List<ReadByteTableResult> byteTables;
            public HgeMonDataEntriesReadResult owDataTable;
        }

        public class HgeMonDataEntriesReadResult
        {
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public List<HgeMonOwDataEntry> dataEntries = new List<HgeMonOwDataEntry>();
        }

        public class HgeMonOwDataEntry
        {
            public int index;
            public string buildingEntryType;
            public string bounceType;
            public string comments;
        }

        public HgeMonDataEntriesReadResult ReadMonDataEntriesFromCodeSection(List<string> lines)
        {
            HgeMonDataEntriesReadResult result = new HgeMonDataEntriesReadResult();
            CodeSection codeSection = new CodeSection();
            CodeSectionType codeSectionType = CodeSectionType.BEGINNING;
            foreach (var line in lines)
            {
                if (line == null)
                {
                    if (codeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.END, codeSection);
                    }
                }
                if (line.TrimStart().StartsWith("overworlddata"))
                {
                    if (codeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(codeSectionType, codeSection);
                        if (codeSectionType == CodeSectionType.BEGINNING) { codeSectionType = CodeSectionType.MIDDLE; }
                        codeSection = new CodeSection();
                    }

                    var pieces = line.Replace("overworlddata ", "").Replace("//", ",").Split(", ")
                        .Select(i => i.Trim()).ToList();

                    if (pieces.Count < 3)
                    {
                        continue;
                    }
                    result.dataEntries.Add(new HgeMonOwDataEntry()
                    {
                        index = pieces[0].ToInt(),
                        bounceType = pieces[2].ToString(),
                        buildingEntryType = pieces[1].ToString(),
                        comments = pieces.GetAtIndexOrDefault(3, "")
                    });
                }
                else
                {
                    codeSection.lines.Add(line);
                }
            }
            return result;
        }
    }
}
