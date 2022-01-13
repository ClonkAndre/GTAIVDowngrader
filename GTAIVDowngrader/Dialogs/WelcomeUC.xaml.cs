using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GTAIVDowngrader.Dialogs {
    public partial class WelcomeUC : UserControl {

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

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            instance.NextStep();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            switch (MessageBox.Show(string.Format("This link takes you to {0} ({1}). Do you want to go there?", uri.Host, uri.ToString()), "Open link?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                case MessageBoxResult.Yes:
                    Process.Start(uri.ToString());
                    break;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            instance.CheckForUpdates(false);
        }
    }
}
