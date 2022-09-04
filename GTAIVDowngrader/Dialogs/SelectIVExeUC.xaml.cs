using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GTAIVDowngrader.Dialogs {
    public partial class SelectIVExeUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Methods
        private void CheckFileVersion(string v)
        {
            try {
                string fName = v;
                IVExecutablePathTextbox.Text = fName;
                if (!string.IsNullOrWhiteSpace(fName)) {
                    if (File.Exists(fName)) {
                        IVExeStatusLabel.Text = "Valid file";
                        IVExeStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\checkCircleWhite.png", UriKind.RelativeOrAbsolute));
                        NextButton.IsEnabled = true;
                        return;
                    }
                    else {
                        IVExeStatusLabel.Text = "File not found";
                        IVExeStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\errorWhite.png", UriKind.RelativeOrAbsolute));
                    }
                }
                else {
                    IVExeStatusLabel.Text = "Invalid path";
                    IVExeStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex) {
                IVExeStatusLabel.Text = string.Format("An error occured{0}Error: {1}", Environment.NewLine, ex.Message);
                IVExeStatusImage.Source = new BitmapImage(new Uri(@"..\Resources\errorWhite.png", UriKind.RelativeOrAbsolute));
            }
            NextButton.IsEnabled = false;
        }
        #endregion

        #region Constuctor
        public SelectIVExeUC()
        {
            InitializeComponent();
        }
        public SelectIVExeUC(MainWindow window)
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
            // Set IVExecutablePath and IVWorkingDirectory
            MainFunctions.downgradingInfo.SetPath(IVExecutablePathTextbox.Text);

            // Go to next step
            instance.NextStep();
        }

        private void IVExecutablePathTextbox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                CheckFileVersion(IVExecutablePathTextbox.Text);
            }
        }
        private void IVExecutablePathTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(IVExecutablePathTextbox.Text)) {
                CheckFileVersion(IVExecutablePathTextbox.Text);
            }
        }
        private void BrowseIVExecutableButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog("Select the GTA IV executable")) {
                ofd.Filters.Add(new CommonFileDialogFilter("Executable", ".exe"));
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {
                    CheckFileVersion(ofd.FileName);
                }
            }
        }

    }
}
