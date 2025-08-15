using HgEngineCsvConverter;
using HGEngineHelper.Code.HGECodeHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Xml.XPath;
using static HgEngineCsvConverter.Code.HgEngineFormDataParser;
using static HGEngineHelper.Code.HGEngineImport.HGEngineCodeParser;

namespace HGEngineHelper.Code.HGEngineImport
{
    public class HGEngineCodeParser
    {
        public class HgeModelWithHeaderFileInfo
        {
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public List<HgEngineObject> hgeObjects = new List<HgEngineObject>();
        }

        private enum ReadMode
        {
            READ_CODE = 1,
            READ_DATA = 2,
        }

        private string GetFirstAttributeNameInLine(string line)
        {
            line = line.TrimStart();
            int attrNameSeperatorIdx = line.IndexOf(" ");
            if (attrNameSeperatorIdx == -1)
            {
                return "";
            }
            else
            {
                return line.Substring(0, attrNameSeperatorIdx).Replace(" ", "") ;
            }
        }

        /// <summary>
        /// Trims a character from the start and end of a line if it exists
        /// </summary>
        private string TrimChar(string line, char characterToRemove)
        {
            if (line.Length == 0)
            {
                return line;
            }
            if (line[0] == characterToRemove)
            {
                return line.Substring(1);
            }
            if (line.Length != 0 && line[line.Length - 1] == characterToRemove)
            {
                return line.Substring(0, line.Length - 1);
            }
            return line;
        }

        public static string StripTrailingComments(string line)
        {
            return StripTrailingComments(line, out _);
        }

        public static string StripTrailingComments(string line, out bool hasBaseExp)
        {
            int commentsIdx = line.IndexOf("//");
            hasBaseExp = line.Contains("baseexp.s");
            if (commentsIdx == -1)
            {
                return line;
            }
            return line.Substring(0, commentsIdx);
        }

        /// <summary>
        /// Splits the line into its pieces while handling strings
        /// </summary>
        private List<string> StringValueSafeSplit(string line)
        {
            List<string> result = new List<string>();
            if (line == null)
            {
                return result;
            }
            while (true)
            {
                line = line.TrimStart();
                if (line == "")
                {
                    return result;
                }
                int endIndex = -1;
                if (line[0] == '"')//Quote line
                {
                    endIndex = line.IndexOf("\"", 1) + 1;
                    if (endIndex >= line.Length)
                    {
                        endIndex = line.Length - 1;
                    }
                }
                else//Basic value
                {
                    endIndex = line.IndexOf(' ');
                }

                if (endIndex == -1)
                {
                    result.Add(line.TrimEnd());
                    return result;
                }
                else
                {
                    string newVal = TrimChar(TrimChar(line.Substring(0, endIndex), ','), '"');
                    result.Add(newVal);
                    line = line.Substring(endIndex + 1);
                }
            }
            return result;
        }

        public HgeModelWithHeaderFileInfo ReadEngineObjects(string filePath, HashSet<string> classAttributeNames)
        {
            HgeModelWithHeaderFileInfo result = new HgeModelWithHeaderFileInfo();
            StreamReader sr = new StreamReader(filePath);
            HgEngineObject newHgeObj = new HgEngineObject();
            CodeSection currentCodeSection = new CodeSection() ;
            CodeSectionType curCodeType = CodeSectionType.BEGINNING;
            ReadMode readMode = ReadMode.READ_CODE;
            List<string> distinctAttrs = new List<string>();
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)//End of File
                {
                    if (readMode == ReadMode.READ_CODE && currentCodeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.END, currentCodeSection);
                    }
                    else if (readMode == ReadMode.READ_DATA && newHgeObj != null)
                    {
                        result.hgeObjects.Add(newHgeObj);
                    }
                    break;
                }

