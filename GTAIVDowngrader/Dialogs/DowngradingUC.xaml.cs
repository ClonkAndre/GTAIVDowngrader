using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;

using GTAIVDowngrader.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class DowngradingUC : UserControl {

        // - - - Downgrading Order - - -
        // 1. Create Backup if user wants to otherwise skip to 2.
        // 2. Start downloads if there is anything to download if not then skip to 3.
        // 3. Install Prerequisites if user wants to otherwise skip to 4.
        // 4. Install GFWL Prerequisites if user checked the Configure For GFWL CheckBox otherwise skip to 5.
        // 5. If user selected a Radio Downgrader, do Radio Downgrade otherwise skip to 6.
        // 6. Do Game Downgrading
        // 7. Install Mods if there any to install otherwise skip to 8.
        // 8. Finish

        #region Variables and Enums
        private MainWindow instance;

        // Download Stuff
        public List<DownloadItem> downloadQueue;
        private DownloadItem currentDownloadItem;
        private DateTime downloadStartTime; // Remaining Time Calculation

        // Processes
        private Process radioDowngraderProc;

        // States
        private InstallState currentInstallState;
        private Prerequisites currentPrerequisite;

        // Other
        public string latestLogFileName;

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

        #region Classes
        public class DownloadItem {

            #region Properties
            public string FileName { get; private set; }
            public string URL { get; private set; }
            public long FileSize { get; private set; }
            #endregion

            #region Constuctor
            public DownloadItem(string fileName, long fileSize, string url)
            {
                FileName = fileName;
                FileSize = fileSize;
                URL = url;
            }
            #endregion

        }
        #endregion

        #region Constructor
        public DowngradingUC()
        {
            downloadQueue = new List<DownloadItem>();
            InitializeComponent();
        }
        public DowngradingUC(MainWindow window)
        {
            instance = window;
            downloadQueue = new List<DownloadItem>();
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

        public void RemoveDownloadItemFromList(string fromURL)
        {
            for (int i = 0; i < downloadQueue.Count; i++) {
                DownloadItem item = downloadQueue[i];
                if (item.URL == fromURL) {
                    downloadQueue.RemoveAt(i);
                }
            }
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
        private void CreateMissingXLivelessAddonFolders()
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

                // Radio stuff
                JsonObjects.DowngradeInformation file;
                switch (MainFunctions.downgradingInfo.SelectedVladivostokType) {
                    case VladivostokTypes.New:
                        file = MainFunctions.GetDowngradeFileByFileName("WithNewVladivostok.zip");
                        downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                        break;
                    case VladivostokTypes.Old:
                        file = MainFunctions.GetDowngradeFileByFileName("WithoutNewVladivostok.zip");
                        downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                        break;
                }
                if (MainFunctions.downgradingInfo.InstallNoEFLCMusicInIVFix) {
                    file = MainFunctions.GetDowngradeFileByFileName("EpisodeOnlyMusicCE.zip");
                    downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                }

                // Mods
                for (int i = 0; i < MainFunctions.downgradingInfo.SelectedMods.Count; i++) {
                    JsonObjects.ModInformation item = MainFunctions.downgradingInfo.SelectedMods[i];
                    downloadQueue.Add(new DownloadItem(item.FileName, item.FileSize, item.DownloadURL));
                }

                // Prerequisites
                if (MainFunctions.downgradingInfo.InstallPrerequisites) {
                    file = MainFunctions.GetDowngradeFileByFileName("directx_Jun2010_redist.exe");
                    downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                    file = MainFunctions.GetDowngradeFileByFileName("vcredist_x86.exe");
                    downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                }
                if (MainFunctions.downgradingInfo.ConfigureForGFWL) {
                    file = MainFunctions.GetDowngradeFileByFileName("gfwlivesetup.exe");
                    downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                    file = MainFunctions.GetDowngradeFileByFileName("xliveredist.msi");
                    downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));

                    if (Environment.Is64BitOperatingSystem) {
                        file = MainFunctions.GetDowngradeFileByFileName("wllogin_64.msi");
                        downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                    }
                    else {
                        file = MainFunctions.GetDowngradeFileByFileName("wllogin_32.msi");
                        downloadQueue.Add(new DownloadItem(file.FileName, file.FileSize, file.DownloadURL));
                    }
                }

                // Log
                if (downloadQueue.Count != 0) {

                    // Log download queue list to file
                    for (int i = 0; i < downloadQueue.Count; i++) {
                        DownloadItem item = downloadQueue[i];
                        AddLogItem(LogType.Info, string.Format("Populated download queue list with: {0}, URL: {1}", item.FileName, item.URL), true, false);
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

                currentDownloadItem = downloadQueue[0];
                AddLogItem(LogType.Info, string.Format("Downloading {0}...", currentDownloadItem.FileName));

                string tempDir = ".\\Data\\Temp";
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                downloadStartTime = DateTime.Now;
                MainFunctions.downloadWebClient.DownloadFileAsync(new Uri(currentDownloadItem.URL), string.Format("{0}\\{1}", tempDir, currentDownloadItem.FileName), currentDownloadItem.URL);
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
                                fileLoc = ".\\Data\\Files\\Radio\\SneedsRadioDowngrader.zip";
                                AddLogItem(LogType.Info, "Extracting files for Sneeds Radio Downgrader...");
                                break;
                            case RadioDowngrader.LegacyDowngrader:
                                fileLoc = ".\\Data\\Files\\Radio\\LegacyRadioDowngrader.zip";
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
                                fileLoc = ".\\Data\\Files\\1080\\1080.zip";
                                break;
                            case GameVersion.v1070:
                                fileLoc = ".\\Data\\Files\\1070\\1070.zip";
                                break;
                            case GameVersion.v1040:
                                fileLoc = ".\\Data\\Files\\1040\\1040.zip";
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
                    ZipArchive archive = ZipFile.OpenRead(fileLoc);
                    ExtractToDirectory(archive, MainFunctions.downgradingInfo.IVWorkingDirectoy, true);
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

        #region Events
        private void DownloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try {
                if (e.Cancelled) AddLogItem(LogType.Warning, string.Format("Download was cancelled for {0}", e.UserState.ToString()));

                if (e.UserState != null)
                    RemoveDownloadItemFromList(e.UserState.ToString());
                else
                    AddLogItem(LogType.Warning, "Could not remove item from download queue because e.UserState was null in DownloadFileCompleted.");

                if (e.Error != null) {
                    string item = e.UserState != null ? e.UserState.ToString() : "NULL";
                    AddLogItem(LogType.Error, string.Format("An error occured while downloading item {0}. Details: {1}", item, e.Error.Message));
                }

                // Download
                Task.Run(() => {

                    if (downloadQueue.Count != 0) { // Download remaining files

                        currentDownloadItem = downloadQueue[downloadQueue.Count - 1];
                        AddLogItem(LogType.Info, string.Format("Downloading {0}...", currentDownloadItem.FileName));
                        downloadStartTime = DateTime.Now;

                        Dispatcher.Invoke(() => MainFunctions.downloadWebClient.DownloadFileAsync(new Uri(currentDownloadItem.URL), string.Format(".\\Data\\Temp\\{0}", currentDownloadItem.FileName), currentDownloadItem.URL));

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
                string item = e.UserState != null ? e.UserState.ToString() : "NULL";
                AddLogItem(LogType.Error, string.Format("[Try Catch] An error occured while downloading item {0}. Details: {1}", item, ex.Message));
                instance.ShowErrorScreen(ex);
            }
        }
        private void DownloadClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            DowngradeProgressBar.Maximum = 100;
            if (currentDownloadItem != null) {
                double progressBarValue = Math.Floor(double.Parse(e.BytesReceived.ToString()) / double.Parse(currentDownloadItem.FileSize.ToString()) * 100);

                TimeSpan timeSpent = DateTime.Now - downloadStartTime;
                int secondsRemaining = (int)(timeSpent.TotalSeconds / e.BytesReceived * (currentDownloadItem.FileSize - e.BytesReceived));
                TimeSpan estTime = TimeSpan.FromSeconds(secondsRemaining);

                DowngradeProgressBar.Value = progressBarValue;
                StatusLabel.Text = string.Format("Downloaded {0} of {1}. Estimated time remaining: {2}", Helper.GetExactFileSize2(e.BytesReceived), Helper.GetExactFileSize2(currentDownloadItem.FileSize), estTime.ToString(@"hh\:mm\:ss"));
            }
            else {
                DowngradeProgressBar.Value = e.ProgressPercentage;
                StatusLabel.Text = string.Format("Downloaded {0} of {1}", Helper.GetExactFileSize2(e.BytesReceived), Helper.GetExactFileSize2(e.TotalBytesToReceive));
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unregister events
            MainFunctions.downloadWebClient.DownloadProgressChanged -= DownloadClient_DownloadProgressChanged;
            MainFunctions.downloadWebClient.DownloadFileCompleted -= DownloadClient_DownloadFileCompleted;
            MainFunctions.downloadWebClient.CancelAsync();

            // Create log file
            try {
                string logFolder = ".\\Data\\Logs";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                latestLogFileName = string.Format("{0}\\Log.{1}.{2}_{3}_{4}.log", logFolder, DateTime.Now.Year.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString());
                File.WriteAllLines(latestLogFileName, MainFunctions.logItems);
            }
            catch (UnauthorizedAccessException) {
                MainFunctions.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A UnauthorizedAccessException occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
            catch (Exception) {
                MainFunctions.Notification.ShowNotification(NotificationType.Error, 5000, "Could not create log file", "A unknown error occured while trying to create log file.", "COULD_NOT_CREATE_LOG_FILE");
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // BottomGrid Colours
            if (MainFunctions.isPrideMonth) {
                if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                    BottomGrid.Background = "#B3000000".ToBrush();
                }
                else { // Use Rainbow Colours
                    BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
                }
            }

            // Register events
            MainFunctions.downloadWebClient.DownloadProgressChanged += DownloadClient_DownloadProgressChanged;
            MainFunctions.downloadWebClient.DownloadFileCompleted += DownloadClient_DownloadFileCompleted;

            // Populate download queue
            canDownloadFiles = PopulateDownloadQueueList();

            // Change taskbar progressbar state
            instance.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            // Do pre stuff
            RemoveOldFolders();
            CreateMissingXLivelessAddonFolders();
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
