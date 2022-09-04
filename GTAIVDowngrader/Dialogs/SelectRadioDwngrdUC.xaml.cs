using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTAIVDowngrader.Dialogs {
    public partial class SelectRadioDwngrdUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SelectRadioDwngrdUC()
        {
            InitializeComponent();
        }
        public SelectRadioDwngrdUC(MainWindow window)
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

            // File check
            SneedsRadioDowngraderCheckbox.IsEnabled = File.Exists(".\\Data\\Files\\Radio\\SneedsRadioDowngrader.zip");
            LegacyRadioDowngraderCheckbox.IsEnabled = File.Exists(".\\Data\\Files\\Radio\\LegacyRadioDowngrader.zip");

            // Reset
            MainFunctions.downgradingInfo.SetRadioDowngrader(RadioDowngrader.None);
            MainFunctions.downgradingInfo.SetVladivostokType(VladivostokTypes.None);
            MainFunctions.downgradingInfo.SetInstallNoEFLCMusicInIVFix(false);
            SneedsRadioDowngraderCheckbox.IsChecked = false;
            LegacyRadioDowngraderCheckbox.IsChecked = false;
            NoEFLCMusicInIVCheckbox.IsChecked = false;
            NextButton.IsEnabled = false;

            // Version check
            if (MainFunctions.downgradingInfo.DowngradeTo == GameVersion.v1040) {

                // Check if Sneeds or Legacy Downgrader Checkbox is enabled
                if (!SneedsRadioDowngraderCheckbox.IsEnabled || !LegacyRadioDowngraderCheckbox.IsEnabled) {
                    ErrorLabel.Visibility = Visibility.Visible;
                }

                SkipButton.IsEnabled = false;
                NoEFLCMusicInIVCheckbox.IsEnabled = false;
            }
            else {

                // Check if Sneeds or Legacy Downgrader Checkbox is enabled
                if (SneedsRadioDowngraderCheckbox.IsEnabled || LegacyRadioDowngraderCheckbox.IsEnabled) {
                    NoEFLCMusicInIVCheckbox.IsEnabled = true;
                }

                ErrorLabel.Visibility = Visibility.Collapsed;
                SkipButton.IsEnabled = true;
            }
        }

        private void SneedsRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetRadioDowngrader(RadioDowngrader.SneedsDowngrader);
            NextButton.IsEnabled = true;
        }
        private void LegacyRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetRadioDowngrader(RadioDowngrader.LegacyDowngrader);
            NextButton.IsEnabled = true;
        }
        private void NoEFLCMusicInIVCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetInstallNoEFLCMusicInIVFix(NoEFLCMusicInIVCheckbox.IsChecked.Value);
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            MainFunctions.AskUserToOpenURL(e.Uri);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFunctions.downgradingInfo.DowngradeTo == GameVersion.v1040) {
                instance.PreviousStep(1);
            }
            else {
                instance.PreviousStep();
            }
        }
        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetRadioDowngrader(RadioDowngrader.None);
            MainFunctions.downgradingInfo.SetVladivostokType(VladivostokTypes.None);
            instance.NextStep(1);
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MainFunctions.downgradingInfo.SelectedRadioDowngrader) {
                case RadioDowngrader.SneedsDowngrader:
                    instance.NextStep();
                    break;
                default:
                    instance.NextStep(1);
                    break;
            }
        }

    }
}
