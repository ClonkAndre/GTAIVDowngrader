using System;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SelectVladivostokTypeUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public SelectVladivostokTypeUC()
        {
            InitializeComponent();
        }
        public SelectVladivostokTypeUC(MainWindow window)
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
            instance.ChangeActionButtonEnabledState(true, true, true, OldVladivostokCheckbox.IsChecked.Value || NewVladivostokCheckbox.IsChecked.Value);
        }

        private void OldVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CDowngradingInfo.SetVladivostokType(VladivostokTypes.Old);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void NewVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CDowngradingInfo.SetVladivostokType(VladivostokTypes.New);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }

    }
}
