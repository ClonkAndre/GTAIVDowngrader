using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Interop;

using Newtonsoft.Json;
using CCL;

using GTAIVDowngrader.Classes.Json;
using GTAIVDowngrader.Controls;
using GTAIVDowngrader.Dialogs;

namespace GTAIVDowngrader
{
    public partial class MainWindow : Window
    {

        #region Variables
        // Downloading
        private WebClient downloadWebClient;
        private bool finishedDownloadingFileInfo, finishedDownloadingMD5Hashes;

        // Current step/dialog
        private Steps currentStep;

        // All steps/dialogs
        private WelcomeUC welcomeUC;
        private SelectIVExeUC selectIVExeUC;
        private MD5FilesCheckerUC md5FilesCheckerUC;
        private MoveGameFilesQuestionUC moveGameFilesQuestionUC;
        private MoveGameFilesUC moveGameFilesUC;
        private SelectDwngrdVersionUC selectDwngrdVersionUC;
        private MultiplayerUC multiplayerUC;
        private SelectRadioDwngrdUC selectRadioDwngrdUC;
        private SelectVladivostokTypeUC selectVladivostokTypeUC;
        private SelectComponentsUC selectComponentsUC;
        private ConfirmUC confirmUC;
        private DowngradingUC downgradingUC;
        private SavefileDowngradeUC savefileDowngradeUC;
        private SavefileDowngradeStep2UC savefileDowngradeStep2UC;
        private SavefileDowngradeStep3UC savefileDowngradeStep3UC;
        private CommandlineUC commandlineUC;
        private FinishUC finishUC;
        private MessageDialogUC messageDialogUC;
        public StandaloneWarningUC standaloneWarningUC;
        private ErrorUC errorUC;
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

                SlideDirection slideDirection = ((int)next < (int)currentStep) ? SlideDirection.TopToBottom : SlideDirection.BottomToTop;

                currentStep = next;

                switch (currentStep)
                {
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
                    case Steps.S11_SavefileDowngrade_2:
                        ContentGrid.Children.Add(savefileDowngradeStep2UC);
                        break;
                    case Steps.S11_SavefileDowngrade_3:
                        ContentGrid.Children.Add(savefileDowngradeStep3UC);
                        break;
                    case Steps.S12_Commandline:
                        ContentGrid.Children.Add(commandlineUC);
                        break;
                    case Steps.S13_Finish:
                        ContentGrid.Children.Add(finishUC);
                        break;

                    // Message Dialog
                    case Steps.MessageDialog:
                        ContentGrid.Children.Add(messageDialogUC);
                        break;

                    // Warning/Error Dialogs
                    case Steps.StandaloneWarning:

                        if (args == null)
                            return;

                        if (args.Count > 1)
                            standaloneWarningUC.SetWarning(args[0].ToString(), args[1].ToString());
                        else
                            return;

                        ContentGrid.Children.Add(standaloneWarningUC);

                        break;
                    case Steps.Error:

                        if (args == null)
                            return;

                        if (args.Count > 1)
                            errorUC = new ErrorUC(this, (Exception)args[0], new List<string>() { args[1].ToString() });
                        else
                            errorUC = new ErrorUC(this, (Exception)args[0]);

                        ContentGrid.Children.Add(errorUC);
                        taskbarItemInfo.ProgressValue = 100;
                        taskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;

                        break;
                }

