using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using CCL;

namespace GTAIVDowngrader.Dialogs
{
    public partial class ErrorUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        private Exception exception;

        private string logFilePath;
        #endregion

        #region Methods
        private void SetPossibleSolutions()
        {
            if (exception is UnauthorizedAccessException) // admin
            {
                DetailsLabel.Text += string.Format("{0}{0}- - - Possible Solutions - - -{0}" +
                    "● Please try restarting the downgrader with administrator privileges.", Environment.NewLine);
            }
            else if (exception is InvalidDataException) // files corrupt
            {
                DetailsLabel.Text += string.Format("{0}{0}The downgrading files seem to be corrupted, this can be the case when the download unexpectedly fails.{0}" +
                    "- - - Possible Solutions - - -{0}" +
                    "● Make sure you have a stable internet connection.{0}" +
                    "● Delete the whole Temp folder located in the Data folder to force a redownload.{0}" +
                    "● Try to downgrade again.{0}" +
                    "● Download the files required for downgrading manually and place them in the Temp folder. Download link can be found on the GTA IV Downgrader GTAForums post.", Environment.NewLine);
            }
            else if (exception is WebException || exception is SocketException) // download error
            {
                DetailsLabel.Text += string.Format("{0}{0}- - - Possible Solutions - - -{0}" +
                    "● Make sure you have a stable internet connection.{0}" +
                    "● If you have a VPN try to disable/enable it and try again.{0}" +
                    "● If you're running on a proxy, try to disable it and try again.", Environment.NewLine);
            }
        }
        #endregion

        #region Constructor
        public ErrorUC(MainWindow mainWindow, Exception ex, List<string> additionalInfos = null)
        {
            instance = mainWindow;
            exception = ex;
            InitializeComponent();

            Core.LogItems.Add("");
            Core.LogItems.Add("- - - AN EXCEPTION OCCURED - - -");

            // Set text
            if (additionalInfos == null)
            {
                DetailsLabel.Text = string.Format("A {1} occured.{0}{2}", Environment.NewLine, exception.GetType().Name, exception.Message);

                // Log error
                Core.LogItems.Add("Type: " + exception.GetType().Name);
                Core.LogItems.Add("Source: " + exception.Source);
                Core.LogItems.Add("TargetSite.Name: " + exception.TargetSite.Name);
                Core.LogItems.Add("Message: " + exception.Message);
                Core.LogItems.Add("");
                Core.LogItems.Add(exception.ToString());
            }
            else
            {
                string str = string.Empty;

                for (int i = 0; i < additionalInfos.Count; i++)
                    str += additionalInfos[i];

                DetailsLabel.Text = string.Format("A {1} occured.{0}{2}{0}{0}- - - Additional Information - - -{0}{3}", Environment.NewLine, exception.GetType().Name, exception.Message, str);

                // Log error
                Core.LogItems.Add("Type: " + exception.GetType().Name);
                Core.LogItems.Add("Source: " + exception.Source);
                Core.LogItems.Add("TargetSite.Name: " + exception.TargetSite.Name);
                Core.LogItems.Add("Message: " + exception.Message);
                Core.LogItems.Add("");
                Core.LogItems.Add(exception.ToString());
                Core.LogItems.Add("");
                Core.LogItems.Add("- - - Additional Information - - -");
                Core.LogItems.Add(str);
            }

            // Write and save log file
            try
            {
                string logFolder = ".\\Data\\Logs";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                logFilePath = string.Format("{0}\\Log.{1}.{2}_{3}_{4}.log", logFolder, DateTime.Now.Year.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString());
                File.WriteAllLines(logFilePath, Core.LogItems);
            }
            catch (UnauthorizedAccessException)
            {
                Core.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A UnauthorizedAccessException occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
            catch (Exception)
            {
                Core.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A unknown error occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }

            // Check exception type
            if (exception is UnauthorizedAccessException)
                instance.ChangeActionButtonVisiblity(true, true, false, true);
            else
                instance.ChangeActionButtonVisiblity(true, false, false, true);

            SetPossibleSolutions();
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            if (File.Exists(logFilePath))
                Process.Start(logFilePath);
            else
                Core.Notification.ShowNotification(NotificationType.Error, 5000, "Could not open log file", "File does not exists.", "COULD_NOT_OPEN_LOG_FILE");
        }
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            try
            {
                string assemblyLoc = Assembly.GetExecutingAssembly().Location;

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = assemblyLoc;
                    proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(assemblyLoc);
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                    proc.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Could not restart downgrader with admin rights. Details: {0}. Closing downgrader.", ex.ToString()), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Environment.Exit(0);
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.BackButtonClicked += Instance_BackButtonClicked;
            instance.NextButtonClicked += Instance_NextButtonClicked;

            instance.ChangeActionButtonVisiblity(true, false, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            instance.NextButton.Content = "Open Log File";
            instance.BackButton.Content = "Restart with admin rights";

            instance.GetMainProgressBar().Foreground = "#B3bd0000".ToBrush();
            instance.GetMainProgressBar().Value = 100;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string logsDir = ".\\Data\\Logs";
            if (Directory.Exists(logsDir))
                Process.Start(logsDir);
            else
                Core.Notification.ShowNotification(NotificationType.Error, 4000, "Logs Folder not found", "The Logs Folder does not exists for whatever Reason.", "LOGS_FOLDER_DOES_NOT_EXISTS");
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
