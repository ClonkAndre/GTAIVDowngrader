using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GTAIVDowngrader.Dialogs {
    public partial class ConfirmUC : UserControl {

        #region Variables
        private MainWindow instance;

        private bool temp;
        #endregion

        #region Methods
        private void LogInfos()
        {
            // Application Information
            if (MainFunctions.gotStartedWithValidCommandLineArgs) {
                MainFunctions.AddLogItem(LogType.Info, "- - - Application Information - - -");
                MainFunctions.AddLogItem(LogType.Info, string.Format("Downgrader got started with commandline argument to path: {0}", MainFunctions.commandLineArgPath));
            }

            // MD5 Check
            MainFunctions.AddLogItem(LogType.Info, "- - - MD5 Check - - -");
            MainFunctions.AddLogItem(LogType.Info, string.Format("MD5 Hash Created: {0}", MainFunctions.downgradingInfo.ReceivedMD5Hash));

            string md5HashFound = MainFunctions.downgradingInfo.RelatedMD5Hash;
            MainFunctions.AddLogItem(LogType.Info, string.Format("MD5 Hash   Found: {0}", string.IsNullOrEmpty(md5HashFound) ? 
                "Couldn't find any MD5 Hash that relates to the created MD5 Hash. This might mean that the selected GTAIV.exe is not version 1.2.0.43." : md5HashFound));

            MainFunctions.AddLogItem(LogType.Info, string.Format("If both MD5 Hashes don't match: The selected GTA IV Installation might be modified (Contains mods)."));

            // Downgrading Informations
            MainFunctions.AddLogItem(LogType.Info, "- - - Downgrading Informations - - -");
            MainFunctions.AddLogItem(LogType.Info, string.Format("Selected downgrading version:     {0}", MainFunctions.downgradingInfo.DowngradeTo.ToString()));
            MainFunctions.AddLogItem(LogType.Info, string.Format("Configure for GFWL:               {0}", MainFunctions.downgradingInfo.ConfigureForGFWL.ToString()));
            MainFunctions.AddLogItem(LogType.Info, string.Format("Selected radio downgrader:        {0}", MainFunctions.downgradingInfo.SelectedRadioDowngrader.ToString()));
            MainFunctions.AddLogItem(LogType.Info, string.Format("Selected vladivostok type:        {0}", MainFunctions.downgradingInfo.SelectedVladivostokType.ToString()));
            MainFunctions.AddLogItem(LogType.Info, string.Format("Install No EFLC Music in IV Fix:  {0}", MainFunctions.downgradingInfo.InstallNoEFLCMusicInIVFix.ToString()));
            MainFunctions.AddLogItem(LogType.Info, string.Format("Install Prerequisites:            {0}", MainFunctions.downgradingInfo.InstallPrerequisites.ToString()));
            MainFunctions.AddLogItem(LogType.Info, string.Format("Create Backup:                    {0}", MakeABackupForMeCheckbox.IsChecked.Value.ToString()));
            MainFunctions.AddLogItem(LogType.Info, string.Format("Create Backup in zip file:        {0}", CreateBackupInZIPFileCheckBox.IsChecked.Value.ToString()));
            MainFunctions.AddLogItem(LogType.Info, "- - - Starting Downgrading Process - - -");
        }
        #endregion

        #region Functions
        public bool CheckBackupDirectory(string bPath)
        {
            if (string.IsNullOrWhiteSpace(bPath)) {
                BackupLocationStatusLabel.Text = "Please select a backup path";
                BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
            }
            else {
                if (Directory.Exists(bPath)) {
                    if (CreateBackupInZIPFileCheckBox.IsChecked.Value) {
                        NextButton.IsEnabled = true;
                        BackupLocationStatusLabel.Text = "Directory is valid!";
                        BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\checkCircleWhite.png", UriKind.RelativeOrAbsolute));
                        return true;
                    }
                    else {
                        if (Directory.GetFiles(bPath).Length <= 0) {
                            NextButton.IsEnabled = true;
                            BackupLocationStatusLabel.Text = "Directory is valid!";
                            BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\checkCircleWhite.png", UriKind.RelativeOrAbsolute));
                            return true;
                        }
                        else {
                            BackupLocationStatusLabel.Text = "The directory you've selected is not empty!";
                            BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
                        }
                    }
                }
                else {
                    BackupLocationStatusLabel.Text = "Directory does not exists!";
                    BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
                }
            }
            NextButton.IsEnabled = false;
            return false;
        }
        public bool CheckIfOldFoldersExists()
        {
            bool pluginsFolderExists = false, scriptsFolderExists = false;

            string pluginsFolder = string.Format("{0}\\plugins", MainFunctions.downgradingInfo.IVWorkingDirectoy);
            if (Directory.Exists(pluginsFolder)) pluginsFolderExists = true;

            string scriptsFolder = string.Format("{0}\\scripts", MainFunctions.downgradingInfo.IVWorkingDirectoy);
            if (Directory.Exists(scriptsFolder)) scriptsFolderExists = true;

            return pluginsFolderExists || scriptsFolderExists;
        }
        #endregion

        #region Constructor
        public ConfirmUC()
        {
            InitializeComponent();
        }
        public ConfirmUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // BottomGrid Colours
            if (MainFunctions.isPrideMonth) {
                if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                    BottomGrid.Background = "#B3000000".ToBrush();
                }
                else { // Use Rainbow Colours
                    BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
                }
            }

            // Calculate download size
            long size = 0;

            // Radio stuff
            switch (MainFunctions.downgradingInfo.SelectedVladivostokType) {
                case VladivostokTypes.New:
                    size += MainFunctions.GetDowngradeFileSizeByFileName("WithNewVladivostok.zip");
                    break;
                case VladivostokTypes.Old:
                    size += MainFunctions.GetDowngradeFileSizeByFileName("WithoutNewVladivostok.zip");
                    break;
            }
            if (MainFunctions.downgradingInfo.InstallNoEFLCMusicInIVFix) {
                size += MainFunctions.GetDowngradeFileSizeByFileName("EpisodeOnlyMusicCE.zip");
            }

            // Mods
            for (int i = 0; i < MainFunctions.downgradingInfo.SelectedMods.Count; i++) {
                size += MainFunctions.downgradingInfo.SelectedMods[i].FileSize;
            }

            // Prerequisites
            if (MainFunctions.downgradingInfo.InstallPrerequisites) {
                size += MainFunctions.GetDowngradeFileSizeByFileName("directx_Jun2010_redist.exe");
                size += MainFunctions.GetDowngradeFileSizeByFileName("vcredist_x86.exe");
            }
            if (MainFunctions.downgradingInfo.ConfigureForGFWL) {
                size += MainFunctions.GetDowngradeFileSizeByFileName("gfwlivesetup.exe");
                size += MainFunctions.GetDowngradeFileSizeByFileName("xliveredist.msi");

                if (Environment.Is64BitOperatingSystem) {
                    size += MainFunctions.GetDowngradeFileSizeByFileName("wllogin_64.msi");
                }
                else {
                    size += MainFunctions.GetDowngradeFileSizeByFileName("wllogin_32.msi");
                }
            }

            DownloadSizeInfoLabel.Text = string.Format("The downgrader will download {0} of data from the internet for this downgrade.", Helper.GetExactFileSize2(size));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            instance.PreviousStep();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfOldFoldersExists()) {
                switch (MessageBox.Show("We've noticed that you still have a plugins or scripts folder inside of the GTA IV directory that should be downgraded. " +
                    "If you don't want to loose them, now it's the time to make a backup of them! Just press No, and make a backup of them. " +
                    "If you don't want them anymore, you can press Yes, this will start the downgrading process, which will delete them.", "Confirm deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning)) {
                    case MessageBoxResult.Yes: break;
                    case MessageBoxResult.No: return;
                }
            }

            if (MakeABackupForMeCheckbox.IsChecked.Value) {
                if (CheckBackupDirectory(BackupLocationTextbox.Text)) {
                    MainFunctions.downgradingInfo.SetTargetBackupPath(BackupLocationTextbox.Text);
                    MainFunctions.downgradingInfo.SetCreateBackupInZipFile(CreateBackupInZIPFileCheckBox.IsChecked.Value);
                    LogInfos();
                    instance.NextStep();
                }
            }
            else {
                LogInfos();
                instance.NextStep();
            }
        }

        private void MakeABackupForMeCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (MakeABackupForMeCheckbox.IsChecked.Value) {
                NextButton.IsEnabled = false;
                BackupStackPanel.Visibility = Visibility.Visible;
                CheckBackupDirectory(BackupLocationTextbox.Text);
            }
            else {
                NextButton.IsEnabled = true;
                BackupStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BrowseBackupLocationButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog("Select backup location"))  {
                ofd.IsFolderPicker = true;
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {
                    BackupLocationTextbox.Text = ofd.FileName;
                    CheckBackupDirectory(BackupLocationTextbox.Text);
                }
            }
        }
        private void CreateBackupInZIPFileCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (temp) {
                CheckBackupDirectory(BackupLocationTextbox.Text);
            }
            else {
                temp = true;
            }
        }
        private void BackupLocationTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckBackupDirectory(BackupLocationTextbox.Text);
        }
        private void BackupLocationTextbox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            CheckBackupDirectory(BackupLocationTextbox.Text);
        }

    }
}
