using CsvConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HgEngineCsvConverter.Code.HgEngineLearnsetDataParser;

namespace HgEngineCsvConverter.Code
{
    public static class HgEngineCsvConverterHelperFunctions
    {
        public static List<string> KeysToRemoveForDescColumn = new List<string>() { "MOVE_", "SPECIES_", "ITEM_" };
        public static string FixDescriptionColumn(List<string> inputList)
        {
            if (inputList.Count == 0)
            {
                return "";
            }
            foreach(var key in KeysToRemoveForDescColumn)
            {
                if (inputList[0].Contains(key))
                {
                    inputList.RemoveAt(0);
                    break;
                }
            }
            var input = String.Join(", ", inputList);
            if (input.Length == 0)
            {
                return input; 
            }
            if (input[0] == '"')
            {
                input = input.Substring(1);
            }
            if (input.Length > 0 && input[input.Length - 1] == '"')
            {
                input = input.Remove(input.Length - 1, 1);
            }
            return input;
        }

        public static Dictionary<string, string> ToDictBasedOnFirst (this List<List<string>> list)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach(var item in list) 
            {
                result[item[0]] = item[1];
            }
            return result;
        }

        public static Dictionary<string, string> GetFileNameToPathDictionary(string basePath, List<string> fileNamesToSearch)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string fileName in fileNamesToSearch)
            {
                var files = Directory.GetFiles(basePath, fileName, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    result[fileName] = files[0];
                }
            }
            return result;
        }

        public static Dictionary<string, int> ReadItemHeaders(string filePath, string keyStart)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            String line = "";
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            while (line != null)
            {
                if (!line.Contains("#define " + keyStart) || line.StartsWith("//"))
                {
                    line = sr.ReadLine();
                    continue;
                }
                List<string> pieces = line.Replace(" + ", ",").Replace("(", ",").Replace(")", "")
                        .Replace("#define ", "").Replace(" ", ",").Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
                if (pieces.Count < 2)
                {
                    line = sr.ReadLine();
                    continue;
                }
                string itemKey = pieces[0].Replace(" ", "");
                if (pieces[1].Contains(keyStart))//is addition
                {
                    int indexToAddTo = result.GetValueOrDefault(pieces[1], 0);
                    int amtToAdd = pieces.Count == 2 ? 0 : pieces[2].ToInt();
                    if (indexToAddTo == 0)
                    {
                        throw new Exception("Error parsing items");
                    }
                    result[pieces[0]] = indexToAddTo + amtToAdd;
                }
                else
                {
                    result[pieces[0]] = pieces[1].ToInt();
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }
    }
}
