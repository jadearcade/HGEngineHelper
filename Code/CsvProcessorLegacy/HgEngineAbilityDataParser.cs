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
    public class HgEngineAbilityDataParser
    {

        public static string ItemKeyString = "abilityKey";
        public static string AbilityKeyStart = "ABILITY_";

        

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

        public static string FileNameAbilityHeaders = "include/constants/ability.h";
        public static string FileNameAbilityName = "data/text/720.txt";
        public static string FileNameAbilityDescription = "data/text/722.txt";
        public static List<string> FileNames = new List<string>() { FileNameAbilityHeaders, FileNameAbilityName, FileNameAbilityDescription };

        
        public class AbilityRow
        {
            [CsvConverter(ColumnIndex =1, ColumnName = "Ability Key")]
            public string abilityKey { get; set; }
            [CsvConverter(ColumnIndex = 4, ColumnName = "Ability ID")]
            public int abilityId { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Ability Name")]
            public string abilityName { get; set; }
            [CsvConverter(ColumnIndex = 3, ColumnName = "Ability Decription")]
            public string abilityDescription { get; set; }
        }

        internal BoolResultWithMessage OutputAbilityDataToCsv(string basePath, string outputPath)
        {
            var fileNameToPathDict = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath, FileNames);
            var missingFileNames = FileNames.Where(i => !fileNameToPathDict.ContainsKey(i)).ToList();
            if (missingFileNames.Count > 0)
            {
                return new BoolResultWithMessage(false, "Missing " + String.Join("; ", missingFileNames));
            }
            Dictionary<string,int> itemHeaderDict = HgEngineCsvConverterHelperFunctions.ReadItemHeaders(fileNameToPathDict[FileNameAbilityHeaders], AbilityKeyStart);
            Dictionary<int, string> itemNameDict = ReadTextArchive(fileNameToPathDict[FileNameAbilityName]);
            Dictionary<int, string> itemDescDict = ReadTextArchive(fileNameToPathDict[FileNameAbilityDescription]);
            List<AbilityRow> abilityRows = new List<AbilityRow>();
            foreach(var kv in itemHeaderDict)
            {
                abilityRows.Add(new AbilityRow()
                {
                    abilityKey = kv.Key,
                    abilityId = kv.Value,
                    abilityName = itemNameDict.GetValueOrDefault(kv.Value, ""),
                    abilityDescription = itemDescDict.GetValueOrDefault(kv.Value, ""),
                });
            }

            using (var fs = File.Create(outputPath))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<AbilityRow>(sw);

                foreach (var monDataRow in abilityRows)
                {
                    service.WriteRecord(monDataRow);
                }
            }
            return new BoolResultWithMessage(true, "") ;
        }
    }
}
