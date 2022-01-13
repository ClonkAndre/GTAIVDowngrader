using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using GTAIVDowngrader.Controls;
using Ionic.Zip;

namespace GTAIVDowngrader.Dialogs {
    public partial class DowngradingUC : UserControl {

        #region Variables and Enums
        private MainWindow instance;

        public List<LogItem> logItems;

        private Task extractionProcessTask;
        private Process radioDowngraderProc, prerequisitesInstallProc;
        private ZipEntry zipEntry;

        private bool errored, backupErrored, prerequisiteErrored, radioDowngraderErrored;
        private int totalFiles;
        private int progress;

        private string modToInstall;
        private StringBuilder logFileBuilder;

        private InstallState currentInstallState;
        private Prerequisites currentPrerequisite;

        // Enums
        public enum InstallState
        {
            Backup,
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
        }
        #endregion

        #region Structs
        public struct LogItem
        {
            #region Properties
            public LogType LogType { get; private set; }
            public string LogText { get; private set; }
            #endregion

            #region Constructor
            public LogItem(LogType type, string text)
            {
                LogType = type;
                LogText = text;
            }
            #endregion

            #region Overrides
            public override string ToString()
            {
                return string.Format("[{0}] {1}", LogType.ToString(), LogText);
            }
            #endregion
        }
        #endregion

        #region Constructor
        public DowngradingUC()
        {
            logItems = new List<LogItem>();
            logFileBuilder = new StringBuilder();
            InitializeComponent();
        }
        public DowngradingUC(MainWindow window)
        {
            instance = window;
            logItems = new List<LogItem>();
            logFileBuilder = new StringBuilder();
            InitializeComponent();
        }
        #endregion

        #region Methods
        public void AddLogItem(string str, bool autoScroll = true)
        {
            Dispatcher.Invoke(() => {
                StatusListbox.Items.Add(str);
                if (autoScroll) {
                    StatusListbox.SelectedIndex = StatusListbox.Items.Count - 1;
                    StatusListbox.ScrollIntoView(StatusListbox.SelectedItem);
                }
            });
        }
        public void AddLogItem(string str, bool alsoAddToLogList, LogType logType, bool dontAddToStatusLogList = false, bool autoScroll = true)
        {
            Dispatcher.Invoke(() => {
                if (!dontAddToStatusLogList) {
                    StatusListbox.Items.Add(str);
                    if (autoScroll) {
                        StatusListbox.SelectedIndex = StatusListbox.Items.Count - 1;
                        StatusListbox.ScrollIntoView(StatusListbox.SelectedItem);
                    }
                }
                if (alsoAddToLogList) {
                    logItems.Add(new LogItem(logType, str));
                }
            });
        }
        public void RefreshCurrentStepLabel()
        {
            Dispatcher.Invoke(() => {
                switch (currentInstallState) {
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
                AddLogItem("Finshed!", true, LogType.Info);
                NextButton.IsEnabled = true;
            });
        }

        public void StartInstallingMods()
        {
            if (instance.s4_SelectComponents.selectedMods.Count != 0) {
                for (int i = 0; i < instance.s4_SelectComponents.selectedMods.Count; i++) {
                    ModItem item = instance.s4_SelectComponents.selectedMods[i];
                    modToInstall = item.ModFilename;
                    instance.s4_SelectComponents.selectedMods.RemoveAt(i);
                    break;
                }
                BeginExtractionProcess(InstallState.ModInstall);
            }
            else {
                Finish();
            }
        }

        #region Backup
        public void StartCreatingBackup()
        {
            string sourcePath = instance.s1_SelectIVExe.IVInstallDirectory;
            string targetPath = instance.confirmUC.BackupLocationTextbox.Text;
            bool createZIP = instance.confirmUC.CreateBackupInZIPFileCheckBox.IsChecked.Value;

            currentInstallState = InstallState.Backup;
            RefreshCurrentStepLabel();
            Task.Run(() => {

                AddLogItem("Creating a backup...");

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
                        Dispatcher.Invoke(() => {
                            DowngradeProgressBar.Maximum = total;
                        });

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
                    logFileBuilder.AppendLine("Failed to create a backup. Details:");
                    logFileBuilder.Append(ex.ToString());
                    logFileBuilder.AppendLine();
                    logFileBuilder.AppendLine();

                    AddLogItem(string.Format("Failed to create backup. Details: {0}", ex.Message), true, LogType.Error);
                    backupErrored = true;
                }

            }).ContinueWith(result => {

                if (!backupErrored) AddLogItem("Backup created!", true, LogType.Info);

                Dispatcher.Invoke(() => {
                    DowngradeProgressBar.IsIndeterminate = false;
                });

                Dispatcher.Invoke(() => {
                    if (instance.s4_SelectComponents.InstallPrerequisitesCheckBox.IsChecked.Value) {
                        StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                    }
                    else {
                        if (instance.s3_SelectRadioDwngrd.selectedRadioDowngrader != RadioDowngrader.None) {
                            BeginExtractionProcess(InstallState.RadioDowngrade);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
                        }
                    }
                });

            });
        }
        #endregion

