using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;

using CCL;

using GTAIVDowngrader.Classes;

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
        private void AddLogItem(LogType type, string str, bool includeTimeStamp = true, bool printInListBox = true)
        {
            Dispatcher.Invoke(() =>
            {
                string logTime = string.Format("{0}", DateTime.Now.ToString("HH:mm:ss"));

                string logText = "";
                if (includeTimeStamp)
                    logText = string.Format("[{0}] [{1}] {2}", logTime, type.ToString(), str);
                else
                    logText = string.Format("[{0}] {1}", type.ToString(), str);

                // Add log to StatusListBox
                if (printInListBox)
                    StatusListbox.Items.Add(logText);

                // Add log to log file
                if (includeTimeStamp)
                    Core.AddLogItem(type, string.Format("[{0}] {1}", logTime, str));
                else
                    Core.AddLogItem(type, str);

                // Auto scroll StatusListBox to last item
                StatusListbox.SelectedIndex = StatusListbox.Items.Count - 1;
                StatusListbox.ScrollIntoView(StatusListbox.SelectedItem);
            });
        }

        private void SetProgressMaximum(int max)
        {
            Dispatcher.Invoke(() =>
            {
                StatusProgressBar.Maximum = max;
            });
        }
        private void SetProgressValue(int value)
        {
            Dispatcher.Invoke(() =>
            {
                StatusProgressBar.Value = value;
            });
        }
        private void IncrementProgressValue()
        {
            Dispatcher.Invoke(() =>
            {
                StatusProgressBar.Value += 1;
            });
        }

        private void SetMainProgressBarAsIndeterminate(bool value)
        {
            Dispatcher.Invoke(() =>
            {
                instance.taskbarItemInfo.ProgressState = value ? TaskbarItemProgressState.Indeterminate : TaskbarItemProgressState.None;
                instance.GetMainProgressBar().IsIndeterminate = value;
            });
        }
        private void SetProgressBarState(int state)
        {
            Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case 0: // Working
                        StatusProgressBar.Foreground = "#0050BF".ToBrush();
                        StatusProgressBar.IsIndeterminate = false;
                        break;
                    case 1: // Working 2
                        StatusProgressBar.Foreground = "#0050BF".ToBrush();
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
            Dispatcher.Invoke(() =>
            {
                instance.ChangeActionButtonEnabledState(true, true, true, enabled);
            });
        }
        private void SetNewGTAIVPath(string path)
        {
            Dispatcher.Invoke(() =>
            {
                DowngradingInfo.SetPath(string.Format("{0}\\GTAIV.exe", path));
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

                AddLogItem(LogType.Info, string.Format("Creating directory {0}...", Path.GetDirectoryName(replacedPath)));

                Directory.CreateDirectory(replacedPath);
            }

            // Copy all the files & Replaces any files with the same name from this folder
            for (int i = 0; i < files.Length; i++)
            {
                string newPath = files[i];
                string replacedPath = newPath.Replace(originalDirPath, rightPath);

                AddLogItem(LogType.Info, string.Format("Moving file {0}...", Path.GetFileName(replacedPath)));

                File.Move(newPath, replacedPath);

                IncrementProgressValue();
            }
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            DowngradingInfo.SetGTAIVInstallationGotMovedByDowngrader(true);
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
            string oldGTAIVPath = DowngradingInfo.IVWorkingDirectoy;
            string newGTAIVPath = DowngradingInfo.NewGTAIVTargetLocation;
            string folderName = Path.GetFileName(oldGTAIVPath);
            MovingLocationText.Text = string.Format("{0}\\{1}", newGTAIVPath, folderName);
            MovingLocationText.ToolTip = MovingLocationText.Text;

            AddLogItem(LogType.Info, "- - - Starting Moving Process - - -");

            Task.Run(() =>
            {
                AResult<bool> result;

                try
                {
                    // Set things
                    SetMainProgressBarAsIndeterminate(true);
                    SetNextButtonEnabledState(false);
                    SetProgressBarState(2);
                    SetProgressMaximum(Directory.GetFiles(oldGTAIVPath, "*.*", SearchOption.AllDirectories).Count());
                    SetProgressValue(0);

                    // Start moving files
                    AddLogItem(LogType.Info, string.Format("Moving {0} to {1}...", folderName, newGTAIVPath));

                    MoveFilesRecursively(folderName, oldGTAIVPath, newGTAIVPath);

                    AddLogItem(LogType.Info, string.Format("Finished moving {0} to {1}!", folderName, newGTAIVPath));

                    // Delete old files
                    SetProgressBarState(1);
                    AddLogItem(LogType.Info, "Deleting remaining files...");
                    Directory.Delete(oldGTAIVPath, true);

                    result = new AResult<bool>(null, true);
                }
                catch (Exception ex)
                {
                    SetProgressBarState(3);
                    AddLogItem(LogType.Error, string.Format("Error while moving files: {0}", ex.Message));
                    result = new AResult<bool>(ex, false);
                }
                
                return result;
            }).ContinueWith(r =>
            {
                AResult<bool> result = r.Result;

                SetMainProgressBarAsIndeterminate(false);
                SetNextButtonEnabledState(true);
                SetProgressBarState(2);
                SetProgressMaximum(0);
                SetProgressValue(0);

                if (result.Result)
                {
                    SetNewGTAIVPath(newGTAIVPath);
                    AddLogItem(LogType.Info, "Moving process completed successfully!");
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
