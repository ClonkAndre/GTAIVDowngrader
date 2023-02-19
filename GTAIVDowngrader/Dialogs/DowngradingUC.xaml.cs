using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;

namespace GTAIVDowngrader.Dialogs {
    public partial class DowngradingUC : UserControl {

        // - - - Downgrading Order - - -
        // 1. Create Backup if user wants to otherwise skip to 2.
        // 2. Start downloading downgrade files and selected mods
        // 3. Install Prerequisites if user wants to otherwise skip to 4.
        // 4. Install GFWL Prerequisites if user checked the Configure For GFWL CheckBox otherwise skip to 5.
        // 5. If user selected a Radio Downgrader, do Radio Downgrade otherwise skip to 6.
        // 6. Do Game Downgrading
        // 7. Install Mods if there any to install otherwise skip to 8.
        // 8. Finish

        #region Variables and Enums
        private MainWindow instance;

        // Download Stuff
        private WebClient downloadWebClient;
        private Queue<FileDownload> downloadQueue;
        private DateTime downloadStartTime; // Remaining Time Calculation

        // Processes
        private Process radioDowngraderProc;

        // States
        private InstallState currentInstallState;
        private Prerequisites currentPrerequisite;

        // Other
        private string modToInstall;
        private bool errored, backupErrored, prerequisiteErrored, radioDowngraderErrored;
        private bool canDownloadFiles;
        private int progress;

        // Enums
        public enum InstallState
        {
            Backup,
            Download, // Can Download: Prerequisites, Old and New Vladivostok, No EFLC Music in IV Fix and selected Mods.
            Prerequisites,
            RadioDowngrade,
            RadioOldVladivostok,
            RadioNewVladivostok,
            RadioNoEFLCMusicInIVFix,
            GameDowngrade,
            ModInstall
        }
        public enum Prerequisites
        {
            VisualCPlusPlus,
            DirectXExtract,
            DirectX,
            GFWL
        }
        #endregion

        #region Structs
        private struct FileDownload
        {
            #region Properties
            public string FileName { get; private set; }
            public string DownloadURL { get; private set; }
            public long FileSize { get; private set; }
            public bool NeedsToBeDecompressed { get; private set; }
            #endregion

            #region Constructor
            public FileDownload(JsonObjects.DowngradeInformation info)
            {
                FileName = info.FileName;
                FileSize = info.FileSize;
                DownloadURL = info.DownloadURL;
                NeedsToBeDecompressed = info.NeedsToBeDecompressed;
            }
            public FileDownload(JsonObjects.ModInformation info)
            {
                FileName = info.FileName;
                FileSize = info.FileSize;
                DownloadURL = info.DownloadURL;
                NeedsToBeDecompressed = false;
            }
            #endregion
        }
        #endregion

        #region Constructor
        public DowngradingUC()
        {
            downloadQueue = new Queue<FileDownload>();
            InitializeComponent();
        }
        public DowngradingUC(MainWindow window)
        {
            instance = window;
            downloadQueue = new Queue<FileDownload>();
            InitializeComponent();
        }
        #endregion
        
        #region Methods

        public void AddLogItem(LogType type, string str, bool includeTimeStamp = true, bool printInListBox = true)
        {
            Dispatcher.Invoke(() => {
                string logTime = string.Format("{0}", DateTime.Now.ToString("HH:mm:ss"));

                string logText = "";
                if (includeTimeStamp) {
                    logText = string.Format("[{0}] [{1}] {2}", logTime, type.ToString(), str);
                }
                else {
                    logText = string.Format("[{0}] {1}", type.ToString(), str);
                }

                if (printInListBox) StatusListbox.Items.Add(logText); // Add log to StatusListBox
                MainFunctions.logItems.Add(logText); // Add log to log list for log file

                // Auto scroll StatusListBox to last item
                StatusListbox.SelectedIndex = StatusListbox.Items.Count - 1;
                StatusListbox.ScrollIntoView(StatusListbox.SelectedItem);
            });
        }

        public void ChangeProgressBarIndeterminateState(bool isIndeterminate)
        {
            Dispatcher.Invoke(() => {
                DowngradeProgressBar.IsIndeterminate = isIndeterminate;
            });
        }
        public void UpdateStatusText(string txt)
        {
            Dispatcher.Invoke(() => {
                StatusLabel.Text = txt;
            });
        }
        public void UpdateExtractionProgressBar(string currentFile, int progress, int totalFiles)
        {
            Dispatcher.Invoke(() => {
                StatusLabel.Text = string.Format("Extracting file {0}, Progress: {1}/{2}", currentFile, progress, totalFiles);
                DowngradeProgressBar.Maximum = totalFiles;
                DowngradeProgressBar.Value = progress;
            });
        }
        public void RefreshCurrentStepLabel()
        {
            Dispatcher.Invoke(() => {
                switch (currentInstallState) {
                    case InstallState.Download:
                        CurrentStepLabel.Text = "Current Step: Downloading files";
                        break;
                    case InstallState.Backup:
                        CurrentStepLabel.Text = "Current Step: Backing up original game files";
                        break;
                    case InstallState.Prerequisites:
                        CurrentStepLabel.Text = "Current Step: Installing Prerequisites";
                        break;
                    case InstallState.RadioDowngrade:
                        CurrentStepLabel.Text = "Current Step: Radio Downgrade";
                        break;
                    case InstallState.RadioOldVladivostok:
                        CurrentStepLabel.Text = "Current Step: Old Vladivostok Install";
                        break;
                    case InstallState.RadioNewVladivostok:
                        CurrentStepLabel.Text = "Current Step: New Vladivostok Install";
                        break;
                    case InstallState.RadioNoEFLCMusicInIVFix:
                        CurrentStepLabel.Text = "Current Step: No EFLC Music In IV Fix Install";
                        break;
                    case InstallState.GameDowngrade:
                        CurrentStepLabel.Text = "Current Step: Game Downgrade";
                        break;
                    case InstallState.ModInstall:
                        CurrentStepLabel.Text = "Current Step: Mod Install";
                        break;
                }
            });
        }
        public void Finish()
        {
            Dispatcher.Invoke(() => {
                instance.taskbarItemInfo.ProgressValue = 100;
                instance.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                DowngradeProgressBar.Foreground = Brushes.Green;
                CurrentStepLabel.Text = "Current Step: Finished";
                StatusLabel.Text = "Finshed! Click on 'Next' to continue.";

                // Perfom last steps
                if (MainFunctions.downgradingInfo.DowngradeTo == GameVersion.v1040) {
                    DeleteDLCsFor1040();
                }

                AddLogItem(LogType.Info, "Finshed!");
                NextButton.IsEnabled = true;
            });
        }

