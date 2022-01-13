using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using GTAIVDowngrader.Dialogs;

namespace GTAIVDowngrader {

    #region Public Enums
    public enum Steps
    {
        Welcome,
        Step1,
        Step2,
        Step3,
        Step3_1,
        Step4,
        Confirm,
        Downgrade,
        SavefileDowngrade,
        Commandline,
        Finish,
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
    #endregion

    public partial class MainWindow : Window {

        #region Variables
        // Update Checker Stuff
        private WebClient getNewestVersionClient;
        private bool checkForUpdatesSilently;
        private readonly string currentVersion = "1.4";

        // Current step/dialog
        public Steps currentStep;

        // All steps/dialogs
        public WelcomeUC welcomeUC;
        public S1_SelectIVExe s1_SelectIVExe;
        public S2_SelectDwngrdVersion s2_SelectDwngrdVersion;
        public S3_SelectRadioDwngrd s3_SelectRadioDwngrd;
        public S3_1_SelectVladivostokType s3_1_SelectVladivostokType;
        public S4_SelectComponents s4_SelectComponents;
        public ConfirmUC confirmUC;
        public DowngradingUC downgradingUC;
        public SavefileDowngradeUC savefileDowngradeUC;
        public CommandlineUC commandlineUC;
        public FinishUC finishUC;
        public StandaloneWarningUC standaloneWarningUC;
        public ErrorUC errorUC;
        #endregion

        #region Methods
        public void NextStep(int skip = 0)
        {
            Steps next = (Steps)((int)(currentStep + 1) + skip);
            ChangeStep(next);
        }
        public void PreviousStep(int skip = 0)
        {
            Steps next = (Steps)((int)(currentStep - 1) - skip);
            ChangeStep(next);
        }
        public void ChangeStep(Steps next, List<object> args = null)
        {
            Dispatcher.Invoke(() => {
                MainGrid.Children.Clear();
                currentStep = next;
                switch (next) {
                    case Steps.Welcome:
                        MainGrid.Children.Add(welcomeUC);
                        break;
                    case Steps.Step1:
                        MainGrid.Children.Add(s1_SelectIVExe);
                        break;
                    case Steps.Step2:
                        MainGrid.Children.Add(s2_SelectDwngrdVersion);
                        break;
                    case Steps.Step3:
                        MainGrid.Children.Add(s3_SelectRadioDwngrd);
                        break;
                    case Steps.Step3_1:
                        MainGrid.Children.Add(s3_1_SelectVladivostokType);
                        break;
                    case Steps.Step4:
                        MainGrid.Children.Add(s4_SelectComponents);
                        break;
                    case Steps.Confirm:
                        MainGrid.Children.Add(confirmUC);
                        break;
                    case Steps.Downgrade:
                        MainGrid.Children.Add(downgradingUC);
                        break;
                    case Steps.SavefileDowngrade:
                        MainGrid.Children.Add(savefileDowngradeUC);
                        break;
                    case Steps.Commandline:
                        MainGrid.Children.Add(commandlineUC);
                        break;
                    case Steps.Finish:
                        MainGrid.Children.Add(finishUC);
                        break;
                    case Steps.StandaloneWarning:
                        if (args == null)
                            return;
                        if (args.Count > 1) {
                            standaloneWarningUC.SetWarning(args[0].ToString(), args[1].ToString());
                        }
                        else {
                            return;
                        }
                        MainGrid.Children.Add(standaloneWarningUC);
                        break;
                    case Steps.Error:
                        if (args == null)
                            return;
                        if (args.Count > 1) {
                            errorUC = new ErrorUC((Exception)args[0], new List<string>() { args[1].ToString() });
                        }
                        else {
                            errorUC = new ErrorUC((Exception)args[0]);
                        }
                        MainGrid.Children.Add(errorUC);
                        taskbarItemInfo.ProgressValue = 100;
                        taskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        break;
                }
            });
        }

