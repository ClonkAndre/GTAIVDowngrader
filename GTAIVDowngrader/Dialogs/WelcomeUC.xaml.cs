using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using CCL;

namespace GTAIVDowngrader.Dialogs
{
    public partial class WelcomeUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public WelcomeUC()
        {
            InitializeComponent();
        }
        public WelcomeUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
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

            instance.ChangeActionButtonVisiblity(true, false, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            DowngraderVersionLabel.Text = string.Format("Version {0}", Core.CUpdateChecker.CurrentVersion);
            DisableRainbowColoursCheckBox.Visibility = Core.IsPrideMonth ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckForUpdatesHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Core.CUpdateChecker.CheckForUpdatesAsync(false);
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

        private void DisableRainbowColoursCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Core.WantsToDisableRainbowColours = DisableRainbowColoursCheckBox.IsChecked.Value;

            if (Core.WantsToDisableRainbowColours) // Revert to default Colour
            {
                instance.BottomActionBorder.Background = "#B3000000".ToBrush();
                instance.UpdateOverallProgress();
            }
            else // Use Rainbow Colours
            {
                instance.BottomActionBorder.Background = Core.GetRainbowGradientBrush();
                instance.UpdateOverallProgress();
            }
        }

    }
}
