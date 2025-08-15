using HGEngineHelper.Code.HGECodeHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static HGEngineHelper.Code.HGEngineExport.HgEngineCodeWriter;

namespace HGEngineHelper.Code.HGEngineExport
{
    public class HgEngineCodeWriter
    {
        public class HgEngineObject
        {
            public string headerClassName => headerInfo.Count > 0 ? headerInfo[0] : "";
            public List<string> headerInfo = new List<string>();
            public string name => headerInfo.Count > 1 ? headerInfo[1] : "";
            public Dictionary<string, List<string>> attributeInfoByLine = new Dictionary<string, List<string>>();
        }

        public class HgEngineObjectWriteInfo<T>
        {
            public HgEngineObjectWriteInfo()
            {

            }
            public List<T> objectsToWrite = new List<T>();
            public List<HgeAttr<T>> attributes = new List<HgeAttr<T>>();
            public Func<T, string> getHeaderRowFunc;
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public string attributeFrontPadding = "    ";
            public Func<T, int> getNumSpacingLinesFunc;
            public List<string> modelSpacing = new List<string>() { "" };
        }

        public class HgeAttr<T>
        {
            public string attr;
            public Func<T, string> valGet;
        }

        public void WriteModelFile<T>(string path, HgEngineObjectWriteInfo<T> writeInfo)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.BEGINNING);

                foreach(var info in writeInfo.objectsToWrite)
                {
                    //Write Header Row
                    var headerRow = writeInfo.getHeaderRowFunc(info);
                    outputFile.WriteLine(headerRow);

                    //Write each attribute
                    foreach(var attribute in writeInfo.attributes)
                    {
                        var value = attribute.valGet(info);
                        if (value.Replace(",", "").Trim() == "")
                        {
                            continue;
                        }
                        outputFile.WriteLine(writeInfo.attributeFrontPadding + attribute.attr + " " + value);
                    }
                    //Write spacing
                    var numSpacingLines = writeInfo.getNumSpacingLinesFunc(info);
                    for(int i = 0; i < numSpacingLines; i++)
                    {
                        outputFile.WriteLine("");
                    }
                }

                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.END);
            }
        }

        public static void WriteCodeSectionIfItExists(StreamWriter writer, ref Dictionary<CodeSectionType, List<CodeSection>> sections, CodeSectionType type)
        {
            if (!sections.ContainsKey(type))
            {
                return;
            }
            foreach(var section in sections[type])
            {
                foreach (var line in section.lines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public class HgEngineDataEntryWriteInfo<T>
        {
            public HgEngineDataEntryWriteInfo()
            {

            }
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public List<T> objectsToWrite = new List<T>();
            public Func<T, string> GetDataEntryClassName;
            public Func<T, string> GetDataEntryKey;
            public Func<T, List<string>> GetDataEntryLineListValue;
            public Func<T, bool> HasDataEntry;
        }

        public void WriteHgEngineDataEntryFile<T>(string path, HgEngineDataEntryWriteInfo<T> writeInfo)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.BEGINNING);

                foreach (var info in writeInfo.objectsToWrite)
                {
                    bool hasDataEntry = writeInfo.HasDataEntry(info);
                    if (!hasDataEntry)
                    {
                        continue;
                    }
                    string dataClassName = writeInfo.GetDataEntryClassName(info);
                    string dataEntryKey = writeInfo.GetDataEntryKey(info);
                    List<string> dataEntryLineListValue = writeInfo.GetDataEntryLineListValue(info);
                    outputFile.WriteLine(dataClassName + " " + dataEntryKey + (dataEntryLineListValue.Count > 0 ? ", " + String.Join(", ", dataEntryLineListValue) : ""));
                }

                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.END);
            }
        }

        public class HgEngineConstantsDictionaryWriteInfo<T>
        {
            public HgEngineConstantsDictionaryWriteInfo()
            {

            }
            public HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            public List<T> objectsToWrite = new List<T>();
            public Func<T, string> GetKeyFunc;
            public Func<T, string> GetValueFunc;
            public Func<T, bool> HasEntryFunc;
            public string frontPadding = "    ";
        }

        public enum CodeSectionRelativePlacementType
        {
            BEFORE = 1,
            AFTER = 2,
        }

        public void WriteConstantsDictionary<T>(string path, HgEngineConstantsDictionaryWriteInfo<T> writeInfo)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.BEGINNING);

                foreach (var info in writeInfo.objectsToWrite)
                {
                    bool hasDataEntry = writeInfo.HasEntryFunc(info);
                    if (!hasDataEntry)
                    {
                        continue;
                    }
                    string key = writeInfo.GetKeyFunc(info);
                    while(key.Length < KeySpace)
                    {
                        key = key + " ";
                    }
                    string value = writeInfo.GetValueFunc(info);
                    string lineToWrite = writeInfo.frontPadding + "[" + key + "] = " + value + ",";
                    outputFile.WriteLine(lineToWrite);
                }

                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.END);
            }
        }
        private const int KeySpace = 36;

        public class HgEngineTableWriteInfo<TKey, TValue>
        {
            public HgEngineTableWriteInfo()
            {

            }
            public HgEngineCodeInfo codeInfo;
            public string className;
            public Func<TKey, bool> doesTerminateClassHaveSpacing;
            public string terminateClassName;
            public Func<TValue, string> getAttributeNameFunc;
            public List<TKey> objectsToWrite;
            public Func<TKey, string> getKeyFunc;
            public Func<TKey, List<string>> getExtraSpacingLinesFunc;
            public string attributeSpacing = "    ";
            public Dictionary<string, List<TValue>> entriesByKey;
            public Func<TValue, List<string>> getValuesForTableEntryFunc;
        }

        public void WriteHgeTableFile<TKey,TValue>(string path, HgEngineTableWriteInfo<TKey, TValue> writeInfo)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.BEGINNING);

                foreach(TKey keyObject in writeInfo.objectsToWrite)
                {
                    string key = writeInfo.getKeyFunc(keyObject);
                    var entries = writeInfo.entriesByKey.GetValueOrDefault(key, new List<TValue>());
                    outputFile.WriteLine(writeInfo.className + " " + key);
                    foreach(var entry in entries)
                    {
                        string attributeClassName = writeInfo.getAttributeNameFunc(entry);
                        List<string> valuesForEntry = writeInfo.getValuesForTableEntryFunc(entry);
                        outputFile.WriteLine(writeInfo.attributeSpacing + attributeClassName + " " + String.Join(", ", valuesForEntry));
                    }
                    bool doesTerminateClassHaveSpacing = writeInfo.doesTerminateClassHaveSpacing(keyObject);
                    outputFile.WriteLine((doesTerminateClassHaveSpacing ? writeInfo.attributeSpacing : "") + writeInfo.terminateClassName);

                    List<string> extraSpacingLines = writeInfo.getExtraSpacingLinesFunc(keyObject);
                    extraSpacingLines.ForEach(i => outputFile.WriteLine(i));
                }

                WriteCodeSectionIfItExists(outputFile, ref writeInfo.codeInfo.codeSections, CodeSectionType.END);
            }
        }
    }
}
