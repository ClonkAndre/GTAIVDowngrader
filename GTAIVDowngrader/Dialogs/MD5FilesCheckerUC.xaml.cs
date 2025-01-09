using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using CCL;

using GTAIVDowngrader.Classes;

namespace GTAIVDowngrader.Dialogs
{ 
    public partial class MD5FilesCheckerUC : UserControl
    {

        #region Variables and Enums
        // Variables
        private MainWindow instance;
        private List<string> testLocations;

        // Enums
        private enum Icon
        {
            Info,
            Check,
            Warning,
            Error
        }
        private enum ProgressBarState
        {
            Working,
            Finished,
            Errored,
            Unknown
        }
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
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Text = str;
            });
        }

        private void SetNavigationButtonsEnabledState(bool enabled)
        {
            Dispatcher.Invoke(() =>
            {

                if (!Core.GotStartedWithValidCommandLineArgs)
                    instance.BackButton.IsEnabled = enabled;

                instance.NextButton.IsEnabled = enabled;

            });
        }
        private void NextStep()
        {
            AResult<bool> result = CheckCurrentLocation(DowngradingInfo.IVWorkingDirectoy);

            if (result.Result)
                instance.NextStep(2);
            else
                instance.NextStep(0, new List<object> { result.Exception.Message });
        }

        /// <summary>
        /// 0 = Info Symbol<br/>
        /// 1 = Check Symbol<br/>
        /// 2 = Warning Symbol<br/>
        /// 3 = Error Red Symbol
        /// </summary>
        private void SetStatusImage(Icon iconToSet)
        {
            Dispatcher.Invoke(() =>
            {
                switch (iconToSet)
                {
                    case Icon.Info: // Info Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\infoWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                    case Icon.Check: // Check Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\checkCircleWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                    case Icon.Warning: // Warning Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\warningWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                    case Icon.Error: // Error Red Symbol
                        StatusImage.Source = new BitmapImage(new Uri(@"..\Resources\errorWhite.png", UriKind.RelativeOrAbsolute));
                        break;
                }
            });
        }

        /// <summary>
        /// 0 = Working<br/>
        /// 1 = Finished<br/>
        /// 2 = Errored<br/>
        /// 3 = Unknown
        /// </summary>
        private void SetProgressBarState(ProgressBarState state)
        {
            Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case ProgressBarState.Working: // Working
                        StatusProgressBar.Foreground = "#0050BF".ToBrush();
                        StatusProgressBar.IsIndeterminate = true;
                        break;
                    case ProgressBarState.Finished: // Finished
                        StatusProgressBar.Foreground = Brushes.Green;
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Value = 100;
                        break;
                    case ProgressBarState.Errored: // Errored
                        StatusProgressBar.Foreground = Brushes.Red;
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Value = 100;
                        break;
                    case ProgressBarState.Unknown: // Unknown
                        StatusProgressBar.Foreground = Brushes.Yellow;
                        StatusProgressBar.IsIndeterminate = false;
                        StatusProgressBar.Value = 100;
                        break;
                }
            });
        }
        #endregion

        #region Functions
        // TODO: This function needs to be fixed in CCL!
        private AResult<string> GetMD5StringFromFolder(string folder, List<string> ignoredFiles = null)
        {
            try
            {
                List<string> files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly).ToList();

                using (MD5 md5 = MD5.Create())
                {
                    // Go through ignored files list and remove entries that should be ignored
                    if (ignoredFiles != null)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            string fileName = Path.GetFileName(files[i]).ToLower();

                            if (ignoredFiles.Contains(fileName))
                            {
                                files.RemoveAt(i);
                                i--;
                            }
                        }
                    }

                    // Generate hash from all files in directory
                    for (int i = 0; i < files.Count; i++)
                    {
                        string file = files[i];

                        // Hash path
                        string realtivePath = file.Substring(folder.Length + 1);
                        byte[] pathBytes = Encoding.UTF8.GetBytes(realtivePath.ToLower());
                        md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                        // Hash contents
                        byte[] contentBytes = File.ReadAllBytes(file);
                        if (contentBytes == null) return new AResult<string>(new ArgumentNullException("contentBytes was null."), null);

                        if (i == (files.Count - 1))
                            md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                        else
                            md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);

                    }

                    return new AResult<string>(null, BitConverter.ToString(md5.Hash).Replace("-", "").ToLower());
                }
            }
            catch (Exception ex)
            {
                return new AResult<string>(ex, null);
            }
        }
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
            if (Core.IsInSimpleMode)
            {
                instance.NextStep(2);
                return;
            }
            if (Core.SkipMD5HashStep)
            {
                instance.NextStep(2);
                return;
            }

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
                    if (!Core.LoadExistingMD5Hashes())
                    {
                        Core.Notification.ShowNotification(NotificationType.Warning, 5000, "MD5 Hashes not available", "Skipping step.", "MD5_HASH_FILE_DOES_NOT_EXISTS");
                        NextStep();
                        return;
                    }

                    Core.Notification.ShowNotification(NotificationType.Warning, 9000, "Offline Mode Information", "Unable to fetch the latest MD5 hashes due to offline mode. Using existing hashes, which may be outdated and could lead to incorrect results.", "OUTDATED_HASHES_WARNING");
                }

                SetNavigationButtonsEnabledState(false);
                SetStatusText("Creating and comparing MD5 Hash...");
                SetStatusImage(Icon.Info);
                SetProgressBarState(ProgressBarState.Working);

                // Start creating MD5 Hash from selected directory
                Task.Run(() =>
                {

                    return GetMD5StringFromFolder(DowngradingInfo.IVWorkingDirectoy, new List<string>()
                    {
                        "876bd1d9393712ac.bin",
                        "playgtaiv.exe",
                        "gta4browser.exe",
                        "installscript.vdf",
                        "installscript_sdk.vdf",
                        "dfa.dll",
                        "steam_api.dll",
                        "title.rgl"
                    });
                
                }).ContinueWith((r) =>
                {
                    AResult<string> result = r.Result;

                    SetNavigationButtonsEnabledState(true);

                    // Validate stuff
                    if (result.Exception != null)
                    {
                        // Log
                        Core.AddLogItem(LogType.Error, string.Format("An error occured while creating MD5 Hash from directory. Details: {0}", result.Exception.Message));

                        // Set stuff
                        SetStatusText(string.Format("An error occured while creating MD5 Hash from directory. However, this does not stop you from downgrading. To continue, press the Next button.{0}" +
                            "Details: {1}", Environment.NewLine, result.Exception.Message));

                        SetStatusImage(Icon.Error);
                        SetProgressBarState(ProgressBarState.Errored);

                        return;
                    }
                    if (result.Result == null)
                    {
                        // Log
                        Core.AddLogItem(LogType.Error, "An unknown error occured while creating MD5 Hash from directory.");
                        
                        // Set stuff
                        SetStatusText("An unknown error occured while creating MD5 Hash from directory. However, this does not stop you from downgrading. To continue, press the Next button.");

                        SetStatusImage(Icon.Error);
                        SetProgressBarState(ProgressBarState.Errored);


                        return;
                    }

#if DEBUG
                    System.Threading.Thread l = new System.Threading.Thread(() =>
                    {
                        Clipboard.SetText(result.Result);
                    });
                    l.SetApartmentState(System.Threading.ApartmentState.STA);
                    l.Start();
                    l.Join();
                    
                    Core.Notification.ShowNotification(NotificationType.Info, 5000, "MD5 Copied", "The generated MD5 Hash was copied to clipboard.");
#endif

                    // Generated Hash exists in the MD5Hashes list.
                    // Game is visibly unmodified.
                    if (Core.HasMD5Hash(result.Result))
                    {

                        // Set stuff
                        SetStatusImage(Icon.Check);
                        SetProgressBarState(ProgressBarState.Finished);

                        SetStatusText(string.Format("The generated MD5 Hash from your selected directory matches with a pre-generated one which was generated from a clean copy of GTA IV!{0}{0}" +
                            "To continue, press the Next button.", Environment.NewLine));

                        // Set Hash for log file
                        DowngradingInfo.SetGeneratedMD5Hash(result.Result);

                        // Automatically get to the next step when started using valid commandline args
                        if (Core.GotStartedWithValidCommandLineArgs)
                            NextStep();

                    }
                    else // Generated Hash was not found in the MD5Hashes list. Directory could be modified.
                    {

                        // Set stuff
                        SetStatusImage(Icon.Warning);
                        SetProgressBarState(ProgressBarState.Unknown);

                        SetStatusText(string.Format("Could not find any MD5 Hashes for your selected directory!{0}{0}" +
                            "- What does this mean?{0}" +
                            "This means that your selected GTA IV directory is probably modified (contains mods) and it is HIGHLY recommended to downgrade a fresh, unmodified copy of GTA IV.{0}{0}" +
                            "- What now?{0}" +
                            "To get the best downgrading experience, redownload GTA IV, and downgrade the freshly downloaded copy of GTA IV.{0}{0}" +
                            "If you are sure that there are NO mods in your selected directory, you can safely continue by pressing the Next button. " +
                            "Please consider sending the log file created by the IV Downgrader at the end of the downgrading process in our Discord server to help improve the IV Downgrader!", Environment.NewLine));

                        // Set Hashes for log file
                        DowngradingInfo.SetGeneratedMD5Hash(result.Result);

                    }

                });
            }
            catch (Exception ex)
            {
                // Log
                Core.AddLogItem(LogType.Error, string.Format("Error while creating MD5 Hash from directory. Details: {0}", ex));

                // Set stuff
                SetNavigationButtonsEnabledState(true);
                SetStatusText(string.Format("Error while creating MD5 Hash from directory. However, this does not stop you from downgrading.{0}" +
                    "Please report this issue to the developer.{0}" +
                    "To continue, press the Next button.{0}{0}" +
                    "Details: {1}", Environment.NewLine, ex.StackTrace));
                SetStatusImage(Icon.Error);
                SetProgressBarState(ProgressBarState.Errored);
            }
        }

    }
}