                List<string> lineSplit = StringValueSafeSplit(StripTrailingComments(line, out bool hasBaseExp));
                if (hasBaseExp && newHgeObj != null)
                {
                    newHgeObj.attributeInfoByLine["hasBaseExpComment"] = new List<string>() ;
                }
                if (lineSplit.Count > 0 && classAttributeNames.Contains(lineSplit[0]) && newHgeObj.headerInfo.Count == 0)
                {
                    //New model found, switch to Reading Data
                    readMode = ReadMode.READ_DATA;
                    if (currentCodeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(curCodeType, currentCodeSection);
                        currentCodeSection = new CodeSection();
                        if (curCodeType == CodeSectionType.BEGINNING){curCodeType = CodeSectionType.MIDDLE;}
                    }
                    newHgeObj.headerInfo = lineSplit;
                    continue;
                }
                if (readMode == ReadMode.READ_DATA && newHgeObj != null)
                {
                    if (lineSplit.Contains("terminatedata") || lineSplit.Count < 2)
                    {
                        readMode = ReadMode.READ_CODE;
                        if (newHgeObj != null)
                        {
                            result.hgeObjects.Add(newHgeObj);
                            newHgeObj = new HgEngineObject();
                        }
                    }
                    else
                    {
                        string attr = lineSplit[0];
                        if (!distinctAttrs.Contains(attr))
                        {
                            distinctAttrs.Add(attr);
                        }
                        lineSplit.RemoveAt(0);
                        newHgeObj.attributeInfoByLine[attr] = lineSplit;
                    }
                }
                if (readMode == ReadMode.READ_CODE)
                {
                    currentCodeSection.lines.Add(line);
                }
            }
            var xxx = String.Join(",", distinctAttrs);
            //close the file
            sr.Close();
            return result;
        }

        public class HgeDataEntryFileReadResult
        {
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public Dictionary<string, List<string>> dataEntries = new Dictionary<string, List<string>>();
        }

        public HgeDataEntryFileReadResult ReadDataEntries(string filePath, string classAttributeName)
        {
            HgeDataEntryFileReadResult result = new HgeDataEntryFileReadResult();
            StreamReader sr = new StreamReader(filePath);
            CodeSection currentCodeSection = new CodeSection();
            CodeSectionType curCodeType = CodeSectionType.BEGINNING;
            List<string> distinctAttributes = new List<string>();
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)//End of File
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.END, currentCodeSection);
                    }
                    break;
                }
                List<string> safeSplit = StringValueSafeSplit(StripTrailingComments(line));
                if ( safeSplit.Count > 0 && safeSplit[0].Contains(classAttributeName))//Data Entry
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(curCodeType, currentCodeSection);
                        currentCodeSection = new CodeSection();
                        if (curCodeType == CodeSectionType.BEGINNING) { curCodeType = CodeSectionType.MIDDLE; }
                    }
                    safeSplit.RemoveAt(0);
                    string attr = safeSplit.FirstOrDefault("");
                    safeSplit.RemoveAt(0);
                    result.dataEntries[attr] = safeSplit;
                }
                else
                {
                    currentCodeSection.lines.Add(line);
                }
            }
            sr.Close();
            return result;
        }

        public class HgeSimpleDictionaryFileReadResult()
        {
            public Dictionary<string, string> dictionaryValues = new Dictionary<string, string>();
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
        }

        public HgeSimpleDictionaryFileReadResult ReadSpeciesDictionary(string filePath)
                => ReadConstantDictionary(filePath, "SPECIES_");

        private Tuple<string, string> ReadLineAsKeyValuePair(string line)
        {
            line = line.Trim();
            List<string> pieces = line.Split('=').ToList();
            string key = pieces.Count == 0 ? "" : TrimChar(TrimChar(pieces[0].Replace(" ", ""), '['), ']');
            string value = pieces.Count < 2 ? "" : TrimChar(pieces[1].Replace(" ", ""), ',');
            return new Tuple<string, string>(key, value);
        }

        public HgeSimpleDictionaryFileReadResult ReadConstantDictionary(string filePath, string beginningOfKey)
        {
            HgeSimpleDictionaryFileReadResult result = new HgeSimpleDictionaryFileReadResult();
            StreamReader sr = new StreamReader(filePath);
            CodeSection currentCodeSection = new CodeSection();
            CodeSectionType curCodeType = CodeSectionType.BEGINNING;
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
                if (line.TrimStart().StartsWith("[" +beginningOfKey))
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(curCodeType, currentCodeSection);
                        currentCodeSection = new CodeSection();
                        if (curCodeType == CodeSectionType.BEGINNING) { curCodeType = CodeSectionType.MIDDLE; }
                    }
                    var kvPair = ReadLineAsKeyValuePair(line);
                    result.dictionaryValues[kvPair.Item1] = kvPair.Item2;
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

        public class HgeConstantsHeaderFileReadResult
        {
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public List<HgEngineConstantHeaderIndex> constants = new List<HgEngineConstantHeaderIndex>();
        }

        private Tuple<string, string> SplitIntoAssignValuePair(string line)
        {
            var lineTrimmed = StripTrailingComments(line).Replace("#define ", "").Trim();
            List<string> result = new List<string>();
            var indexOfWhitespace = lineTrimmed.IndexOf(' ');
            if (indexOfWhitespace == -1)
            {

                return new Tuple<string, string>(lineTrimmed, "");
            }
            var assign = lineTrimmed.Substring(0, indexOfWhitespace);
            var value = lineTrimmed.Substring(indexOfWhitespace).Trim();
            return new Tuple<string, string>(assign, value);
        }

        private static List<string> BaseOperators = new List<string>() { "+", "-" };

        private AddSubtractPair ProcessAddSubtractPair(string piece)
        {
            foreach(var baseOperator in BaseOperators)
            {
                if (piece.Contains(baseOperator))
                {
                    var pair = TrimChar(TrimChar(piece, ')'), '(').Split(baseOperator);
                    return new AddSubtractPair()
                    {
                        pairOperator = baseOperator,
                        part1 = pair[0],
                        part2 = pair[1],
                    };
                }
            }
            return new AddSubtractPair()
            {
                part1 = piece,
                pairOperator = "",
                part2 = "",
            };
        }

        public HgeConstantsHeaderFileReadResult ReadConstantsHeaderFile(string filePath, string beginningOfKey)
        {
            HgeConstantsHeaderFileReadResult result = new HgeConstantsHeaderFileReadResult();
            StreamReader sr = new StreamReader(filePath);
            CodeSection currentCodeSection = new CodeSection();
            CodeSectionType curCodeType = CodeSectionType.BEGINNING;
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
                if (line.TrimStart().StartsWith("#define " + beginningOfKey))
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        if (currentCodeSection.lines.Any(i => i != "") || curCodeType == CodeSectionType.BEGINNING)
                        {
                            result.codeInfo.codeSections.AddValueToCorrespondingList(curCodeType, currentCodeSection);
                        }
                        currentCodeSection = new CodeSection();
                        if (curCodeType == CodeSectionType.BEGINNING) { curCodeType = CodeSectionType.MIDDLE; }
                    }

                    var assignValuePair = SplitIntoAssignValuePair(line);
                    var keyAddPair = ProcessAddSubtractPair(assignValuePair.Item1);
                    var valueAddPair = ProcessAddSubtractPair(assignValuePair.Item2);
                    result.constants.Add(new HgEngineConstantHeaderIndex()
                    {
                        key = keyAddPair,
                        value = valueAddPair,
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

        public class HgeTableReadResult
        {
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public Dictionary<string, List<List<string>>> evosByKey = new Dictionary<string, List<List<string>>>();
        }

        public class HgeTableEntry
        {
            public string key = "";
            public List<List<string>> entries = new List<List<string>>();
        }

        public HgeTableReadResult ReadHgeTableFile(string filePath, string headerClassName, string attributeName, string endClassName)
        {
            HgeTableReadResult result = new HgeTableReadResult();
            StreamReader sr = new StreamReader(filePath);
            CodeSection currentCodeSection = new CodeSection();
            CodeSectionType curCodeType = CodeSectionType.BEGINNING;
            HgeTableEntry curTableEntry = null;

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

                string lineFixedForData = StripTrailingComments(line.Trim()).Trim();
                if (lineFixedForData.StartsWith(headerClassName))
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        if (currentCodeSection.lines.Any(i => i != "") || curCodeType == CodeSectionType.BEGINNING)
                        {
                            result.codeInfo.codeSections.AddValueToCorrespondingList(curCodeType, currentCodeSection);
                        }
                        currentCodeSection = new CodeSection();
                        if (curCodeType == CodeSectionType.BEGINNING) { curCodeType = CodeSectionType.MIDDLE; }
                    }

                    curTableEntry = new HgeTableEntry()
                    {
                        key = lineFixedForData.Replace(headerClassName + " ", "")
                    };
                }
                else if (curTableEntry == null)
                {
                    currentCodeSection.lines.Add(line);
                }
                else if (lineFixedForData.StartsWith(endClassName))
                {
                    result.evosByKey[curTableEntry.key] = curTableEntry.entries;
                    curTableEntry = null;
                }else if (lineFixedForData.StartsWith(attributeName))
                {
                    List<string> pieces = lineFixedForData.Replace(",", "").Split(" ").ToList();
                    curTableEntry.entries.Add(pieces);
                }
            }
            //close the file
            sr.Close();
            return result;
        }

        public class ReadByteTableResult
        {
            public string tableName = "";
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public List<ByteTableEntry> entries = new List<ByteTableEntry>();
        }

        public class ByteTableEntry
        {
            public string key;
            public string byteClassName;
            public string value;
        }

        public class ReadByteTablesFileResult
        {
            public List<ReadByteTableResult> byteTables = new List<ReadByteTableResult>();
            public CodeSection lastCodeSection = new CodeSection();
        }

        public ReadByteTablesFileResult ReadByteTablesFile(string path, List<string> byteTableNames)
        {
            ReadByteTablesFileResult result = new ReadByteTablesFileResult();
            StreamReader sr = new StreamReader(path);
            CodeSection currentCodeSection = new CodeSection();
            CodeSectionType curCodeType = CodeSectionType.BEGINNING;
            bool foundLastLine = false;
            foreach(var tableName in byteTableNames)
            {
                while (true)//Search for the table name and record the lines before it
                {
                    var line = sr.ReadLine();
                    if (line == null)//This shouldnt happen
                    {
                        foundLastLine = true;
                        break;
                    }else if (line.Trim().StartsWith(tableName))
                    {
                        var resultThisTable = ReadByteTable(sr, tableName, out foundLastLine);
                        resultThisTable.codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.BEGINNING, currentCodeSection);
                        result.byteTables.Add(resultThisTable);
                        currentCodeSection = new CodeSection();
                        break;
                    }
                    else
                    {
                        currentCodeSection.lines.Add(line);
                    }
                }
                if (foundLastLine) { break;  }
            }
            if (foundLastLine)
            {
                sr.Close();
                return result;
            }
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                {
                    result.lastCodeSection = currentCodeSection; 
                    break;
                }
                currentCodeSection.lines.Add(line);
            }
            //close the file
            sr.Close();
            return result;
        }

        public ReadByteTableResult ReadByteTable(StreamReader sr, string tableName, out bool isLastLineNull)
        {
            ReadByteTableResult result = new ReadByteTableResult()
            {
                tableName = tableName,
            };
            CodeSection currentCodeSection = new CodeSection();
            CodeSectionType curCodeType = CodeSectionType.MIDDLE;
            isLastLineNull = false;

            while (true)
            {
                var line = sr.ReadLine();
                if (line == null || line.Contains(".close"))
                {
                    if (line != null){currentCodeSection.lines.Add(line);}
                    else{isLastLineNull = true;}
                    result.codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.END, currentCodeSection);
                    break;
                }
                else if (line.StartsWith("/*") )
                {
                    if (currentCodeSection.lines.Count > 0)
                    {
                        result.codeInfo.codeSections.AddValueToCorrespondingList(curCodeType, currentCodeSection);
                        currentCodeSection = new CodeSection();
                    }
                    List<string> pieces = line.Replace("/*", "").Replace("*/", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (pieces.Count < 3)
                    {
                        continue;//Throw Exception?
                    }
                    result.entries.Add(new ByteTableEntry()
                    {
                        byteClassName = pieces[1],
                        key = pieces[0],
                        value = pieces[2],
                    });
                }
                else
                {
                    currentCodeSection.lines.Add(line);
                }
            }
            return result;
        }
    }
}