        #region PreChecks
        private void RemoveOldFolders()
        {
            try {
                string pluginsFolder = string.Format("{0}\\plugins", MainFunctions.downgradingInfo.IVWorkingDirectoy);
                if (Directory.Exists(pluginsFolder)) {
                    Directory.Delete(pluginsFolder, true);

                    // Check if deletion was successfull
                    if (!Directory.Exists(pluginsFolder)) {
                        AddLogItem(LogType.Info, "plugins folder deleted.");
                    }
                    else {
                        AddLogItem(LogType.Warning, "plugins folder was not deleted! Please delete manually.");
                    }
                }

                string scriptsFolder = string.Format("{0}\\scripts", MainFunctions.downgradingInfo.IVWorkingDirectoy);
                if (Directory.Exists(scriptsFolder)) {
                    Directory.Delete(scriptsFolder, true);

                    // Check if deletion was successfull
                    if (!Directory.Exists(scriptsFolder)) {
                        AddLogItem(LogType.Info, "scripts folder deleted.");
                    }
                    else {
                        AddLogItem(LogType.Warning, "scripts folder was not deleted! Please delete manually.");
                    }
                }
            }
            catch (Exception ex) {
                AddLogItem(LogType.Info, string.Format("Could not remove old folders. Details: {0}", ex.Message));
            }
        }
        private void CreateXLivelessAddonFolders()
        {
            if (MainFunctions.downgradingInfo.DowngradeTo == GameVersion.v1040) {
                string savesDir = MainFunctions.downgradingInfo.IVWorkingDirectoy + "\\saves";
                string settingsDir = MainFunctions.downgradingInfo.IVWorkingDirectoy + "\\settings";

                // Create saves folder
                Directory.CreateDirectory(savesDir);
                if (Directory.Exists(savesDir)) AddLogItem(LogType.Info, "saves folder created for XLivelessAddon");

                // Create settings folder
                Directory.CreateDirectory(settingsDir);
                if (Directory.Exists(settingsDir)) AddLogItem(LogType.Info, "settings folder created for XLivelessAddon");
            }
        }
        private bool KillAnyGTAProcessIfRunning()
        {
            try {
                Process gtaEncoderProcess = Process.GetProcessesByName("gtaEncoder").FirstOrDefault();
                Process gta4BrowserProcess = Process.GetProcessesByName("gta4Browser").FirstOrDefault();
                Process gtaivProcess = Process.GetProcessesByName("GTAIV").FirstOrDefault();

                // No process is running
                if (gtaEncoderProcess == null
                    && gta4BrowserProcess == null
                    && gtaivProcess == null) {
                    return false;
                }

                // Some process is still running
                if (gtaEncoderProcess != null) {
                    gtaEncoderProcess.Kill();

                    if (gtaEncoderProcess.WaitForExit(100)) {
                        AddLogItem(LogType.Info, "Killed gtaEncoder Process.");

                        gtaEncoderProcess.Dispose();
                        gtaEncoderProcess = null;
                    }
                }
                if (gta4BrowserProcess != null) {
                    gta4BrowserProcess.Kill();

                    if (gta4BrowserProcess.WaitForExit(100)) {
                        AddLogItem(LogType.Info, "Killed gta4Browser Process.");

                        gta4BrowserProcess.Dispose();
                        gta4BrowserProcess = null;
                    }
                }
                if (gtaivProcess != null) {
                    gtaivProcess.Kill();

                    if (gtaivProcess.WaitForExit(100)) {
                        AddLogItem(LogType.Info, "Killed GTAIV Process.");

                        gtaivProcess.Dispose();
                        gtaivProcess = null;
                    }
                }

                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        #endregion

        #region Backup
        public void StartCreatingBackup()
        {
            string sourcePath = MainFunctions.downgradingInfo.IVWorkingDirectoy;
            string targetPath = MainFunctions.downgradingInfo.IVTargetBackupDirectory;
            bool createZIP = MainFunctions.downgradingInfo.CreateBackupInZipFile;

            currentInstallState = InstallState.Backup;
            RefreshCurrentStepLabel();
            Task.Run(() => {

                AddLogItem(LogType.Info, "Creating a backup...");

                try {
                    string[] directorys = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
                    string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    int total = directorys.Length + files.Length;

                    if (createZIP) {
                        string zipFileName = string.Format("{0}\\GTAIV.Backup.{1}.{2}{3}{4}.zip", targetPath, DateTime.Now.Year.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString());

                        Dispatcher.Invoke(() => {
                            DowngradeProgressBar.IsIndeterminate = true;
                            StatusLabel.Text = string.Format("Backing up game files to file {0}", Path.GetFileName(zipFileName));
                        });

                        // ZIP
                        System.IO.Compression.ZipFile.CreateFromDirectory(sourcePath, zipFileName);
                    }
                    else {
                        Dispatcher.Invoke(() => { DowngradeProgressBar.Maximum = total; });

                        // Create all of the directories
                        foreach (string dirPath in directorys) {
                            progress++;
                            Dispatcher.Invoke(() => {
                                StatusLabel.Text = string.Format("Copying directory: {0}, Progress: {1}/{2}", Path.GetDirectoryName(dirPath), progress.ToString(), total.ToString());
                                DowngradeProgressBar.Value = progress;
                            });
                            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                        }

                        // Copy all the files & Replaces any files with the same name
                        foreach (string newPath in files) {
                            progress++;
                            Dispatcher.Invoke(() => {
                                StatusLabel.Text = string.Format("Copying file: {0}, Progress: {1}/{2}", Path.GetFileName(newPath), progress.ToString(), total.ToString());
                                DowngradeProgressBar.Value = progress;
                            });
                            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                        }
                    }
                }
                catch (Exception ex) {
                    AddLogItem(LogType.Error, string.Format("Failed to create backup! Details: {0}", ex.Message));
                    backupErrored = true;
                }

            }).ContinueWith(result => { // Finished

                if (!backupErrored) AddLogItem(LogType.Info, "Backup created!");

                Dispatcher.Invoke(() => {
                    DowngradeProgressBar.IsIndeterminate = false;

                    if (canDownloadFiles) { // Download Files
                        StartDownloads();
                    }
                    else { // Continue with something else
                        if (MainFunctions.downgradingInfo.InstallPrerequisites) { // Install Prerequisites
                            StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                        }
                        else {
                            if (MainFunctions.downgradingInfo.ConfigureForGFWL) { // Install GFWL Prerequisites
                                StartInstallPrerequisites(Prerequisites.GFWL);
                            }
                            else { // Continue with Radio/Game downgrade
                                if (MainFunctions.downgradingInfo.SelectedRadioDowngrader != RadioDowngrader.None) {
                                    BeginExtractionProcess(InstallState.RadioDowngrade);
                                }
                                else {
                                    BeginExtractionProcess(InstallState.GameDowngrade);
                                }
                            }
                        }
                    }
                });

            });
        }
        #endregion

        #region Download
        private bool PopulateDownloadQueueList()
        {
            try {

                string filePath;

                // Game stuff
                switch (MainFunctions.downgradingInfo.DowngradeTo) {
                    case GameVersion.v1080:
                        filePath = GetFileLocationInTempFolder("1080.zip");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("1080.zip")));
                        }
                        else {

                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath)) {
                                downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("1080.zip")));
                                AddLogItem(LogType.Info, "1080.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                            }
                            else {
                                AddLogItem(LogType.Info, "1080.zip is already downloaded. Skipping.");
                            }
                            
                        }
                        break;
                    case GameVersion.v1070:
                        filePath = GetFileLocationInTempFolder("1070.zip");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("1070.zip")));
                        }
                        else {

                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath)) {
                                downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("1070.zip")));
                                AddLogItem(LogType.Info, "1070.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                            }
                            else {
                                AddLogItem(LogType.Info, "1070.zip is already downloaded. Skipping.");
                            }

                        }
                        break;
                    case GameVersion.v1040:
                        filePath = GetFileLocationInTempFolder("1040.zip");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("1040.zip")));
                        }
                        else {

                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath)) {
                                downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("1040.zip")));
                                AddLogItem(LogType.Info, "1040.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                            }
                            else {
                                AddLogItem(LogType.Info, "1040.zip is already downloaded. Skipping.");
                            }

                        }
                        break;
                }

                // Radio stuff
                switch (MainFunctions.downgradingInfo.SelectedRadioDowngrader) {
                    case RadioDowngrader.SneedsDowngrader:
                        filePath = GetFileLocationInTempFolder("SneedsRadioDowngrader.zip");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("SneedsRadioDowngrader.zip")));
                        }
                        else {

                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath)) {
                                downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("SneedsRadioDowngrader.zip")));
                                AddLogItem(LogType.Info, "SneedsRadioDowngrader.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                            }
                            else {
                                AddLogItem(LogType.Info, "SneedsRadioDowngrader.zip is already downloaded. Skipping.");
                            }

                        }
                        break;
                    case RadioDowngrader.LegacyDowngrader:
                        filePath = GetFileLocationInTempFolder("LegacyRadioDowngrader.zip");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("LegacyRadioDowngrader.zip")));
                        }
                        else {

                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath)) {
                                downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("LegacyRadioDowngrader.zip")));
                                AddLogItem(LogType.Info, "LegacyRadioDowngrader.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                            }
                            else {
                                AddLogItem(LogType.Info, "LegacyRadioDowngrader.zip is already downloaded. Skipping.");
                            }

                        }
                        break;
                }
                switch (MainFunctions.downgradingInfo.SelectedVladivostokType) {
                    case VladivostokTypes.New:
                        filePath = GetFileLocationInTempFolder("WithNewVladivostok.zip");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("WithNewVladivostok.zip")));
                        }
                        else {

                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath)) {
                                downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("WithNewVladivostok.zip")));
                                AddLogItem(LogType.Info, "WithNewVladivostok.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                            }
                            else {
                                AddLogItem(LogType.Info, "WithNewVladivostok.zip is already downloaded. Skipping.");
                            }

                        }
                        break;
                    case VladivostokTypes.Old:
                        filePath = GetFileLocationInTempFolder("WithoutNewVladivostok.zip");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("WithoutNewVladivostok.zip")));
                        }
                        else {

                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath)) {
                                downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("WithoutNewVladivostok.zip")));
                                AddLogItem(LogType.Info, "WithoutNewVladivostok.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                            }
                            else {
                                AddLogItem(LogType.Info, "WithoutNewVladivostok.zip is already downloaded. Skipping.");
                            }

                        }
                        break;
                }

                if (MainFunctions.downgradingInfo.InstallNoEFLCMusicInIVFix) {
                    filePath = GetFileLocationInTempFolder("EpisodeOnlyMusicCE.zip");
                    if (!File.Exists(filePath)) {
                        downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("EpisodeOnlyMusicCE.zip")));
                    }
                    else {

                        // Check if already existing zip file is corrupted or not
                        if (IsZipFileCorrupted(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("EpisodeOnlyMusicCE.zip")));
                            AddLogItem(LogType.Info, "EpisodeOnlyMusicCE.zip is already downloaded but the file is corrupted. Adding to download queue to redownload.");
                        }
                        else {
                            AddLogItem(LogType.Info, "EpisodeOnlyMusicCE.zip is already downloaded. Skipping.");
                        }
                        
                    }
                } 

                // Mods
                for (int i = 0; i < MainFunctions.downgradingInfo.SelectedMods.Count; i++) {
                    downloadQueue.Enqueue(new FileDownload(MainFunctions.downgradingInfo.SelectedMods[i]));
                }

                // Prerequisites
                if (MainFunctions.downgradingInfo.InstallPrerequisites) {
                    filePath = GetFileLocationInTempFolder("directx_Jun2010_redist.exe");
                    if (!File.Exists(filePath)) {
                        downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("directx_Jun2010_redist.exe")));
                    }
                    else {
                        AddLogItem(LogType.Info, "directx_Jun2010_redist.exe is already downloaded. Skipping.");
                    }

                    filePath = GetFileLocationInTempFolder("vcredist_x86.exe");
                    if (!File.Exists(filePath)) {
                        downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("vcredist_x86.exe")));
                    }
                    else {
                        AddLogItem(LogType.Info, "vcredist_x86.exe is already downloaded. Skipping.");
                    }
                }
                if (MainFunctions.downgradingInfo.ConfigureForGFWL) {
                    filePath = GetFileLocationInTempFolder("gfwlivesetup.exe");
                    if (!File.Exists(filePath)) {
                        downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("gfwlivesetup.exe")));
                    }
                    else {
                        AddLogItem(LogType.Info, "gfwlivesetup.exe is already downloaded. Skipping.");
                    }

                    filePath = GetFileLocationInTempFolder("xliveredist.msi");
                    if (!File.Exists(filePath)) {
                        downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("xliveredist.msi")));
                    }
                    else {
                        AddLogItem(LogType.Info, "xliveredist.msi is already downloaded. Skipping.");
                    }

                    if (Environment.Is64BitOperatingSystem) {
                        filePath = GetFileLocationInTempFolder("wllogin_64.msi");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("wllogin_64.msi")));
                        }
                        else {
                            AddLogItem(LogType.Info, "wllogin_64.msi is already downloaded. Skipping.");
                        }
                    }
                    else {
                        filePath = GetFileLocationInTempFolder("wllogin_32.msi");
                        if (!File.Exists(filePath)) {
                            downloadQueue.Enqueue(new FileDownload(MainFunctions.GetDowngradeFileByFileName("wllogin_32.msi")));
                        }
                        else {
                            AddLogItem(LogType.Info, "wllogin_32.msi is already downloaded. Skipping.");
                        }
                    }
                }

                // Log
                if (downloadQueue.Count != 0) {

                    // Log download queue list to file
                    FileDownload[] items = downloadQueue.ToArray();
                    for (int i = 0; i < items.Length; i++) {
                        FileDownload item = items[i];
                        AddLogItem(LogType.Info, string.Format("Populated download queue list with: {0}, URL: {1}", item.FileName, item.DownloadURL), true, false);
                    }

                    AddLogItem(LogType.Info, "Finished populating download queue list.");
                }
                else {
                    AddLogItem(LogType.Info, "Nothing selected to populate download queue list.");
                }

                return downloadQueue.Count > 0;
            }
            catch (Exception ex) {
                AddLogItem(LogType.Warning, string.Format("Could not populate the download queue list! Details: {0}", ex.Message));
            }
            return false;
        }
        private void StartDownloads()
        {
            try {
                currentInstallState = InstallState.Download;
                RefreshCurrentStepLabel();

                AddLogItem(LogType.Info, "Starting downloads...");

                // Get next download item in queue and log
                FileDownload nextDownloadItem = downloadQueue.Dequeue();
                AddLogItem(LogType.Info, string.Format("Downloading {0}...", nextDownloadItem.FileName));

                // Check and create temp dir if it doesn't exists
                string tempDir = ".\\Data\\Temp";
                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

                // Google Drive Method
                //if (!string.IsNullOrEmpty(nextDownloadItem.GoogleDriveID)) downloadURL = string.Format("https://www.googleapis.com/drive/v3/files/{0}?alt=media&key=AIzaSyDLAFt7hb18Zo1Fdxv3xQsXnxMjhYCImIs", nextDownloadItem.GoogleDriveID);

                // Start downloading
                downloadStartTime = DateTime.Now;
                downloadWebClient.DownloadFileAsync(new Uri(nextDownloadItem.DownloadURL), string.Format("{0}\\{1}", tempDir, nextDownloadItem.FileName), nextDownloadItem);
            }
            catch (Exception ex) {
                AddLogItem(LogType.Error, string.Format("Error while trying to start downloading stuff from the download queue! Continuing without mods. Details: {0}", ex.Message));

                if (MainFunctions.downgradingInfo.InstallPrerequisites) { // Install Prerequisites
                    StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                }
                else {
                    if (MainFunctions.downgradingInfo.ConfigureForGFWL) { // Install GFWL Prerequisites
                        StartInstallPrerequisites(Prerequisites.GFWL);
                    }
                    else { // Continue with Radio/Game downgrade
                        if (MainFunctions.downgradingInfo.SelectedRadioDowngrader != RadioDowngrader.None) {
                            BeginExtractionProcess(InstallState.RadioDowngrade);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
                        }
                    }
                }
            }
        }
        #endregion

        #region Prerequisites
        public void StartInstallPrerequisites(Prerequisites state)
        {
            prerequisiteErrored = false;

            currentInstallState = InstallState.Prerequisites;
            currentPrerequisite = state;
            RefreshCurrentStepLabel();

            Dispatcher.Invoke(() => {
                DowngradeProgressBar.IsIndeterminate = true;
                StatusLabel.Text = "Installing Prerequisites. This process could take a few minutes...";
            });

            Task.Run(() => {

                string tempDirDirectXLoc = ".\\Data\\Temp\\DirectX";
                if (!Directory.Exists(tempDirDirectXLoc)) Directory.CreateDirectory(tempDirDirectXLoc);

                using (Process p = new Process()) {

                    p.EnableRaisingEvents = true;

                    switch (currentPrerequisite) {
                        case Prerequisites.VisualCPlusPlus:
                            AddLogItem(LogType.Info, "Installing Visual C++...");
                            p.StartInfo.FileName = ".\\Data\\Temp\\vcredist_x86.exe";
                            p.StartInfo.Arguments = "/Q";
                            break;
                        case Prerequisites.DirectXExtract:
                            AddLogItem(LogType.Info, "Extracting DirectX setup files...");
                            p.StartInfo.FileName = ".\\Data\\Temp\\directx_Jun2010_redist.exe";
                            p.StartInfo.Arguments = string.Format("/Q /C /T:\"{0}\"", Path.GetFullPath(tempDirDirectXLoc));
                            break;
                        case Prerequisites.DirectX:
                            AddLogItem(LogType.Info, "Installing DirectX...");
                            if (Directory.Exists(tempDirDirectXLoc)) {
                                string setupFile = string.Format("{0}\\DXSETUP.exe", tempDirDirectXLoc);
                                if (File.Exists(setupFile)) {
                                    p.StartInfo.FileName = setupFile;
                                    p.StartInfo.Arguments = "/silent";
                                }
                            }
                            break;
                        case Prerequisites.GFWL:
                            AddLogItem(LogType.Info, "Installing Games for Windows Live...");
                            p.StartInfo.FileName = ".\\Data\\Temp\\gfwlivesetup.exe";
                            break;
                    }

                    try {
                        p.Start();
                        p.WaitForExit();
                    }
                    catch (Exception ex) {
                        AddLogItem(LogType.Error, string.Format("An error occured while installing prerequisite '{0}'. Details: {1}", currentPrerequisite.ToString(), ex.Message));
                        prerequisiteErrored = true;
                    }

                }
            }).ContinueWith(result => {

                switch (currentPrerequisite) {
                    case Prerequisites.VisualCPlusPlus:
                        if (!prerequisiteErrored) AddLogItem(LogType.Info,"Visual C++ installed!");
                        StartInstallPrerequisites(Prerequisites.DirectXExtract);
                        break;

                    case Prerequisites.DirectXExtract:
                        if (!prerequisiteErrored) AddLogItem(LogType.Info, "DirectX setup files extracted");
                        StartInstallPrerequisites(Prerequisites.DirectX);
                        break;
                    case Prerequisites.DirectX:
                        if (!prerequisiteErrored) AddLogItem(LogType.Info, "DirectX installed!");
                        Dispatcher.Invoke(() => { DowngradeProgressBar.IsIndeterminate = false; });

                        // Install GFWL if selected or continue with Radio Downgrade
                        if (MainFunctions.downgradingInfo.ConfigureForGFWL)
                            StartInstallPrerequisites(Prerequisites.GFWL);
                        else
                            BeginExtractionProcess(InstallState.RadioDowngrade);

                        break;

                    case Prerequisites.GFWL:
                        if (!prerequisiteErrored) AddLogItem(LogType.Info, "Games for Windows Live installed!");
                        Dispatcher.Invoke(() => { DowngradeProgressBar.IsIndeterminate = false; });
                        BeginExtractionProcess(InstallState.RadioDowngrade);
                        break;
                }

            });
        }
        #endregion

        #region Radio
        public void StartRadioDowngrade()
        {
            string fileLoc = string.Format("{0}\\install.bat", MainFunctions.downgradingInfo.IVWorkingDirectoy);
            if (File.Exists(fileLoc)) {
                radioDowngraderProc = new Process();
                radioDowngraderProc.EnableRaisingEvents = true;
                radioDowngraderProc.OutputDataReceived += RadioDowngraderProc_OutputDataReceived;
                radioDowngraderProc.ErrorDataReceived += RadioDowngraderProc_ErrorDataReceived;
                radioDowngraderProc.Exited += RadioDowngraderProc_Exited;
                radioDowngraderProc.StartInfo.FileName = fileLoc;
                radioDowngraderProc.StartInfo.WorkingDirectory = MainFunctions.downgradingInfo.IVWorkingDirectoy;
                radioDowngraderProc.StartInfo.UseShellExecute = false;
                radioDowngraderProc.StartInfo.CreateNoWindow = true;
                radioDowngraderProc.StartInfo.RedirectStandardOutput = true;
                radioDowngraderProc.Start();
                radioDowngraderProc.BeginOutputReadLine();
                radioDowngraderProc.WaitForExit();
            }
            else {
                radioDowngraderErrored = true;
                AddLogItem(LogType.Error, "Could not downgrade the radio! File install.bat does not exists. Continuing without radio downgrade.");
                RadioDowngraderProc_Exited(this, EventArgs.Empty);
            }
        }
        private void RadioDowngraderProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try {
                string rawLine = e.Data;
                if (!string.IsNullOrWhiteSpace(rawLine)) {
                    if (rawLine.Contains(">")) {
                        string line = rawLine.Split('>')[1];
                        AddLogItem(LogType.Info, string.Format("[Radio Downgrader] {0}", line));
                    }
                }
            }
            catch (Exception ex) {
                AddLogItem(LogType.Error, string.Format("Error in 'RadioDowngraderProc_OutputDataReceived'. Details: {0}", ex.Message));
            }
        }
        private void RadioDowngraderProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try {
                string rawLine = e.Data;
                if (!string.IsNullOrWhiteSpace(rawLine)) {
                    if (rawLine.Contains(">")) {
                        string line = rawLine.Split('>')[1];
                        AddLogItem(LogType.Error, string.Format("[Radio Downgrader] {0}", line));
                    }
                }
            }
            catch (Exception ex) {
                AddLogItem(LogType.Info, string.Format("Error in 'RadioDowngraderProc_ErrorDataReceived'. Details: {0}", ex.Message));
            }
        }
        private void RadioDowngraderProc_Exited(object sender, EventArgs e)
        {
            try {
                if (!radioDowngraderErrored) AddLogItem(LogType.Info, "Radio downgrade finished!");

                // Clean up
                if (radioDowngraderProc != null) {
                    if (!radioDowngraderProc.HasExited) radioDowngraderProc.Kill();
                    radioDowngraderProc.OutputDataReceived -= RadioDowngraderProc_OutputDataReceived;
                    radioDowngraderProc.Exited -= RadioDowngraderProc_Exited;
                    radioDowngraderProc.Dispose();
                    radioDowngraderProc = null;
                }

                ChangeProgressBarIndeterminateState(false);
                Dispatcher.Invoke(() => {
                    if (MainFunctions.downgradingInfo.SelectedRadioDowngrader == RadioDowngrader.SneedsDowngrader) {
                        switch (MainFunctions.downgradingInfo.SelectedVladivostokType) {
                            case VladivostokTypes.Old:
                                BeginExtractionProcess(InstallState.RadioOldVladivostok);
                                break;
                            case VladivostokTypes.New:
                                BeginExtractionProcess(InstallState.RadioNewVladivostok);
                                break;
                        }
                    }
                    else {
                        if (MainFunctions.downgradingInfo.InstallNoEFLCMusicInIVFix) {
                            BeginExtractionProcess(InstallState.RadioNoEFLCMusicInIVFix);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
                        }
                    }
                });
            }
            catch (Exception ex) {
                instance.ShowErrorScreen(ex);
            }
        }
        #endregion

        #region Extraction
        private void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite) {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries) {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);

                progress++;
                UpdateExtractionProgressBar(file.Name, progress, archive.Entries.Count);
            }
        }
        public void BeginExtractionProcess(InstallState state)
        {
            currentInstallState = state;
            RefreshCurrentStepLabel();

            Task.Run(() => {
                string fileLoc = string.Empty;

                // Set file location from InstallState
                switch (state) {

                    // Radio Downgrade
                    case InstallState.RadioDowngrade:

                        switch (MainFunctions.downgradingInfo.SelectedRadioDowngrader) {
                            case RadioDowngrader.SneedsDowngrader:
                                fileLoc = ".\\Data\\Temp\\SneedsRadioDowngrader.zip";
                                AddLogItem(LogType.Info, "Extracting files for Sneeds Radio Downgrader...");
                                break;
                            case RadioDowngrader.LegacyDowngrader:
                                fileLoc = ".\\Data\\Temp\\LegacyRadioDowngrader.zip";
                                AddLogItem(LogType.Info, "Extracting files for Legacy Radio Downgrader...");
                                break;
                            default:
                                radioDowngraderErrored = true;
                                AddLogItem(LogType.Warning, "selectedRadioDowngrader is none! Continuing without radio downgrade.");
                                RadioDowngraderProc_Exited(this, EventArgs.Empty);
                                return;
                        }

                        break;
                    case InstallState.RadioOldVladivostok:

                        fileLoc = ".\\Data\\Temp\\WithoutNewVladivostok.zip";
                        AddLogItem(LogType.Info, "Extracting old Vladivostok files...");

                        break;
                    case InstallState.RadioNewVladivostok:

                        fileLoc = ".\\Data\\Temp\\WithNewVladivostok.zip";
                        AddLogItem(LogType.Info, "Extracting new Vladivostok files...");

                        break;
                    case InstallState.RadioNoEFLCMusicInIVFix:

                        fileLoc = ".\\Data\\Temp\\EpisodeOnlyMusicCE.zip";
                        AddLogItem(LogType.Info, "Extracting files for No EFLC Music In IV Fix...");

                        break;

                        // Game Downgrade
                    case InstallState.GameDowngrade:

                        switch (MainFunctions.downgradingInfo.DowngradeTo) {
                            case GameVersion.v1080:
                                fileLoc = ".\\Data\\Temp\\1080.zip";
                                break;
                            case GameVersion.v1070:
                                fileLoc = ".\\Data\\Temp\\1070.zip";
                                break;
                            case GameVersion.v1040:
                                fileLoc = ".\\Data\\Temp\\1040.zip";
                                break;
                        }
                        AddLogItem(LogType.Info, "Extracting files for game downgrader...");

                        break;
                    case InstallState.ModInstall:

                        fileLoc = string.Format(".\\Data\\Temp\\{0}", modToInstall);

                        break;
                }

                // Checks
                if (string.IsNullOrEmpty(fileLoc)) {
                    AddLogItem(LogType.Warning, "fileLoc is empty!");
                    return;
                }
                if (!File.Exists(fileLoc)) {
                    AddLogItem(LogType.Warning, string.Format("File {0} not found. Skipping.", Path.GetFileName(fileLoc)));
                    return;
                }

                // Begin extracting file
                try {
                    using (ZipArchive archive = ZipFile.OpenRead(fileLoc)) {
                        ExtractToDirectory(archive, MainFunctions.downgradingInfo.IVWorkingDirectoy, true);
                    }
                }
                catch (Exception ex) { // On error...
                    errored = true;

                    AddLogItem(LogType.Error, string.Format("An error occured. Details: {0}", ex.Message));
                    instance.ChangeStep(Steps.Error, new List<object>() { ex, string.Format("Crashed at step: {0}", currentInstallState.ToString()) });
                }

            }).ContinueWith(result => {
                if (errored)
                    return;

                switch(currentInstallState) {
                    case InstallState.RadioDowngrade:
                        AddLogItem(LogType.Info, "Radio downgrade files extracted! Starting radio downgrade.");
                        UpdateStatusText("Radio is currently getting downgraded...");
                        ChangeProgressBarIndeterminateState(true);
                        StartRadioDowngrade();
                        break;
                    case InstallState.RadioOldVladivostok:
                        AddLogItem(LogType.Info, "Old Vladivostok files extracted and installed!");

                        if (MainFunctions.downgradingInfo.InstallNoEFLCMusicInIVFix) {
                            BeginExtractionProcess(InstallState.RadioNoEFLCMusicInIVFix);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
                        }
                        break;
                    case InstallState.RadioNewVladivostok:
                        AddLogItem(LogType.Info, "New Vladivostok files extracted and installed!");

                        if (MainFunctions.downgradingInfo.InstallNoEFLCMusicInIVFix) {
                            BeginExtractionProcess(InstallState.RadioNoEFLCMusicInIVFix);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
                        }
                        break;
                    case InstallState.RadioNoEFLCMusicInIVFix:
                        AddLogItem(LogType.Info, "No EFLC Music In IV Fix files extracted and installed!");
                        BeginExtractionProcess(InstallState.GameDowngrade);
                        break;
                    case InstallState.GameDowngrade:
                        AddLogItem(LogType.Info, "Game downgrade files extracted and installed!");

                        if (MainFunctions.downgradingInfo.SelectedMods.Count != 0) {
                            AddLogItem(LogType.Info, "Extracting and installing selected mods...");
                            StartInstallingMods();
                        }
                        else {
                            Finish();
                        }
                        break;
                    case InstallState.ModInstall:
                        StartInstallingMods();
                        break;
                }

                result.Dispose();
            });
        }
        #endregion

        #region Mods
        public void StartInstallingMods()
        {
            if (MainFunctions.downgradingInfo.SelectedMods.Count != 0) {
                for (int i = 0; i < MainFunctions.downgradingInfo.SelectedMods.Count; i++) {
                    JsonObjects.ModInformation item = MainFunctions.downgradingInfo.SelectedMods[i];
                    modToInstall = item.FileName;
                    MainFunctions.downgradingInfo.SelectedMods.RemoveAt(i);
                    break;
                }
                BeginExtractionProcess(InstallState.ModInstall);
            }
            else {
                Finish();
            }
        }
        #endregion

        #region FinalThings
        private void DeleteDLCsFor1040()
        {
            try {
                string tbogtDir = MainFunctions.downgradingInfo.IVWorkingDirectoy + "\\TBoGT";
                string tladDir = MainFunctions.downgradingInfo.IVWorkingDirectoy + "\\TLAD";
                if (Directory.Exists(tbogtDir)) {
                    Directory.Delete(tbogtDir, true);
                    if (!Directory.Exists(tbogtDir)) AddLogItem(LogType.Info, "Deleted unnecessary TBoGT DLC Folder because GTA IV 1040 can't load DLCs.");
                }
                if (Directory.Exists(tladDir)) {
                    Directory.Delete(tladDir, true);
                    if (!Directory.Exists(tladDir)) AddLogItem(LogType.Info, "Deleted unnecessary TLAD DLC Folder because GTA IV 1040 can't load DLCs.");
                }
            }
            catch (Exception ex) {
                AddLogItem(LogType.Error, string.Format("Could not remove DLC Folders! Details: {0}", ex.Message));
            }
        }
        #endregion

        #endregion

        #region Functions
        public string GetFileLocationInTempFolder(string fileName)
        {
            return string.Format(".\\Data\\Temp\\{0}", fileName);
        }
        public bool IsZipFileCorrupted(string filePath)
        {
            try {
                using (ZipArchive arch = ZipFile.OpenRead(filePath))
                    return false;
            }
            catch (Exception) {
                return true;
            }
        }
        #endregion

        #region Events
        private void DownloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            FileDownload downloadItem = (FileDownload)e.UserState;

            try {
                if (e.Cancelled) AddLogItem(LogType.Warning, string.Format("Download was cancelled for {0}", downloadItem.FileName));
                if (e.Error != null) AddLogItem(LogType.Error, string.Format("An error occured while downloading item {0}. Details: {1}", downloadItem.FileName, e.Error.Message));

                // Download
                Task.Run(() => {

                    // Decompress file if necessary
                    if (downloadItem.NeedsToBeDecompressed) {

                        string txt = string.Format("Decompressing {0}...", downloadItem.FileName);
                        AddLogItem(LogType.Info, txt);
                        UpdateStatusText(txt);
                        ChangeProgressBarIndeterminateState(true);

                        string fileLoc = string.Format(".\\Data\\Temp\\{0}", downloadItem.FileName);

                        // Get byte array from file
                        byte[] bArr = null;
                        using (FileStream fs = new FileStream(fileLoc, FileMode.Open, FileAccess.Read)) {
                            bArr = Helper.GetByteArray(fs);
                        }

                        // Decompress byte array
                        bArr = Helper.DataCompression.DecompressByteArray(bArr);

                        // Write decompressed byte array to file
                        using (FileStream fs = new FileStream(fileLoc, FileMode.Open, FileAccess.Write)) {
                            fs.Write(bArr, 0, bArr.Length);
                        }

                        // Free stuff
                        bArr = null;

                        // Force garbage collection to free some memory
                        GC.Collect();

                        AddLogItem(LogType.Info, string.Format("{0} decompressed!", downloadItem.FileName));
                        ChangeProgressBarIndeterminateState(false);

                    }

                    // Download remaining files or continue with downgrading
                    if (downloadQueue.Count != 0) { // Download remaining files

                        // Get next download item in queue and log
                        FileDownload nextDownloadItem = downloadQueue.Dequeue();
                        AddLogItem(LogType.Info, string.Format("Downloading {0}...", nextDownloadItem.FileName));

                        // Start downloading
                        downloadStartTime = DateTime.Now;
                        Dispatcher.Invoke(() => downloadWebClient.DownloadFileAsync(new Uri(nextDownloadItem.DownloadURL), string.Format(".\\Data\\Temp\\{0}", nextDownloadItem.FileName), nextDownloadItem));

                    }
                    else { // Continue with downgrading
                        Dispatcher.Invoke(() => {
                            AddLogItem(LogType.Info, "Download of files completed!");

                            if (MainFunctions.downgradingInfo.InstallPrerequisites) { // Install Prerequisites
                                StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                            }
                            else {
                                if (MainFunctions.downgradingInfo.ConfigureForGFWL) { // Install GFWL Prerequisites
                                    StartInstallPrerequisites(Prerequisites.GFWL);
                                }
                                else { // Continue with Radio/Game downgrade
                                    if (MainFunctions.downgradingInfo.SelectedRadioDowngrader != RadioDowngrader.None) {
                                        BeginExtractionProcess(InstallState.RadioDowngrade);
                                    }
                                    else {
                                        BeginExtractionProcess(InstallState.GameDowngrade);
                                    }
                                }
                            }
                        });
                    }

                });
            }
            catch (Exception ex) {
                AddLogItem(LogType.Error, string.Format("[Try Catch] An error occured while downloading item {0}. Details: {1}", downloadItem.FileName, ex.Message));
                instance.ShowErrorScreen(ex);
            }
        }
        private void DownloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            FileDownload currentDownloadItem = (FileDownload)e.UserState;

            TimeSpan timeSpent = DateTime.Now - downloadStartTime;
            int secondsRemaining = (int)(timeSpent.TotalSeconds / e.BytesReceived * (currentDownloadItem.FileSize - e.BytesReceived));
            TimeSpan estTime = TimeSpan.FromSeconds(secondsRemaining);

            double percentageDownloaded = (double)e.BytesReceived / currentDownloadItem.FileSize * 100;
            DowngradeProgressBar.Value = percentageDownloaded;
            StatusLabel.Text = string.Format("Downloaded {0} of {1} ({2}%). Estimated time remaining: {3}", Helper.GetExactFileSize2(e.BytesReceived), Helper.GetExactFileSize2(currentDownloadItem.FileSize), Math.Round(percentageDownloaded, 0).ToString(), estTime.ToString(@"hh\:mm\:ss"));
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Destroy WebClient
            downloadWebClient.DownloadProgressChanged -= DownloadClient_DownloadProgressChanged;
            downloadWebClient.DownloadFileCompleted -= DownloadClient_DownloadFileCompleted;
            downloadWebClient.CancelAsync();
            downloadWebClient.Dispose();
            downloadWebClient = null;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Init WebClient
            downloadWebClient = new WebClient();
            downloadWebClient.Credentials = new NetworkCredential("ivdowngr", "7MY4qi2a8g");
            downloadWebClient.DownloadProgressChanged += DownloadClient_DownloadProgressChanged;
            downloadWebClient.DownloadFileCompleted += DownloadClient_DownloadFileCompleted;

            // BottomGrid Colours
            if (MainFunctions.isPrideMonth) {
                if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                    BottomGrid.Background = "#B3000000".ToBrush();
                }
                else { // Use Rainbow Colours
                    BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
                }
            }

            // Populate download queue
            canDownloadFiles = PopulateDownloadQueueList();

            // Change taskbar progressbar state
            instance.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            // Do pre stuff
            RemoveOldFolders();
            CreateXLivelessAddonFolders();
            if (KillAnyGTAProcessIfRunning()) AddLogItem(LogType.Info, "Any currently opened GTA IV Process got killed.");

            // Start things
            try {
                if (instance.confirmUC.MakeABackupForMeCheckbox.IsChecked.Value) { // Create Backup
                    StartCreatingBackup();
                }
                else {
                    if (canDownloadFiles) { // Download Files
                        StartDownloads();
                    }
                    else { // Continue without downloading files
                        if (MainFunctions.downgradingInfo.InstallPrerequisites) { // Install Prerequisites
                            StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                        }
                        else {
                            if (MainFunctions.downgradingInfo.ConfigureForGFWL) { // Install GFWL Prerequisites
                                StartInstallPrerequisites(Prerequisites.GFWL);
                            }
                            else { // Continue with Radio/Game downgrade
                                if (MainFunctions.downgradingInfo.SelectedRadioDowngrader != RadioDowngrader.None) {
                                    BeginExtractionProcess(InstallState.RadioDowngrade);
                                }
                                else {
                                    BeginExtractionProcess(InstallState.GameDowngrade);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                instance.ShowErrorScreen(ex);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            instance.NextStep();
        }

    }
}
