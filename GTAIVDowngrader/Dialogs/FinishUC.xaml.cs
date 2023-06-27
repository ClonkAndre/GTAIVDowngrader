using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        #region Events
        private bool Instance_ExitButtonClicked()
        {
            if (File.Exists(latestLogFileName))
                Process.Start(latestLogFileName);
            else
                Core.Notification.ShowNotification(NotificationType.Warning, 4000, "Log file does not exists", "Could not open log file because it does not exists.", "FILE_DOES_NOT_EXISTS");

            return true;
        }
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            string fileLoc = string.Format("{0}\\PlayGTAIV.exe", Core.CDowngradingInfo.IVWorkingDirectoy);
            try
            {
                if (File.Exists(fileLoc))
                {
                    Process process = new Process();
                    process.StartInfo.FileName = fileLoc;
                    process.StartInfo.WorkingDirectory = Core.CDowngradingInfo.IVWorkingDirectoy;
                    process.Start();
                }
                else
                {
                    MessageBox.Show("Couldn't find PlayGTAIV.exe", "GTAIVDowngrader.exe");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while trying to start GTA IV. Details: " + ex.ToString(), "GTAIVDowngrader.exe");
            }

            Environment.Exit(0);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;
            instance.ExitButtonClicked += Instance_ExitButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            instance.ExitButton.Content = "Show Log";
            instance.BackButton.Content = "Play GTA IV";
            instance.NextButton.Content = "Exit";

            if (Core.IsPrideMonth)
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char9.png", UriKind.Relative));

            // Create log file
            try
            {
                string logFolder = ".\\Data\\Logs";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                latestLogFileName = string.Format("{0}\\Log.{1}.{2}_{3}_{4}.log", logFolder, DateTime.Now.Year.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString());
                File.WriteAllLines(latestLogFileName, Core.LogItems);
            }
            catch (UnauthorizedAccessException)
            {
                Core.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A UnauthorizedAccessException occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
            catch (Exception)
            {
                Core.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A unknown error occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
