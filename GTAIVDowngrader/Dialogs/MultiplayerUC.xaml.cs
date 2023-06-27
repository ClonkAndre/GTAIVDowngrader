using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GTAIVDowngrader.Dialogs {
    public partial class MultiplayerUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public MultiplayerUC(MainWindow mainWindow)
        {
            instance = mainWindow;
            InitializeComponent();
        }
        public MultiplayerUC()
        {
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
            Core.CDowngradingInfo.SetConfigureForGFWL(ConfigureForGFWLCheckBox.IsChecked.Value);
            instance.NextStep();
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
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
