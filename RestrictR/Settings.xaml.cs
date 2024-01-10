using Microsoft.UI.Xaml.Controls;

namespace RestrictR
{
    // This page renders the help page
    // Documentation function ID - HELP_VIEW
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }


        public string Version
        {
            get
            {
                var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

    }
}
