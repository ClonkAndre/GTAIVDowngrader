using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
            // Show message and skip select components tab if in offline mode
            if (Core.IsInOfflineMode)
            {
                instance.ShowMessageDialogScreen("Offline Mode Information",
                    string.Format("The downgrader is currently in offline mode and therefore it cannot download any modifications.{0}" +
                    "After the downgrade, you gonna have to download and install each mod that you want manually!{0}{0}" +
                    "Highly Recommended Mods{0}" +
                    "- Ultimate ASI Loader{0}" +
                    "- ZolikaPatch", Environment.NewLine),
                    Steps.S9_Confirm);

                // Force this to be true
                Core.CurrentDowngradingInfo.SetInstallPrerequisites(true);
            }
            else
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

            if (Core.Is420())
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char2.png", UriKind.Relative));
            if (Core.IsPrideMonth)
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char9.png", UriKind.Relative));
        }

        private void OldVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetVladivostokType(VladivostokTypes.Old);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void NewVladivostokCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Core.CurrentDowngradingInfo.SetVladivostokType(VladivostokTypes.New);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }

    }
}
