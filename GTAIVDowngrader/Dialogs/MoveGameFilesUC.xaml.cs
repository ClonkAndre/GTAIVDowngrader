using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using CCL;
using MS.WindowsAPICodePack.Internal;

namespace GTAIVDowngrader.Dialogs
{
    public partial class MoveGameFilesUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public MoveGameFilesUC()
        {
            InitializeComponent();
        }
        public MoveGameFilesUC(MainWindow mainWindow)
        {
            instance = mainWindow;
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void AddLogItem(string str)
        {
            Dispatcher.Invoke(() => {
                StatusListbox.Items.Add(str);
            });
        }

        private void SetProgressMaximum(int max)
        {
            Dispatcher.Invoke(() => {
                StatusProgressBar.Maximum = max;
            });
        }
        private void SetProgressValue(int value)
        {
            Dispatcher.Invoke(() => {
                StatusProgressBar.Value = value;
            });
        }
        private void IncrementProgressValue()
        {
            Dispatcher.Invoke(() => {
                StatusProgressBar.Value += 1;
            });
        }

        private void SetProgressBarState(int state)
        {
            Dispatcher.Invoke(() => {
                switch (state)
                {
                    case 0: // Working
                        StatusProgressBar.Foreground = (Brush)Core.CBrushConverter.ConvertFrom("#0050BF");
                        StatusProgressBar.IsIndeterminate = false;
                        break;
                    case 1: // Working 2
                        StatusProgressBar.Foreground = (Brush)Core.CBrushConverter.ConvertFrom("#0050BF");
                        StatusProgressBar.IsIndeterminate = true;
                        break;
                    case 2: // Finished
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Foreground = Brushes.Green;
                        StatusProgressBar.Maximum = 100;
                        StatusProgressBar.Value = 100;
                        break;
                    case 3: // Errored
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Foreground = Brushes.Red;
                        StatusProgressBar.Maximum = 100;
                        StatusProgressBar.Value = 100;
                        break;
                }
            });
        }
        private void SetNextButtonEnabledState(bool enabled)
        {
            Dispatcher.Invoke(() => {
                instance.ChangeActionButtonEnabledState(true, true, true, enabled);
            });
        }
        private void SetNewGTAIVPath(string path)
        {
            Dispatcher.Invoke(() => {
                string exePath = string.Format("{0}\\GTAIV.exe", path);
                Core.CDowngradingInfo.SetPath(exePath);
            });
        }

        private void MoveFilesRecursively(string directoryName, string originalDirPath, string targetPath)
        {
            string rightPath = string.Format("{0}\\{1}", targetPath, directoryName);

            if (!Directory.Exists(rightPath))
                Directory.CreateDirectory(rightPath);

            string[] directorys = Directory.GetDirectories(originalDirPath, "*", SearchOption.AllDirectories);
            string[] files = Directory.GetFiles(originalDirPath, "*.*", SearchOption.AllDirectories);

            // Create all of the directories from this folder
            for (int i = 0; i < directorys.Length; i++)
            {
                string dirPath = directorys[i];
                string replacedPath = dirPath.Replace(originalDirPath, rightPath);
                Directory.CreateDirectory(replacedPath);
            }

            // Copy all the files & Replaces any files with the same name from this folder
            for (int i = 0; i < files.Length; i++)
            {
                string newPath = files[i];
                string replacedPath = newPath.Replace(originalDirPath, rightPath);
                File.Move(newPath, replacedPath);
                IncrementProgressValue();
            }
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            Core.CDowngradingInfo.SetGTAIVInstallationGotMovedByDowngrader(true);
            instance.NextStep();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;

            instance.ChangeActionButtonVisiblity(false, false, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, false);

            StatusListbox.Items.Clear();
            string oldGTAIVPath = Core.CDowngradingInfo.IVWorkingDirectoy;
            string newGTAIVPath = Core.CDowngradingInfo.NewGTAIVTargetLocation;
            string folderName = Path.GetFileName(oldGTAIVPath);
            MovingLocationText.Text = string.Format(@"{0}\{1}", newGTAIVPath, folderName);

            Task.Run(() => {
                AResult<bool> result;

                try
                {
                    // Get file count of old GTA IV path
                    int fileCount = Directory.GetFiles(oldGTAIVPath, "*.*", SearchOption.AllDirectories).Count();

                    // Set things
                    SetNextButtonEnabledState(false);
                    SetProgressBarState(2);
                    SetProgressMaximum(fileCount);
                    SetProgressValue(0);

                    // Start moving files
                    AddLogItem(string.Format("Starting to move {0} to {1}...", folderName, newGTAIVPath));

                    MoveFilesRecursively(folderName, oldGTAIVPath, newGTAIVPath);

                    AddLogItem(string.Format("Finished moving {0} to {1}!", folderName, newGTAIVPath));

                    // Delete old files
                    SetProgressBarState(1);
                    AddLogItem("Deleting remaining files...");
                    Directory.Delete(oldGTAIVPath, true);

                    //SetProgressBarState(2);

                    result = new AResult<bool>(null, true);
                }
                catch (Exception ex)
                {
                    SetProgressBarState(3);
                    AddLogItem(string.Format("Error while moving files: {0}", ex.Message));
                    result = new AResult<bool>(ex, false);
                }
                
                return result;
            }).ContinueWith(r => {
                AResult<bool> result = r.Result;
                if (result.Result)
                {
                    SetNewGTAIVPath(newGTAIVPath);
                    AddLogItem("Moving process completed successfully!");
                    SetNextButtonEnabledState(true);
                }
                else
                {
                    if (result.Exception is UnauthorizedAccessException)
                    {
                        instance.ChangeStep(Steps.StandaloneWarning, new List<object>() { "Moving process did not complete successfully",
                            "An UnauthorizedAccessException occured. Please run the downgrader as an administrator and try again." });
                    }
                    else
                    {
                        instance.ChangeStep(Steps.StandaloneWarning, new List<object>() { "Moving process did not complete successfully", string.Format("Your GTA IV directory might be corrupted now.{0}" +
                        "Please close the downgrader and verify that the old GTA IV directory still contains all files. If not, then you might be able to just copy all files that got moved to the new directory back into the old GTA IV directory.{0}{0}" +
                        "Error details: {2} {3} {1}{0}{0}" +
                        "Please copy the error and send it into the #help channel on Clonk's discord!", Environment.NewLine, result.Exception.StackTrace, result.Exception.GetType().Name, result.Exception.Message) });
                    }

                    instance.standaloneWarningUC.SetRedProgressBar();
                }
            });
        }

    }
}
