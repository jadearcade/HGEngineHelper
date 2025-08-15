using CsvConverter;
using CsvHelper;
using HgEngineCsvConverter.Code;
using HGEngineHelper.Code.CsvProcessing;
using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.Code.HGEngineImport
{
    public class HgEngineFormDataParser
    {

        public List<string> TypesToTrack = new List<string>()
        {
            "MEGA_EVOLUTIONS",
            "PRIMAL_REVERSION",
            "alolan forms",
            "totems",
            "galarian forms",
            "cosmetic forms",
            "Battle Forms",
            "hisuian forms",
            "noble pokemon",
            "significant gender differences",
            "paldean forms",
        };
        public static string SpeciesKeyStart = "SPECIES_";

        public class HgEngineFormDataReadResult
        {
            public List<FormesForSpeciesInfo> formes;
            public HgEngineCodeInfo codeInfo;
        }

        public HgEngineFormDataReadResult ReadSpeciesFormTable(string filePath)
        {
            List<FormesForSpeciesInfo> result = new List<FormesForSpeciesInfo>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            string currentType = "Unknown";
            FormesForSpeciesInfo currentSpeciesInfo = null;
            HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            CodeSection currentCodeSection = new CodeSection();
            while (line != null)
            {
                bool isTypeLine = false;
                foreach (string typeToTrack in TypesToTrack)
                {
                    if (line.Contains(typeToTrack))
                    {
                        currentType = typeToTrack;
                        isTypeLine = true;
                        break;
                    }
                }
                if (isTypeLine)
                {

                }
                else if (line.Contains("},"))
                {
                    if (currentSpeciesInfo != null)
                    {
                        result.Add(currentSpeciesInfo);
                        currentSpeciesInfo = null;
                    }
                }
                else if (currentSpeciesInfo == null && line.Contains(SpeciesKeyStart))
                {
                    currentSpeciesInfo = new FormesForSpeciesInfo()
                    {
                        type = currentType,
                        forms = new List<SpeciesForm>(),
                        speciesKey = HGEngineCodeParser.StripTrailingComments(line.Replace(" ", "").Replace("[", "").Replace("]", "").Replace("=", "").Replace("{", "")),
                    };
                    if (currentCodeSection != null)
                    {
                        codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.BEGINNING, currentCodeSection);
                        currentCodeSection = null;
                    }
                }
                else if (currentSpeciesInfo != null)
                {
                    currentSpeciesInfo.forms.Add(new SpeciesForm()
                    {
                        needsReversion = line.Contains("NEEDS_REVERSION"),
                        formSpeciesKey = line.Replace(" ", "").Replace(",", "").Split("|").Last()
                    });
                }else if (result.Count == 0 && (currentCodeSection != null))
                {
                    currentCodeSection.lines.Add(line);
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return new HgEngineFormDataReadResult()
            {
                formes = result,
                codeInfo = codeInfo,
            };
        }

        public class MegaForm
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "Species")]
            public string speciesKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Item")]
            public string itemKey { get; set; }
            [CsvConverter(ColumnIndex = 3, ColumnName = "Form Index")]
            public int formIndex { get; set; }
            public string moveKey { get; set; } = "";
        }

        public enum MegaFormReadType
        {
            READ_CODE = 1,
            READ_ITEM_BASED = 2,
            READ_MOVE_BASED = 3,
        }

        public List<MegaForm> ReadMegaFormTableAndSaveCodeInfo(string filePath)
        {
            List<MegaForm> result = new List<MegaForm>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            string currentType = "Unknown";
            MegaForm currentMegaForm = null;
            bool activelySearching = false;
            MegaFormReadType readType = MegaFormReadType.READ_CODE;
            HgEngineCodeInfo codeInfo = new HgEngineCodeInfo();
            CodeSection currentCodeSection = new CodeSection();

            while (line != null)
            {
                if (readType == MegaFormReadType.READ_CODE) { currentCodeSection.lines.Add(line); }
                if (line.Contains("const struct MegaStruct sMegaTable"))
                {
                    readType = MegaFormReadType.READ_ITEM_BASED;
                    currentCodeSection.lines.Add("{");
                    codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.BEGINNING, currentCodeSection);
                }else if (line.Contains("const struct MegaStructMove sMegaMoveTable"))
                {
                    readType = MegaFormReadType.READ_MOVE_BASED;
                    currentCodeSection.lines.Add("{");
                    codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.MIDDLE, currentCodeSection);
                }
                else if (readType == MegaFormReadType.READ_ITEM_BASED)
                {
                    if (line.Contains(".monindex = ")) {
                        if (currentMegaForm != null) { result.Add(currentMegaForm); }
                        currentMegaForm = new MegaForm() { speciesKey = line.Replace(".monindex = ", "").Trim().Replace(",", ""), };
                    }
                    else if (line.Contains("};"))
                    {
                        if (currentMegaForm != null) { result.Add(currentMegaForm); }
                        currentMegaForm = null;
                        readType = MegaFormReadType.READ_CODE;
                        currentCodeSection = new CodeSection();
                        currentCodeSection.lines.Add(line);
                    }
                    else if (currentMegaForm != null) {
                        if (line.Contains(".itemindex = ")) { currentMegaForm.itemKey = line.Replace(".itemindex = ", "").Trim().Replace(",", ""); }
                        else if (line.Contains(".form = ")) { currentMegaForm.formIndex = line.Replace(".form = ", "").Trim().Replace(",", "").ToInt(); }
                    }
                } else if (readType == MegaFormReadType.READ_MOVE_BASED)
                {
                    if (line.Contains(".monindex = "))
                    {
                        if (currentMegaForm != null) { result.Add(currentMegaForm); }
                        currentMegaForm = new MegaForm() { speciesKey = line.Replace(".monindex = ", "").Trim().Replace(",", ""), };
                    }
                    else if (line.Contains("};"))
                    {
                        if (currentMegaForm != null) { result.Add(currentMegaForm); }
                        readType = MegaFormReadType.READ_CODE;
                        currentCodeSection = new CodeSection();
                        currentCodeSection.lines.Add(line);
                    }
                    else if (currentMegaForm != null)
                    {
                        if (line.Contains(".moveindex = ")) { currentMegaForm.moveKey = line.Replace(".moveindex = ", "").Trim().Replace(",", ""); }
                        else if (line.Contains(".form = ")) { currentMegaForm.formIndex = line.Replace(".form = ", "").Trim().Replace(",", "").ToInt(); }
                    }
                }
                line = sr.ReadLine();
            }
            if ((currentCodeSection?.lines.Count ?? 0) > 0)
            {
                codeInfo.codeSections.AddValueToCorrespondingList(CodeSectionType.END, currentCodeSection);
            }
            //close the file
            sr.Close();

            ImporterFromHge.SaveCodeInfo(codeInfo, HgEnginePaths.MegaFormTableFileName);

            return result;
        }

        public void ImportFormData()
        {
            string subPath = HgEnginePaths.SpeciesFormFileName;
            List<FormesForSpeciesInfoRow> rowsToWriteCsv = new List<FormesForSpeciesInfoRow>();
            var speciesData = ReadSpeciesFormTable(App.ProjectInfo.hgEnginePath + "/" + subPath);
            Dictionary<string, int> formToRevertToDictionary = ImportFormReversionMappingDataAndSaveCode();
            var megaForms = ReadMegaFormTableAndSaveCodeInfo(App.ProjectInfo.hgEnginePath + "/" + HgEnginePaths.MegaFormTableFileName);
            Dictionary<string, string> megaMovesBySpeciesKeyAndFormIdx 
                = HelperFunctions.ConvertListToDictionaryOfValues(megaForms, i => i.speciesKey + "," + i.formIndex.ToString(), i => i.moveKey);
            Dictionary<string, string> megaItemsBySpeciesKeyAndFormIdx 
                = HelperFunctions.ConvertListToDictionaryOfValues(megaForms, i => i.speciesKey + "," + i.formIndex.ToString(), i => i.itemKey);
            foreach (var speciesDataRow in speciesData.formes)
            {
                int formIndex = 1;
                foreach(var form in speciesDataRow.forms)
                {
                    string formKeyIdx = speciesDataRow.speciesKey + "," + formIndex.ToString();
                    rowsToWriteCsv.Add(new FormesForSpeciesInfoRow()
                    {
                        NeedsReversion = form.needsReversion,
                        FormSpeciesKey = form.formSpeciesKey,
                        SpeciesKey = speciesDataRow.speciesKey,
                        Type = speciesDataRow.type,
                        FormIndex = formIndex++,
                        FormToRevertToIndex = formToRevertToDictionary.GetValueOrDefault(form.formSpeciesKey, 0),
                        MegaItem = megaItemsBySpeciesKeyAndFormIdx.GetValueOrDefault(formKeyIdx, ""),
                        MegaMove = megaMovesBySpeciesKeyAndFormIdx.GetValueOrDefault(formKeyIdx, ""),
                    });
                }
            }

            string codeOutputPath = App.ProjectInfo.hgeTestOutputFolder + subPath;
            string csvOutputPath = App.ProjectInfo.dataFolder +CsvFileNames.SpeciesFormMapping;
            Directory.CreateDirectory(Path.GetDirectoryName(codeOutputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(csvOutputPath));
            ImporterFromHge.SaveCodeInfo(speciesData.codeInfo, subPath);

            using (var sw = new StreamWriter(csvOutputPath))
            using (var csv = new CsvWriter(sw, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rowsToWriteCsv);
            }
        }

        public Dictionary<string, int> ImportFormReversionMappingDataAndSaveCode()
        {
            string subPath = HgEnginePaths.FormReversionMappingFileName;
            var parser = (new HGEngineCodeParser());
            var fileDictionaryReadResult = parser.ReadConstantDictionary(App.ProjectInfo.hgEnginePath + "/" + subPath, "SPECIES_");
            ImporterFromHge.SaveCodeInfo(fileDictionaryReadResult.codeInfo, subPath);
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach(var kvPair in fileDictionaryReadResult.dictionaryValues)
            {
                result[kvPair.Key.Replace("-SPECIES_MEGA_START", "")] = kvPair.Value.ToInt();
            }
            return result;
        }
    }
}
