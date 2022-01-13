using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTAIVDowngrader.Dialogs {
    public partial class S3_SelectRadioDwngrd : UserControl {

        #region Variables
        private MainWindow instance;

        public RadioDowngrader selectedRadioDowngrader = RadioDowngrader.None;
        public bool InstallNoEFLCMusicInIVFix;
        #endregion

        #region Constructor
        public S3_SelectRadioDwngrd()
        {
            InitializeComponent();
        }
        public S3_SelectRadioDwngrd(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (instance.s2_SelectDwngrdVersion.selectedVersion == GameVersion.v1040) {
                SkipButton.IsEnabled = false;
                NoEFLCMusicInIVCheckbox.IsChecked = false;
                NoEFLCMusicInIVCheckbox.IsEnabled = false;
            }
            else {
                SkipButton.IsEnabled = true;
            }

            if (!File.Exists(".\\Downgrader\\Files\\Radio\\SneedsRadioDowngrader.zip")) {
                SneedsRadioDowngraderCheckbox.IsEnabled = false;
            }
            else {
                SneedsRadioDowngraderCheckbox.IsEnabled = true;
            }
            if (!File.Exists(".\\Downgrader\\Files\\Radio\\LegacyRadioDowngrader.zip")) {
                LegacyRadioDowngraderCheckbox.IsEnabled = false;
            }
            else {
                LegacyRadioDowngraderCheckbox.IsEnabled = true;
            }
            if (!File.Exists(".\\Downgrader\\Files\\Radio\\EpisodeOnlyMusicCE.zip")) {
                NoEFLCMusicInIVCheckbox.IsEnabled = false;
                NoEFLCMusicInIVCheckbox.IsChecked = false;
            }
            else {
                if (SneedsRadioDowngraderCheckbox.IsEnabled || LegacyRadioDowngraderCheckbox.IsEnabled) {
                    if (SneedsRadioDowngraderCheckbox.IsEnabled) {
                        //NoEFLCMusicInIVCheckbox.IsEnabled = SneedsRadioDowngraderCheckbox.IsChecked.Value;
                        if (SneedsRadioDowngraderCheckbox.IsChecked.Value) {
                            NextButton.IsEnabled = true;
                            NoEFLCMusicInIVCheckbox.IsEnabled = true;
                        }
                        else {
                            if (!(LegacyRadioDowngraderCheckbox.IsEnabled && LegacyRadioDowngraderCheckbox.IsChecked.Value)) NoEFLCMusicInIVCheckbox.IsEnabled = false;
                        }
                    }
                    else {
                        if (SneedsRadioDowngraderCheckbox.IsChecked.Value) {
                            NextButton.IsEnabled = false;
                            NoEFLCMusicInIVCheckbox.IsEnabled = false;
                        }
                        else {
                            if (!(LegacyRadioDowngraderCheckbox.IsEnabled && LegacyRadioDowngraderCheckbox.IsChecked.Value)) NoEFLCMusicInIVCheckbox.IsEnabled = false;
                        }
                    }
                    if (LegacyRadioDowngraderCheckbox.IsEnabled) {
                        //NoEFLCMusicInIVCheckbox.IsEnabled = LegacyRadioDowngraderCheckbox.IsChecked.Value;
                        if (LegacyRadioDowngraderCheckbox.IsChecked.Value) {
                            NextButton.IsEnabled = true;
                            NoEFLCMusicInIVCheckbox.IsEnabled = true;
                        }
                        else {
                            if (!(SneedsRadioDowngraderCheckbox.IsEnabled && SneedsRadioDowngraderCheckbox.IsChecked.Value)) NoEFLCMusicInIVCheckbox.IsEnabled = false;
                        }
                    }
                    else {
                        if (LegacyRadioDowngraderCheckbox.IsChecked.Value) {
                            NextButton.IsEnabled = false;
                            NoEFLCMusicInIVCheckbox.IsEnabled = false;
                        }
                        else {
                            if (!(SneedsRadioDowngraderCheckbox.IsEnabled && SneedsRadioDowngraderCheckbox.IsChecked.Value)) NoEFLCMusicInIVCheckbox.IsEnabled = false;
                        }
                    }
                }
            }

            if (SneedsRadioDowngraderCheckbox.IsEnabled || LegacyRadioDowngraderCheckbox.IsEnabled) {
                ErrorLabel.Visibility = Visibility.Hidden;
                if (SneedsRadioDowngraderCheckbox.IsEnabled) {
                    if (SneedsRadioDowngraderCheckbox.IsChecked.Value) {
                        NextButton.IsEnabled = true;
                        //NoEFLCMusicInIVCheckbox.IsChecked = false;
                    }
                }
                else {
                    if (SneedsRadioDowngraderCheckbox.IsChecked.Value) {
                        NextButton.IsEnabled = false;
                        //NoEFLCMusicInIVCheckbox.IsChecked = false;
                    }
                }
                if (LegacyRadioDowngraderCheckbox.IsEnabled) {
                    if (LegacyRadioDowngraderCheckbox.IsChecked.Value) {
                        NextButton.IsEnabled = true;
                        //NoEFLCMusicInIVCheckbox.IsChecked = false;
                    }
                }
                else {
                    if (LegacyRadioDowngraderCheckbox.IsChecked.Value) {
                        NextButton.IsEnabled = false;
                        //NoEFLCMusicInIVCheckbox.IsChecked = false;
                    }
                }
            }
            else {
                ErrorLabel.Visibility = Visibility.Visible;
                NextButton.IsEnabled = false;
                NoEFLCMusicInIVCheckbox.IsEnabled = false;
                NoEFLCMusicInIVCheckbox.IsChecked = false;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            instance.PreviousStep();
        }
        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            selectedRadioDowngrader = RadioDowngrader.None;
            instance.NextStep(1);
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (selectedRadioDowngrader) {
                case RadioDowngrader.SneedsDowngrader:
                    instance.NextStep();
                    break;
                default:
                    instance.NextStep(1);
                    break;
            }
        }

        private void SneedsRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            selectedRadioDowngrader = RadioDowngrader.SneedsDowngrader;
            NextButton.IsEnabled = true;
            if (instance.s2_SelectDwngrdVersion.selectedVersion != GameVersion.v1040) {
                if (!File.Exists(".\\Downgrader\\Files\\Radio\\EpisodeOnlyMusicCE.zip")) {
                    NoEFLCMusicInIVCheckbox.IsEnabled = false;
                }
                else {
                    NoEFLCMusicInIVCheckbox.IsEnabled = true;
                }
            }
        }
        private void LegacyRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            selectedRadioDowngrader = RadioDowngrader.LegacyDowngrader;
            NextButton.IsEnabled = true;
            if (instance.s2_SelectDwngrdVersion.selectedVersion != GameVersion.v1040) {
                if (!File.Exists(".\\Downgrader\\Files\\Radio\\EpisodeOnlyMusicCE.zip")) {
                    NoEFLCMusicInIVCheckbox.IsEnabled = false;
                }
                else {
                    NoEFLCMusicInIVCheckbox.IsEnabled = true;
                }
            }
        }

        private void NoEFLCMusicInIVCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            InstallNoEFLCMusicInIVFix = NoEFLCMusicInIVCheckbox.IsChecked.Value;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            switch (MessageBox.Show(string.Format("This link takes you to {0} ({1}). Do you want to go there?", uri.Host, uri.ToString()), "Open link?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                case MessageBoxResult.Yes:
                    Process.Start(uri.ToString());
                    break;
            }
        }

        private void LegacyRadioDowngraderCheckbox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!LegacyRadioDowngraderCheckbox.IsEnabled) {
                LegacyRadioDowngraderCheckbox.Foreground = Brushes.Gray;
            }
            else {
                LegacyRadioDowngraderCheckbox.Foreground = Brushes.White;
            }
        }
        private void SneedsRadioDowngraderCheckbox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!SneedsRadioDowngraderCheckbox.IsEnabled) {
                SneedsRadioDowngraderCheckbox.Foreground = Brushes.Gray;
            }
            else {
                SneedsRadioDowngraderCheckbox.Foreground = Brushes.White;
            }
        }
        private void NoEFLCMusicInIVCheckbox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!NoEFLCMusicInIVCheckbox.IsEnabled) {
                NoEFLCMusicInIVCheckbox.Foreground = Brushes.Gray;
            }
            else {
                NoEFLCMusicInIVCheckbox.Foreground = Brushes.White;
            }
        }

    }
}
