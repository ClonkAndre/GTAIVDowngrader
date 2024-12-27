using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SavefileDowngradeStep3UC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SavefileDowngradeStep3UC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        public SavefileDowngradeStep3UC()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep(1);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            if (Core.Is420())
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char2.png", UriKind.Relative));
            if (Core.IsPrideMonth)
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char9.png", UriKind.Relative));

            // Change location text based on downgrading options
            if (Core.CurrentDowngradingInfo.ConfigureForGFWL) {
                gfwlLocationTextBlock.Visibility = Visibility.Visible;
                xliveLocationTextBlock.Visibility = Visibility.Collapsed;
            }
            else {
                gfwlLocationTextBlock.Visibility = Visibility.Collapsed;
                xliveLocationTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            switch (uri.Host) {
                case "gotologsfolder":
                    string logsFolder = ".\\Data\\Logs";
                    if (Directory.Exists(logsFolder))
                    {
                        Process.Start(logsFolder);
                    }
                    else
                    {
                        Directory.CreateDirectory(logsFolder);
                        Process.Start(logsFolder);
                    }
                    break;
                case "gotosavegamesoutputfolder":
                    string savegamesFolder = ".\\Data\\Savegames";
                    if (Directory.Exists(savegamesFolder))
                    {
                        Process.Start(savegamesFolder);
                    }
                    else
                    {
                        Directory.CreateDirectory(savegamesFolder);
                        Process.Start(savegamesFolder);
                    }
                    break;
                case "gotolocalappdataxlivefolder":
                    string localAppDataXliveFolder = string.Format("{0}\\Rockstar Games\\GTA IV\\savegames\\user_ee000000deadc0de", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                    if (Directory.Exists(localAppDataXliveFolder))
                    {
                        Process.Start(localAppDataXliveFolder);
                    }
                    else
                    {
                        Directory.CreateDirectory(localAppDataXliveFolder);
                        Process.Start(localAppDataXliveFolder);
                    }
                    break;
                case "gotolocalappdatafolder":
                    string localAppDataFolder = string.Format("{0}\\Rockstar Games\\GTA IV\\savegames", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                    if (Directory.Exists(localAppDataFolder))
                    {
                        Process.Start(localAppDataFolder);
                    }
                    else
                    {
                        Directory.CreateDirectory(localAppDataFolder);
                        Process.Start(localAppDataFolder);
                    }
                    break;
                default:
                    Core.AskUserToOpenURL(e.Uri);
                    break;
            }
        }

    }
}
