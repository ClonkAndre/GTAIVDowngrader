using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Newtonsoft.Json;
using GTAIVDowngrader.Dialogs;
using static Helper;

namespace GTAIVDowngrader {

    #region Public Enums
    public enum Steps
    {
        S0_Welcome = 0,
        S1_SelectIVExe,
        S2_MD5FilesChecker,
        S3_MoveGameFilesQuestion,
        S4_MoveGameFiles,
        S5_SelectDwngrdVersion,
        S6_Multiplayer,
        S7_SelectRadioDwngrd,
        S7_1_SelectVladivostokType,
        S8_SelectComponents,
        S9_Confirm,
        S10_Downgrade,
        S11_SavefileDowngrade,
        S12_Commandline,
        S13_Finish,

        StandaloneWarning,
        Error
    }
    public enum LogType
    {
        Info,
        Warning,
        Error
    }
    public enum GameVersion
    {
        v1080,
        v1070,
        v1040
    }
    public enum ModVersion
    {
        All = 3,
        v1080 = 0,
        v1070 = 1,
        v1040 = 2,
    }
    public enum RadioDowngrader
    {
        None,
        SneedsDowngrader,
        LegacyDowngrader
    }
    public enum VladivostokTypes
    {
        None,
        New,
        Old
    }
    public enum ModType
    {
        ASILoader,
        ASIMod,
        ScriptHook,
        ScriptHookMod,
        ScriptHookHook,
        ScriptHookDotNet,
        ScriptHookDotNetMod
    }
    public enum NotificationType {
        Info,
        Warning,
        Error,
        Success
    }
    #endregion

    public partial class MainWindow : Window {

        #region Variables
        // Other
        private bool finishedDownloadingFileInfo, finishedDownloadingMD5Hashes;

        // Current step/dialog
        public Steps currentStep;

        // All steps/dialogs
        public WelcomeUC welcomeUC;
        public SelectIVExeUC selectIVExeUC;
        public MD5FilesCheckerUC md5FilesCheckerUC;
        public MoveGameFilesQuestionUC moveGameFilesQuestionUC;
        public MoveGameFilesUC moveGameFilesUC;
        public SelectDwngrdVersionUC selectDwngrdVersionUC;
        public MultiplayerUC multiplayerUC;
        public SelectRadioDwngrdUC selectRadioDwngrdUC;
        public SelectVladivostokTypeUC selectVladivostokTypeUC;
        public SelectComponentsUC selectComponentsUC;
        public ConfirmUC confirmUC;
        public DowngradingUC downgradingUC;
        public SavefileDowngradeUC savefileDowngradeUC;
        public CommandlineUC commandlineUC;
        public FinishUC finishUC;
        public StandaloneWarningUC standaloneWarningUC;
        public ErrorUC errorUC;
        #endregion

        #region Methods
        public void NextStep(int skip = 0, List<object> args = null)
        {
            Steps next = (Steps)((int)(currentStep + 1) + skip);
            ChangeStep(next, args);
        }
        public void PreviousStep(int skip = 0, List<object> args = null)
        {
            Steps next = (Steps)((int)(currentStep - 1) - skip);
            ChangeStep(next, args);
        }
        public void ChangeStep(Steps next, List<object> args = null)
        {
            Dispatcher.Invoke(() => {
                ContentGrid.Children.Clear();
                currentStep = next;
                switch (next) {
                    case Steps.S0_Welcome:
                        ContentGrid.Children.Add(welcomeUC);
                        break;
                    case Steps.S1_SelectIVExe:
                        ContentGrid.Children.Add(selectIVExeUC);
                        break;
                    case Steps.S2_MD5FilesChecker:
                        ContentGrid.Children.Add(md5FilesCheckerUC);
                        break;
                    case Steps.S3_MoveGameFilesQuestion:
                        ContentGrid.Children.Add(moveGameFilesQuestionUC);
                        moveGameFilesQuestionUC.SetErrorText(args[0].ToString());
                        break;
                    case Steps.S4_MoveGameFiles:
                        ContentGrid.Children.Add(moveGameFilesUC);
                        break;
                    case Steps.S5_SelectDwngrdVersion:
                        ContentGrid.Children.Add(selectDwngrdVersionUC);
                        break;
                    case Steps.S6_Multiplayer:
                        ContentGrid.Children.Add(multiplayerUC);
                        break;
                    case Steps.S7_SelectRadioDwngrd:
                        ContentGrid.Children.Add(selectRadioDwngrdUC);
                        break;
                    case Steps.S7_1_SelectVladivostokType:
                        ContentGrid.Children.Add(selectVladivostokTypeUC);
                        break;
                    case Steps.S8_SelectComponents:
                        ContentGrid.Children.Add(selectComponentsUC);
                        break;
                    case Steps.S9_Confirm:
                        ContentGrid.Children.Add(confirmUC);
                        break;
                    case Steps.S10_Downgrade:
                        ContentGrid.Children.Add(downgradingUC);
                        break;
                    case Steps.S11_SavefileDowngrade:
                        ContentGrid.Children.Add(savefileDowngradeUC);
                        break;
                    case Steps.S12_Commandline:
                        ContentGrid.Children.Add(commandlineUC);
                        break;
                    case Steps.S13_Finish:
                        ContentGrid.Children.Add(finishUC);
                        break;

                        // Warning/Error Dialogs
                    case Steps.StandaloneWarning:
                        if (args == null)
                            return;
                        if (args.Count > 1) {
                            standaloneWarningUC.SetWarning(args[0].ToString(), args[1].ToString());
                        }
                        else {
                            return;
                        }
                        ContentGrid.Children.Add(standaloneWarningUC);
                        break;
                    case Steps.Error:
                        if (args == null)
                            return;
                        if (args.Count > 1) {
                            errorUC = new ErrorUC(this, (Exception)args[0], new List<string>() { args[1].ToString() });
                        }
                        else {
                            errorUC = new ErrorUC(this, (Exception)args[0]);
                        }
                        ContentGrid.Children.Add(errorUC);
                        taskbarItemInfo.ProgressValue = 100;
                        taskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        break;
                }
            });
        }

