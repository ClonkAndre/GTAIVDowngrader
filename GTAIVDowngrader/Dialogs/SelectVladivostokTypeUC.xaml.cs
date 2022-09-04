using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class SelectVladivostokTypeUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SelectVladivostokTypeUC()
        {
            InitializeComponent();
        }
        public SelectVladivostokTypeUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainFunctions.isPrideMonth) {
                if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                    BottomGrid.Background = "#B3000000".ToBrush();
                }
                else { // Use Rainbow Colours
                    BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
                }
            }
        }

        private void OldVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetVladivostokType(VladivostokTypes.Old);
            NextButton.IsEnabled = true;
        }
        private void NewVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MainFunctions.downgradingInfo.SetVladivostokType(VladivostokTypes.New);
            NextButton.IsEnabled = true;
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
            instance.NextStep();
        }

    }
}
