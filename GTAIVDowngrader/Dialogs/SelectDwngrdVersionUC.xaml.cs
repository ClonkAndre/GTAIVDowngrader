using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTAIVDowngrader.Dialogs {
    public partial class SelectDwngrdVersionUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SelectDwngrdVersionUC()
        {
            InitializeComponent();
        }
        public SelectDwngrdVersionUC(MainWindow window)
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

            if (MainFunctions.gotStartedWithValidCommandLineArgs) {
                BackButton.IsEnabled = false;
            }
            else {
                BackButton.IsEnabled = !MainFunctions.downgradingInfo.GTAIVInstallationGotMovedByDowngrader;
            }

            // File check
            if (!File.Exists(".\\Data\\Files\\1080\\1080.zip")) {
                IV1080Radiobtn.IsEnabled = false;
            }
            else {
                IV1080Radiobtn.IsEnabled = true;
            }
            if (!File.Exists(".\\Data\\Files\\1070\\1070.zip")) {
                IV1070Radiobtn.IsEnabled = false;
            }
            else {
                IV1070Radiobtn.IsEnabled = true;
            }
            if (!File.Exists(".\\Data\\Files\\1040\\1040.zip")) {
                IV1040Radiobtn.IsEnabled = false;
            }
            else {
                IV1070Radiobtn.IsEnabled = true;
            }

            if (IV1080Radiobtn.IsEnabled || IV1070Radiobtn.IsEnabled || IV1040Radiobtn.IsEnabled) {
                ErrorLabel.Visibility = Visibility.Collapsed;
            }
            else if (!IV1080Radiobtn.IsEnabled && !IV1070Radiobtn.IsEnabled && !IV1040Radiobtn.IsEnabled) {
                ErrorLabel.Visibility = Visibility.Visible;
                NextButton.IsEnabled = false;
            }
            else {
                ErrorLabel.Visibility = Visibility.Visible;
                NextButton.IsEnabled = false;
            }
        }

        private void IV1080Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetDowngradeVersion(GameVersion.v1080);
            NextButton.IsEnabled = true;
        }
        private void IV1070Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetDowngradeVersion(GameVersion.v1070);
            NextButton.IsEnabled = true;
        }
        private void IV1040Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetDowngradeVersion(GameVersion.v1040);
            NextButton.IsEnabled = true;
        }

        private void IV1080Radiobtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IV1080Radiobtn.IsEnabled) {
                IV1080Radiobtn.Foreground = Brushes.White;
            }
            else {
                IV1080Radiobtn.Foreground = Brushes.Gray;
            }
        }
        private void IV1070Radiobtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IV1070Radiobtn.IsEnabled) {
                IV1070Radiobtn.Foreground = Brushes.White;
            }
            else {
                IV1070Radiobtn.Foreground = Brushes.Gray;
            }
        }
        private void IV1040Radiobtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IV1040Radiobtn.IsEnabled) {
                IV1040Radiobtn.Foreground = Brushes.White;
            }
            else {
                IV1040Radiobtn.Foreground = Brushes.Gray;
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
            instance.PreviousStep(3);
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFunctions.downgradingInfo.DowngradeTo == GameVersion.v1040) {
                MainFunctions.downgradingInfo.SetRadioDowngrader(RadioDowngrader.LegacyDowngrader);
                MainFunctions.downgradingInfo.SetVladivostokType(VladivostokTypes.None);
                MainFunctions.downgradingInfo.SetInstallNoEFLCMusicInIVFix(false);
                MainFunctions.downgradingInfo.SetConfigureForGFWL(false);
                instance.NextStep(3);
            }
            else {
                instance.NextStep();
            }
        }

    }
}
