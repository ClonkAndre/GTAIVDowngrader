using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace GTAIVDowngrader.Dialogs {
    public partial class MoveGameFilesQuestionUC : UserControl {

        #region Variables
        private MainWindow instance;

        private List<string> testLocations;
        public string NewGTAIVTargetLocation;
        #endregion

        #region Constructor
        public MoveGameFilesQuestionUC()
        {
            testLocations = new List<string>();
            InitializeComponent();
        }
        public MoveGameFilesQuestionUC(MainWindow mainWindow)
        {
            instance = mainWindow;
            testLocations = new List<string>();
            InitializeComponent();
        }
        #endregion

        #region Methods
        public void SetErrorText(string str)
        {
            ErrorTextLabel.Text = str;
        }

        private void SetStatusText(string str)
        {
            StatusTextBlock.Text = str;
        }
        private void SetNextButtonEnabledState(bool enabled)
        {
            instance.ChangeActionButtonEnabledState(true, instance.BackButton.IsEnabled, true, enabled);
        }

        private void CheckMoveLocation(string loc)
        {
            if (string.IsNullOrWhiteSpace(loc))
            {
                SetNextButtonEnabledState(false);
                SetStatusText("Path can't be empty!");
                return;
            }
            if (!Directory.Exists(loc))
            {
                SetNextButtonEnabledState(false);
                SetStatusText("Directory not found!");
                return;
            }

            string path = loc.ToLower();
            if (path.Contains("program files") || path.Contains("program files (x86)"))
            {
                SetNextButtonEnabledState(false);
                SetStatusText("GTA IV can't be moved in the selected directory.");
                return;
            }

            // Check permissions
            string testFolderLoc = string.Format("{0}\\DowngraderPermissionTestDir", path).ToLower();
            testLocations.Add(testFolderLoc);

            try
            {
                Directory.CreateDirectory(testFolderLoc);
                File.WriteAllText(testFolderLoc + "\\test.txt", "This is a test.");

                // Passed test
                NewGTAIVTargetLocation = loc;
                SetNextButtonEnabledState(true);
                SetStatusText("GTA IV can be moved to this directory!");
            }
            catch (Exception)
            {
                SetNextButtonEnabledState(false);
                SetStatusText("GTA IV can't be moved in the selected directory. Not enough permissions.");
            }

            for (int i = 0; i < testLocations.Count; i++)
            {
                string testLoc = testLocations[i];
                if (Directory.Exists(testLoc)) Directory.Delete(testLoc, true);
                testLocations.RemoveAt(i);
            }
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            Core.CurrentDowngradingInfo.SetNewGTAIVTargetLocation(MoveLocationTextbox.Text);
            instance.NextStep();
        }
        private void Instance_SkipButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep(1);
        }
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep(1);
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.BackButtonClicked -= Instance_BackButtonClicked;
            instance.SkipButtonClicked -= Instance_SkipButtonClicked;
            instance.NextButtonClicked -= Instance_NextButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.BackButtonClicked += Instance_BackButtonClicked;
            instance.SkipButtonClicked += Instance_SkipButtonClicked;
            instance.NextButtonClicked += Instance_NextButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, true, true);
            instance.ChangeActionButtonEnabledState(true, true, true, false);

            // Commandline
            if (Core.GotStartedWithValidCommandLineArgs)
                instance.ChangeActionButtonEnabledState(true, false, true, false);
        }

        private void BrowseMoveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog("Select new location GTA IV should be moved to"))
            {
                ofd.IsFolderPicker = true;
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                    MoveLocationTextbox.Text = ofd.FileName;
            }
        }
        private void MoveLocationTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckMoveLocation(MoveLocationTextbox.Text);
        }

    }
}