        public void ShowExitMsg(bool suppressMsg = false)
        {
            if (!suppressMsg) {
                switch (MessageBox.Show("Do you really want to quit?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                    case MessageBoxResult.Yes:
                        Close();
                        break;
                }
            }
            else {
                Close();
            }
        }
        public void ShowErrorScreen(Exception e)
        {
            ChangeStep(Steps.Error, new List<object>() { e });
        }
        #endregion

        #region Events
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ChangeStep(Steps.Error, new List<object>() { e.Exception });
            e.Handled = true;
        }

        private void UpdateChecker_VersionCheckCompleted(MainFunctions.UpdateChecker.VersionCheckInfo result)
        {
            if (result.NewVersionAvailable) {
                switch (MessageBox.Show(string.Format("GTA IV Downgrader version {0} available! Do you want to visit the download page?", result.NewVersion), "New update available!", MessageBoxButton.YesNo, MessageBoxImage.Information)) {
                    case MessageBoxResult.Yes:
                        if (!string.IsNullOrWhiteSpace(result.DownloadURL)) {
                            Process.Start(result.DownloadURL);
                        }
                        else {
                            MessageBox.Show("Could not open download page.", "Error");
                        }
                        break;
                }
            }
            else {
                if (!result.IsSilentCheck) {
                    MainFunctions.Notification.ShowNotification(NotificationType.Info, 4000, "No new update", "There is no new update available at the moment.", "NO_NEW_UPDATE");
                }
            }
        }
        private void UpdateChecker_VersionCheckFailed(Exception e)
        {
            MainFunctions.Notification.ShowNotification(NotificationType.Warning, 4000, "Update check failed", "Could not check for updates.", "UPDATE_CHECK_FAILED");
        }

        private void DownloadWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try {
                if (e.Cancelled)
                    return;

                if (e.Error == null) {
                    string result = e.Result;

                    if (!string.IsNullOrWhiteSpace(result)) {
                        switch (e.UserState.ToString()) {

                            #region IS_PRIDE_MONTH_CHECK
                            case "IS_PRIDE_MONTH_CHECK":

                                MainFunctions.isPrideMonth = e.Result == "1";
                                MainFunctions.downloadWebClient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/main/v1.7_and_up/downgradingFiles.json"), "DOWNGRADING_FILES");

                                break;
                            #endregion

                            #region DOWNGRADING_FILES
                            case "DOWNGRADING_FILES":

                                Console.WriteLine("- - - Downgrading Files - - -");

                                MainFunctions.downgradeFiles = JsonConvert.DeserializeObject<List<JsonObjects.DowngradeInformation>>(result);
                                if (MainFunctions.downgradeFiles.Count != 0) MainFunctions.downgradeFiles.ForEach(d => Console.WriteLine(d.ToString())); // Print to console

                                Console.WriteLine("");
                                MainFunctions.downloadWebClient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/main/v1.7_and_up/md5Hashes.json"), "MD5_HASHES");
                                finishedDownloadingFileInfo = true;

                                break;
                            #endregion

                            #region MD5_HASHES
                            case "MD5_HASHES":

                                Console.WriteLine("- - - MD5 Hashes - - -");

                                MainFunctions.mD5Hashes = JsonConvert.DeserializeObject<List<JsonObjects.MD5Hash>>(result);
                                if (MainFunctions.mD5Hashes.Count != 0) MainFunctions.mD5Hashes.ForEach(hash => Console.WriteLine(hash.ToString())); // Print to console

                                finishedDownloadingMD5Hashes = true;
                                MainFunctions.downloadWebClient.DownloadStringCompleted -= DownloadWebClient_DownloadStringCompleted;

                                break;
                                #endregion

                        }
                    }
                    else {
                        ShowErrorScreen(new Exception("An unknown error occured while trying to retrieve downgrading info."));
                    }
                }
                else {
                    ShowErrorScreen(new Exception(string.Format("An error occured while trying to retrieve downgrading info.{0}{1}", Environment.NewLine, e.Error.ToString())));
                }
            }
            catch (Exception ex) {
                ShowErrorScreen(new Exception(string.Format("An error occured while trying to retrieve downgrading info.{0}{1}", Environment.NewLine, ex.ToString())));
            }
        }
        #endregion