                AnimateDialog(slideDirection);
                UpdateOverallProgress();

            });
        }

        private void ApplyTheme()
        {
            Dispatcher.Invoke(() =>
            {
                if (Core.IsPrideMonth)
                {
                    BottomActionBorder.Background = Core.GetRainbowGradientBrush();
                }
                else
                {
                    if (Core.Is420())
                    {
                        BottomActionBorder.Background = Core.Get420GradientBrush();
                    }
                }
            });
        }
        public void UpdateOverallProgress(bool forceColorToBeGreen = false)
        {
            for (int i = 0; i < OverallProgressStackPanel.Children.Count; i++)
            {
                TintImage tintImage = OverallProgressStackPanel.Children[i] as TintImage;

                if (i > (int)currentStep)
                    tintImage.TintColor = "#737373".ToBrush();
                else
                {
                    // Check if colors can change
                    if ((int)currentStep > (int)Steps.S13_Finish)
                        break;

                    if (forceColorToBeGreen)
                    {
                        tintImage.TintColor = Brushes.Green;
                        continue;
                    }

                    // Rainbow or not!
                    if (Core.IsPrideMonth)
                    {
                        switch (i)
                        {
                            case 0:
                                tintImage.TintColor = "#f20002".ToBrush();
                                break;
                            case 1:
                                tintImage.TintColor = "#f20002".ToBrush();
                                break;
                            case 2:
                                tintImage.TintColor = "#f24d00".ToBrush();
                                break;
                            case 3:
                                tintImage.TintColor = "#f76e00".ToBrush();
                                break;
                            case 4:
                                tintImage.TintColor = "#f76e00".ToBrush();
                                break;
                            case 5:
                                tintImage.TintColor = "#f7c600".ToBrush();
                                break;
                            case 6:
                                tintImage.TintColor = "#f6f704".ToBrush();
                                break;
                            case 7:
                                tintImage.TintColor = "#f6f704".ToBrush();
                                break;
                            case 8:
                                tintImage.TintColor = "#9af704".ToBrush();
                                break;
                            case 9:
                                tintImage.TintColor = "#13d717".ToBrush();
                                break;
                            case 10:
                                tintImage.TintColor = "#13d717".ToBrush();
                                break;
                            case 11:
                                tintImage.TintColor = "#137cd7".ToBrush();
                                break;
                            case 12:
                                tintImage.TintColor = "#2f4df7".ToBrush();
                                break;
                            case 13:
                                tintImage.TintColor = "#2f4df7".ToBrush();
                                break;
                            case 14:
                                tintImage.TintColor = "#432ff7".ToBrush();
                                break;
                            case 15:
                                tintImage.TintColor = "#7622a1".ToBrush();
                                break;
                            case 16:
                                tintImage.TintColor = "#ca03f4".ToBrush();
                                break;
                        }
                    }
                    else
                    {
                        // Check other stuff
                        if (Core.Is420())
                        {
                            tintImage.TintColor = Brushes.Lime;
                        }
                        else
                        {
                            tintImage.TintColor = Brushes.Gold;
                        }
                    }
                }
            }
        }

        private void AnimateDialog(SlideDirection dir)
        {
            // Moving animation
            ThicknessAnimation tA = null;

            switch (dir)
            {
                case SlideDirection.TopToBottom:

                    tA = new ThicknessAnimation {
                        From = new Thickness(10, -50, 10, 185),
                        To = new Thickness(10, 10, 10, 135),
                        FillBehavior = FillBehavior.HoldEnd,
                        Duration = new Duration(TimeSpan.FromSeconds(0.2))
                    };

                    break;
                case SlideDirection.BottomToTop:

                    tA = new ThicknessAnimation {
                        From = new Thickness(10, 50, 10, 85),
                        To = new Thickness(10, 10, 10, 135),
                        FillBehavior = FillBehavior.HoldEnd,
                        Duration = new Duration(TimeSpan.FromSeconds(0.2))
                    };

                    break;
            }

            // Fade in animation
            DoubleAnimation dA = new DoubleAnimation {
                From = 0.0,
                To = 1.0,
                FillBehavior = FillBehavior.HoldEnd,
                Duration = new Duration(TimeSpan.FromSeconds(0.35))
            };

            // Begin animations
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(dA);
            storyboard.Children.Add(tA);
            Storyboard.SetTarget(dA, ContentBorder);
            Storyboard.SetTargetProperty(dA, new PropertyPath(OpacityProperty));
            Storyboard.SetTarget(tA, ContentBorder);
            Storyboard.SetTargetProperty(tA, new PropertyPath(MarginProperty));
            storyboard.Begin();
        }

        public void ChangeActionButtonVisiblity(bool exitVisible, bool backVisible, bool skipVisible, bool nextVisible)
        {
            ExitButton.Visibility = exitVisible ? Visibility.Visible : Visibility.Collapsed;
            BackButton.Visibility = backVisible ? Visibility.Visible : Visibility.Collapsed;
            SkipButton.Visibility = skipVisible ? Visibility.Visible : Visibility.Collapsed;
            NextButton.Visibility = nextVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        public void ChangeActionButtonEnabledState(bool exitEnabled, bool backEnabled, bool skipEnabled, bool nextEnabled)
        {
            ExitButton.IsEnabled = exitEnabled;
            BackButton.IsEnabled = backEnabled;
            SkipButton.IsEnabled = skipEnabled;
            NextButton.IsEnabled = nextEnabled;
        }

        public void ShowMessageDialogScreen(string title, string desc, Steps continueWith, List<object> args = null, string backButtonText = "", Action backButtonAction = null, string skipButtonText = "", Action skipButtonAction = null, Action nextButtonAction = null)
        {
            if (nextButtonAction == null)
                messageDialogUC.ResetOverridenNextButtonClick();
            else
                messageDialogUC.OverrideNextButtonClick(nextButtonAction);

            messageDialogUC.SetMessage(title, desc, continueWith, args, backButtonText, backButtonAction, skipButtonText, skipButtonAction);
            ChangeStep(Steps.MessageDialog);
        }
        public void ShowStandaloneWarningScreen(string title, string desc)
        {
            ChangeStep(Steps.StandaloneWarning, new List<object>() { title, desc });
        }
        public void ShowErrorScreen(Exception e)
        {
            ChangeStep(Steps.Error, new List<object>() { e });
        }

        private void ContinueWithAdminCheck()
        {
            // Check if downgrader did not got started with admin privileges
            if (!Core.IsAppRunningWithAdminPrivileges)
            {
                ShowMessageDialogScreen("Not running with administrator privileges",
                    string.Format("The downgrader was not started with administrator privileges, it is recommended to close the downgrader, and re-run it as an administrator (Right click on IVDowngrader.exe, and click on 'Run as administrator').{0}{0}" +
                                    "Click on the Continue button if you would still like to continue running the downgrader without administrator privileges.", Environment.NewLine),
                    Steps.S0_Welcome);

                messageDialogUC.OverrideNextButtonClick(() => ContinueWithDefaultStartupRoutine());
            }
            else
            {
                ContinueWithDefaultStartupRoutine();
            }
        }
        private void ContinueWithDefaultStartupRoutine()
        {
            // Skip checks and show warning message if offline mode is active
            if (Core.IsInOfflineMode)
            {
                ShowMessageDialogScreen("The downgrader is running in offline mode",
                    string.Format("This means that the downgrader will not be able to download any files for the downgrading process, and you need to download them yourself.{0}" +
                    "Click on the 'Downgrading Files' button to visit the download page for all of the downgrading files.{0}" +
                    "Once you downloaded all the files that you want for the downgrade, place them in the 'Data -> Temp' folder.{0}{0}" +
                    "If you have any questions, feel free to join the discord server and ask in the #help channel.", Environment.NewLine),
                    Steps.S0_Welcome,
                    null,
                    "Downgrading Files",
                    () => Web.AskUserToGoToURL(new Uri("https://mega.nz/folder/Fn0Q3LhY#_0t1VZQFuQX22lMxRZNB1A")),
                    "Discord Server",
                    () => Web.AskUserToGoToURL(new Uri("https://discord.gg/QtAgvkMeJ5")));

                ApplyTheme();

                return;
            }

            // Check if you're connected to the internet
            ShowStandaloneWarningScreen("Checking for an internet connection", "Please wait while we're checking for an internet connection...");

            Task.Run(() =>
            {

                return Web.CheckForInternetConnection();

            }).ContinueWith(t =>
            {
                if (!t.Result.Result) // No internet connection
                {

                    Core.AddLogItem(LogType.Warning, "Internet check returned false.");

                    ShowStandaloneWarningScreen("No connection to the internet",
                        string.Format("Attempt to check if you're connected to the internet failed.{0}" +
                        "This version of the downgrader requires an internet connection.{0}" +
                        "Please make sure that you're connected to the internet, and then try running the downgrader again.{0}{0}" +
                        "This warning is not always correct though, so if you still want to try to continue with the downgrading process, then press the 'Continue anyway' button.", Environment.NewLine));

                    standaloneWarningUC.SetContinueAnywayButton();

                }
                else // Internet connection
                {
                    DownloadRequiredData();
                }
            });
        }
        public void DownloadRequiredData()
        {
            Dispatcher.Invoke(() =>
            {

                ShowStandaloneWarningScreen("Retrieving information", string.Format("Please wait while the downgrader finished retrieving necessary information...{0}" +
                    "If you are stuck at this step, please try to restart the downgrader.", Environment.NewLine));

                // Download required data
                downloadWebClient.DownloadStringAsync(new Uri("https://www.dropbox.com/s/egrkznd2xl7cdd9/isPrideMonth.txt?dl=1"), "IS_PRIDE_MONTH_CHECK");

                Task.Run(() =>
                {

#if DEBUG
                    Core.TheUpdateChecker.CheckForUpdatesAsync(true, true);
#else
                    Core.TheUpdateChecker.CheckForUpdatesAsync(true);
#endif

                    while (!finishedDownloadingFileInfo && !finishedDownloadingMD5Hashes) // Wait till downloads are complete
                        Thread.Sleep(1500);

                }).ContinueWith(r => // Downloads completed
                {

                    ApplyTheme();

                    // Check if downgrader got started with valid argument
                    if (Core.GotStartedWithValidCommandLineArgs)
                    {
                        ChangeStep(Steps.S2_MD5FilesChecker);
                        return;
                    }

                    // Show welcome screen
                    ChangeStep(Steps.S0_Welcome);

                });

            });
        }
        #endregion
        
        #region Functions
        public ProgressBar GetMainProgressBar()
        {
            return MainProgressBar;
        }
        #endregion

        #region Events and Delegates

        // Custom
        public event EventHandler NextButtonClicked;
        public event EventHandler SkipButtonClicked;
        public event EventHandler BackButtonClicked;

        public delegate bool ExitClickedDelegate();
        public event ExitClickedDelegate ExitButtonClicked;

        // Other
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ChangeStep(Steps.Error, new List<object>() { e.Exception });
            e.Handled = true;
        }

        private void DownloadWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                    return;

                string userState = e.UserState != null ? e.UserState.ToString() : null;

                if (string.IsNullOrEmpty(userState))
                {
                    ShowErrorScreen(new Exception(string.Format("[STRING] (1) Unknown error while trying to read user state.")));
                    return;
                }

                if (e.Error != null)
                {
                    ShowErrorScreen(new Exception(string.Format("[STRING] (1) An error occured while trying to retrieve downgrading info.{0}{1}", Environment.NewLine, e.Error.ToString()), e.Error));
                    return;
                }

                // Get the result and do validation
                string result = e.Result;

                if (string.IsNullOrWhiteSpace(result))
                {
                    ShowErrorScreen(new Exception("An unknown error occured while trying to retrieve downgrading info."));
                    return;
                }

                switch (userState)
                {

                    #region IS_PRIDE_MONTH_CHECK
                    case "IS_PRIDE_MONTH_CHECK":
                        {
                            // Check if it is pride month
                            Core.IsPrideMonth = e.Result == "1";

                            // The download link for downloading the downgrading files info
#if DEBUG
                            string dLink = "https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/refs/heads/testing/v2.2_and_up/downgradingFiles.json";
#else
                            string dLink = "https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/main/v2.2_and_up/downgradingFiles.json";
#endif

                            if (Core.UseAlternativeDownloadLinks)
                            {
                                dLink = "https://www.dropbox.com/scl/fi/7682k1hfmkvy24gyjsvxq/downgradingFiles.json?rlkey=wocqup7jor02xop2e90up136n&dl=1";
                                Core.AddLogItem(LogType.Warning, "Using alternative download link for downgradingFiles.json! Files might be outdated!");

                                string filePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\downgradingFiles.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                                downloadWebClient.DownloadFileAsync(new Uri(dLink), filePath, "DOWNGRADING_FILES");
                            }
                            else
                            {
                                downloadWebClient.DownloadStringAsync(new Uri(dLink), "DOWNGRADING_FILES");
                            }

                            break;
                        }
                    #endregion

                    #region DOWNGRADING_FILES
                    case "DOWNGRADING_FILES":
                        {

                            // Parse result
                            Core.DowngradeFiles = JsonConvert.DeserializeObject<List<DowngradeFileDetails>>(result);

#if DEBUG
                            // Debug
                            Console.WriteLine("- - - Downgrading Files - - -");

                            if (Core.DowngradeFiles.Count != 0)
                                Core.DowngradeFiles.ForEach(d => Console.WriteLine(d.ToString())); // Print to console

                            Console.WriteLine("");
#endif

                            // The download link for downloading the MD5 Hashes
#if DEBUG
                            string dLink = "https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/refs/heads/testing/v2.2_and_up/md5Hashes.json";
#else
                            string dLink = "https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/main/v2.2_and_up/md5Hashes.json";
#endif

                            downloadWebClient.DownloadStringAsync(new Uri(dLink), "MD5_HASHES");
                            finishedDownloadingFileInfo = true;

                            break;
                        }
                    #endregion

                    #region MD5_HASHES
                    case "MD5_HASHES":

                        // Save result to file
                        string md5HashesFilePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\md5Hashes_v2.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                        File.WriteAllText(md5HashesFilePath, result);

                        // Parse result
                        Core.MD5Hashes = JsonConvert.DeserializeObject<List<string>>(result);

#if DEBUG
                        // Debug
                        Console.WriteLine("- - - MD5 Hashes - - -");

                        if (Core.MD5Hashes.Count != 0)
                            Core.MD5Hashes.ForEach(hash => Console.WriteLine(hash)); // Print to console
#endif

                        finishedDownloadingMD5Hashes = true;

                        // Now begin downloading supporter information
                        downloadWebClient.DownloadStringAsync(new Uri("https://www.dropbox.com/scl/fi/ml4gx8sgzmznru5llcqpm/TierOneSupporter.json?rlkey=wldl4y6swfb2oy96h763grft3&dl=1"), "TIER_1_SUPPORTER");



                        break;
                    #endregion

                    #region TIER_1_SUPPORTER
                    case "TIER_1_SUPPORTER":

                        // Parse result
                        Core.Tier1Supporter = JsonConvert.DeserializeObject<List<string>>(result);

#if DEBUG
                        // Debug
                        Console.WriteLine("- - - Tier 1 Supporter - - -");

                        if (Core.Tier1Supporter.Count != 0)
                            Core.Tier1Supporter.ForEach(x => Console.WriteLine(x));
#endif

                        // Download Tier 2 supporter
                        downloadWebClient.DownloadStringAsync(new Uri("https://www.dropbox.com/scl/fi/lvt1vd1uvkn7noalsw483/TierTwoSupporter.json?rlkey=7kdoynmiep0nilnyqqbib4s1e&dl=1"), "TIER_2_SUPPORTER");

                        break;
                    #endregion

                    #region TIER_2_SUPPORTER
                    case "TIER_2_SUPPORTER":

                        // Parse result
                        Core.Tier2Supporter = JsonConvert.DeserializeObject<List<string>>(result);

#if DEBUG
                        // Debug
                        Console.WriteLine("- - - Tier 2 Supporter - - -");

                        if (Core.Tier2Supporter.Count != 0)
                            Core.Tier2Supporter.ForEach(x => Console.WriteLine(x));
#endif

                        // Download Tier 3 supporter
                        downloadWebClient.DownloadStringAsync(new Uri("https://www.dropbox.com/scl/fi/6ey0rb7uib8bxd8t0suy3/TierThreeSupporter.json?rlkey=33svhsggspdvbwdnpcnx684j4&dl=1"), "TIER_3_SUPPORTER");

                        break;
                    #endregion

                    #region TIER_3_SUPPORTER
                    case "TIER_3_SUPPORTER":

                        // Parse result
                        Core.Tier3Supporter = JsonConvert.DeserializeObject<List<string>>(result);

#if DEBUG
                        // Debug
                        Console.WriteLine("- - - Tier 3 Supporter - - -");

                        if (Core.Tier3Supporter.Count != 0)
                            Core.Tier3Supporter.ForEach(x => Console.WriteLine(x));
#endif

                        // Destroy WebClient
                        downloadWebClient.DownloadStringCompleted -= DownloadWebClient_DownloadStringCompleted;
                        downloadWebClient.CancelAsync();
                        downloadWebClient.Dispose();
                        downloadWebClient = null;

                        break;
                        #endregion

                }
            }
            catch (Exception ex)
            {
                ShowErrorScreen(new Exception(string.Format("[STRING] (2) An error occured while trying to retrieve downgrading info.{0}{1}", Environment.NewLine, ex.ToString())));
            }
        }
        private void DownloadWebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                    return;

                if (e.Error != null)
                {
                    ShowErrorScreen(new Exception(string.Format("[FILE] (1) An error occured while trying to retrieve downgrading info.{0}{1}", Environment.NewLine, e.Error.ToString()), e.Error));
                    return;
                }

                switch (e.UserState.ToString())
                {

                    #region DOWNGRADING_FILES
                    case "DOWNGRADING_FILES":
                        {

                            // Read file
                            string filePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\downgradingFiles.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

                            // show failed message when file does not exists

                            Core.DowngradeFiles = JsonConvert.DeserializeObject<List<DowngradeFileDetails>>(File.ReadAllText(filePath));

#if DEBUG
                            // Debug
                            Console.WriteLine("- - - Downgrading Files - - -");

                            if (Core.DowngradeFiles.Count != 0)
                                Core.DowngradeFiles.ForEach(d => Console.WriteLine(d.ToString())); // Print to console

                            Console.WriteLine("");
#endif

                            Core.AddLogItem(LogType.Warning, "Using alternative download link for md5Hashes.json! Hashes might be outdated!");

                            string md5HashesFilePathWrite = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\md5Hashes.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

                            downloadWebClient.DownloadFileAsync(new Uri("https://www.dropbox.com/scl/fi/qxx9wzne7jhjoe8w7l5sc/md5Hashes.json?rlkey=2wik2odxfqxk7a25fwhgv7trk&dl=1"), md5HashesFilePathWrite, "MD5_HASHES");
                            finishedDownloadingFileInfo = true;

                            break;
                        }
                    #endregion

                    #region MD5_HASHES
                    case "MD5_HASHES":

                        // Read file
                        string md5HashesFilePathRead = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\md5Hashes_v2.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

                        // show failed message when file does not exists

                        Core.MD5Hashes = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(md5HashesFilePathRead));

