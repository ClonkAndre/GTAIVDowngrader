using System;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class S3_1_SelectVladivostokType : UserControl {

        #region Variables
        private MainWindow instance;

        public VladivostokTypes selectedVladivostokType;
        #endregion

        #region Constructor
        public S3_1_SelectVladivostokType()
        {
            InitializeComponent();
        }
        public S3_1_SelectVladivostokType(MainWindow window)
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
            instance.NextStep();
        }

        private void OldVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            selectedVladivostokType = VladivostokTypes.Old;
            NextButton.IsEnabled = true;
        }
        private void NewVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            selectedVladivostokType = VladivostokTypes.New;
            NextButton.IsEnabled = true;
        }

    }
}