        #region Constructor
        public MainWindow()
        {
            // Check if application got start from within a zip file.
            string startupDir = AppDomain.CurrentDomain.BaseDirectory;
            if (startupDir.Contains("AppData") && startupDir.Contains("Temp")) {
                MessageBox.Show("The GTA IV Downgrader can't be started from within a zip file. Please unextract it somewhere, and then try launching it again. The application will now close.", "Launch error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            // Checks if d3d9.dll exists in current directory.
            if (File.Exists("d3d9.dll")) {
                MessageBox.Show("The GTA IV Downgrader couldn't be started because it conflicts with the d3d9.dll file inside of this directory. " +
                    "Please move the GTA IV Downgrader to another location. The application will now close.", "Launch error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            // Register global exception handler
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            // Initialize Components
            InitializeComponent();

            // Check OS Version
            OperatingSystem osInfo = Environment.OSVersion;
            if (osInfo.Platform == PlatformID.Win32NT) {
                if (osInfo.Version.Major == 6 && osInfo.Version.Minor == 1) { // Windows 7
                    // Apply WebClient Protocol Fix
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                }
            }

            // MainFunctions
            MainFunctions mainFunctions = new MainFunctions(this, "1.7");
            MainFunctions.updateChecker.VersionCheckCompleted += UpdateChecker_VersionCheckCompleted;
            MainFunctions.updateChecker.VersionCheckFailed += UpdateChecker_VersionCheckFailed;
            MainFunctions.downloadWebClient.DownloadStringCompleted += DownloadWebClient_DownloadStringCompleted;

            // Dialogs
            welcomeUC = new WelcomeUC(this);
            selectIVExeUC = new SelectIVExeUC(this);
            md5FilesCheckerUC = new MD5FilesCheckerUC(this);
            moveGameFilesQuestionUC = new MoveGameFilesQuestionUC(this);
            moveGameFilesUC = new MoveGameFilesUC(this);
            selectDwngrdVersionUC = new SelectDwngrdVersionUC(this);
            multiplayerUC = new MultiplayerUC(this);
            selectRadioDwngrdUC = new SelectRadioDwngrdUC(this);
            selectVladivostokTypeUC = new SelectVladivostokTypeUC(this);
            selectComponentsUC = new SelectComponentsUC(this);
            confirmUC = new ConfirmUC(this);
            savefileDowngradeUC = new SavefileDowngradeUC(this);
            commandlineUC = new CommandlineUC(this);
            finishUC = new FinishUC(this);
            downgradingUC = new DowngradingUC(this);
            standaloneWarningUC = new StandaloneWarningUC(this);
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string tempDirLoc = ".\\Data\\Temp";

            if (currentStep == Steps.S10_Downgrade) {
                switch (MessageBox.Show("Do you really want to quit?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                    case MessageBoxResult.Yes:
                        if (Directory.Exists(tempDirLoc)) Directory.Delete(tempDirLoc, true);
                        Environment.Exit(0);
                        break;
                    default:
                        e.Cancel = true;
                        return;
                }
            }

            if (Directory.Exists(tempDirLoc)) Directory.Delete(tempDirLoc, true);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if everything is ok
            if (!Directory.Exists(".\\Data")) {
                ChangeStep(Steps.StandaloneWarning, new List<object>() { "Data folder not found", string.Format("The Downgrader could not find the Data folder that should be next to the GTAIVDowngrader.exe{0}" +
                    "All the important files are in the Data folder for the Downgrader, please reinstall if necessary.", Environment.NewLine) });
                return;
            }

            StringBuilder missingThingsSB = new StringBuilder();
            if (!File.Exists(".\\Data\\bin\\Microsoft.WindowsAPICodePack.dll"))         missingThingsSB.AppendLine("- Microsoft.WindowsAPICodePack.dll");
            if (!File.Exists(".\\Data\\bin\\Microsoft.WindowsAPICodePack.Shell.dll"))   missingThingsSB.AppendLine("- Microsoft.WindowsAPICodePack.Shell.dll");
            if (!File.Exists(".\\Data\\bin\\Newtonsoft.Json.dll"))                      missingThingsSB.AppendLine("- Newtonsoft.Json.dll");

            if (missingThingsSB.Length != 0) { // Something is missing
                ChangeStep(Steps.StandaloneWarning, new List<object>() { "Missing important files", string.Format("The downgrader is missing some important files.{0}" +
                    "The following files are missing:{0}{1}{0}" +
                    "If you downloaded a hotfix and tried to run it without any of those files above, then the downgrader can't run properly and that's why you see this message. Please replace the original GTAIVDowngrader.exe from the full version with the one from the hotfix you've just downloaded. See the gtaforums post, or read the readme for more information.", Environment.NewLine, missingThingsSB.ToString()) });
                return;
            }
            else {
                missingThingsSB = null;
            }

            // Check if you're connected to the internet
            ChangeStep(Steps.StandaloneWarning, new List<object>() { "Checking for an internet connection", "Please wait while we're checking for an internet connection." });
            Task.Run(() => {
                return CheckForInternetConnection();
            }).ContinueWith(t => {
                if (!t.Result) { // No internet connection

                    ChangeStep(Steps.StandaloneWarning, new List<object>() { "No connection to the internet", string.Format("Attempt to check if you're connected to the internet failed.{0}" +
                        "This version of the downgrader requires a internet connection.{0}Please make sure that you're connected to the internet, and then try running the downgrader again.", Environment.NewLine) });

                }
                else { // Internet connection
                    Dispatcher.Invoke(() => {

                        // Download required data
                        MainFunctions.downloadWebClient.DownloadStringAsync(new Uri("https://www.dropbox.com/s/egrkznd2xl7cdd9/isPrideMonth.txt?dl=1"), "IS_PRIDE_MONTH_CHECK");
                        MainFunctions.updateChecker.CheckForUpdates(true);

                        ChangeStep(Steps.StandaloneWarning, new List<object> { "Retrieving informations", "Please wait while the downgrader finished retrieving necessary informations... If you are stuck at this step, please try to restart the downgrader." });

                        Task.Run(() => {
                            while (!finishedDownloadingFileInfo && !finishedDownloadingMD5Hashes) { // Wait till downloads are complete
                                Thread.Sleep(1500);
                            }
                        }).ContinueWith(r => { // Downloads completed

                            // Check commandline args
                            string[] cmdArgs = Environment.GetCommandLineArgs();
                            if (cmdArgs.Length > 1) {
                                string gtaIVExecutablePath = cmdArgs[1];
                                if (!string.IsNullOrWhiteSpace(gtaIVExecutablePath)) {
                                    if (File.Exists(gtaIVExecutablePath)) {
                                        if (gtaIVExecutablePath.ToLower().Contains("gtaiv.exe")) {
                                            MainFunctions.gotStartedWithValidCommandLineArgs = true;
                                            MainFunctions.commandLineArgPath = gtaIVExecutablePath;
                                            MainFunctions.downgradingInfo.SetPath(gtaIVExecutablePath);

                                            ChangeStep(Steps.S2_MD5FilesChecker);

                                            return;
                                        }
                                        else {
                                            MainFunctions.Notification.ShowNotification(NotificationType.Warning, 5000, "Commandline Argument", "Expected path to GTAIV.exe, instead got path to another file.", "ARGUMENT_EXPECTED_GTA_IV_EXE");
                                        }
                                    }
                                    else {
                                        MainFunctions.Notification.ShowNotification(NotificationType.Warning, 5000, "Commandline Argument", "File does not exists from given argument.", "ARGUMENT_FILE_NOT_FOUND");
                                    }
                                }
                                else {
                                    MainFunctions.Notification.ShowNotification(NotificationType.Warning, 5000, "Commandline Argument", "Expected path to GTAIV.exe, instead got a null or whitespace string.", "ARGUMENT_NULL_OR_WHITESPACE");
                                }
                            }

                            // Show welcome screen
                            ChangeStep(Steps.S0_Welcome);

                        });

                    });
                }
            });
        }

    }
}
