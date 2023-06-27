using System;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class SelectRadioDwngrdUC : UserControl {

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

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            if (Core.CDowngradingInfo.DowngradeTo == GameVersion.v1040)
                instance.PreviousStep(1);
            else
                instance.PreviousStep();
        }
        private void Instance_SkipButtonClicked(object sender, EventArgs e)
        {
            Core.CDowngradingInfo.SetRadioDowngrader(RadioDowngrader.None);
            Core.CDowngradingInfo.SetVladivostokType(VladivostokTypes.None);
            instance.NextStep(1);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            switch (Core.CDowngradingInfo.SelectedRadioDowngrader)
            {
                case RadioDowngrader.SneedsDowngrader:
                    instance.NextStep();
                    break;
                default:
                    instance.NextStep(1);
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
            Core.CDowngradingInfo.SetRadioDowngrader(RadioDowngrader.None);
            Core.CDowngradingInfo.SetVladivostokType(VladivostokTypes.None);
            Core.CDowngradingInfo.SetInstallNoEFLCMusicInIVFix(false);
            SneedsRadioDowngraderCheckbox.IsChecked = false;
            LegacyRadioDowngraderCheckbox.IsChecked = false;
            NoEFLCMusicInIVCheckbox.IsChecked = false;
        }

        private void SneedsRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CDowngradingInfo.SetRadioDowngrader(RadioDowngrader.SneedsDowngrader);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void LegacyRadioDowngraderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CDowngradingInfo.SetRadioDowngrader(RadioDowngrader.LegacyDowngrader);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void NoEFLCMusicInIVCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Core.CDowngradingInfo.SetInstallNoEFLCMusicInIVFix(NoEFLCMusicInIVCheckbox.IsChecked.Value);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
