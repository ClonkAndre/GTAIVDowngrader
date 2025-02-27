﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Microsoft.WindowsAPICodePack.Dialogs;
using CCL;

using GTAIVDowngrader.Classes;
using GTAIVDowngrader.Classes.Json.Modification;

namespace GTAIVDowngrader.Dialogs
{
    public partial class ConfirmUC : UserControl
    {

        #region Variables
        private MainWindow instance;

        private bool temp;
        #endregion

        #region Methods
        private void CalculateDownloadSize()
        {
            long size = 0;

            // Game Downgrade
            size += Core.GetDowngradeFileSizeByFileName(string.Concat(DowngradingInfo.DowngradeTo, ".zip"));

            // Radio Downgrade
            if (DowngradingInfo.WasAnyRadioDowngraderSelected())
            {
                size += Core.GetDowngradeFileSizeByFileName(string.Concat(DowngradingInfo.SelectedRadioDowngrader, ".zip"));

                if (DowngradingInfo.IsSelectedRadioDowngraderSneeds())
                    size += Core.GetDowngradeFileSizeByFileName(string.Concat(DowngradingInfo.SelectedVladivostokType, ".zip"));

                if (DowngradingInfo.InstallNoEFLCMusicInIVFix)
                    size += Core.GetDowngradeFileSizeByFileName("NoEpisodeMusicInIV.zip");
            }

            // Mods
            ModDetails[] selectedMods = DowngradingInfo.SelectedMods.ToArray();
            for (int i = 0; i < selectedMods.Length; i++)
            {
                size += selectedMods[i].FileDetails.SizeInBytes;
            }

            // Optional Mod Stuff
            OptionalComponentInfo[] selectedOptionalComponentInfos = DowngradingInfo.SelectedOptionalComponents.ToArray();
            for (int i = 0; i < selectedOptionalComponentInfos.Length; i++)
            {
                size += selectedOptionalComponentInfos[i].FileDetails.SizeInBytes;
            }

            // Prerequisites
            if (DowngradingInfo.InstallPrerequisites)
            {
                size += Core.GetDowngradeFileSizeByFileName("directx_Jun2010_redist.exe");
                size += Core.GetDowngradeFileSizeByFileName("VisualCppRedist_AIO_x86_x64.exe");
            }
            if (DowngradingInfo.ConfigureForGFWL)
            {
                size += Core.GetDowngradeFileSizeByFileName("gfwlivesetup.exe");
                size += Core.GetDowngradeFileSizeByFileName("xliveredist.msi");
                size += Core.GetDowngradeFileSizeByFileName(Environment.Is64BitOperatingSystem ? "wllogin_64.msi" : "wllogin_32.msi");
            }

            DownloadSizeInfoLabel.Text = string.Format("The downgrader will download {0} of data from the internet for this downgrade.", FileHelper.GetExactFileSizeAdvanced(size));
        }
        #endregion

        #region Functions
        public bool CheckBackupDirectory(string bPath)
        {
            if (string.IsNullOrWhiteSpace(bPath))
            {
                BackupLocationStatusLabel.Text = "Please select a backup path";
                BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                if (Directory.Exists(bPath))
                {
                    if (CreateBackupInZIPFileCheckBox.IsChecked.Value)
                    {
                        instance.ChangeActionButtonEnabledState(true, true, true, true);
                        BackupLocationStatusLabel.Text = "Directory is valid!";
                        BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\checkCircleWhite.png", UriKind.RelativeOrAbsolute));
                        return true;
                    }
                    else
                    {
                        if (Directory.GetFiles(bPath).Length <= 0)
                        {
                            instance.ChangeActionButtonEnabledState(true, true, true, true);
                            BackupLocationStatusLabel.Text = "Directory is valid!";
                            BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\checkCircleWhite.png", UriKind.RelativeOrAbsolute));
                            return true;
                        }
                        else
                        {
                            BackupLocationStatusLabel.Text = "The directory you've selected is not empty!";
                            BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
                        }
                    }
                }
                else
                {
                    BackupLocationStatusLabel.Text = "Directory does not exists!";
                    BackupLocationStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
                }
            }

            instance.ChangeActionButtonEnabledState(true, true, true, false);
            return false;
        }
        public bool CheckIfOldFoldersExists()
        {
            bool pluginsFolderExists = false, scriptsFolderExists = false;

            string pluginsFolder = string.Format("{0}\\plugins", DowngradingInfo.IVWorkingDirectoy);
            if (Directory.Exists(pluginsFolder))
                pluginsFolderExists = true;

            string scriptsFolder = string.Format("{0}\\scripts", DowngradingInfo.IVWorkingDirectoy);
            if (Directory.Exists(scriptsFolder))
                scriptsFolderExists = true;

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

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            if (Core.IsInOfflineMode)
                instance.PreviousStep(DowngradingInfo.DowngradeTo == "1040" ? 4 : 2);
            else
                instance.PreviousStep();
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            if (CheckIfOldFoldersExists())
            {
                switch (MessageBox.Show("We've noticed that you still have a plugins and/or scripts folder inside of the GTA IV directory that should be downgraded. " +
                    "If you don't want to loose them, now it's the time to make a backup of them! Just press No, and start backing them up. " +
                    "If you don't want them anymore, you can press Yes, this will start the downgrading process, which will delete them.", "Confirm deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    case MessageBoxResult.Yes: break;
                    case MessageBoxResult.No: return;
                }
            }

            if (MakeABackupForMeCheckbox.IsChecked.Value)
            {
                if (CheckBackupDirectory(BackupLocationTextbox.Text))
                {
                    DowngradingInfo.SetTargetBackupPath(BackupLocationTextbox.Text);
                    DowngradingInfo.SetCreateBackupInZipFile(CreateBackupInZIPFileCheckBox.IsChecked.Value);
                    Core.LogDowngradingInfos();
                    instance.NextStep();
                }
            }
            else
            {
                Core.LogDowngradingInfos();
                instance.NextStep();
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButton.Content = "Next";

            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            instance.NextButton.Content = "Downgrade";

            if (Core.IsPrideMonth)
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char9.png", UriKind.Relative));

            // Hide and skip stuff
            if (Core.IsInOfflineMode)
            {
                DownloadSizeInfoLabel.Visibility = Visibility.Collapsed;
                return;
            }

            // Calculate download size
            CalculateDownloadSize();
        }

        private void MakeABackupForMeCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            DowngradingInfo.SetWantsToCreateBackup(MakeABackupForMeCheckbox.IsChecked.Value);

            if (MakeABackupForMeCheckbox.IsChecked.Value)
            {
                instance.ChangeActionButtonEnabledState(true, true, true, false);
                BackupStackPanel.Visibility = Visibility.Visible;
                CheckBackupDirectory(BackupLocationTextbox.Text);
            }
            else
            {
                instance.ChangeActionButtonEnabledState(true, true, true, true);
                BackupStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BrowseBackupLocationButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog("Select backup location"))
            {
                ofd.IsFolderPicker = true;
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    BackupLocationTextbox.Text = ofd.FileName;
                    CheckBackupDirectory(BackupLocationTextbox.Text);
                }
            }
        }
        private void CreateBackupInZIPFileCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (temp)
                CheckBackupDirectory(BackupLocationTextbox.Text);
            else
                temp = true;
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
