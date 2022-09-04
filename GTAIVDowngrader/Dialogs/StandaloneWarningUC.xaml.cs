using System;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class StandaloneWarningUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public StandaloneWarningUC()
        {
            InitializeComponent();
        }
        public StandaloneWarningUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Methods
        public void SetWarning(string title, string desc)
        {
            TitleLabel.Text = title;
            DescriptionLabel.Text = desc;
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.Close();
        }

    }
}
