using HgEngineCsvConverter.Code;
using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;

namespace HGEngineHelper.Code.HGEngineExport
{
    public class SpeciesFormFileWriter
    {
        public void WriteSpeciesFormFile(string path, List<FormesForSpeciesInfoRow> formes, HgEngineCodeInfo codeInfo)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            Dictionary<string, List<FormesForSpeciesInfoRow>> formesByFormType
                = HelperFunctions.ConvertListToDictionaryOfLists(formes, i => i.Type);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref codeInfo.codeSections, CodeSectionType.BEGINNING);

                foreach (var formType in HgEngineFormDataParser.TypesToTrack)
                {
                    GetExtraLinesPerTypeBeginning(formType).ForEach(i => outputFile.WriteLine(i));

                    var formesForThisType = formesByFormType.GetValueOrDefault(formType, new List<FormesForSpeciesInfoRow>());
                    foreach(var speciesKeyAndFormes in formesForThisType.GroupBy(i => i.SpeciesKey).Select(i => new { speciesKey = i.Key, formes = i.ToList() }))
                    {
                        outputFile.WriteLine("    [{0}] = ".FormatStr(speciesKeyAndFormes.speciesKey) + "{");

                        foreach(var form in speciesKeyAndFormes.formes)
                        {
                            string line = "        " + (form.NeedsReversion ? "NEEDS_REVERSION | " : "") + form.FormSpeciesKey + ",";
                            outputFile.WriteLine(line);
                        }

                        outputFile.WriteLine("    },");
                    }

                    GetExtraLinesPerTypeEnding(formType).ForEach(i => outputFile.WriteLine(i));
                }

                outputFile.WriteLine("};");
                //HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref codeInfo.codeSections, CodeSectionType.END);
            }
        }

        public void WriteFormSpeciesMappingFile(string path, List<FormesForSpeciesInfoRow> formes)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref codeInfoFormToSpeciesMapping.codeSections, CodeSectionType.BEGINNING);

                foreach(var form in formes)
                {
                    string line = "    [{0} - SPECIES_MEGA_START] = {1},".FormatStr(form.FormSpeciesKey, form.SpeciesKey);
                    outputFile.WriteLine(line);
                }

                outputFile.WriteLine("};");
                //HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref codeInfo.codeSections, CodeSectionType.END);
            }
        }

        public void WriteFormReversionMappingFile(string path, List<FormesForSpeciesInfoRow> formes, HgEngineCodeInfo codeInfo)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref codeInfo.codeSections, CodeSectionType.BEGINNING);

                foreach (var form in formes.Where(i => i.NeedsReversion && i.FormToRevertToIndex != 0))
                {
                    string line = "    [{0} - SPECIES_MEGA_START]".FormatStr(form.FormSpeciesKey).PadRight(76)
                        + "= {0},".FormatStr(form.FormToRevertToIndex);
                    outputFile.WriteLine(line);
                }

             //   outputFile.WriteLine("};");
               HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref codeInfo.codeSections, CodeSectionType.END);
            }
        }

        public static HgEngineCodeInfo codeInfoFormToSpeciesMapping = new HgEngineCodeInfo()
        {
            codeSections = new Dictionary<CodeSectionType, List<CodeSection>>()
            {
                {CodeSectionType.BEGINNING, new List<CodeSection>()
                {
                    new CodeSection()
                    {
                        lines = new List<string>()
                        {
                            "#include \"../include/types.h\"",
                            "#include \"../include/config.h\"",
                            "#include \"../include/pokemon.h\"",
                            "#include \"../include/constants/species.h\"",
                            "",
                            "const u16 UNUSED FormToSpeciesMapping[] =",
                            "{"
                        }
                    }
                } }
            }
        };

        public static string FormeTypeHeaderCommentLineFormat = "    /**{0}**/";

        public List<string> GetExtraLinesPerTypeBeginning(string type)
        {
            if (type == HgEngineFormDataParser.FormeTypeMega)
            {
                return new List<string>() { "#ifdef " + type };
            }else if (type == HgEngineFormDataParser.FormeTypePrimal)
            {
                return new List<string>() { "#ifdef " + type };
            }
            return new List<string>() { "", FormeTypeHeaderCommentLineFormat.FormatStr(type) };
        }

        public List<string> GetExtraLinesPerTypeEnding(string type)
        {
            if (type == HgEngineFormDataParser.FormeTypeMega)
            {
                return new List<string>() { "#endif // {0}".FormatStr(HgEngineFormDataParser.FormeTypeMega), "" };
            }
            else if (type == HgEngineFormDataParser.FormeTypePrimal)
            {
                return new List<string>() { "#endif // {0}".FormatStr(HgEngineFormDataParser.FormeTypePrimal) };
            }
            return new List<string>();
        }

        public void WriteMegaFormFile(string path, List<FormesForSpeciesInfoRow> formes, HgEngineCodeInfo hgEngineCodeInfo)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref hgEngineCodeInfo.codeSections, CodeSectionType.BEGINNING);

                //Write Item Based Megas
                foreach (var form in formes.Where(i => i.MegaItem != ""))
                {
                    List<string> linesToWrite = new List<string>()
                    {
                        "    {",
                        "        .monindex = {0},".FormatStr(form.SpeciesKey),
                        "        .itemindex = {0},".FormatStr(form.MegaItem),
                        "        .form = {0},".FormatStr(form.FormIndex),
                        "    },",
                    };
                    linesToWrite.ForEach(i => outputFile.WriteLine(i));
                }

                HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref hgEngineCodeInfo.codeSections, CodeSectionType.MIDDLE);

                //Write Move Based Megas
                foreach (var form in formes.Where(i => i.MegaMove != ""))
                {
                    List<string> linesToWrite = new List<string>()
                    {
                        "    {",
                        "        .monindex = {0},".FormatStr(form.SpeciesKey),
                        "        .moveindex = {0},".FormatStr(form.MegaMove),
                        "        .form = {0},".FormatStr(form.FormIndex),
                        "    },",
                    };
                    linesToWrite.ForEach(i => outputFile.WriteLine(i));
                }

               // outputFile.WriteLine("};");
                HgEngineCodeWriter.WriteCodeSectionIfItExists(outputFile, ref hgEngineCodeInfo.codeSections, CodeSectionType.END);
            }
        }
    }
}
