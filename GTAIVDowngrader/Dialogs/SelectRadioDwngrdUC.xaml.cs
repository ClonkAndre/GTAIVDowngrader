﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SelectRadioDwngrdUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SelectRadioDwngrdUC()
        {
            InitializeComponent();
        }
        public SelectRadioDwngrdUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void NextStep()
        {
            // Show message and skip select components tab if in offline mode
            if (Core.IsInOfflineMode)
            {
                instance.ShowMessageDialogScreen("Offline Mode Information",
                    string.Format("The downgrader is currently in offline mode and therefore, it cannot download any modifications.{0}" +
                    "After the downgrade, you gonna have to download and install each mod that you want manually!", Environment.NewLine),
                    Steps.S9_Confirm);

                // Force this to be true
                Core.CurrentDowngradingInfo.SetInstallPrerequisites(true);
            }
            else
                instance.NextStep(1);
        }
        #endregion

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            if (Core.CurrentDowngradingInfo.DowngradeTo == GameVersion.v1040)
                instance.PreviousStep(1);
            else
                instance.PreviousStep();
        }
        private void Instance_SkipButtonClicked(object sender, EventArgs e)
        {
            Core.CurrentDowngradingInfo.SetRadioDowngrader(RadioDowngrader.None);
            Core.CurrentDowngradingInfo.SetVladivostokType(VladivostokTypes.None);
            NextStep();
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            switch (Core.CurrentDowngradingInfo.SelectedRadioDowngrader)
            {
                case RadioDowngrader.SneedsDowngrader:
                    instance.NextStep();
                    break;
                default:
                    NextStep();
                    break;
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.SkipButtonClicked -= Instance_SkipButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.SkipButtonClicked += Instance_SkipButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, true, true);
            instance.ChangeActionButtonEnabledState(true, true, true, false);

            // Reset
            Core.CurrentDowngradingInfo.SetRadioDowngrader(RadioDowngrader.None);
            Core.CurrentDowngradingInfo.SetVladivostokType(VladivostokTypes.None);
            Core.CurrentDowngradingInfo.SetInstallNoEFLCMusicInIVFix(false);
            SneedsRadioDowngraderCheckbox.IsChecked = false;
            LegacyRadioDowngraderCheckbox.IsChecked = false;
            NoEFLCMusicInIVCheckbox.IsChecked = false;
        }

        private void SneedsRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetRadioDowngrader(RadioDowngrader.SneedsDowngrader);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void LegacyRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetRadioDowngrader(RadioDowngrader.LegacyDowngrader);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void NoEFLCMusicInIVCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetInstallNoEFLCMusicInIVFix(NoEFLCMusicInIVCheckbox.IsChecked.Value);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
