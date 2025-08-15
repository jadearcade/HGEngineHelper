using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace HgEngineCsvConverter
{
    public class HgEngineObject
    {
        public string headerClassName => headerInfo.Count > 0 ? headerInfo[0] : "";
        public List<string> headerInfo = new List<string>();
        public string name => headerInfo.Count > 1 ? headerInfo[1] : "";
        public Dictionary<string, List<string>> attributeInfoByLine = new Dictionary<string, List<string>>();
    }

    public class HgEngineDataEntry
    {
        public List<string> attributes = new List<string>();
    }

    public class HgEngineDataParser
    {

        public List<List<string>> ReadDataEntries(string filePath, string classAttributeName)
        {
            List<List<string>> result = new List<List<string>>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            HgEngineObject newHgEngineObject = null;
            int lineNum = 0;
            while (line != null)
            {
                line = line.Replace(", ", ",").Replace(" ", ",");
                List<string> valueList = line.Split(",").ToList();
                if (valueList.Count <= 1 || valueList[0] != classAttributeName)
                {
                    line = sr.ReadLine();
                    continue;
                }

                valueList.RemoveAt(0);
                result.Add(valueList);

                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }

        public Dictionary<string, string> ReadSpeciesDictionary(string filePath)
                => ReadConstantDictionary(filePath, "[SPECIES_");

        public Dictionary<string, string> ReadConstantDictionary(string filePath, string beginning)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            String line;
                StreamReader sr = new StreamReader(filePath);
                line = sr.ReadLine();
                int lineNum = 0;
                while (line != null)
                {
                    if (line.IndexOf(beginning) == -1)
                    {
                        line = sr.ReadLine();
                        continue;
                    }
                    List<string> tempLines = line.Replace(" ", "").Replace(",", "").Replace("[", "").Replace("]", "").Replace("=", ",").Split(",").ToList(); ;
                    if (tempLines.Count != 2)
                    {
                        line = sr.ReadLine();
                        continue;
                    }
                    result[tempLines[0]] = tempLines[1];

                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
            return result;
        }

        public List<HgEngineObject> ReadEngineObjects(string filePath, string classAttributeName)
        {
            List<HgEngineObject> result = new List<HgEngineObject>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            HgEngineObject newHgEngineObject = null;
            while (line != null)
            {
                if (line == "" || (line.IndexOf(classAttributeName) == -1 && newHgEngineObject == null))
                {
                    if (newHgEngineObject != null){
                        result.Add(newHgEngineObject);
                        newHgEngineObject = null;
                    }
                    line = sr.ReadLine();
                    continue;
                }
                line = line.Replace("    ", "");
                int attrNameSeperatorIdx = line.IndexOf(" ");
                if (attrNameSeperatorIdx == -1)
                {
                    line = sr.ReadLine();
                    continue;
                }
                string attributeName = line.Substring(0, attrNameSeperatorIdx).Replace(" ", "");
                List<string> valueList = line.Substring(attrNameSeperatorIdx + 1).Split(", ").ToList();
                if (newHgEngineObject == null)
                {
                    if (attributeName == classAttributeName)
                    {
                        newHgEngineObject = new HgEngineObject()
                        {
                            headerInfo = valueList,
                        };
                    }
                }else{
                    newHgEngineObject.attributeInfoByLine[attributeName] = valueList;
                }
                line = sr.ReadLine();
            }
            if (newHgEngineObject != null)
            {
                result.Add(newHgEngineObject);
            }
            //close the file
            sr.Close();
            return result;
        }
    }
}