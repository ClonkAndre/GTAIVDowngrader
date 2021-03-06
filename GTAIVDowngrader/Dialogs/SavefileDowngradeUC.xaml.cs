using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class SavefileDowngradeUC : UserControl {

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.taskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            instance.NextStep();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            switch (uri.Host) {
                case "gotodocumentsfolder":
                    string documentsFolder = string.Format("{0}\\Rockstar Games\\GTA IV\\savegames", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    if (Directory.Exists(documentsFolder)) {
                        Process.Start(documentsFolder);
                    }
                    else {
                        Directory.CreateDirectory(documentsFolder);
                        Process.Start(documentsFolder);
                    }
                    break;
                case "gotolocalappdatafolder":
                    string localAppDataFolder = string.Format("{0}\\Rockstar Games\\GTA IV\\savegames\\user_ee000000deadc0de", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                    if (Directory.Exists(localAppDataFolder)) {
                        Process.Start(localAppDataFolder);
                    }
                    else {
                        Directory.CreateDirectory(localAppDataFolder);
                        Process.Start(localAppDataFolder);
                    }
                    break;
                default:
                    switch (MessageBox.Show(string.Format("This link takes you to {0} ({1}). Do you want to go there?", uri.Host, uri.ToString()), "Open link?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                        case MessageBoxResult.Yes:
                            Process.Start(uri.ToString());
                            break;
                    }
                    break;
            }
        }

    }
}
