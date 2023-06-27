using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Newtonsoft.Json;
using CCL;

namespace GTAIVDowngrader.Dialogs
{
    public partial class MD5FilesCheckerUC : UserControl
    {

        #region Variables
        private MainWindow instance;

        private List<string> testLocations;
        #endregion

        #region Constructor
        public MD5FilesCheckerUC()
        {
            testLocations = new List<string>();
            InitializeComponent();
        }
        public MD5FilesCheckerUC(MainWindow window)
        {
            testLocations = new List<string>();
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void SetStatusText(string str)
        {
            Dispatcher.Invoke(() => {
                StatusLabel.Text = str;
            });
        }

        private void SetNavigationButtonsEnabledState(bool enabled)
        {
            Dispatcher.Invoke(() => {

                if (!Core.GotStartedWithValidCommandLineArgs)
                    instance.BackButton.IsEnabled = enabled;

                instance.NextButton.IsEnabled = enabled;

            });
        }
        private void NextStep()
        {
            AResult<bool> result = CheckCurrentLocation(Core.CDowngradingInfo.IVWorkingDirectoy);

            if (result.Result)
                instance.NextStep(2);
            else
                instance.NextStep(0, new List<object> { result.Exception.Message });
        }

        /// <summary></summary>
        /// <param name="imgToSet">
        /// 0 = Info Symbol<br/>
        /// 1 = Check Symbol<br/>
        /// 2 = Warning Symbol<br/>
        /// 3 = Error Red Symbol
        /// </param>
        private void SetStatusImage(int imgToSet)
        {
            Dispatcher.Invoke(() => {
                switch (imgToSet)
                {
                    case 0: // Info Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\infoWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                    case 1: // Check Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\checkCircleWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                    case 2: // Warning Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                    case 3: // Error Red Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\errorWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            });
        }

        /// <summary></summary>
        /// <param name="state">
        /// 0 = Working<br/>
        /// 1 = Finished<br/>
        /// 2 = Errored<br/>
        /// 3 = Unknown
        /// </param>
        private void SetProgressBarState(int state)
        {
            Dispatcher.Invoke(() => {
                switch (state)
                {
                    case 0: // Working
                        StatusProgressBar.Foreground = (Brush)Core.CBrushConverter.ConvertFrom("#0050BF");
                        StatusProgressBar.IsIndeterminate = true;
                        break;
                    case 1: // Finished
                        StatusProgressBar.Foreground = Brushes.Green;
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Value = 100;
                        break;
                    case 2: // Errored
                        StatusProgressBar.Foreground = Brushes.Red;
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Value = 100;
                        break;
                    case 3: // Unknown
                        StatusProgressBar.Foreground = Brushes.Yellow;
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Value = 100;
                        break;
                }
            });
        }
        #endregion

        #region Functions
        private AResult<bool> CheckCurrentLocation(string loc)
        {
            string root = Path.GetPathRoot(loc);
            string path = loc.ToLower();

            // Check if GTA IV is in a program files folder
            if (path.Contains("program files") || path.Contains("program files (x86)"))
                return new AResult<bool>(new Exception("GTA IV is in one of the Program Files folder which it shouldn't be!"), false); // Return check result

            // Check if GTA IV is on drive A:\ or B:\
            if (root == @"A:\" || root == @"B:\")
                return new AResult<bool>(new Exception(@"GTA IV is on drive A:\ or B:\ which it shouldn't be!"), false);  // Return check result

            // Check permissions
            string testFolderLoc = string.Format("{0}\\DowngraderPermissionTestDir", path).ToLower();
            testLocations.Add(testFolderLoc);

            bool result = false;
            try
            {
                Directory.CreateDirectory(testFolderLoc);
                File.WriteAllText(testFolderLoc + "\\test.txt", "This is a test.");

                // Passed test
                result = true;
            }
            catch (Exception)
            {
                // Failed test
                result = false;
            }

            // Remove test files
            for (int i = 0; i < testLocations.Count; i++)
            {
                string testLoc = testLocations[i];
                if (Directory.Exists(testLoc)) Directory.Delete(testLoc, true);
                testLocations.RemoveAt(i);
            }

            return new AResult<bool>(new Exception("The current GTA IV directory has insufficient permissions!"), result); // Return check result
        }
        #endregion

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep();
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            NextStep();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                instance.NextButtonClicked += Instance_NextButtonClicked;
                instance.BackButtonClicked += Instance_BackButtonClicked;

                instance.ChangeActionButtonVisiblity(true, true, false, true);

                // Log more application Information
                Core.AddLogItem(LogType.Info, string.Format("In Offline Mode: {0}", Core.IsInOfflineMode));
                if (Core.GotStartedWithValidCommandLineArgs)
                    Core.AddLogItem(LogType.Info, string.Format("Downgrader got started with commandline argument to path: {0}", Core.CommandLineArgPath));

                if (Core.GotStartedWithValidCommandLineArgs)
                    instance.BackButton.IsEnabled = false;

                // Check offline mode
                if (Core.IsInOfflineMode)
                {
                    string md5HashesFilePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\md5Hashes.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                    if (!File.Exists(md5HashesFilePath))
                    {
                        Core.Notification.ShowNotification(NotificationType.Warning, 5000, "MD5 Hashes not available", "Skipping step.", "MD5_HASH_FILE_DOES_NOT_EXISTS");
                        NextStep();
                        return;
                    }
                    else
                    {
                        // Load already existing hashes
                        Core.MD5Hashes = JsonConvert.DeserializeObject<List<JsonObjects.MD5Hash>>(File.ReadAllText(md5HashesFilePath));
                        
                        Core.Notification.ShowNotification(NotificationType.Warning, 9000, "Offline Mode Information", "Can't update Md5 Hashes because of offline mode. Using already existing ones. Hashes might be outdated and result in a wrong result.", "OUTDATED_HASHES_WARNING");
                    }
                }

                SetNavigationButtonsEnabledState(false);
                SetStatusText("Creating and comparing MD5 Hash...");
                SetStatusImage(0);
                SetProgressBarState(0);

                // Get file version of the selected executable file and the related MD5 Hash
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Core.CDowngradingInfo.IVExecutablePath);
                string fileVersion = (fvi != null && fvi.FileVersion != null) ? fvi.FileVersion.Replace(",", ".").Replace(" ", "") : "";
                List<string> relatedMD5Hashes = Core.GetMD5HashesFromVersion(fileVersion);

                Task.Run(() => {
                    return FileHelper.GetMD5StringFromFolder(Core.CDowngradingInfo.IVWorkingDirectoy, new List<string>() { "installscript.vdf", "installscript_sdk.vdf" }); // Get MD5 from selected directory
                }).ContinueWith((r) => {
                    AResult<string> result = r.Result;

                    SetNavigationButtonsEnabledState(true);

                    if (result.Exception == null)
                    {
                        if (result.Result != null)
                        {

                            if (relatedMD5Hashes != null)
                            {
                                // Generated Hash is equal to one of the related Hashes. Version is NOT modified.
                                if (relatedMD5Hashes.Contains(result.Result))
                                {

                                    SetStatusImage(1);
                                    SetProgressBarState(1);
                                    SetStatusText("No problems found while generating and comparing MD5 Hash. To continue, press the Next button.");

                                    // Set Hashes for log file
                                    Core.CDowngradingInfo.SetReceivedMD5Hash(result.Result);
                                    Core.CDowngradingInfo.SetRelatedMD5Hash(result.Result);

                                    if (Core.GotStartedWithValidCommandLineArgs)
                                        NextStep();

                                }
                                else // Generated Hash is NOT equal to any of the seemingly related Hashes! This means that the version is modified.
                                {

                                    SetStatusImage(2);
                                    SetProgressBarState(3);
                                    SetStatusText(string.Format("WARNING: MD5 Hash of version {1} does not match any of the expected MD5 Hashes!{0}{0}" +
                                        "- What does this mean?{0}" +
                                        "This means that the selected directory of GTA IV is probably modified (contains mods) and it is HIGHLY recommended to downgrade a fresh, unmodified copy of GTA IV.{0}{0}" +
                                        "- What now?{0}" +
                                        "To get the best downgrading experience, redownload GTA IV, and downgrade the freshly downloaded copy of GTA IV.{0}{0}" +
                                        "If you are sure that there are NO mods in the selected directory, you can safely continue by pressing the Next button. " +
                                        "Please also consider sending the log file created by the downgrader at the end of the downgrading process in our Discord server to help improve the GTA IV Downgrader. Thanks!", Environment.NewLine, fileVersion));

                                    // Set Hashes for log file
                                    Core.CDowngradingInfo.SetReceivedMD5Hash(result.Result);
                                    Core.CDowngradingInfo.SetRelatedMD5Hash(string.Format("No related MD5 Hash found for this GTA IV {0} installation.", fileVersion));

                                }
                            }
                            else // Unknown version
                            {

                                SetStatusImage(2);
                                SetProgressBarState(3);
                                SetStatusText(string.Format("Could not compare MD5 for version {0} of GTA IV. This probably means that the selected GTAIV.exe is not 1.2.0.43 (or higher). Please note that this downgrader is mainly used to downgrade from version 1.2.0.43 (or higher) to 1.0.8.0, 1.0.7.0 or 1.0.4.0. However, this does not stop you from downgrading. To continue, press the Next button.", string.IsNullOrEmpty(fileVersion) ? "UNKNOWN" : fileVersion));

                                // Set Hashes for log file
                                Core.CDowngradingInfo.SetReceivedMD5Hash(result.Result);
                                Core.CDowngradingInfo.SetRelatedMD5Hash(string.Format("No MD5 Hash found for version {0} of GTA IV.", fileVersion));

                            }

                            return;
                        }
                        else
                        {

                            SetStatusText("An unknown error occured while creating MD5 Hash from directory. However, this does not stop you from downgrading. To continue, press the Next button.");

                            // Log
                            Core.AddLogItem(LogType.Error, "An unknown error occured while creating MD5 Hash from directory.");

                        }
                    }
                    else
                    {

                        SetStatusText(string.Format("An error occured while creating MD5 Hash from directory. However, this does not stop you from downgrading. To continue, press the Next button.{0}Details: {1}", Environment.NewLine, result.Exception.Message));

                        // Log
                        Core.AddLogItem(LogType.Error, string.Format("An error occured while creating MD5 Hash from directory. Details: {0}", result.Exception.Message));

                    }

                    SetStatusImage(3);
                    SetProgressBarState(2);
                });
            }
            catch (Exception ex)
            {
                SetNavigationButtonsEnabledState(true);
                SetStatusText(string.Format("Error while creating MD5 Hash from directory. However, this does not stop you from downgrading. To continue, press the Next button.{0}Details: {1}", Environment.NewLine, ex.StackTrace));
                SetStatusImage(3);
                SetProgressBarState(2);

                // Log
                Core.AddLogItem(LogType.Error, string.Format("Error while creating MD5 Hash from directory. Details: {0}", ex.StackTrace));
            }
        }

    }
}
