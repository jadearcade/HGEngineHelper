using CsvConverter;
using HGEngineHelper.Code.CsvProcessing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HgEngineCsvConverter.Code.HgEngineMoveDataParser;

namespace HgEngineCsvConverter.Code
{
    public class HgEngineFormDataParser
    {
        public static string FormeTypeMega = "MEGA_EVOLUTIONS";
        public static string FormeTypePrimal = "PRIMAL_REVERSION";
        public static string FormeTypeAlolan = "alolan forms";
        public static string FormeTypeTotems = "totems";
        public static string FormeTypeGalarian = "galarian forms";
        public static string FormeTypeCosmetic = "cosmetic forms";
        public static string FormeTypeBattle = "Battle Forms";
        public static string FormeTypeHisuian = "hisuian forms";
        public static string FormeTypeNoble = "noble pokemon";
        public static string FormeTypeGender = "significant gender differences";
        public static string FormeTypePaldean = "paldean forms";

        public static List<string> TypesToTrack = new List<string>()
        {
            FormeTypeMega, FormeTypePrimal, FormeTypeAlolan, FormeTypeTotems, FormeTypeGalarian,
            FormeTypeCosmetic, FormeTypeBattle, FormeTypeHisuian, FormeTypeNoble, FormeTypeGender, FormeTypePaldean
        };
        public static string SpeciesKeyStart = "SPECIES_";

        public List<FormesForSpeciesInfo> ReadSpeciesFormTable(string filePath)
        {
            List<FormesForSpeciesInfo> result = new List<FormesForSpeciesInfo>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            string currentType = "Unknown";
            FormesForSpeciesInfo currentSpeciesInfo = null;
            while (line != null)
            {
                bool isTypeLine = false;
                foreach(string typeToTrack in TypesToTrack)
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
                    };
                }else if (currentSpeciesInfo == null && line.Contains(SpeciesKeyStart))
                {
                    currentSpeciesInfo = new FormesForSpeciesInfo()
                    {
                        type = currentType,
                        forms = new List<SpeciesForm>(),
                        speciesKey = line.Replace(" ", "").Replace("[","").Replace("]","").Replace("=","").Replace("{",""),
                    };
                }
                else if (currentSpeciesInfo != null)
                {
                    currentSpeciesInfo.forms.Add(new SpeciesForm()
                    {
                        needsReversion = line.Contains("NEEDS_REVERSION"),
                        formSpeciesKey = line.Replace(" ", "").Replace(",","").Split("|").Last()
                    });
                }
                    line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }

        public class MegaForm
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "Species")]
            public string speciesKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Item")]
            public string itemKey { get; set; }
            [CsvConverter(ColumnIndex = 3, ColumnName = "Form Index")]
            public int formIndex { get; set; }
        }

        public List<MegaForm> ReadMegaFormTable(string filePath)
        {
            List<MegaForm> result = new List<MegaForm>();
            String line;
            StreamReader sr = new StreamReader(filePath);
            line = sr.ReadLine();
            string currentType = "Unknown";
            MegaForm currentMegaForm = null;
            bool activelySearching = false;
            while (line != null)
            {
                if (line.Contains("const struct MegaStruct sMegaTable"))
                {
                    activelySearching = true;
                }
                if (activelySearching)
                {
                    if (line.Contains("        .monindex = "))
                    {
                        if (currentMegaForm != null)
                        {
                            result.Add(currentMegaForm);
                        }
                        currentMegaForm = new MegaForm()
                        {
                            speciesKey = line.Replace("        .monindex = ", "").Replace(",",""),
                        };
                    }else if (currentMegaForm != null)
                    {
                        if (line.Contains("        .itemindex = "))
                        {
                            currentMegaForm.itemKey = line.Replace("        .itemindex = ", "").Replace(",", "");
                        }else if  (line.Contains("        .form = "))
                        {
                            currentMegaForm.formIndex = line.Replace("        .form = ", "").Replace(",", "").ToInt();
                        }
                    }else if (line.Contains("};"))
                    {
                        if (currentMegaForm != null)
                        {
                            result.Add(currentMegaForm);
                        }
                        break;
                    }
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            return result;
        }

        public string SpeciesFormFileName = "PokeFormDataTbl.c";
        public string MegaFormTableFileName = "mega.c";
        public BoolResultWithMessage CreateSpeciesFormTableCsv(string basePath, string outputPath)
        {
            var filePath = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath, new List<string>() { SpeciesFormFileName }).GetValueOrDefault(SpeciesFormFileName);
            if ((filePath ?? "") == "")
            {
                return new BoolResultWithMessage(false, "Missing " + SpeciesFormFileName);
            }
            List<FormesForSpeciesInfoRow> rowsToWriteCsv = new List<FormesForSpeciesInfoRow>();
            var speciesData = ReadSpeciesFormTable(filePath);
            foreach(var speciesDataRow in speciesData)
            {
                rowsToWriteCsv.AddRange(speciesDataRow.forms.Select(i => new FormesForSpeciesInfoRow()
                {
                    NeedsReversion = i.needsReversion,
                    FormSpeciesKey = i.formSpeciesKey,
                    SpeciesKey = speciesDataRow.speciesKey,
                    Type = speciesDataRow.type
                }));
            }
            using(var fs = File.Create(outputPath))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<FormesForSpeciesInfoRow>(sw);

                foreach (var row in rowsToWriteCsv)
                {
                    service.WriteRecord(row);
                }
            }
            return new BoolResultWithMessage(true, "");
        }

        internal BoolResultWithMessage CreateMegaFormTableCsv(string basePath, string outputPath)
        {
            var filePath = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath, new List<string>() { MegaFormTableFileName }).GetValueOrDefault(MegaFormTableFileName);
            if ((filePath ?? "") == "")
            {
                return new BoolResultWithMessage(false, "Missing " + MegaFormTableFileName);
            }
            var speciesData = ReadMegaFormTable(filePath);
            using (var fs = File.Create(outputPath))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<MegaForm>(sw);

                foreach (var row in speciesData)
                {
                    service.WriteRecord(row);
                }
            }
            return new BoolResultWithMessage(true, "");
        }
    }
}
