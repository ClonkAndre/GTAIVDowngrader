﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

using Newtonsoft.Json;
using CCL;

using GTAIVDowngrader.Classes;
using GTAIVDowngrader.Classes.Json;
using GTAIVDowngrader.Controls;

namespace GTAIVDowngrader
{
    internal class Core
    {

        #region Variables
        public static MainWindow MainApplicationWindow;

        // Lists
        public static List<string> LogItems;
        public static List<string> Tier1Supporter;
        public static List<string> Tier2Supporter;
        public static List<string> Tier3Supporter;
        public static List<string> MD5Hashes;
        public static List<CommandLineArgument> CommandLineArguments;
        public static List<DowngradeFileDetails> DowngradeFiles;

        // Commandline Options
        public static bool IsInOfflineMode;
        public static bool IsInSimpleMode;
        public static bool UseAlternativeDownloadLinks;
        public static bool SkipMD5HashStep;

        public static bool GotStartedWithValidCommandLineArgs;
        public static string CommandLineArgPath;

        // Other
        public static bool IsAppRunningWithAdminPrivileges;
        public static bool IsPrideMonth;
        public static UpdateChecker TheUpdateChecker;
        public static DowngradingInfo CurrentDowngradingInfo;
        #endregion

        #region Classes
        public class Notification
        {

            #region Variables
            private static List<NotificationItem> queuedNotifications;
            #endregion

            #region Events
            private static void Item_DeleteEvent(object sender, EventArgs e)
            {
                NotificationItem senderItem = (NotificationItem)sender;
                MainApplicationWindow.NotificationsStackPanel.Children.Remove(senderItem);

                for (int i = 0; i < queuedNotifications.Count; i++)
                {
                    NotificationItem item = queuedNotifications[i];
                    MainApplicationWindow.NotificationsStackPanel.Children.Add(item);
                    item.ShowNotifiction();
                    queuedNotifications.RemoveAt(i);
                    break;
                }
            }
            #endregion