#if DEBUG
                        // Debug
                        Console.WriteLine("- - - MD5 Hashes - - -");

                        if (Core.MD5Hashes.Count != 0)
                            Core.MD5Hashes.ForEach(hash => Console.WriteLine(hash.ToString())); // Print to console
#endif

                        finishedDownloadingMD5Hashes = true;

                        // Destroy WebClient
                        downloadWebClient.DownloadStringCompleted -= DownloadWebClient_DownloadStringCompleted;
                        downloadWebClient.CancelAsync();
                        downloadWebClient.Dispose();
                        downloadWebClient = null;

                        break;
                        #endregion

                }
            }
            catch (Exception ex)
            {
                ShowErrorScreen(new Exception(string.Format("[FILE] (2) An error occured while trying to retrieve downgrading info.{0}{1}", Environment.NewLine, ex.ToString())));
            }
        }

        #endregion

        #region Constructor
        public MainWindow()
        {
            // Check if application got started from within a zip file.
            string startupDir = AppDomain.CurrentDomain.BaseDirectory;
            if (startupDir.Contains("AppData") && startupDir.Contains("Temp"))
            {
                MessageBox.Show("The IV Downgrader can't be started from within a zip file. Please unextract it somewhere, and then try launching it again. The application will now close.", "Launch error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
                return;
            }

            // Checks if d3d9.dll exists in current directory.
            if (File.Exists("d3d9.dll"))
            {
                MessageBox.Show("The IV Downgrader couldn't be started because it conflicts with the d3d9.dll file inside of this directory. " +
                    "Please move the IV Downgrader to another location. The application will now close.", "Launch error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(2);
                return;
            }

            // Check if current culture is india
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            if (currentCulture.Name.Contains("IN") || currentCulture.DisplayName.ToLower().Contains("india"))
                Core.UseAlternativeDownloadLinks = true;

            // Set current thread culture to en-US so error messages and such will be in english
            Culture.SetThreadCulture("en-US");

            // Register global exception handler
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            // Initialize Components
            InitializeComponent();

            // Init Core
            Core.Init(this, "2.2");

            // Check OS Version
            OperatingSystem osInfo = Environment.OSVersion;
            if (osInfo.Platform == PlatformID.Win32NT)
            {
                if (osInfo.Version.Major == 6 && osInfo.Version.Minor == 1) // Windows 7
                {
                    // Apply WebClient Protocol Fix
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                }
            }

            // Init WebClient
            downloadWebClient = new WebClient();
            downloadWebClient.DownloadStringCompleted += DownloadWebClient_DownloadStringCompleted;
            downloadWebClient.DownloadFileCompleted += DownloadWebClient_DownloadFileCompleted;

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
            savefileDowngradeStep2UC = new SavefileDowngradeStep2UC(this);
            savefileDowngradeStep3UC = new SavefileDowngradeStep3UC(this);
            commandlineUC = new CommandlineUC(this);
            finishUC = new FinishUC(this);
            downgradingUC = new DowngradingUC(this);
            messageDialogUC = new MessageDialogUC(this);
            standaloneWarningUC = new StandaloneWarningUC(this);
        }
        #endregion

        #region Overrides
        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                // Disable hardware acceleration when OS is Unix
                OperatingSystem osInfo = Environment.OSVersion;
                if (osInfo.Platform == PlatformID.Unix)
                {
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
                    Core.AddLogItem(LogType.Info, "Disabled hardware acceleration because OS is Unix.");
                }
            }

            base.OnSourceInitialized(e);
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (MessageBox.Show("Do you really want to quit?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.No:
                    e.Cancel = true;
                    return;
            }

            // Create log file
            if (currentStep != Steps.S13_Finish || currentStep != Steps.Error)
            {
                try
                {
                    string logFolder = ".\\Data\\Logs";
                    if (!Directory.Exists(logFolder))
                        Directory.CreateDirectory(logFolder);

                    string fileName = string.Format("{0}\\Log.{1}.{2}_{3}_{4}.log", logFolder, DateTime.Now.Year.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString());
                    File.WriteAllLines(fileName, Core.LogItems);
                }
                catch (Exception) { }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if everything is ok
            if (!Directory.Exists(".\\Data"))
            {
                ShowStandaloneWarningScreen("Data folder not found",
                    string.Format("The downgrader could not find the 'Data' folder that should be next to the 'IVDowngrader.exe' file.{0}" +
                    "When moving the downgrader to another location, make sure to move EVERYTHING to the new location and not just the 'IVDowngrader.exe' file!{0}" + 
                    "Please redownload the downgrader if necessary.", Environment.NewLine));

                standaloneWarningUC.SetRedProgressBar();
                return;
            }

            StringBuilder missingThingsSB = new StringBuilder();
            if (!File.Exists(".\\Data\\bin\\Microsoft.WindowsAPICodePack.dll"))         missingThingsSB.AppendLine("- Microsoft.WindowsAPICodePack.dll");
            if (!File.Exists(".\\Data\\bin\\Microsoft.WindowsAPICodePack.Shell.dll"))   missingThingsSB.AppendLine("- Microsoft.WindowsAPICodePack.Shell.dll");
            if (!File.Exists(".\\Data\\bin\\Newtonsoft.Json.dll"))                      missingThingsSB.AppendLine("- Newtonsoft.Json.dll");
            if (!File.Exists(".\\Data\\bin\\ClonksCodingLib.dll"))                      missingThingsSB.AppendLine("- ClonksCodingLib.dll");

            // Check if something is missing
            if (missingThingsSB.Length != 0)
            {
                ShowStandaloneWarningScreen("Missing important files",
                    string.Format("The downgrader is missing some important files that should be in the 'Data -> bin' folder!{0}" +
                    "Please redownload the downgrader if necessary.{0}{0}" +
                    "The following files are missing:{0}{1}", Environment.NewLine, missingThingsSB.ToString()));

                standaloneWarningUC.SetRedProgressBar();
                return;
            }
            else
            {
                missingThingsSB = null;
            }

            // Check if app was started as admin
            Core.DoAdminCheck();

            // Read command line
            Core.ReadCommandLine();
            
            // Set window title
            Core.SetMainWindowTitle();

            // Log startup stuff
            Core.LogStartupInfo();

            // Check if the current OS Platform is Unix or MacOSX
            OperatingSystem os = Environment.OSVersion;
            if (os.Platform == PlatformID.Unix || os.Platform == PlatformID.MacOSX)
            {
                ShowMessageDialogScreen("Unsupported Operating System",
                    string.Format("The downgrader is running on the {1} platform. Only Windows is officially supported. You might encounter bugs while using the downgrader, which is to be expected.{0}{0}" +
                    "Press the Continue button to continue.", Environment.NewLine, os.Platform.ToString()),
                    Steps.S0_Welcome);

                messageDialogUC.OverrideNextButtonClick(() => ContinueWithAdminCheck());
            }
            else
            {
                ContinueWithAdminCheck();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            SkipButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            bool? result = ExitButtonClicked?.Invoke();

            if (result.HasValue)
            {
                if (!result.Value)
                    Close();
            }
            else
                Close();
        }

    }
}
