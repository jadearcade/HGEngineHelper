using CsvConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HgEngineCsvConverter.Code.HgEngineMonDataCsvExporter;

namespace HgEngineCsvConverter.Code
{
    public class HgEngineLearnsetDataParser
    {
        public class LevelUpMoveData
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "Pokemon Key")]
            public string speciesKey { get; set; } = "";
            [CsvConverter(ColumnIndex = 3, ColumnName = "Move Key")]
            public string moveKey { get; set; } = "";
            [CsvConverter(ColumnIndex = 2, ColumnName = "Level")]
            public int level { get; set; } = 0;
        }

        public class TmMoveData
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "TM Key")]
            public string tmKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Species Key")]
            public string speciesKey { get; set; }
        }

        public class EggMoveData
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "Tutor Key")]
            public string moveKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Dummy")]
            public string dummy { get; set; } = "Egg";
            [CsvConverter(ColumnIndex = 3, ColumnName = "Move Key")]
            public string speciesKey { get; set; }
        }

        public class TutorMoveLearnsetRow
        {
            [CsvConverter(ColumnIndex = 4, ColumnName = "Tutor Key")]
            public string tutorKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Dummy")]
            public string dummy { get; set; } = "Tutor";
            [CsvConverter(ColumnIndex = 1, ColumnName = "Pokemon Key")]
            public string speciesKey { get; set; }
            [CsvConverter(ColumnIndex = 3, ColumnName = "Move Key")]
            public string moveKey { get; set; }
        }

        public class TutorMoveOption
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "Tutor Key")]
            public string tutorKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Move Key")]
            public string moveKey { get; set; }
            [CsvConverter(ColumnIndex = 3, ColumnName = "BP Cost")]
            public int bpCost { get; set; }
            public List<string> speciesKeys = new List<string>();
        }

        public static string SpeciesKeyStart = "SPECIES_";
        public static string ItemKeyStart = "ITEM_";
        public static string MoveKeyStart = "MOVE_";
        public static string TypeKeyStart = "TYPE_";
        public static string LevelUpMovesFileName = "levelupdata.s";
        public static string TutorDataFileName = "tutordata.txt";
        public static string TmLearnsetFileName = "tmlearnset.txt";
        public static string EggMoveFileName = "eggmoves.s";
        public static List<string> FileNames = new List<string>()
        {
            LevelUpMovesFileName, TutorDataFileName, TmLearnsetFileName, EggMoveFileName,
        };

        private List<string> GetTmCsvLines(Dictionary<string, HashSet<string>> tmsAvailableBySpecies, List<string> tms)
        {
            List<string> result = new List<string>();
            result.Add("Species Key," + String.Join(",", tms));
            foreach(var kv in tmsAvailableBySpecies.OrderBy(i => i.Key))
            {
                List<string> linePieces = new List<string>();
                linePieces.Add(kv.Key);
                foreach(var tm in tms)
                {
                    linePieces.Add(kv.Value.Contains(tm) ? "TRUE" : "FALSE");
                }
                result.Add(String.Join(",", linePieces));
            }
            return result;
        }

        public BoolResultWithMessage CreateLearnsetRelatedCsvs(string basePath, string outputPathLearnsets, string outputPathTms, string outputPathTutorOptions)
        {
            var fileNameToPathDict = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath, FileNames);
            List<string> missingFileNames = new List<string>();
            foreach (var fileName in FileNames)
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

            List<LevelUpMoveData> levelUpMoves = ReadLevelUpData(fileNameToPathDict[LevelUpMovesFileName]);
            Dictionary<string, HashSet<string>> tmMoveLearnset = ReadTmLearnset(fileNameToPathDict[TmLearnsetFileName], out Dictionary<string, string> tmKeyToMoveKeyDict);
            List<string> tmlearnsetCsvLines = GetTmCsvLines(tmMoveLearnset, tmKeyToMoveKeyDict.Select(i => i.Key).OrderBy(i => i).ToList());
            List<EggMoveData> eggMoves = ReadEggMoves(fileNameToPathDict[EggMoveFileName]);
            List<TutorMoveOption> tutorMoves = ReadTutorData(fileNameToPathDict[TutorDataFileName]);
            List<TutorMoveLearnsetRow> tutorMoveLearnsetRows = new List<TutorMoveLearnsetRow>();
            foreach(var tutorMove in tutorMoves)
            {
                foreach(var speciesKey in tutorMove.speciesKeys)
                {
                    tutorMoveLearnsetRows.Add(new TutorMoveLearnsetRow()
                    {
                        speciesKey = speciesKey,
                        moveKey = tutorMove.moveKey,
                        tutorKey = tutorMove.tutorKey,
                    });
                }
            }

            using (var fs = File.Create(outputPathLearnsets + "egg_ls.csv"))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<EggMoveData>(sw);

                foreach (var row in eggMoves)
                {
                    service.WriteRecord(row);
                }
            }
            using (var fs = File.Create(outputPathLearnsets + "tm_ls.csv"))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                foreach (var line in tmlearnsetCsvLines)
                {
                    sw.WriteLine(line);
                }
            }
            using (var fs = File.Create(outputPathLearnsets + "tutor_ls.csv"))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<TutorMoveLearnsetRow>(sw);

                foreach (var row in tutorMoveLearnsetRows)
                {
                    service.WriteRecord(row);
                }
            }
            using (var fs = File.Create(outputPathLearnsets + "levelup_ls.csv"))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<LevelUpMoveData>(sw);

                foreach (var row in levelUpMoves)
                {
                    service.WriteRecord(row);
                }
            }
            using (var fs = File.Create(outputPathTms))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<TmRow>(sw);

                foreach (var tmRow in tmKeyToMoveKeyDict.Select(i => new TmRow() { moveKey = i.Value, tmKey = i.Key }))
                {
                    service.WriteRecord(tmRow);
                }
            }
            using (var fs = File.Create(outputPathTutorOptions))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<TutorMoveOption>(sw);

                foreach (var tutorMove in tutorMoves)
                {
                    service.WriteRecord(tutorMove);
                }
            }

            return new BoolResultWithMessage(true, "Successful");
        }
        
        public class TmRow
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "TM Key")]
            public string tmKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Move Key")]
            public string moveKey { get; set; }
        }

        //public class GeneralMoveData
        //{
        //    [CsvConverter(ColumnIndex = 1, ColumnName = "Pokemon Key")]
        //    public string speciesKey { get; set; } = "";
        //    [CsvConverter(ColumnIndex = 2, ColumnName = "Learn Type")]
        //    public string moveLearnTypeKey { get; set; } = "";
        //    [CsvConverter(ColumnIndex = 3, ColumnName = "Move Key")]
        //    public string moveKey { get; set; } = "";
        //    [CsvConverter(ColumnIndex = 4, ColumnName = "Level")]
        //    public int level { get; set; } = 0;
        //    [CsvConverter(ColumnIndex = 5, ColumnName = "TM Key")]
        //    public string tmKey { get; set; } = "";
        //    [CsvConverter(ColumnIndex = 6, ColumnName = "Tutor Key")]
        //    public string tutorKey { get; set; } = "";
        //}

        public List<LevelUpMoveData> ReadLevelUpData(string filePath)
        {
            List<LevelUpMoveData> result = new List<LevelUpMoveData>();
            String line;
                StreamReader sr = new StreamReader(filePath);
                line = sr.ReadLine();
                string currentSpecies = "";
                while (line != null)
                {
                    if (line.Contains("levelup"))
                    {
                        currentSpecies = line.Split(" ")[1];
                    }
                    else if (line.Contains("terminatelearnset"))
                    {
                        currentSpecies = "";
                    }
                    else if (line.Contains("learnset") && currentSpecies != "")
                    {
                        List<string> pieces = line.Replace("learnset ", "").Replace(" ", "").Split(",").ToList();
                        result.Add(new LevelUpMoveData()
                        {
                            level = pieces[1].ToInt(),
                            moveKey = pieces[0],
                            speciesKey = currentSpecies
                        });
                    }
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
            return result;
        }

        public Dictionary<string, HashSet<string>> ReadTmLearnset(string filePath, out Dictionary<string, string> tmKeyToMoveKeyDict)
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
            tmKeyToMoveKeyDict = new Dictionary<string, string>();
            String line;
            string currentTmKey = "";
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            while (line != null)
            {
                if (line.Contains(':'))
                {
                    List<string> pieces = line.Replace(":", "").Split(" ").ToList();
                    currentTmKey = pieces[0];
                    tmKeyToMoveKeyDict[currentTmKey] = pieces[1];
                }
                else if (line.Contains("SPECIES_") && currentTmKey != "")
                {
                    var speciesKey = line.Replace(" ", "");
                    if (!result.ContainsKey(speciesKey))
                    {
                        result[speciesKey] = new HashSet<string>();
                    }
                    result[speciesKey].Add(currentTmKey);
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();


            return result;
        }

        public List<EggMoveData> ReadEggMoves(string filePath)
        {
            List<EggMoveData> result = new List<EggMoveData>();
            String line;
            string currentSpecies = "";
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            while (line != null)
            {
                if (line.Contains("eggmoveentry"))
                {
                    currentSpecies = line.Split(" ")[1];
                }else if (line.Contains("terminateeggmove"))
                {
                    currentSpecies = "";
                }
                else if (line.Contains("eggmove") && currentSpecies != "")
                {
                    var moveKey = line.Replace("    ", "").Split(" ")[1];
                    result.Add(new EggMoveData()
                    {
                        moveKey = moveKey,
                        speciesKey = currentSpecies
                    });
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }

        public List<TutorMoveOption> ReadTutorData(string filePath)
        {
            List<TutorMoveOption> result = new List<TutorMoveOption>();
            String line;
            TutorMoveOption currentMoveOption = null;
                StreamReader sr = new StreamReader(filePath);
                line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Contains("TUTOR_"))
                    {
                        if (currentMoveOption != null)
                        {
                            result.Add(currentMoveOption);
                        }
                        var pieces = line.Replace(":", "").Split(" ");
                        currentMoveOption = new TutorMoveOption()
                        {
                            moveKey = pieces[1],
                            tutorKey = pieces[0],
                            bpCost = pieces[2].ToInt(),
                            speciesKeys = new List<string>()
                        };
                    }
                    else if (line.Contains("SPECIES_") && currentMoveOption != null)
                    {
                        currentMoveOption.speciesKeys.Add(line.Replace(" ", ""));
                    }
                    line = sr.ReadLine();
                }
                if (currentMoveOption != null)
                {
                    result.Add(currentMoveOption);
                }
                //close the file
                sr.Close();
            return result;
        }
    }
}
