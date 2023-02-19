using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class FinishUC : UserControl {

        #region Variables
        private MainWindow instance;
        private string latestLogFileName;
        #endregion

        #region Constructor
        public FinishUC()
        {
            InitializeComponent();
        }
        public FinishUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
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

            // Create log file
            try {
                string logFolder = ".\\Data\\Logs";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                latestLogFileName = string.Format("{0}\\Log.{1}.{2}_{3}_{4}.log", logFolder, DateTime.Now.Year.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString());
                File.WriteAllLines(latestLogFileName, MainFunctions.logItems);
            }
            catch (UnauthorizedAccessException) {
                MainFunctions.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A UnauthorizedAccessException occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
            catch (Exception) {
                MainFunctions.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A unknown error occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string fileLoc = string.Format("{0}\\PlayGTAIV.exe", MainFunctions.downgradingInfo.IVWorkingDirectoy);
            try {
                if (File.Exists(fileLoc)) {
                    Process process = new Process();
                    process.StartInfo.FileName = fileLoc;
                    process.StartInfo.WorkingDirectory = MainFunctions.downgradingInfo.IVWorkingDirectoy;
                    process.Start();
                    instance.Close();
                }
                else {
                    MessageBox.Show("Couldn't find PlayGTAIV.exe", "GTAIVDowngrader.exe");
                    instance.Close();
                }
            }
            catch (Exception ex) {
                MessageBox.Show("An error occured while trying to start GTA IV. Details: " + ex.ToString(), "GTAIVDowngrader.exe");
                instance.Close();
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.Close();
        }

        private void ShowLog_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(latestLogFileName)) {
                Process.Start(latestLogFileName);
            }
            else {
                MainFunctions.Notification.ShowNotification(NotificationType.Warning, 4000, "Log file does not exists", "Could not open log file because it does not exists.", "FILE_DOES_NOT_EXISTS");
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            MainFunctions.AskUserToOpenURL(e.Uri);
        }

    }
}