            #region Functions
            public static NotificationItem GetNotifyItemFromAdditionalInfo(string additionalInfo)
            {
                NotificationItem foundItem = null;
                MainApplicationWindow.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < MainApplicationWindow.NotificationsStackPanel.Children.Count; i++)
                    {
                        NotificationItem item = (NotificationItem)MainApplicationWindow.NotificationsStackPanel.Children[i];
                        if (item.AdditionnalInformations == additionalInfo)
                            foundItem = item;
                    }
                });
                if (queuedNotifications != null)
                {
                    for (int i = 0; i < queuedNotifications.Count; i++)
                    {
                        NotificationItem item = queuedNotifications[i];
                        if (item.AdditionnalInformations == additionalInfo)
                            return item;
                    }
                }
                return foundItem;
            }
            public static bool DoesNotifyItemExistsWithAdditionalInfo(string additionalInfo)
            {
                bool foundItem = false;
                MainApplicationWindow.Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < MainApplicationWindow.NotificationsStackPanel.Children.Count; i++)
                    {
                        NotificationItem item = (NotificationItem)MainApplicationWindow.NotificationsStackPanel.Children[i];
                        if (item.AdditionnalInformations == additionalInfo)
                            foundItem = true;
                    }
                });
                if (queuedNotifications != null)
                {
                    for (int i = 0; i < queuedNotifications.Count; i++)
                    {
                        NotificationItem item = queuedNotifications[i];
                        if (item.AdditionnalInformations == additionalInfo)
                            return true;
                    }
                }
                return foundItem;
            }
            #endregion

            public static void ShowNotification(NotificationType type, int showTime, string title, string description, string additionalInfo = "")
            {
                if (!string.IsNullOrWhiteSpace(additionalInfo))
                {
                    if (DoesNotifyItemExistsWithAdditionalInfo(additionalInfo))
                        return;
                }

                MainApplicationWindow.Dispatcher.Invoke(() =>
                {
                    if (queuedNotifications == null)
                        queuedNotifications = new List<NotificationItem>();

                    NotificationItem item = new NotificationItem(showTime, title, description, additionalInfo);
                    item.DeleteEvent += Item_DeleteEvent;
                    item.Margin = new Thickness(0, 7, 0, 0);
                    item.Visibility = Visibility.Collapsed;
                    item.Opacity = 0.0;

                    SolidColorBrush color;

                    switch (type)
                    {
                        case NotificationType.Info:
                            color = "#137CBD".ToBrush();
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                        case NotificationType.Warning:
                            color = "#b36b24".ToBrush();
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                        case NotificationType.Error:
                            color = "#DE350C".ToBrush();
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                        case NotificationType.Success:
                            color = "#0F9960".ToBrush();
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                    }

                    if (MainApplicationWindow.NotificationsStackPanel.Children.Count >= 2)
                    {
                        queuedNotifications.Add(item);
                    }
                    else
                    {
                        MainApplicationWindow.NotificationsStackPanel.Children.Add(item);
                        item.ShowNotifiction();
                    }
                });
            }

        }
        #endregion

        #region Events
        private static void TheUpdateChecker_UpdateCheckCompleted(UpdateChecker.VersionInfoObject result)
        {
            if (result == null)
                return;

            if (result.NewVersionAvailable)
            {
                switch (MessageBox.Show(string.Format("GTA IV Downgrader version {0} available! Do you want to visit the download page?", result.CurrentVersion), "New update available!", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    case MessageBoxResult.Yes:
                        if (!string.IsNullOrWhiteSpace(result.DownloadPage))
                            Process.Start(result.DownloadPage);
                        else
                            Notification.ShowNotification(NotificationType.Warning, 4000, "Error", "Could not open download page.", "COULD_NOT_OPEN_DOWNLOAD_PAGE");
                        break;
                }
            }
            else
            {
                if (!result.SilentCheck)
                    Notification.ShowNotification(NotificationType.Info, 4000, "No new update", "There is no new update available at the moment.", "NO_NEW_UPDATE");
            }
        }
        private static void TheUpdateChecker_UpdateCheckFailed(Exception e)
        {
            Notification.ShowNotification(NotificationType.Warning, 4000, "Update check failed", e.Message, "UPDATE_CHECK_FAILED");
        }
        #endregion

        #region Methods
        public static void Init(MainWindow mainApplicationWindow, string currentVersion)
        {
            MainApplicationWindow = mainApplicationWindow;

            // LocalAppData
            CreateAppFoldersInLocalAppData();

            // Lists
            LogItems =              new List<string>();
            Tier1Supporter =        new List<string>();
            Tier2Supporter =        new List<string>();
            Tier3Supporter =        new List<string>();
            MD5Hashes =             new List<string>();
            CommandLineArguments =  new List<CommandLineArgument>();
            DowngradeFiles =        new List<DowngradeFileDetails>();
            

            AddIVCommandLineArguments(); // Populate "CommandLineArguments" list

            // Other
            TheUpdateChecker = new UpdateChecker(currentVersion,
                "https://www.dropbox.com/s/ug2oijo32hqw9dk/version.json?dl=1",
                "https://www.dropbox.com/s/yc71hjq7w8a8es8/debug_version.json?dl=1");
            TheUpdateChecker.UpdateCheckCompleted += TheUpdateChecker_UpdateCheckCompleted;
            TheUpdateChecker.UpdateCheckFailed += TheUpdateChecker_UpdateCheckFailed;
            CurrentDowngradingInfo = new DowngradingInfo();
        }
        public static void SetMainWindowTitle()
        {
            StringBuilder sb = new StringBuilder("GTA IV Downgrader");

            if (IsInOfflineMode)
                sb.Append(" [offline mode]");
            if (IsInSimpleMode)
                sb.Append(" [simple mode]");

            MainApplicationWindow.Title = sb.ToString();
        }
        public static void LogStartupInfo()
        {
            AddLogItem(LogType.Info, "- - - Application Information - - -");
            AddLogItem(LogType.Info, string.Format("Running on: {0}", Environment.OSVersion));
            AddLogItem(LogType.Info, string.Format("Running as admin: {0}", IsAppRunningWithAdminPrivileges));
            AddLogItem(LogType.Info, string.Format("Downgrader Version: {0}", TheUpdateChecker.CurrentVersion));
            AddLogItem(LogType.Info, string.Format("Commandline: {0}", Environment.GetCommandLineArgs().ConvertStringArrayToString()));

            if (UseAlternativeDownloadLinks)
                AddLogItem(LogType.Info, "Using alternative download links for downgrading information.");
        }
        public static void LogDowngradingInfos()
        {
            // MD5 Check
            AddLogItem(LogType.Info, "- - - MD5 Check - - -");
            AddLogItem(LogType.Info, string.Format("Generated MD5 Hash: {0}", CurrentDowngradingInfo.GeneratedMD5Hash));
            AddLogItem(LogType.Info, string.Format("Was MD5 Hash Found: {0}", HasMD5Hash(CurrentDowngradingInfo.GeneratedMD5Hash)));

            AddLogItem(LogType.Info, string.Format("When no MD5 Hash was found: The selected GTA IV directory might be modified (Contains mods)."));

            // Downgrading Information
            AddLogItem(LogType.Info, "- - - Downgrading Information - - -");
            AddLogItem(LogType.Info, string.Format("Game Path:                        {0}", CurrentDowngradingInfo.IVWorkingDirectoy));
            AddLogItem(LogType.Info, string.Format("Game Executable Path:             {0}", CurrentDowngradingInfo.IVExecutablePath));
            AddLogItem(LogType.Info, string.Format("Selected downgrading version:     {0}", CurrentDowngradingInfo.DowngradeTo));
            AddLogItem(LogType.Info, string.Format("Configure for GFWL:               {0}", CurrentDowngradingInfo.ConfigureForGFWL));
            AddLogItem(LogType.Info, string.Format("Selected radio downgrader:        {0}", CurrentDowngradingInfo.SelectedRadioDowngrader));
            AddLogItem(LogType.Info, string.Format("Selected vladivostok type:        {0}", CurrentDowngradingInfo.SelectedVladivostokType));
            AddLogItem(LogType.Info, string.Format("Install No EFLC Music in IV Fix:  {0}", CurrentDowngradingInfo.InstallNoEFLCMusicInIVFix));
            AddLogItem(LogType.Info, string.Format("Install Prerequisites:            {0}", CurrentDowngradingInfo.InstallPrerequisites));
            AddLogItem(LogType.Info, string.Format("Create Backup:                    {0}", CurrentDowngradingInfo.WantsToCreateBackup));
            AddLogItem(LogType.Info, string.Format("Create Backup in zip file:        {0}", CurrentDowngradingInfo.CreateBackupInZipFile));
        }

        public static void DoAdminCheck()
        {
            IsAppRunningWithAdminPrivileges = UAC.IsAppRunningWithAdminPrivileges();
        }

        private static void AddIVCommandLineArguments()
        {
            // Graphics
            CommandLineArguments.Add(new CommandLineArgument(0, "-renderquality", "Sets the render quality of the game. (0-4)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-shadowdensity", "Sets the shadow density of the game. (0-16)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-texturequality", "Sets the texture quality of the game. (0-2)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-viewdistance", "Sets the view distance of the game (0-99)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-detailquality", "Sets the detail quality of the game. (0-99)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-novblank", "Disables wait for vblank (No Vsync)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-norestrictions", "Do not limit graphics settings"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-width", "Sets the width of the main render window (default is 800)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-height", "Sets the height of the main render window (default is 600)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-safemode", "Runs the graphics in the lowest setting possible"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-frameLimit", "Limits frame to interval of refresh rate (ex. If refreshrate is 60HZ –frameLimit 1 = Locks down to 60HZ)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-refreshrate", "Sets the refresh rate of the main render window"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-fullscreen", "Forces fullscreen mode"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-windowed", "Forces windowed mode"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-availablevidmem", "Sets the amount of physical Video Memory (ex. -availablevidmem 1024.0 with 1024 being your amount of VRAM because 1024MB = 1GB)"));
            CommandLineArguments.Add(new CommandLineArgument(0, "-percentvidmem", "Sets the percentage of video memory to make available to GTA"));
            
            // Audio
            CommandLineArguments.Add(new CommandLineArgument(1, "-fullspecaudio", "Forces high-end CPU audio footprint"));
            CommandLineArguments.Add(new CommandLineArgument(1, "-minspecaudio", "Forces low-end CPU audio footprint"));
            
            // System
            CommandLineArguments.Add(new CommandLineArgument(2, "-noprecache", "Disables precache of resources"));
            CommandLineArguments.Add(new CommandLineArgument(2, "-nomemrestrict", "Disables memory restrictions"));
        }

        private static void CreateAppFoldersInLocalAppData()
        {
            string ivDowngraderDataPath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            // Create directories if the main directory doesn't exists
            if (!Directory.Exists(ivDowngraderDataPath))
            {
                Directory.CreateDirectory(ivDowngraderDataPath);
                Directory.CreateDirectory(ivDowngraderDataPath + "\\DownloadedData");
            }
        }

        /// <summary>
        /// Adds something to the log list that later gets saved to a log file.
        /// </summary>
        /// <param name="type">Log type</param>
        /// <param name="str">The text you want to log</param>
        /// <param name="includeTimeStamp">Include timestamp or not</param>
        public static void AddLogItem(LogType type, string str, bool includeTimeStamp = true)
        {
            string logTime = string.Format("{0}", DateTime.Now.ToString("HH:mm:ss"));

            string logText = "";
            if (includeTimeStamp)
                logText = string.Format("[{0}] [{1}] {2}", logTime, type.ToString(), str);
            else
                logText = string.Format("[{0}] {1}", type.ToString(), str);

            LogItems.Add(logText); // Add log to log list for log file
        }

        /// <summary>
        /// Shows a messagebox that asks the user if he wants to navigate to the given webpage.
        /// </summary>
        /// <param name="uri">The Uri</param>
        public static void AskUserToOpenURL(Uri uri)
        {
            if (uri != null) {
                switch (MessageBox.Show(string.Format("This link takes you to {0} ({1}). Do you want to go there?", uri.Host, uri.ToString()), "Open link?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                    case MessageBoxResult.Yes:
                        Process.Start(uri.ToString());
                        break;
                }
            }
            else {
                MessageBox.Show(string.Format("Sorry, this somehow didn't worked.{0}Please report this error in the #bug-report channel in my discord server and explain which link caused this error. Thanks!", Environment.NewLine), "Unknown Error");
            }
        }
        #endregion

        #region Functions
        // out bool offlineMode, out bool simpleMode, out bool forceAlternativeDownloadLinks, out string gtaExecutablePath
        public static bool ReadCommandLine()
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();

            for (int i = 0; i < cmdArgs.Length; i++)
            {
                string arg = cmdArgs[i];

                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                string argLower = arg.ToLower();

                // Check arguments
                if (argLower.Contains("-offline"))
                {
                    IsInOfflineMode = true;
                    continue;
                }
                else if (argLower.Contains("-simple"))
                {
                    IsInSimpleMode = true;
                    continue;
                }
                else if (argLower.Contains("-forceUseAltDownloadLinks"))
                {
                    UseAlternativeDownloadLinks = true;
                    continue;
                }
                else if (argLower.Contains("-skipMD5HashStep"))
                {
                    SkipMD5HashStep = true;
                    continue;
                }
                else if (argLower.Contains("gtaiv.exe"))
                {
                    // Check if given executable path is valid
                    if (File.Exists(arg))
                    {
                        CommandLineArgPath = arg;
                        GotStartedWithValidCommandLineArgs = true;
                        CurrentDowngradingInfo.SetPath(CommandLineArgPath);
                    }
                }
            }

            return IsInOfflineMode || IsInSimpleMode || UseAlternativeDownloadLinks || !string.IsNullOrEmpty(CommandLineArgPath);
        }

        public static bool LoadExistingMD5Hashes()
        {
            try
            {
                string md5HashesFilePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\md5Hashes_v2.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

                if (!File.Exists(md5HashesFilePath))
                {
                    AddLogItem(LogType.Warning, "Local MD5 Hash file is not available. Cannot perform MD5 Hash step.");
                    return false;
                }

                // Load already existing hashes
                MD5Hashes = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(md5HashesFilePath));

                return MD5Hashes.Count != 0;
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("Failed to load local MD5 Hashes from file! Details: {0}", ex));
            }

            return false;
        }
        public static bool HasMD5Hash(string hash)
        {
            return MD5Hashes.Contains(hash);
        }

        public static DowngradeFileDetails GetDowngradeFileByFileName(string fileName)
        {
            return DowngradeFiles.Where(x => x.FileDetails.Name == fileName).FirstOrDefault();
        }
        public static long GetDowngradeFileSizeByFileName(string fileName)
        {
            DowngradeFileDetails file = DowngradeFiles.Where(x => x.FileDetails.Name == fileName).FirstOrDefault();

            if (file != null)
                return file.FileDetails.SizeInBytes;

            return 0L;
        }

        public static bool AreThereAnySupporters()
        {
            return Tier1Supporter.Count != 0 || Tier2Supporter.Count != 0 || Tier3Supporter.Count != 0;
        }

        public static bool Is420()
        {
            DateTime dtNow = DateTime.Now;
            return dtNow.Day == 20 && dtNow.Month == 4;
        }
        public static bool IsClonksBirthday()
        {
            DateTime dtNow = DateTime.Now;
            return dtNow.Day == 4 && dtNow.Month == 7;
        }
        public static bool IsIVDowngraderReleaseDay()
        {
            DateTime dtNow = DateTime.Now;
            return dtNow.Day == 25 && dtNow.Month == 10;
        }
        public static bool IsIVLauncherReleaseDay()
        {
            DateTime dtNow = DateTime.Now;
            return dtNow.Day == 19 && dtNow.Month == 12;
        }
        public static bool IsIVSDKDotNetReleaseDay()
        {
            DateTime dtNow = DateTime.Now;
            return dtNow.Day == 22 && dtNow.Month == 10;
        }

        public static Brush GetRainbowGradientBrush()
        {
            try
            {
                LinearGradientBrush brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0, 0.5);
                brush.EndPoint = new Point(1, 0.5);

                brush.GradientStops.Add(new GradientStop() { Offset = 0.100,    Color = "#B3FF0000".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.1666,   Color = "#B3FF7F00".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.450,    Color = "#B3FFFF00".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.600,    Color = "#B300FF00".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.7700,   Color = "#B30000FF".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.8699,   Color = "#B34B0082".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 1,        Color = "#B39400D3".ToColor() });

                return brush;
            }
            catch (Exception) { }
            return "#B3000000".ToBrush();
        }
        public static Brush Get420GradientBrush()
        {
            try
            {
                LinearGradientBrush brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0, 0.5);
                brush.EndPoint = new Point(1, 0.5);

                brush.GradientStops.Add(new GradientStop() { Offset = 0.100,    Color = "#759C4B".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.1666,   Color = "#487A30".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.450,    Color = "#408136".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.600,    Color = "#6A984A".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.7700,   Color = "#5B8A4A".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 0.8699,   Color = "#629347".ToColor() });
                brush.GradientStops.Add(new GradientStop() { Offset = 1,        Color = "#79AC4D".ToColor() });

                return brush;
            }
            catch (Exception) { }
            return "#408136".ToBrush();
        }
        #endregion

    }
}
