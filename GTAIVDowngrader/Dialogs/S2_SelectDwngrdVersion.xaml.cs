using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTAIVDowngrader.Dialogs {
    public partial class S2_SelectDwngrdVersion : UserControl {

        #region Variables
        private MainWindow instance;

        public GameVersion selectedVersion;
        #endregion

        #region Constructor
        public S2_SelectDwngrdVersion()
        {
            InitializeComponent();
        }
        public S2_SelectDwngrdVersion(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(".\\Downgrader\\Files\\1080\\1080.zip")) {
                IV1080Radiobtn.IsEnabled = false;
            }
            else {
                IV1080Radiobtn.IsEnabled = true;
            }
            if (!File.Exists(".\\Downgrader\\Files\\1070\\1070.zip")) {
                IV1070Radiobtn.IsEnabled = false;
            }
            else {
                IV1070Radiobtn.IsEnabled = true;
            }
            if (!File.Exists(".\\Downgrader\\Files\\1040\\1040.zip")) {
                IV1040Radiobtn.IsEnabled = false;
            }
            else {
                IV1070Radiobtn.IsEnabled = true;
            }

            if (IV1080Radiobtn.IsEnabled || IV1070Radiobtn.IsEnabled || IV1040Radiobtn.IsEnabled) {
                ErrorLabel.Visibility = Visibility.Collapsed;
                //if (IV1080Radiobtn.IsEnabled) {
                //    if (IV1080Radiobtn.IsChecked.Value) {
                //        NextButton.IsEnabled = true;
                //    }
                //}
                //else {
                //    if (IV1080Radiobtn.IsChecked.Value) {
                //        NextButton.IsEnabled = false;
                //    }
                //}
                //if (IV1070Radiobtn.IsEnabled) {
                //    if (IV1070Radiobtn.IsChecked.Value) {
                //        NextButton.IsEnabled = true;
                //    }
                //}
                //else {
                //    if (IV1070Radiobtn.IsChecked.Value) {
                //        NextButton.IsEnabled = false;
                //    }
                //}
            }
            else if (!IV1080Radiobtn.IsEnabled && !IV1070Radiobtn.IsEnabled && !IV1040Radiobtn.IsEnabled) {
                ErrorLabel.Visibility = Visibility.Visible;
                NextButton.IsEnabled = false;
            }
            else {
                ErrorLabel.Visibility = Visibility.Visible;
                NextButton.IsEnabled = false;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            instance.PreviousStep();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            instance.NextStep();
        }

        private void IV1080Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            selectedVersion = GameVersion.v1080;
            NextButton.IsEnabled = true;
        }
        private void IV1070Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            selectedVersion = GameVersion.v1070;
            NextButton.IsEnabled = true;
        }
        private void IV1040Radiobtn_Checked(object sender, RoutedEventArgs e)
        {
            selectedVersion = GameVersion.v1040;
            NextButton.IsEnabled = true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            switch (MessageBox.Show(string.Format("This link takes you to {0} ({1}). Do you want to go there?", uri.Host, uri.ToString()), "Open link?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                case MessageBoxResult.Yes:
                    Process.Start(uri.ToString());
                    break;
            }
        }

        private void IV1080Radiobtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IV1080Radiobtn.IsEnabled) {
                IV1080Radiobtn.Foreground = Brushes.White;
            }
            else {
                IV1080Radiobtn.Foreground = Brushes.Gray;
            }
        }
        private void IV1070Radiobtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IV1070Radiobtn.IsEnabled) {
                IV1070Radiobtn.Foreground = Brushes.White;
            }
            else {
                IV1070Radiobtn.Foreground = Brushes.Gray;
            }
        }
        private void IV1040Radiobtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IV1040Radiobtn.IsEnabled) {
                IV1040Radiobtn.Foreground = Brushes.White;
            }
            else {
                IV1040Radiobtn.Foreground = Brushes.Gray;
            }
        }

    }
}
