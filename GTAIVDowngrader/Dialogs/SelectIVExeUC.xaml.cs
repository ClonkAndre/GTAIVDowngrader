using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace GTAIVDowngrader.Dialogs {
    public partial class SelectIVExeUC : UserControl {

        #region Variables
        private MainWindow instance;
        private string gtaivExecutablePath;
        #endregion

        #region Methods
        private void CheckDirectory(string dir)
        {
            try {
                if (!Directory.Exists(dir)) {
                    StatusTextBlock.Text = "Directory does not exists!";
                    NextButton.IsEnabled = false;
                    return;
                }

                // Check if GTA IV exists in the directory
                bool foundGTAIV = false;
                string[] files = Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++) {
                    string file = files[i];
                    string fileName = Path.GetFileName(file).ToLower();

                    if (fileName.Contains("gtaiv")) {
                        foundGTAIV = true;
                        gtaivExecutablePath = file;
                        break;
                    }
                }

                StatusTextBlock.Text = foundGTAIV ? "Valid directory!" : "GTAIV.exe was not found in the selected directory! If you are sure it's in there, you can continue.";
                NextButton.IsEnabled = true;
            }
            catch (Exception ex) {
                StatusTextBlock.Text = string.Format("Error: {0}", ex.Message);
                NextButton.IsEnabled = false;
            }
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
            MainFunctions.downgradingInfo.SetPath(gtaivExecutablePath);

            // Go to next step
            instance.NextStep();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog("Select GTA IV directory that should be downgraded")) {
                ofd.IsFolderPicker = true;
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {
                    PathTextBox.Text = ofd.FileName;
                }
            }
        }
        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDirectory(PathTextBox.Text);
        }

    }
}
