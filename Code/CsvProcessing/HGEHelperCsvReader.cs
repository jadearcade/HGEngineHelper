using CsvHelper;
using CsvHelper.Configuration;
using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper.Settings;
using HGEngineHelper.Code.HGEngineImport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HgEngineCsvConverter.Code.HgEngineEvoDataParser;

namespace HGEngineHelper.Code.CsvProcessing
{
    public class HGEHelperCsvReader
    {
        public List<MonInfo> GetPokemonForProject(HGEngineHelperProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                return new List<MonInfo>();
            }
            return GetPokemon(projectInfo.dataFolder + CsvFileNames.PokemonCsvFileName);
        }

        public List<MonInfo> GetPokemon(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<MonInfo>().ToList() ;
            }
        }

        public List<MonEvo> GetEvosForProject(HGEngineHelperProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                return new List<MonEvo>();
            }
            return GetEvos(projectInfo.dataFolder + CsvFileNames.EvolutionsCsvFileName);
        }

        public List<MonEvo> GetEvos(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<MonEvo>().ToList();
            }
        }

        public List<FormesForSpeciesInfoRow> GetFormesForProject(HGEngineHelperProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                return new List<FormesForSpeciesInfoRow>();
            }
            return GetFormesFromCsv(projectInfo.dataFolder + CsvFileNames.SpeciesFormMapping);
        }

        public List<FormesForSpeciesInfoRow> GetFormesFromCsv(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<FormesForSpeciesInfoRow>().ToList();
            }
        }
    }
}
