using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class ErrorUC : UserControl {

        #region Variables
        private MainWindow instance;
        private Exception exception;
        #endregion

        #region Constructor
        public ErrorUC(MainWindow mainWindow, Exception ex, List<string> additionalInfos = null)
        {
            instance = mainWindow;
            exception = ex;
            InitializeComponent();

            // Set text
            if (additionalInfos == null) {
                DetailsLabel.Text = string.Format("{1}", Environment.NewLine, exception.ToString());
            }
            else {
                string str = string.Empty;

                for (int i = 0; i < additionalInfos.Count; i++) {
                    str += additionalInfos[i];
                }

                DetailsLabel.Text = string.Format("{1}{0}{0}Additional Informations{0}{2}", Environment.NewLine, exception.ToString(), str);
            }

            // Check exception type
            if (ex is UnauthorizedAccessException) {
                RestartAdminButton.Visibility = Visibility.Visible;
            }
            else {
                RestartAdminButton.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainFunctions.isPrideMonth) {
                if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                    BottomGrid.Background = "#B3000000".ToBrush();
                }
                else { // Use Rainbow Colours
                    BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.Close();
        }
        private void RestartAdminButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                string assemblyLoc = Assembly.GetExecutingAssembly().Location;

                using (Process proc = new Process()) {
                    proc.StartInfo.FileName = assemblyLoc;
                    proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(assemblyLoc);
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                    proc.Start();
                }

                instance.ShowExitMsg(true);
            }
            catch (Exception ex) {
                MessageBox.Show(string.Format("Could not restart downgrader with admin rights. Details: {0}. Closing downgrader.", ex.ToString()), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                instance.ShowExitMsg(true);
            }
        }
        private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DetailsLabel.Text);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string logsDir = ".\\Data\\Logs";
            if (Directory.Exists(logsDir)) {
                Process.Start(logsDir);
            }
            else {
                MainFunctions.Notification.ShowNotification(NotificationType.Error, 4000, "Logs Folder not found", "The Logs Folder does not exists for whatever Reason.", "LOGS_FOLDER_DOES_NOT_EXISTS");
            }
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            MainFunctions.AskUserToOpenURL(e.Uri);
        }

    }
}
