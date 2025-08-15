using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.HGEConverter.Exporter
{
    public class HgeMonExporter
    {
        public void CreateMonDataFiles(MonInfo pokemonInfo)
        {
            //When parsing HGE code files, record "non-data" sections by key
            List<string> fileNames = new List<string>() {
                HgEnginePaths.MonDataFileName ,
            HgEnginePaths.BabyMonDataFileName,
            HgEnginePaths.BaseExperienceFileName,
            HgEnginePaths.HiddenAbilityFileName,
            HgEnginePaths.SpeciesFileName,
            HgEnginePaths.SpeciesIncFileName,};
            
            //BaseExperienceTable.c and HiddenAbilityTable.c are simple dictionaries

            //species.inc and species.h need special
            //starts with MAX_ or ends with _START
            //Record these references, want to only support 3 spots:
            //Fakemons, Megas, and Misc Forms
        }
    }
}
