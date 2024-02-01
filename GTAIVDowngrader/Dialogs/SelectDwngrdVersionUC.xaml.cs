using System;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SelectDwngrdVersionUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SelectDwngrdVersionUC()
        {
            InitializeComponent();
        }
        public SelectDwngrdVersionUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep(3);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            if (Core.CurrentDowngradingInfo.DowngradeTo == GameVersion.v1040)
            {
                Core.CurrentDowngradingInfo.SetRadioDowngrader(RadioDowngrader.LegacyDowngrader);
                Core.CurrentDowngradingInfo.SetVladivostokType(VladivostokTypes.None);
                Core.CurrentDowngradingInfo.SetInstallNoEFLCMusicInIVFix(false);
                Core.CurrentDowngradingInfo.SetConfigureForGFWL(false);

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
                    instance.NextStep(3);
            }
            else
            {
                instance.NextStep();
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, (IV1040Radiobtn.IsChecked.Value || IV1070Radiobtn.IsChecked.Value || IV1080Radiobtn.IsChecked.Value));
        }

        private void IV1080Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetDowngradeVersion(GameVersion.v1080);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void IV1070Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetDowngradeVersion(GameVersion.v1070);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void IV1040Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetDowngradeVersion(GameVersion.v1040);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
