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
        private void Instance_BackButtonClicked(object sender, System.EventArgs e)
        {
            instance.PreviousStep(3);
        }
        private void Instance_NextButtonClicked(object sender, System.EventArgs e)
        {
            if (Core.CDowngradingInfo.DowngradeTo == GameVersion.v1040)
            {
                Core.CDowngradingInfo.SetRadioDowngrader(RadioDowngrader.LegacyDowngrader);
                Core.CDowngradingInfo.SetVladivostokType(VladivostokTypes.None);
                Core.CDowngradingInfo.SetInstallNoEFLCMusicInIVFix(false);
                Core.CDowngradingInfo.SetConfigureForGFWL(false);
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
            Core.CDowngradingInfo.SetDowngradeVersion(GameVersion.v1080);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void IV1070Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            Core.CDowngradingInfo.SetDowngradeVersion(GameVersion.v1070);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void IV1040Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            Core.CDowngradingInfo.SetDowngradeVersion(GameVersion.v1040);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
