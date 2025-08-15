using HgEngineCsvConverter.Code;
using HGEngineHelper.Code.HGECodeHelper;
using System.Text.RegularExpressions;

namespace HGEngineHelper.Code.CsvProcessing.Models
{
    public class MonInfo
    {
        public MonInfo()
        {

        }
        //defined in mondata.s
        public string Key { get; set; }
        public string Name { get; set; }
        public string Type1 { get; set; }
        public string Type2 { get; set; }
        public string Ability1 { get; set; }
        public string Ability2 { get; set; }
        public string HiddenAbility { get; set; }
        public string GetTypeLine(bool fairyTypeImplemented)
        {
            var tern = ConvertToFairyTypeTernary(fairyTypeImplemented);
            return tern.Item1 + ", " + tern.Item2;
        }

        public Tuple<string, string> ConvertToFairyTypeTernary(bool fairyTypeImplemented)
        {
            bool type1Fairy = Type1.Contains("FAIRY");
            bool type2Fairy = Type2.Contains("FAIRY");
            if (!fairyTypeImplemented || (!type1Fairy && !type2Fairy))
            {
                return new Tuple<string, string>(Type1, Type2);
            }
            var type1Fixed = !type1Fairy ? Type1 : ("(FAIRY_TYPE_IMPLEMENTED) ? " + Type1 + " : " + "TYPE_NORMAL");
            var type2Fixed = "";
            if (Type1 == Type2)
            {
                type2Fixed = type1Fixed;
            }
            else if (type2Fairy)
            {
                type2Fixed = ("(FAIRY_TYPE_IMPLEMENTED) ? " + Type2 + " : " + Type1);
            }
            else
            {
                type2Fixed = Type2;
            }
            return new Tuple<string, string>(type1Fixed, type2Fixed);
        }
        public void SetBaseStats(List<string> baseStats)
        {
            HP = baseStats.GetAtIndexOrDefault(0, "").Trim().ToInt();
            Atk = baseStats.GetAtIndexOrDefault(1, "").Trim().ToInt();
            Def = baseStats.GetAtIndexOrDefault(2, "").Trim().ToInt();
            SpA = baseStats.GetAtIndexOrDefault(4, "").Trim().ToInt();
            SpD = baseStats.GetAtIndexOrDefault(5, "").Trim().ToInt();
            Spe = baseStats.GetAtIndexOrDefault(3, "").Trim().ToInt();
        }
        public int HP { get; set; } = 0;
        public int Atk { get; set; } = 0;
        public int Def { get; set; } = 0;
        public int Spe { get; set; } = 0;
        public int SpA { get; set; } = 0;
        public int SpD { get; set; } = 0;
        public string DexEntry { get; set; }
        public string DexClassification { get; set; }
        public string DexHeight { get; set; }
        public string DexWeight { get; set; }
        public string BabyMonSpecies { get; set; }
        public string GrowthRate { get; set; }
        public string EggGroup1 { get; set; }
        public string EggGroup2 { get; set; }
        public int EggCycles { get; set; }
        public int CatchRate { get; set; }
        public int GenderRatio { get; set; }
        public int BaseFriendship { get; set; }
        public string Item1 { get; set; }
        public string Item2 { get; set; }
        public void SetEvYields(List<string> evYields)
        {
            EV_HP = evYields.GetAtIndexOrDefault(0, "").Trim().ToInt();
            EV_Atk = evYields.GetAtIndexOrDefault(1, "").Trim().ToInt();
            EV_Def = evYields.GetAtIndexOrDefault(2, "").Trim().ToInt();
            EV_SpA = evYields.GetAtIndexOrDefault(4, "").Trim().ToInt();
            EV_SpD = evYields.GetAtIndexOrDefault(5, "").Trim().ToInt();
            EV_Spe = evYields.GetAtIndexOrDefault(3, "").Trim().ToInt();
        }
        public int EV_HP { get; set; } = 0;
        public int EV_Atk { get; set; } = 0;
        public int EV_Def { get; set; } = 0;
        public int EV_Spe { get; set; } = 0;
        public int EV_SpA { get; set; } = 0;
        public int EV_SpD { get; set; } = 0;
        public int RunChance { get; set; }
        public string ColorFlip { get; set; }
        public int ColorFlipInteger { get; set; }
        //heightTable.s
        public void SetHeightTableEntries(List<string> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }
            FBackHeightOffset = entries?.GetAtIndexOrDefault(0, "").ToIntNullable() ?? null;
            MBackHeightOffset = entries?.GetAtIndexOrDefault(1, "").ToIntNullable() ?? null;
            FFrontHeightOffset = entries?.GetAtIndexOrDefault(2, "").ToIntNullable() ?? null;
            MFrontHeightOffset = entries?.GetAtIndexOrDefault(3, "").ToIntNullable() ?? null;
        }
        public int? FBackHeightOffset { get; set; } = null;
        public int? MBackHeightOffset { get; set; } = null;
        public int? FFrontHeightOffset { get; set; } = null;
        public int? MFrontHeightOffset { get; set; } = null;
        public void SetSpriteOffsetEntries(List<string> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }
            FrontAnim = entries?.GetAtIndexOrDefault(0, "").ToInt() ?? 0;
            BackAnim = entries?.GetAtIndexOrDefault(1, "").ToInt() ?? 0;
            MonOffy = entries?.GetAtIndexOrDefault(2, "").ToInt() ?? 0;
            ShadowOffx = entries?.GetAtIndexOrDefault(3, "").ToInt() ?? 0;
            ShadowSize = entries?.GetAtIndexOrDefault(4, "") ?? "";
        }
        public int FrontAnim { get; set; } = 0;
        public int BackAnim { get; set; } = 0;
        public int MonOffy { get; set; } = 0;
        public int ShadowOffx { get; set; } = 0;
        public string ShadowSize { get; set; } = "";
        public int BaseExp { get; set; }
        public string SpeciesIndexValue1 { get; set; } = "";
        public string SpeciesIndexValueOp { get; set; } = "";
        public string SpeciesIndexValue2 { get; set; } = "";

        public string GetGroupKey()
        {
            return SpeciesIndexValue1.Contains("_") ? SpeciesIndexValue1.Replace("(","").Replace(")","").Trim() : "";
        }

        public string CanonicalGroupName = "Canonical";
        public static string CANONICAL_GROUP_KEY = "";
        public static string FAKEMON_GROUP_KEY = "MAX_CANONICAL_MON_NUM";
        public static string MEGA_GROUP_KEY = "SPECIES_MEGA_START";
        public static string PRIMAL_GROUP_KEY = "SPECIES_PRIMAL_START";
        public static string ALOLAN_GROUP_KEY = "SPECIES_ALOLAN_REGIONAL_START";
        public static string GALARIAN_GROUP_KEY = "SPECIES_GALARIAN_REGIONAL_START";
        public static string MISC_FORM_GROUP_KEY = "SPECIES_MISC_FORM_START";
        public static string GENDER_DIFF_GROUP_KEY = "SPECIES_SIGNIFICANT_GENDER_DIFFERENCE_START";
        public static string HISUIAN_GROUP_KEY = "SPECIES_HISUIAN_REGIONAL_START";
        public static string PALDEAN_GROUP_KEY = "SPECIES_PALDEAN_FORMS_START";

        public static List<GroupStartIndexNamePair> PokemonGroups = new List<GroupStartIndexNamePair>()
        {
            new GroupStartIndexNamePair(CANONICAL_GROUP_KEY, "Canonical Mon" ),
            new GroupStartIndexNamePair(FAKEMON_GROUP_KEY, "Fakemon" ),
            new GroupStartIndexNamePair(MEGA_GROUP_KEY, "Mega" ),
            new GroupStartIndexNamePair(PRIMAL_GROUP_KEY, "Primal" ),
            new GroupStartIndexNamePair(ALOLAN_GROUP_KEY, "Alolan" ),
            new GroupStartIndexNamePair (GALARIAN_GROUP_KEY, "Galarian"),
            new GroupStartIndexNamePair (MISC_FORM_GROUP_KEY, "Misc Forms"),
            new GroupStartIndexNamePair (HISUIAN_GROUP_KEY, "Hisuian"),
            new GroupStartIndexNamePair (GENDER_DIFF_GROUP_KEY, "Gender Diff."),
            new GroupStartIndexNamePair (PALDEAN_GROUP_KEY, "Paldean"),
        };
        
        public class GroupStartIndexNamePair
        {
            public string groupKey;
            public string groupName;
            public GroupStartIndexNamePair(string _key, string _value)
            {
                groupKey = _key;
                groupName = _value;
            }
        }

        public AddSubtractPair GetSpeciesIndex()
        {
            return new AddSubtractPair() { part1 = SpeciesIndexValue1, pairOperator = SpeciesIndexValueOp, part2 = SpeciesIndexValue2 };
        }
        public bool hasBaseExpComment { get; set; } = false;

        public void SetAddSubtractPairIndexValue(AddSubtractPair pair)
        {
            if (pair == null)
            {
                if (Key == "SPECIES_MIMEJR")
                {
                    SpeciesIndexValue1 = "439";
                }
                return;
            }
            SpeciesIndexValue1 = pair.part1;
            SpeciesIndexValue2 = pair.part2;
            SpeciesIndexValueOp = pair.pairOperator;
        }
        public string KeyBasic => Key.Replace("SPECIES_", "");

        public string GetFirstPartOfKey()
        {
            int indexOfUnderscore = KeyBasic.IndexOf("_");
            if (indexOfUnderscore == -1)
            {
                return KeyBasic;
            }
            return KeyBasic.Substring(0, indexOfUnderscore) ;
        }

        public string GetLastPartOfKey()
        {
            int indexOfUnderscore = KeyBasic.LastIndexOf("_");
            if (indexOfUnderscore == -1)
            {
                return KeyBasic;
            }
            return KeyBasic.Substring(indexOfUnderscore, KeyBasic.Length - indexOfUnderscore).Replace("_", "");
        }

        public static int GenOne = 151;
        public static int GenTwo = 251;
        public static int GenThree = 386;
        public static int GenFour = 493;
        public static int GenFive = 699;
        public enum PokemonGeneration
        {
            GEN_ONE,
            GEN_TWO,
            GEN_THREE,
            GEN_FOUR,
            GEN_FIVE,
            GEN_SIX_PLUS,
        }

        public int? GetCanonicalMonNumber()
        {
            if (GetGroupKey() != CANONICAL_GROUP_KEY)
            {
                return null;
            }
            return SpeciesIndexValue1.ToInt();

        }

        public PokemonGeneration? GetPokemonGenerationForCanonicalMon()
        {
            int? canonicalNum = GetCanonicalMonNumber();
            if (!canonicalNum.HasValue)
            {
                return null;
            }
            if (canonicalNum.Value > GenFive)
            {
                return PokemonGeneration.GEN_SIX_PLUS;
            }
            else if (canonicalNum.Value > GenFour)
            {
                return PokemonGeneration.GEN_FIVE;
            }
            else if (canonicalNum.Value > GenThree)
            {
                return PokemonGeneration.GEN_FOUR;
            }
            else if (canonicalNum.Value > GenTwo)
            {
                return PokemonGeneration.GEN_THREE;
            }else if(canonicalNum.Value > GenOne)
            {
                return PokemonGeneration.GEN_TWO;
            }
            return PokemonGeneration.GEN_ONE;
        }
    }
}
