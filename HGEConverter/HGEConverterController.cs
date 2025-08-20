using HgEngineCsvConverter;
using HgEngineCsvConverter.Code;
using HGEngineHelper.Code.CsvProcessing;
using HGEngineHelper.Code.HGECodeHelper.Settings;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.HGEConverter
{
    public class HGEConverterController
    {
        public sealed record ProgressInfo (
            double Value, string Message
        );

        public interface HGEImportProgress : IProgress<ProgressInfo>
        {

        }

        public async Task UpdateProjectDataFromHgEngine(HGEImportProgress progress)
        {
            CancellationToken token = new CancellationToken(false);
            BoolResultWithMessage result = ReadAllHgEngineData(progress, token);
            if (token.IsCancellationRequested || !result.successful)
            {
                return;
            }
            try
            {
                CopyAllPokemonSprites(progress, token);
            }
            catch(Exception E)
            {
                progress.Report(new ProgressInfo(0, "Sprite Copy Error: " + E.Message));
                return;
            }
            
            if (token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                CopyAllPokemonOverworlds(progress, token);
            }
            catch(Exception E)
            {
                progress.Report(new ProgressInfo(0, "Overworld Copy Error: " + E.Message));
                return;
            }
            if (token.IsCancellationRequested)
            {
                return;
            }

            App.ProjectInfo.lastSyncedFromHgEngine = System.DateTime.Now.ToUniversalTime();
            App.SaveProjectInfo();
            progress.Report(new ProgressInfo(100, "Complete"));
        }

        public static string HgEngineGraphicsBaseSubpath = "\\data\\graphics";
        public static string HgEngineGraphicsSpritesSubpath = HgEngineGraphicsBaseSubpath + "\\sprites";
        public static string HgEngineGraphicsOverworldsSubpath = HgEngineGraphicsBaseSubpath + "\\overworlds";

        private void CopyAllPokemonSprites(HGEImportProgress progress, CancellationToken token)
        {
            string copyFromPath = App.ProjectInfo.hgEnginePath + HgEngineGraphicsSpritesSubpath;
            string copyToPath = App.ProjectInfo.spritesFolder;
            var directories = Directory.GetDirectories(copyFromPath);
            var progressAmountPerCopy = (double)ProgressValueGraphicsSprites / (double)directories.Length;
            int directoriesCopied  = 0;
            Directory.CreateDirectory(copyToPath);
            foreach(var spriteDirectoryToCopy in directories)
            {
                if (spriteDirectoryToCopy == null)
                {
                    continue;
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
                string directoryName = spriteDirectoryToCopy.Replace(Path.GetDirectoryName(spriteDirectoryToCopy) + "\\", "");
                progress.Report(new ProgressInfo(progressAmountPerCopy * directoriesCopied + ProgressValueGraphicsSpritesMin
                    , "Copying sprite folder: " + directoryName));

                //Directory.CreateDirectory(copyToPath + "\\" + directoryName);
                FileSystem.CopyDirectory(spriteDirectoryToCopy, copyToPath + "\\" + directoryName, true);
                
                directoriesCopied++;
            }
        }

        private void CopyAllPokemonOverworlds(HGEImportProgress progress, CancellationToken token)
        {
            string copyFromPath = App.ProjectInfo.hgEnginePath + HgEngineGraphicsOverworldsSubpath;
            string copyToPath = App.ProjectInfo.overworldsFolder;
            var filesToCopy = Directory.GetFiles(copyFromPath);
            var progressAmountPerCopy = (double)ProgressValueGraphicsOverworlds / (double)filesToCopy.Length;
            int filesCopied = 0;
            Directory.CreateDirectory(copyToPath);
            foreach (var fileToCopy in filesToCopy)
            {
                if (fileToCopy == null)
                {
                    continue;
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
                string fileName = Path.GetFileName(fileToCopy);
                progress.Report(new ProgressInfo(progressAmountPerCopy * filesCopied + ProgressValueGraphicsOverworldsMin
                    , "Copying overworld file: " + fileName));

                //Directory.CreateDirectory(copyToPath + "\\" + directoryName);
                File.Copy(fileToCopy, copyToPath + "\\" + fileName, true);

                filesCopied++;
            }
        }

        private int ProgressValueMinCsvs = 0;
        private int ProgressValueCsvs = 20;
        private int ProgressValueMaxCsvs => ProgressValueMinCsvs + ProgressValueCsvs;
        private int ProgressValueGraphicsSpritesMin => ProgressValueMaxCsvs;
        private int ProgressValueGraphicsSprites = 40;
        private int ProgressValueGraphicsSpritesMax => ProgressValueGraphicsSpritesMin + ProgressValueGraphicsSprites;
        private int ProgressValueGraphicsOverworldsMin => ProgressValueGraphicsSpritesMax;
        private int ProgressValueGraphicsOverworlds = 40;
        private int ProgressValueGraphicsOverworldsMax => ProgressValueGraphicsOverworldsMin + ProgressValueGraphicsOverworlds;


        public BoolResultWithMessage ReadAllHgEngineData(IProgress<ProgressInfo> progressInfo, CancellationToken token)
        {
            return new BoolResultWithMessage(true, "");
            if (token.IsCancellationRequested)
            {
                return new BoolResultWithMessage(false, "Cancelled");
            }
            BoolResultWithMessage result = ReadAllHgEngineDataAndOutputToCsv(App.ProjectInfo.hgEnginePath, App.ProjectInfo.dataFolder, progressInfo);
            if (!result.successful)
            {
                progressInfo.Report(new ProgressInfo(0, result.message));
            }
            return result;
        }

        public BoolResultWithMessage ReadAllHgEngineDataAndOutputToCsv(string basePath, string outputDirectory, IProgress<ProgressInfo> progressInfo)
        {
            Directory.CreateDirectory(outputDirectory);

            List<BoolResultWithMessage> results = new List<BoolResultWithMessage>();
            progressInfo.Report(new ProgressInfo((0 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading mon data"));
            results.Add((new HgEngineMonDataCsvExporter()).CreateJoinedMonDataCsv(basePath, outputDirectory + CsvFileNames.PokemonCsvFileName));
            
            progressInfo.Report(new ProgressInfo((1 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading learnsets"));
            results.Add((new HgEngineLearnsetDataParser()).CreateLearnsetRelatedCsvs(basePath, outputDirectory, outputDirectory + "tms.csv", outputDirectory + "tutorMoves.csv"));
            
            progressInfo.Report(new ProgressInfo((2 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading moves"));
            results.Add((new HgEngineMoveDataParser()).CreateMoveDataCsv(basePath, outputDirectory + "moves.csv"));

            progressInfo.Report(new ProgressInfo((3 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading forms"));
            results.Add((new HgEngineFormDataParser()).CreateSpeciesFormTableCsv(basePath, outputDirectory + "speciesFormMapping.csv"));

            progressInfo.Report(new ProgressInfo((4 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading megas"));
            results.Add((new HgEngineFormDataParser()).CreateMegaFormTableCsv(basePath, outputDirectory + "megaforms.csv"));

            progressInfo.Report(new ProgressInfo((5 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading evolutions"));
            results.Add((new HgEngineEvoDataParser()).CreateEvoDataFormTableCsv(basePath, outputDirectory + "evolutions.csv"));

            progressInfo.Report(new ProgressInfo((6 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading item data"));
            results.Add((new HgEngineItemDataParser()).OutputItemDataToCsv(basePath, outputDirectory + "itemdata.csv"));

            progressInfo.Report(new ProgressInfo((7 * ProgressValueCsvs / 8) + ProgressValueMinCsvs, "Reading ability data"));
            results.Add(new HgEngineAbilityDataParser().OutputAbilityDataToCsv(basePath, outputDirectory + "ability.csv"));
            if (results.Any(i => !i.successful))
            {
                return new BoolResultWithMessage(false, String.Join("; ", results.Where(i => i.successful)));
            }
            return new BoolResultWithMessage(true, "");
        }
    }
}
