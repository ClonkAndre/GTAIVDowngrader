using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GTAIVDowngrader.Dialogs
{
    public partial class FinishUC : UserControl
    {

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

        #region Methods
        private void CreateRecommendedCommandline()
        {
            if (!Core.IsInSimpleMode)
                return;

            string path = string.Format("{0}\\commandline.txt", Core.CurrentDowngradingInfo.IVWorkingDirectoy);
            
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("-nomemrestrict");
                builder.AppendLine("-norestrictions");
                File.WriteAllText(path, builder.ToString());

                Core.AddLogItem(LogType.Info, "Created a recommended commandline.");
            }
            catch (Exception ex)
            {
                Core.AddLogItem(LogType.Error, string.Format("Failed to create recommended commandline! Details: {0}", ex));
            }
        }
        private void CreateLogFile()
        {
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
                Core.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "An unknown error occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
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
            string fileLoc = string.Format("{0}\\PlayGTAIV.exe", Core.CurrentDowngradingInfo.IVWorkingDirectoy);
            try
            {
                if (File.Exists(fileLoc))
                {
                    Process process = new Process();
                    process.StartInfo.FileName = fileLoc;
                    process.StartInfo.WorkingDirectory = Core.CurrentDowngradingInfo.IVWorkingDirectoy;
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

            instance.UpdateOverallProgress(true);

            if (Core.AreThereAnySupporters())
            {
                // Add them to their container
                if (Core.Tier3Supporter.Count != 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    for (int i = 0; i < Core.Tier3Supporter.Count; i++)
                    {
                        if (i == (Core.Tier3Supporter.Count - 1))
                            stringBuilder.Append(Core.Tier3Supporter[i]);
                        else
                            stringBuilder.Append(Core.Tier3Supporter[i] + ", ");
                    }

                    Tier3SupportersTextBlock.Text = stringBuilder.ToString();
                }

                if (Core.Tier2Supporter.Count != 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    for (int i = 0; i < Core.Tier2Supporter.Count; i++)
                    {
                        if (i == (Core.Tier2Supporter.Count - 1))
                            stringBuilder.Append(Core.Tier2Supporter[i]);
                        else
                            stringBuilder.Append(Core.Tier2Supporter[i] + ", ");
                    }

                    Tier2SupportersTextBlock.Text = stringBuilder.ToString();
                }

                if (Core.Tier1Supporter.Count != 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    for (int i = 0; i < Core.Tier1Supporter.Count; i++)
                    {
                        if (i == (Core.Tier1Supporter.Count - 1))
                            stringBuilder.Append(Core.Tier1Supporter[i]);
                        else
                            stringBuilder.Append(Core.Tier1Supporter[i] + ", ");
                    }

                    Tier1SupportersTextBlock.Text = stringBuilder.ToString();
                }

                SupportersGrid.Visibility = Visibility.Visible;
            }

            if (Core.Is420())
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char2.png", UriKind.Relative));
            if (Core.IsPrideMonth)
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char9.png", UriKind.Relative));

            // Create stuff
            CreateRecommendedCommandline();
            CreateLogFile();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
