using System;
using System.Windows;
using System.Windows.Controls;

using CCL;

namespace GTAIVDowngrader.Dialogs
{
    public partial class StandaloneWarningUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public StandaloneWarningUC()
        {
            InitializeComponent();
        }
        public StandaloneWarningUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Methods
        public void SetWarning(string title, string desc)
        {
            TitleLabel.Text = title;
            DescriptionLabel.Text = desc;
        }
        public void SetRedProgressBar()
        {
            Dispatcher.Invoke(() => {
                instance.GetMainProgressBar().Foreground = "#B3bd0000".ToBrush();
                instance.GetMainProgressBar().Value = 100;
            });
        }
        public void SetContinueAnywayButton()
        {
            Dispatcher.Invoke(() => {
                instance.ChangeActionButtonVisiblity(true, false, false, true);
                instance.NextButton.Content = "Continue anyway";
            });
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            Core.AddLogItem(LogType.Info, "User continued using the downgrader after internet check failed.");
            instance.DownloadRequiredData();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.NextButton.Content = "Next";
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;

            instance.ChangeActionButtonVisiblity(true, false, false, false);
        }

    }
}
