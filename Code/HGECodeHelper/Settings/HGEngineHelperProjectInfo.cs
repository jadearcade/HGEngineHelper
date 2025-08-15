using HGEngineHelper.Project;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.Code.HGECodeHelper.Settings
{
    public class HGEngineHelperProjectInfo
    {
        public string name;
        public string hgEnginePath;
        public DateTime? lastSyncedFromHgEngine;
        public DateTime? lastSyncedToHgEngine;
        public bool fairyTypeImplemented = true;
        public string pathToProjectFolder;
        public string projectFileName => pathToProjectFolder + "\\" + name + ConfigurationController.ProjectExtension;
        public string dataFolder => pathToProjectFolder + "\\data\\";
        public string overworldsFolder => pathToProjectFolder + "\\overworlds\\";
        public string spritesFolder => pathToProjectFolder + "\\sprites\\";
        public string hgeCodeInfoFolder => pathToProjectFolder + "\\hgeCodeInfo\\";
        public string hgeTestOutputFolder => pathToProjectFolder + "\\hgeTestOutput\\";
        public void Save()
        {
            File.WriteAllText(projectFileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