        public void ShowExitMsg()
        {
            switch (MessageBox.Show("Do you really want to quit?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                case MessageBoxResult.Yes:
                    Environment.Exit(0);
                    break;
            }
        }
        public void ShowErrorScreen(Exception e)
        {
            ChangeStep(Steps.Error, new List<object>() { e });
        }

        #region Update Checker
        private void GetNewestModVersionClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try {
                if (e.Error == null) {
                    if (!e.Cancelled) {
                        if (string.Compare(currentVersion, e.Result) <= -1) { // Update available
                            welcomeUC.DowngraderVersionLabel.Text = string.Format("Version {0} - Update to version {1} available!", currentVersion, e.Result);
                            switch (MessageBox.Show(string.Format("GTA IV Downgrader {0} is available! Do you want to visit the download page?", e.Result), "Update available!", MessageBoxButton.YesNo, MessageBoxImage.Information)) {

                                case MessageBoxResult.Yes:
                                    string downloadPage = getNewestVersionClient.DownloadString("https://www.dropbox.com/s/gjznit4iy7y5oe9/downloadPage.txt?dl=1");
                                    if (!string.IsNullOrWhiteSpace(downloadPage)) {
                                        Process.Start(downloadPage);
                                    }
                                    else {
                                        MessageBox.Show("Could not open download page.");
                                    }
                                    break;
                            }
                        }
                        else { // No update available
                            if (!checkForUpdatesSilently) {
                                MessageBox.Show("There is currently no new update available.", "No new update");
                            }
                        }
                    }
                    else {
                        MessageBox.Show("An unknown error occurred while receiving version info.", "Checking for updates error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else {
                    MessageBox.Show("Error while receiving version info. Details: " + e.Error.Message, "Checking for updates error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Checking for updates error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void CheckForUpdates(bool silent)
        {
            try {
                checkForUpdatesSilently = silent;
                getNewestVersionClient.DownloadStringAsync(new Uri("https://www.dropbox.com/s/7uoje4hsvyv8opu/offlineVersion.txt?dl=1"));
            }
            catch (Exception ex) {
                MessageBox.Show("CheckForUpdates error. Details: " + ex.Message, "Checking for updates error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #endregion

        #region Events
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ChangeStep(Steps.Error, new List<object>() { e.Exception });
            e.Handled = true;
        }
        #endregion

        #region Constructor
        public MainWindow()
        {
            // Register global exception handler
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            InitializeComponent();

            getNewestVersionClient = new WebClient();
            getNewestVersionClient.DownloadStringCompleted += GetNewestModVersionClient_DownloadStringCompleted;

            welcomeUC = new WelcomeUC(this);
            s1_SelectIVExe = new S1_SelectIVExe(this);
            s2_SelectDwngrdVersion = new S2_SelectDwngrdVersion(this);
            s3_SelectRadioDwngrd = new S3_SelectRadioDwngrd(this);
            s3_1_SelectVladivostokType = new S3_1_SelectVladivostokType(this);
            s4_SelectComponents = new S4_SelectComponents(this);
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
            if (currentStep == Steps.Downgrade) {
                switch (MessageBox.Show("Do you really want to quit?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                    case MessageBoxResult.Yes:
                        Environment.Exit(0);
                        break;
                    default:
                        e.Cancel = true;
                        return;
                }
            }

            string tempDirLoc = ".\\Downgrader\\Temp";
            if (Directory.Exists(tempDirLoc)) Directory.Delete(tempDirLoc, true);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if everything is ok
            StringBuilder missingThingsSB = new StringBuilder();
            if (!File.Exists("DotNetZip.dll")) {
                missingThingsSB.AppendLine("- DotNetZip.dll");
            }
            if (!File.Exists("INIController.dll")) {
                missingThingsSB.AppendLine("- INIController.dll");
            }
            if (!File.Exists("Microsoft.WindowsAPICodePack.dll")) {
                missingThingsSB.AppendLine("- Microsoft.WindowsAPICodePack.dll");
            }
            if (!File.Exists("Microsoft.WindowsAPICodePack.Shell.dll")) {
                missingThingsSB.AppendLine("- Microsoft.WindowsAPICodePack.Shell.dll");
            }

            if (missingThingsSB.Length != 0) { // Something is missing
                ChangeStep(Steps.StandaloneWarning, new List<object>() { "Missing important files", string.Format("The downgrader is missing some important files.{0}The following files are missing:{0}{1}{0}If you downloaded a hotfix and tried to run it without any of those files above, then the downgrader can't run properly and that's why you see this message. Please replace the original GTAIVDowngrader.exe from the full version with the one from the hotfix you've just downloaded. See the gtaforums post, or read the readme for more information.", Environment.NewLine, missingThingsSB.ToString()) });
                return;
            }

            welcomeUC.DowngraderVersionLabel.Text = string.Format("Version {0}", currentVersion);
            CheckForUpdates(true);
            ChangeStep(Steps.Welcome);
        }

    }
}
