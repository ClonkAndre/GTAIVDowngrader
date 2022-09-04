using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class MultiplayerUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public MultiplayerUC(MainWindow mainWindow)
        {
            instance = mainWindow;
            InitializeComponent();
        }
        public MultiplayerUC()
        {
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
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetConfigureForGFWL(ConfigureForGFWLCheckBox.IsChecked.Value);
            instance.NextStep();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            instance.PreviousStep();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }

    }
}
