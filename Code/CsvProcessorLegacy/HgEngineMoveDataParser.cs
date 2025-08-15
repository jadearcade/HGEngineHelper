using CsvConverter;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HgEngineCsvConverter.Code.HgEngineLearnsetDataParser;

namespace HgEngineCsvConverter.Code
{
    public class HgEngineMoveDataParser
    {
        public class OutputMoveData
        {
            [CsvConverter(ColumnIndex = 1, ColumnName = "Move Key")]
            public string moveKey { get; set; }
            [CsvConverter(ColumnIndex = 2, ColumnName = "Move Name")]
            public string moveName { get; set; }
            [CsvConverter(ColumnIndex = 3, ColumnName = "Type")]
            public string typeKey { get; set; }
            [CsvConverter(ColumnIndex = 4, ColumnName = "Phys Special Split")]
            public string physSpecSplit { get; set; }
            [CsvConverter(ColumnIndex = 5, ColumnName = "Base Power")]
            public int basePower { get; set; }
            [CsvConverter(ColumnIndex = 6, ColumnName = "Accuracy")]
            public int accuracy { get; set; }
            [CsvConverter(ColumnIndex = 7, ColumnName = "PP")]
            public int pp { get; set; }
            [CsvConverter(ColumnIndex = 8, ColumnName = "Effect Chance")]
            public int effectchance { get; set; }
            [CsvConverter(ColumnIndex = 10, ColumnName = "Move Description")]
            public string movedescription { get; set; }
            [CsvConverter(ColumnIndex = 11, ColumnName = "Battle Effect Key")]
            public string battleEffectKey { get; set; }
            [CsvConverter(ColumnIndex = 12, ColumnName = "Battle Effect Comment")]
            public string battleEffectComment { get; set; }
            [CsvConverter(ColumnIndex = 13, ColumnName = "Target")]
            public string target { get; set; }
            [CsvConverter(ColumnIndex = 14, ColumnName = "Priority")]
            public int priority { get; set; }

            public List<string> flags { get; set; }
            [CsvConverter(ColumnIndex = 15, ColumnName = "Contest Type")]
            public string contesttype { get; set; }
            [CsvConverter(ColumnIndex = 16, ColumnName = "Appeal")]
            public string appeal { get; set; }
            [CsvConverter(ColumnIndex = 17, ColumnName = "F_Contact")]
            public bool fContact => flags.Contains("FLAG_CONTACT");
            [CsvConverter(ColumnIndex = 18, ColumnName = "F_HideShadow")]
            public bool fHideShadow => flags.Contains("FLAG_HIDE_SHADOW");
            [CsvConverter(ColumnIndex = 19, ColumnName = "F_KeepHpBar")]
            public bool fKeepHpBar => flags.Contains("FLAG_KEEP_HP_BAR");
            [CsvConverter(ColumnIndex = 20, ColumnName = "F_KingsRock")]
            public bool fKingsRock => flags.Contains("FLAG_KINGS_ROCK");
            [CsvConverter(ColumnIndex = 21, ColumnName = "F_MagicCoat")]
            public bool fMagicCoat => flags.Contains("FLAG_MAGIC_COAT");
            [CsvConverter(ColumnIndex = 22, ColumnName = "F_MirrorMove")]
            public bool fMirrorMove => flags.Contains("FLAG_MIRROR_MOVE");
            [CsvConverter(ColumnIndex = 23, ColumnName = "F_Protect")]
            public bool fProtect => flags.Contains("FLAG_PROTECT");
            [CsvConverter(ColumnIndex = 24, ColumnName = "F_Snatch")]
            public bool fSnatch => flags.Contains("FLAG_SNATCH");
        }
        public List<OutputMoveData> ReadMoveData(string path)
        {
            HgEngineDataParser hgEngineDataParser = new HgEngineDataParser();
            List<OutputMoveData> result = new List<OutputMoveData>();
            List<HgEngineObject> hgEngineObjects = hgEngineDataParser.ReadEngineObjects(path, "movedata");
            foreach(var obj in hgEngineObjects)
            {
                List<string> flags = obj.attributeInfoByLine["flags"][0].Replace(" ","").Split('|').ToList();
                if (flags.Contains("0x00"))
                {
                    flags.Remove("0x00");
                }
                string battleEffectKeyOuter = obj.attributeInfoByLine["battleeffect"][0].Replace(" ", "");
                List<string> pieces = battleEffectKeyOuter.Split("\\\\", StringSplitOptions.RemoveEmptyEntries).ToList();
                result.Add(new OutputMoveData()
                {
                    moveKey = obj.name,
                    moveName = HgEngineCsvConverterHelperFunctions.FixDescriptionColumn(obj.headerInfo),
                    accuracy = obj.attributeInfoByLine["accuracy"][0].ToInt(),
                    pp = obj.attributeInfoByLine["pp"][0].ToInt(),
                    effectchance = obj.attributeInfoByLine["effectchance"][0].ToInt(),
                    priority = obj.attributeInfoByLine["priority"][0].ToInt(),
                    basePower = obj.attributeInfoByLine["basepower"][0].ToInt(),
                    target = obj.attributeInfoByLine["target"][0],
                    appeal = obj.attributeInfoByLine["appeal"][0],
                    flags = flags,
                    contesttype = obj.attributeInfoByLine["contesttype"][0],
                    battleEffectKey = pieces[0],
                    battleEffectComment = pieces.Count > 1 ? pieces[1] : "",
                    physSpecSplit = obj.attributeInfoByLine["pss"][0],
                    typeKey = obj.attributeInfoByLine["type"][0].Replace(" ", "").Replace("(", "").Replace(")", "").Replace("FAIRY_TYPE_IMPLEMENTED?", "").Split(":").FirstOrDefault(""),
                    movedescription = HgEngineCsvConverterHelperFunctions.FixDescriptionColumn(obj.attributeInfoByLine["movedescription"]),
                });
            }
            return result;
        }

        public class MoveFlagOutputRow
        {
            public string flagKey { get; set; }
            public string moveKey { get; set; }
        }

        public string MovesFileName = "moves.s";
        public BoolResultWithMessage CreateMoveDataCsv(string basePath, string outputPath)
        {
            var filePath = HgEngineCsvConverterHelperFunctions.GetFileNameToPathDictionary(basePath + "\\armips\\data\\", new List<string>() { MovesFileName }).GetValueOrDefault(MovesFileName);
            if ((filePath ?? "") == "")
            {
                return new BoolResultWithMessage(false, "Missing " + MovesFileName);
            }
            var moveData = ReadMoveData(filePath);
            using (var fs = File.Create(outputPath))
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                var service = new CsvWriterService<OutputMoveData>(sw);

                foreach (var moveDataRow in moveData)
                {
                    service.WriteRecord(moveDataRow);
                }
            }

            return new BoolResultWithMessage(true, "");
        }
    }
}
