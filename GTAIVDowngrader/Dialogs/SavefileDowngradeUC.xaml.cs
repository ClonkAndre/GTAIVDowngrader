using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SavefileDowngradeUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SavefileDowngradeUC()
        {
            InitializeComponent();
        }
        public SavefileDowngradeUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_SkipButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep(2);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.SkipButtonClicked -= Instance_SkipButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.SkipButtonClicked += Instance_SkipButtonClicked;

            instance.ChangeActionButtonVisiblity(true, false, true, true);

            // Reset progressbar stuff
            instance.taskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            instance.GetMainProgressBar().Value = 0;

            if (Core.Is420())
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char2.png", UriKind.Relative));
            if (Core.IsPrideMonth)
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char9.png", UriKind.Relative));

            if (Core.IsInOfflineMode)
            {
                instance.ChangeActionButtonEnabledState(true, true, true, false);
                NonOfflineModeText.Visibility = Visibility.Collapsed;
                OfflineModeText.Visibility = Visibility.Visible;
            }
            else
            {
                instance.ChangeActionButtonEnabledState(true, true, true, true);
                NonOfflineModeText.Visibility = Visibility.Visible;
                OfflineModeText.Visibility = Visibility.Collapsed;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            switch (uri.Host) {
                case "gotodocumentsfolder":
                    string documentsFolder = string.Format("{0}\\Rockstar Games\\GTA IV\\savegames", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    if (Directory.Exists(documentsFolder))
                    {
                        Process.Start(documentsFolder);
                    }
                    else
                    {
                        Directory.CreateDirectory(documentsFolder);
                        Process.Start(documentsFolder);
                    }
                    break;
                case "gotolocalappdatafolder":
                    string localAppDataFolder = string.Format("{0}\\Rockstar Games\\GTA IV\\savegames\\user_ee000000deadc0de", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
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
