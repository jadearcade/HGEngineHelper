using HGEngineHelper.Code.HGECodeHelper.Settings;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace HGEngineHelper.Project
{
    public class OpenProjectController
    {
        public bool OpenProjectFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            try
            {
                return OpenProject(ReadProjectInfoFromJsonFile(fileName));
            }
            catch (FileNotFoundException ex)
            {
                return false;
            }
        }

        public HGEngineHelperProjectInfo ReadProjectInfoFromJsonFile(string fileName)
        {
            string jsonStringFromFile = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<HGEngineHelperProjectInfo>(jsonStringFromFile);
        }

        public void OpenOpenProjectDialog(Window parentWindow)
        {
            var openFileDialog = (new OpenFileDialog());
            openFileDialog.DefaultDirectory = (new ConfigurationController()).GetDefaultProjectsFolderLocation();
            string filter = "HGEHelper Project files (*" + ConfigurationController.ProjectExtension
                + ")|*" + ConfigurationController.ProjectExtension + "|All files (*.*)|*.*";
            openFileDialog.Filter = filter;
            openFileDialog.Title = "Open HGEHelper Project";
            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue ? !result.Value : false)
            {
                return;
            }
            bool openProjectResult = (new OpenProjectController()).OpenProjectFromFile(openFileDialog.FileName);
            if (openProjectResult)
            {
                (new MainWindow()).Show();
                parentWindow.Close();
            }
        }

        public bool OpenProject(HGEngineHelperProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                return false;
            }
            App.CurrentApp.SetCurrentProject(projectInfo);
            return true;
        }
    }
}
