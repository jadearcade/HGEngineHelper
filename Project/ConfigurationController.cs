using HgEngineCsvConverter.Code;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.Project
{
    public class ConfigurationController
    {
        public string GetDefaultFileLocation()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public static string ProjectExtension = ".hgeh";

        public string GetDefaultProjectsFolderLocation()
        {
            return ConfigurationManager.AppSettings["DefaultProjectsLocation"] ?? GetDefaultFileLocation();
        }

        public void SetDefaultProjectsFolderLocation(string newValue)
        {
            ConfigurationManager.AppSettings["DefaultProjectsLocation"] = newValue;
        }

        public bool GetAutoOpenLastProjectOnOpen()
        {
            return (ConfigurationManager.AppSettings["AutoOpenLastProjectOnOpen"] ?? "") == "1";
        }
    }
}