        #region Radio
        public void StartRadioDowngrade()
        {
            string fileLoc = string.Format("{0}\\install.bat", instance.s1_SelectIVExe.IVInstallDirectory);
            if (File.Exists(fileLoc)) {
                radioDowngraderProc = new Process();
                radioDowngraderProc.EnableRaisingEvents = true;
                radioDowngraderProc.OutputDataReceived += RadioDowngraderProc_OutputDataReceived;
                radioDowngraderProc.Exited += RadioDowngraderProc_Exited;
                radioDowngraderProc.StartInfo.FileName = fileLoc;
                radioDowngraderProc.StartInfo.WorkingDirectory = instance.s1_SelectIVExe.IVInstallDirectory;
                radioDowngraderProc.StartInfo.UseShellExecute = false;
                radioDowngraderProc.StartInfo.CreateNoWindow = true;
                radioDowngraderProc.StartInfo.RedirectStandardOutput = true;
                radioDowngraderProc.Start();
                radioDowngraderProc.BeginOutputReadLine();
                radioDowngraderProc.WaitForExit();
            }
            else {
                radioDowngraderErrored = true;
                AddLogItem("Error while performing radio downgrade: File 'install.bat' does not exists! Continuing without radio downgrade...", true, LogType.Error);
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
                        AddLogItem(string.Format("[Radio Downgrader] {0}", line));
                    }
                }
            }
            catch (Exception ex) {
                logFileBuilder.AppendLine("An error occured in 'RadioDowngraderProc_OutputDataReceived'. Details:");
                logFileBuilder.Append(ex.ToString());
                logFileBuilder.AppendLine();
                logFileBuilder.AppendLine();
                AddLogItem(string.Format("Error in 'RadioDowngraderProc_OutputDataReceived'. Details: {0}", ex.Message), true, LogType.Error);
            }
        }
        private void RadioDowngraderProc_Exited(object sender, EventArgs e)
        {
            try {
                if (!radioDowngraderErrored) AddLogItem("Radio downgrade finished!", true, LogType.Info);

                if (radioDowngraderProc != null) {
                    if (!radioDowngraderProc.HasExited) radioDowngraderProc.Kill();
                    radioDowngraderProc.OutputDataReceived -= RadioDowngraderProc_OutputDataReceived;
                    radioDowngraderProc.Exited -= RadioDowngraderProc_Exited;
                    radioDowngraderProc.Dispose();
                    radioDowngraderProc = null;
                }

                Dispatcher.Invoke(() => {
                    if (instance.s3_SelectRadioDwngrd.selectedRadioDowngrader == RadioDowngrader.SneedsDowngrader) {
                        switch (instance.s3_1_SelectVladivostokType.selectedVladivostokType) {
                            case VladivostokTypes.Old:
                                BeginExtractionProcess(InstallState.RadioOldVladivostok);
                                break;
                            case VladivostokTypes.New:
                                BeginExtractionProcess(InstallState.RadioNewVladivostok);
                                break;
                        }
                    }
                    else {
                        if (instance.s3_SelectRadioDwngrd.NoEFLCMusicInIVCheckbox.IsEnabled) {
                            if (instance.s3_SelectRadioDwngrd.InstallNoEFLCMusicInIVFix) {
                                BeginExtractionProcess(InstallState.RadioNoEFLCMusicInIVFix);
                            }
                            else {
                                BeginExtractionProcess(InstallState.GameDowngrade);
                            }
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

        #region Prerequisites
        public void StartInstallPrerequisites(Prerequisites state)
        {
            prerequisiteErrored = false;

            currentInstallState = InstallState.Prerequisites;
            currentPrerequisite = state;

            Dispatcher.Invoke(() => {
                DowngradeProgressBar.IsIndeterminate = true;
                StatusLabel.Text = "Installing Prerequisites. This process could take a few minutes...";
            });

            RefreshCurrentStepLabel();
            Task.Run(() => {

                // Sleep for a second
                Thread.Sleep(1000);

                prerequisitesInstallProc = new Process();
                prerequisitesInstallProc.EnableRaisingEvents = true;
                //prerequisitesInstallProc.StartInfo.CreateNoWindow = true;

                string tempDirDirectXLoc = ".\\Downgrader\\Temp\\DirectX";
                if (!Directory.Exists(tempDirDirectXLoc)) {
                    Directory.CreateDirectory(tempDirDirectXLoc);
                }

                switch (currentPrerequisite) {
                    case Prerequisites.VisualCPlusPlus:
                        AddLogItem("Installing Visual C++...");
                        prerequisitesInstallProc.StartInfo.FileName = ".\\Downgrader\\Files\\Redistributables\\vcredist_x86.exe";
                        prerequisitesInstallProc.StartInfo.Arguments = "/Q";
                        break;
                    case Prerequisites.DirectXExtract:
                        AddLogItem("Extracting DirectX setup files...");
                        prerequisitesInstallProc.StartInfo.FileName = ".\\Downgrader\\Files\\Redistributables\\directx_Jun2010_redist.exe";
                        prerequisitesInstallProc.StartInfo.Arguments = string.Format("/Q /C /T:\"{0}\"", Path.GetFullPath(tempDirDirectXLoc));
                        break;
                    case Prerequisites.DirectX:
                        AddLogItem("Installing DirectX...");
                        if (Directory.Exists(tempDirDirectXLoc)) {
                            string setupFile = string.Format("{0}\\DXSETUP.exe", tempDirDirectXLoc);
                            if (File.Exists(setupFile)) {
                                prerequisitesInstallProc.StartInfo.FileName = setupFile;
                                prerequisitesInstallProc.StartInfo.Arguments = "/silent";
                            }
                        }
                        break;
                }

                try {
                    prerequisitesInstallProc.Start();
                    prerequisitesInstallProc.WaitForExit();
                }
                catch (Exception ex) {
                    logFileBuilder.AppendLine("Failed to install prerequisites. Details:");
                    logFileBuilder.Append(ex.ToString());
                    logFileBuilder.AppendLine();
                    logFileBuilder.AppendLine();

                    AddLogItem(string.Format("An error occured while installing prerequisite '{0}'. Details: {1}", currentPrerequisite.ToString(), ex.Message), true, LogType.Error);
                    prerequisiteErrored = true;
                }

            }).ContinueWith(result => {

                switch (currentPrerequisite) {
                    case Prerequisites.VisualCPlusPlus:
                        if (!prerequisiteErrored) AddLogItem("Visual C++ installed", true, LogType.Info);
                        StartInstallPrerequisites(Prerequisites.DirectXExtract);
                        break;
                    case Prerequisites.DirectXExtract:
                        if (!prerequisiteErrored) AddLogItem("DirectX setup files extracted", true, LogType.Info);
                        StartInstallPrerequisites(Prerequisites.DirectX);
                        break;
                    case Prerequisites.DirectX:
                        if (!prerequisiteErrored) AddLogItem("DirectX installed", true, LogType.Info);
                        Dispatcher.Invoke(() => {
                            DowngradeProgressBar.IsIndeterminate = false;
                        });
                        BeginExtractionProcess(InstallState.RadioDowngrade);
                        break;
                }

                prerequisitesInstallProc.Dispose();
                prerequisitesInstallProc = null;
                result.Dispose();
            });
        }
        #endregion

        #region Extraction
        public void BeginExtractionProcess(InstallState state)
        {
            currentInstallState = state;
            RefreshCurrentStepLabel();

            extractionProcessTask = Task.Run(() => {
                string fileLoc = string.Empty;

                // Set file location from InstallState
                switch (state) {
                    case InstallState.RadioDowngrade:

                        switch (instance.s3_SelectRadioDwngrd.selectedRadioDowngrader) {
                            case RadioDowngrader.SneedsDowngrader:
                                fileLoc = ".\\Downgrader\\Files\\Radio\\SneedsRadioDowngrader.zip";
                                break;
                            case RadioDowngrader.LegacyDowngrader:
                                fileLoc = ".\\Downgrader\\Files\\Radio\\LegacyRadioDowngrader.zip";
                                break;
                            default:
                                radioDowngraderErrored = true;
                                AddLogItem("[WARNING] selectedRadioDowngrader is none! Continuing without radio downgrade...");
                                AddLogItem("selectedRadioDowngrader is none!", true, LogType.Warning, true);
                                RadioDowngraderProc_Exited(this, EventArgs.Empty);
                                return;
                        }
                        AddLogItem("Extracting files for radio downgrader...");

                        break;
                    case InstallState.RadioOldVladivostok:

                        fileLoc = ".\\Downgrader\\Files\\Radio\\WithoutNewVladivostok.zip";
                        AddLogItem("Extracting old Vladivostok files...");

                        break;
                    case InstallState.RadioNewVladivostok:

                        fileLoc = ".\\Downgrader\\Files\\Radio\\WithNewVladivostok.zip";
                        AddLogItem("Extracting new Vladivostok files...");

                        break;
                    case InstallState.RadioNoEFLCMusicInIVFix:

                        fileLoc = ".\\Downgrader\\Files\\Radio\\EpisodeOnlyMusicCE.zip";
                        AddLogItem("Extracting files for No EFLC Music In IV Fix...");

                        break;
                    case InstallState.GameDowngrade:

                        switch (instance.s2_SelectDwngrdVersion.selectedVersion) {
                            case GameVersion.v1080:
                                fileLoc = ".\\Downgrader\\Files\\1080\\1080.zip";
                                break;
                            case GameVersion.v1070:
                                fileLoc = ".\\Downgrader\\Files\\1070\\1070.zip";
                                break;
                            case GameVersion.v1040:
                                fileLoc = ".\\Downgrader\\Files\\1040\\1040.zip";
                                break;
                        }
                        AddLogItem("Extracting files for game downgrader...");

                        break;
                    case InstallState.ModInstall:

                        fileLoc = string.Format(".\\Downgrader\\Files\\Mods\\{0}", modToInstall);

                        break;
                }

                if (string.IsNullOrEmpty(fileLoc)) {
                    AddLogItem("[WARNING] fileLoc is empty!");
                    AddLogItem("fileLoc is empty!", true, LogType.Warning, true);
                    return;
                }

                // Wait for a second just in case........
                Thread.Sleep(1000);

                // Begin extracting file
                try {
                    using (ZipFile zip = ZipFile.Read(fileLoc)) {
                        zip.ExtractProgress += Zip_ExtractProgress;
                        
                        totalFiles = zip.Count;
                        progress = 0;

                        foreach (ZipEntry entry in zip) {
                            progress++;
                            zipEntry = entry;
                            entry.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                            entry.ZipErrorAction = ZipErrorAction.Skip;

                            string entryFileLoc = string.Format("{0}\\{1}", instance.s1_SelectIVExe.IVInstallDirectory, entry.FileName);

                            if (string.IsNullOrEmpty(entryFileLoc)) {
                                AddLogItem("[WARNING] entryFileLoc is empty!");
                                AddLogItem("entryFileLoc is empty!", true, LogType.Warning, true);
                                return;
                            }
                            if (string.IsNullOrEmpty(entry.FileName)) {
                                AddLogItem("[WARNING] entry.FileName is empty!");
                                AddLogItem("entry.FileName is empty!", true, LogType.Warning, true);
                                return;
                            }

                            entry.Extract(instance.s1_SelectIVExe.IVInstallDirectory, ExtractExistingFileAction.OverwriteSilently);

                            //if (File.Exists(entryFileLoc)) {
                            //    //AddLogItem(string.Format("File '{0}' exists. Deleting file and extracting new file.", entry.FileName));
                            //    File.Delete(entryFileLoc);
                            //    entry.Extract(instance.s1_SelectIVExe.IVInstallDirectory, ExtractExistingFileAction.OverwriteSilently);
                            //}
                            //else {
                            //    //AddLogItem(string.Format("File '{0}' does NOT exists. Extracting new file.", entry.FileName));
                            //    entry.Extract(instance.s1_SelectIVExe.IVInstallDirectory, ExtractExistingFileAction.OverwriteSilently);
                            //}
                        }
                    }
                }
                catch (Exception ex) { // On error...
                    errored = true;
                    logFileBuilder.AppendLine("An error occured while extracting. Details:");
                    logFileBuilder.Append(ex.ToString());
                    logFileBuilder.AppendLine();
                    logFileBuilder.AppendLine();

                    AddLogItem("An error occured... Details: " + ex.Message, true, LogType.Error);
                    if (zipEntry != null) {
                        instance.ChangeStep(Steps.Error, new List<object>() { ex, string.Format("Crashed at step: {1}{0}Zip Info: {2}", Environment.NewLine, currentInstallState.ToString(), zipEntry.Info) });
                    }
                    else {
                        instance.ChangeStep(Steps.Error, new List<object>() { ex, string.Format("Crashed at step: {0}", currentInstallState.ToString()) });
                    }
                }
            }).ContinueWith(result => {
                if (errored)
                    return;

                switch(currentInstallState) {
                    case InstallState.RadioDowngrade:
                        AddLogItem("Radio downgrade files extracted!", true, LogType.Info);
                        AddLogItem("Starting radio downgrade...");
                        StartRadioDowngrade();
                        break;
                    case InstallState.RadioOldVladivostok:
                        AddLogItem("Old Vladivostok files extracted and installed!", true, LogType.Info);

                        if (instance.s3_SelectRadioDwngrd.InstallNoEFLCMusicInIVFix) {
                            BeginExtractionProcess(InstallState.RadioNoEFLCMusicInIVFix);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
                        }
                        break;
                    case InstallState.RadioNewVladivostok:
                        AddLogItem("New Vladivostok files extracted and installed!", true, LogType.Info);

                        if (instance.s3_SelectRadioDwngrd.InstallNoEFLCMusicInIVFix) {
                            BeginExtractionProcess(InstallState.RadioNoEFLCMusicInIVFix);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
                        }
                        break;
                    case InstallState.RadioNoEFLCMusicInIVFix:
                        AddLogItem("No EFLC Music In IV Fix files extracted and installed!", true, LogType.Info);
                        BeginExtractionProcess(InstallState.GameDowngrade);
                        break;
                    case InstallState.GameDowngrade:
                        AddLogItem("Game downgrade files extracted and installed!", true, LogType.Info);

                        if (instance.s4_SelectComponents.selectedMods.Count != 0) {
                            AddLogItem("Extracting and installing selected mods...");
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
        private void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (errored)
                return;

            Dispatcher.Invoke(() => {
                if (e.CurrentEntry != null) StatusLabel.Text = string.Format("Extracting file {0}, Progress: {1}/{2}", e.CurrentEntry.FileName, progress, totalFiles);
                DowngradeProgressBar.Maximum = totalFiles;
                DowngradeProgressBar.Value = progress;
            });
        }
        #endregion

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            logItems.Clear();
            try {
                instance.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                if (instance.confirmUC.MakeABackupForMeCheckbox.IsChecked.Value) {
                    StartCreatingBackup();
                }
                else {
                    if (instance.s4_SelectComponents.InstallPrerequisitesCheckBox.IsChecked.Value) {
                        StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                    }
                    else {
                        if (instance.s3_SelectRadioDwngrd.selectedRadioDowngrader != RadioDowngrader.None) {
                            BeginExtractionProcess(InstallState.RadioDowngrade);
                        }
                        else {
                            BeginExtractionProcess(InstallState.GameDowngrade);
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
            try {
                if (logFileBuilder.Length != 0) File.WriteAllText(string.Format("Log.{0}.{1}{2}{3}.zip", DateTime.Now.Year.ToString(), DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(), DateTime.Now.Second.ToString()), logFileBuilder.ToString());
            }
            catch (Exception ex) {
                MessageBox.Show(string.Format("Failed to create an error log! That should never be the case, but you can still continue.{0}Details: {1}", Environment.NewLine, ex.Message), "Log creation error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            instance.NextStep();
        }

    }
}
