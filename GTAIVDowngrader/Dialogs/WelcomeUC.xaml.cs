using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GTAIVDowngrader.Dialogs {
    public partial class WelcomeUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public WelcomeUC()
        {
            InitializeComponent();
        }
        public WelcomeUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DowngraderVersionLabel.Text = string.Format("Version {0}", MainFunctions.updateChecker.currentVersion);

            if (MainFunctions.isPrideMonth) {
                DisableRainbowColoursCheckBox.Visibility = Visibility.Visible;

                if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                    BottomGrid.Background = "#B3000000".ToBrush();
                }
                else { // Use Rainbow Colours
                    BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
                }
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            instance.NextStep();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }

        private void CheckForUpdatesHyperLink_Click(object sender, RoutedEventArgs e)
        {
            MainFunctions.updateChecker.CheckForUpdates(false);
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            MainFunctions.AskUserToOpenURL(e.Uri);
        }

        private void DisableRainbowColoursCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MainFunctions.wantsToDisableRainbowColours = DisableRainbowColoursCheckBox.IsChecked.Value;

            if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                BottomGrid.Background = "#B3000000".ToBrush();
            }
            else { // Use Rainbow Colours
                BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
            }
        }

    }
}
