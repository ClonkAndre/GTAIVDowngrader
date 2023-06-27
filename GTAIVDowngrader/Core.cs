using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using CCL;

using GTAIVDowngrader.Controls;
using GTAIVDowngrader.JsonObjects;

namespace GTAIVDowngrader
{

    #region Public Classes
    public class DowngradingInfo
    {

        #region Properties
        public string IVExecutablePath { get; private set; }
        public string IVWorkingDirectoy { get; private set; }
        public string IVTargetBackupDirectory { get; private set; }
        public string ReceivedMD5Hash { get; private set; }
        public string RelatedMD5Hash { get; private set; }
        public string NewGTAIVTargetLocation { get; private set; }

        public GameVersion DowngradeTo { get; private set; }
        public RadioDowngrader SelectedRadioDowngrader { get; private set; }
        public VladivostokTypes SelectedVladivostokType { get; private set; }

        public bool ConfigureForGFWL { get; private set; }
        public bool InstallNoEFLCMusicInIVFix { get; private set; }
        public bool InstallPrerequisites { get; private set; }
        public bool WantsToCreateBackup { get; private set; }
        public bool CreateBackupInZipFile { get; private set; }
        public bool GTAIVInstallationGotMovedByDowngrader { get; private set; }

        public List<ModInformation> SelectedMods;
        public List<OptionalComponentInfo> SelectedOptionalComponents;
        #endregion

        #region Constructor
        public DowngradingInfo()
        {
            SelectedMods = new List<ModInformation>();
            SelectedOptionalComponents = new List<OptionalComponentInfo>();
        }
        #endregion

        #region Methods
        public void SetPath(string executablePath)
        {
            IVExecutablePath = executablePath;
            IVWorkingDirectoy = Path.GetDirectoryName(executablePath);
        }
        public void SetTargetBackupPath(string backupPath)
        {
            IVTargetBackupDirectory = backupPath;
        }
        public void SetReceivedMD5Hash(string hash)
        {
            ReceivedMD5Hash = hash;
        }
        public void SetRelatedMD5Hash(string hash)
        {
            RelatedMD5Hash = hash;
        }
        public void SetNewGTAIVTargetLocation(string location)
        {
            NewGTAIVTargetLocation = location;
        }

        public void SetDowngradeVersion(GameVersion version)
        {
            DowngradeTo = version;
        }
        public void SetRadioDowngrader(RadioDowngrader radioDowngrader)
        {
            SelectedRadioDowngrader = radioDowngrader;
        }
        public void SetVladivostokType(VladivostokTypes type)
        {
            SelectedVladivostokType = type;
        }

        public void SetConfigureForGFWL(bool value)
        {
            ConfigureForGFWL = value;
        }
        public void SetInstallNoEFLCMusicInIVFix(bool value)
        {
            InstallNoEFLCMusicInIVFix = value;
        }
        public void SetInstallPrerequisites(bool value)
        {
            InstallPrerequisites = value;
        }
        public void SetWantsToCreateBackup(bool value)
        {
            WantsToCreateBackup = value;
        }
        public void SetCreateBackupInZipFile(bool value)
        {
            CreateBackupInZipFile = value;
        }
        public void SetGTAIVInstallationGotMovedByDowngrader(bool value)
        {
            GTAIVInstallationGotMovedByDowngrader = value;
        }
        #endregion

    }
    #endregion

    #region Public Structs
    public struct CommandLineArgument
    {

        #region Properties
        public int Category { get; private set; }
        public string ArgumentName { get; private set; }
        public string ArgumentDescription { get; private set; }
        #endregion

        #region Constructor
        public CommandLineArgument(int category, string aName, string aDesc)
        {
            Category = category;
            ArgumentName = aName;
            ArgumentDescription = aDesc;
        }
        #endregion

    }
    #endregion

    internal class Core
    {

        #region Variables
        public static MainWindow MainApplicationWindow;

        // Lists
        public static List<string> LogItems;
        public static List<CommandLineArgument> CommandLineArguments;
        public static List<JsonObjects.DowngradeInformation> DowngradeFiles;
        public static List<JsonObjects.MD5Hash> MD5Hashes;

        // Other
        public static bool IsInOfflineMode;
        public static bool GotStartedWithValidCommandLineArgs;
        public static bool IsPrideMonth, WantsToDisableRainbowColours;
        public static bool UseAlternativeDownloadLinks;
        public static string CommandLineArgPath;
        public static BrushConverter CBrushConverter;
        public static UpdateChecker CUpdateChecker;
        public static DowngradingInfo CDowngradingInfo;
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

