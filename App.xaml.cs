using HGEngineHelper.Code.HGECodeHelper.Settings;
using System.Configuration;
using System.Data;
using System.Windows;

namespace HGEngineHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private HGEngineHelperProjectInfo _ProjectInfo;
        public void SetCurrentProject(HGEngineHelperProjectInfo newValue)
        {
            _ProjectInfo = newValue;
        }

        public static void SaveProjectInfo()
        {
            ProjectInfo.Save();
        }

        public static HGEngineHelperProjectInfo ProjectInfo => CurrentApp?._ProjectInfo;
        public static App CurrentApp => (App)App.Current;
    }

}
