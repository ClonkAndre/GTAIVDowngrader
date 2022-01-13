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
            if (MakeABackupForMeCheckbox.IsChecked.Value) {
                if (CheckBackupDirectory(BackupLocationTextbox.Text)) {
                    instance.NextStep();
                }
            }
            else {
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
