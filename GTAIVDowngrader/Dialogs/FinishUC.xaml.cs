using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class FinishUC : UserControl {

        #region Variables
        private MainWindow instance;
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
            string logFile = instance.downgradingUC.latestLogFileName;
            if (File.Exists(logFile)) {
                Process.Start(logFile);
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
