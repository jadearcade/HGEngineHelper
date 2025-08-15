using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.Code.HGECodeHelper
{
    //Store json(?) files for each of these when parsing in, need a "key" per file name. This will help create the file when parsing out
    //  Store list of "CodeSections" with enum for their placement in the file and lists per enum (example START, END, and special ones per file type, perhaps simply a list)
    //  When parsing in files that have "refs" (ex: (ITEM_MEGA_STONES_START + 1)) need to store them in csvs as "ConstantFilePositionDescription"
    //  and have a way to generate these for each "type" that has these refs (species, mons, items). 
    //  Store ConstantFilePositionDescriptions in order that they appear
    //  I think I will note what version of HGE the Helper is up to, and anything past that might not be supported (but likely will work because I dont think these core files will be changed much)
    //  Many of these will need custom parsers anyways
    //      Consider how this might change if need to merge from hg-engine...Can user read just the CodeSections and Re-load the "refs" for each code file?
    //      Certain new CodeSections in unexpected places could cause problems, but if the CodeSection types are just everything between where we're writing, it should work and players can re-import
    //35 different files currently (split into folders?)
    public static class HgEnginePaths
    {
        //Pokemon
        public static string MonDataFileName = "armips/data/mondata.s";//Model
        public static string BabyMonDataFileName = "armips/data/babymons.s";//Model (Single Line)
        public static string BaseExperienceFileName = "data/BaseExperienceTable.c";//Dictionary
        public static string HiddenAbilityFileName = "data/HiddenAbilityTable.c";//Dictionary
        public static string SpeciesFileName = "include/constants/species.h";//Constant file w/refs
        public static string SpeciesIncFileName = "asm/include/species.inc";//Constant file w/refs (Same as species.h so only for exporting to hge)
        //Evos
        public static string EvoDataFileName = "armips/data/evodata.s";//Basic List w/header row with terminate
                                                                       //Formes
        public static string FormReversionMappingFileName = "data/FormReversionMapping.c";//Out only 
        public static string FormToSpeciesMappingFileName = "data/FormToSpeciesMapping.c";//Out only 
        public static string SpeciesFormFileName = "data/PokeFormDataTbl.c";//"Complex" Dictionary
        public static string MegaFormTableFileName = "src/battle/mega.c";//struct list (losts of code, but the one part that needs to change is simple)

        //Learnset - This may end up changing into a single JSON file with the learnset overhaul. Hold off for now
        public static string TutorDataFileName = "armips/data/tutordata.txt";//Basic List w/ header row (needs investigation for BAD_EGG stuff)
        public static string TmLearnsetFileName = "armips/data/tmlearnset.txt";//Basic List w/header row (needs investigating regarding item icon/palette)
        public static string EggMovesFileName = "armips/data/eggmoves.s";//Basic List w/header row
        public static string LevelUpLearnsetFileName = "armips/data/levelupdata.s";//Basic List w/header row and terminate
        //Sprites
        public static string SpriteOffsetFileName = "armips/data/spriteOffsets.s";//Basic Lists (inline model)
        public static string HeightTableFileName = "armips/data/heightTable.s";//Basic Lists (inline model)
        //Overworlds
        public static string SpeciesToOwGfxTableFileName = "data/SpeciesToOWGfx.c";//Basic Dictionary, look up snover split
        public static string OverworldTableFileName = "src/field/overworld_table.c";//Complex lists, lots of code, doable though
        public static string MonOverworldsFileName = "armips/data/monoverworlds.s";//Byte tables - GDimorphismTable, NumOfOwFormsPerMon, overworldTableData list
        public static string IconPaletteTableFileName = "armips/data/iconpalettetable.s";//Byte table
        //Dex
        public static string RegionalDexFileName = "armips/data/regionaldex.s";//Halfword table
        public static string DexWeightFileName = "armips/data/pokedex/weight.s";//Word table
        //public static string DexDataFileName = "armips/data/pokedex/pokedexdata.s";
        public static string DexAreaDataFileName = "armips/data/pokedex/areadata.s";//Low priority, VERY complicated file, basically model-based but requires understanding
        //Dex - Gender Specific
        public static string DexMonScaleDivisorFileNameFormat = "armips/data/pokedex/{0}monscaledivisor.s";//Halfword table
        public static string DexMonPosYFileNameFormat = "armips/data/pokedex/{0}monypos.s";//Halfword table
        public static string DexTrainerScaleDivisorFileNameFormat = "armips/data/pokedex/{0}trainerscaledivisor.s";//Halfword table
        public static string DexTrainerPosYFileNameFormat = "armips/data/pokedex/{0}trainerypos.s";//Halfword table
        //Dex - Sortlists - armips/data/pokedex/sortlists/*
        //?????  Gen 6+ commented ????
        //Trainers
        public static string TrainersFileName = "armips/data/trainers/trainers.s";//Models with header model
        public static string TrainersTextFileName = "armips/data/trainers/trainertext.s";//CONFUSING!!! Needs more research, should be doable, look into python script
        //Encounters
        public static string EncountersTableFileName = "armips/data/encounters.s";//Header Model with lists, needs custom parsing
        //Abilities
        public static string AbilityHeadersFileName = "include/constants/ability.h";//Constants file (no refs)
        public static string AbilityNameFileName = "data/text/720.txt";//text archive (easy)
        public static string AbilityDescriptionFileName = "data/text/722.txt";//text archive (easy)
        //Items
        public static string ItemDataFileName = "data/itemdata/itemdata.c";//Dictionary with model and formula keys (explorer kit thingy)
        public static string ItemHeaderFileName = "include/constants/item.h";//Constants file w/refs and formulas
        public static string ItemDescriptionTextArchiveFileName = "data/text/221.txt";//text archive (easy)
        public static string ItemNameTextArchiveFileName = "data/text/222.txt";//text archive (easy)
        //Moves
        public static string MovesFileName = "armips/data/moves.s";//Basic model, terminate data line, move description after
    }
}