                for (int i = 0; i < queuedNotifications.Count; i++) {
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
                MainApplicationWindow.Dispatcher.Invoke(() => {
                    for (int i = 0; i < MainApplicationWindow.NotificationsStackPanel.Children.Count; i++) {
                        NotificationItem item = (NotificationItem)MainApplicationWindow.NotificationsStackPanel.Children[i];
                        if (item.AdditionnalInformations == additionalInfo) foundItem = item;
                    }
                });
                if (queuedNotifications != null) {
                    for (int i = 0; i < queuedNotifications.Count; i++) {
                        NotificationItem item = queuedNotifications[i];
                        if (item.AdditionnalInformations == additionalInfo) return item;
                    }
                }
                return foundItem;
            }
            public static bool DoesNotifyItemExistsWithAdditionalInfo(string additionalInfo)
            {
                bool foundItem = false;
                MainApplicationWindow.Dispatcher.Invoke(() => {
                    for (int i = 0; i < MainApplicationWindow.NotificationsStackPanel.Children.Count; i++) {
                        NotificationItem item = (NotificationItem)MainApplicationWindow.NotificationsStackPanel.Children[i];
                        if (item.AdditionnalInformations == additionalInfo) foundItem = true;
                    }
                });
                if (queuedNotifications != null) {
                    for (int i = 0; i < queuedNotifications.Count; i++) {
                        NotificationItem item = queuedNotifications[i];
                        if (item.AdditionnalInformations == additionalInfo) return true;
                    }
                }
                return foundItem;
            }
            #endregion

            public static void ShowNotification(NotificationType type, int showTime, string title, string description, string additionalInfo = "")
            {
                if (!string.IsNullOrWhiteSpace(additionalInfo)) {
                    if (DoesNotifyItemExistsWithAdditionalInfo(additionalInfo))
                        return;
                }

                MainApplicationWindow.Dispatcher.Invoke(() => {
                    if (queuedNotifications == null)
                        queuedNotifications = new List<NotificationItem>();

                    NotificationItem item = new NotificationItem(showTime, title, description, additionalInfo);
                    item.DeleteEvent += Item_DeleteEvent;
                    item.Margin = new Thickness(0, 7, 0, 0);
                    item.Visibility = Visibility.Collapsed;
                    item.Opacity = 0.0;

                    SolidColorBrush color;

                    switch (type) {
                        case NotificationType.Info:
                            color = (SolidColorBrush)CBrushConverter.ConvertFrom("#137CBD");
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = System.Windows.Media.Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                        case NotificationType.Warning:
                            color = (SolidColorBrush)CBrushConverter.ConvertFrom("#b36b24");
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = System.Windows.Media.Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                        case NotificationType.Error:
                            color = (SolidColorBrush)CBrushConverter.ConvertFrom("#DE350C");
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = System.Windows.Media.Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                        case NotificationType.Success:
                            color = (SolidColorBrush)CBrushConverter.ConvertFrom("#0F9960");
                            item.NotificationColor = color;
                            item.NotificationBorderEffectColor = System.Windows.Media.Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
                            break;
                    }

                    if (MainApplicationWindow.NotificationsStackPanel.Children.Count >= 2) {
                        queuedNotifications.Add(item);
                    }
                    else {
                        MainApplicationWindow.NotificationsStackPanel.Children.Add(item);
                        item.ShowNotifiction();
                    }
                });
            }

        }
        #endregion

        #region Constructor
        public Core(MainWindow mainApplicationWindow, string currentVersion)
        {
            MainApplicationWindow = mainApplicationWindow;

            // LocalAppData
            CreateAppFoldersInLocalAppData();

            // Lists
            LogItems = new List<string>();
            CommandLineArguments = new List<CommandLineArgument>();
            AddCommandLineArguments(); // Populate "CommandLineArguments" list
            DowngradeFiles = new List<JsonObjects.DowngradeInformation>();
            MD5Hashes = new List<JsonObjects.MD5Hash>();

            // Other
            CBrushConverter = new BrushConverter();
            CUpdateChecker = new UpdateChecker(currentVersion,
                "https://www.dropbox.com/s/ug2oijo32hqw9dk/version.json?dl=1",
                "https://www.dropbox.com/s/yc71hjq7w8a8es8/debug_version.json?dl=1");
            CDowngradingInfo = new DowngradingInfo();
        }
        #endregion

        #region Methods
        private void AddCommandLineArguments()
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

        private void CreateAppFoldersInLocalAppData()
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
        public static JsonObjects.DowngradeInformation GetDowngradeFileByFileName(string fileName)
        {
            for (int i = 0; i < DowngradeFiles.Count; i++)
            {
                JsonObjects.DowngradeInformation file = DowngradeFiles[i];
                if (file.FileName == fileName) return file;
            }
            return null;
        }
        public static List<string> GetMD5HashesFromVersion(string version)
        {
            return MD5Hashes.Where(x => x.Version == version).Select(y => y.Hash).ToList();
        }

        public static long GetDowngradeFileSizeByFileName(string fileName)
        {
            for (int i = 0; i < DowngradeFiles.Count; i++)
            {
                JsonObjects.DowngradeInformation file = DowngradeFiles[i];
                if (file.FileName == fileName)
                    return file.FileSize;
            }
            return 0;
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
        #endregion

    }
}
