using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SelectIVExeUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        private string gtaivExecutablePath;
        #endregion

        #region Methods
        private void CheckDirectoryOrFile(string path)
        {
            try
            {
                // If path is a directory
                if (Directory.Exists(path))
                {

                    // Check if GTA IV exists in the directory
                    bool foundGTAIV = false;
                    string[] files = Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        string fileName = Path.GetFileName(file).ToLower();

                        if (fileName.Contains("gtaiv"))
                        {
                            foundGTAIV = true;
                            gtaivExecutablePath = file;
                            break;
                        }
                    }

                    if (foundGTAIV)
                    {
                        StatusTextBlock.Text = "Valid directory!";
                        instance.ChangeActionButtonEnabledState(true, true, true, true);
                    }
                    else
                    {
                        StatusTextBlock.Text = "GTAIV.exe was not found in the selected directory!";
                        instance.ChangeActionButtonEnabledState(true, true, true, false);
                    }

                    return;
                }

                // If path is a file
                if (File.Exists(path))
                {
                    gtaivExecutablePath = path;
                    StatusTextBlock.Text = "Valid file!";
                    instance.ChangeActionButtonEnabledState(true, true, true, true);

                    return;
                }

                StatusTextBlock.Text = "Please select a GTA IV directory, or the GTAIV.exe file.";
                instance.ChangeActionButtonEnabledState(true, true, true, false);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = string.Format("Error: {0}", ex.Message);
                instance.ChangeActionButtonEnabledState(true, true, true, false);
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

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep();
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            // Set IVExecutablePath and IVWorkingDirectory
            Core.CurrentDowngradingInfo.SetPath(gtaivExecutablePath);

            // Go to next step
            instance.NextStep();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.BackButtonClicked -= Instance_BackButtonClicked;
            instance.NextButtonClicked -= Instance_NextButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.BackButtonClicked += Instance_BackButtonClicked;
            instance.NextButtonClicked += Instance_NextButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, false);

            CheckDirectoryOrFile(PathTextBox.Text);
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog("Select GTA IV directory that should be downgraded"))
            {
                ofd.IsFolderPicker = true;
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                    PathTextBox.Text = ofd.FileName;
            }
        }
        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog("Select GTA IV executable that should be downgraded"))
            {
                ofd.IsFolderPicker = false;
                ofd.Filters.Add(new CommonFileDialogFilter("Executable", ".exe"));
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                    PathTextBox.Text = ofd.FileName;
            }
        }

        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDirectoryOrFile(PathTextBox.Text);
        }

    }
}
