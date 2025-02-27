﻿using System;
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

using CCL;
using Newtonsoft.Json.Linq;
using IniParser;
using IniParser.Model;

using GTAIVDowngrader.Classes;
using GTAIVDowngrader.Classes.Json;
using GTAIVDowngrader.Classes.Json.Modification;
using GTAIVDowngrader.Classes.Json.PostInstallActions;

namespace GTAIVDowngrader.Dialogs
{
    public partial class DowngradingUC : UserControl
    {

        // This class can be made a lot better.
        // I will probably refactor this class in future updates.

        // - - - Downgrading Order - - -
        // 1. Create Backup if user wants to otherwise skip to 2.
        // 2. Start downloading downgrade files and selected mods
        // 3. Install Prerequisites if user wants to otherwise skip to 4.
        // 4. Install GFWL Prerequisites if user checked the Configure For GFWL CheckBox otherwise skip to 5.
        // 5. If user selected a Radio Downgrader, do Radio Downgrade otherwise skip to 6.
        // 6. Do Game Downgrading
        // 7. Install Mods if there any to install otherwise skip to 9.
        // 8. Install optional mod components if there are any otherwise skip to 9.
        // 9. Finish

        #region Consts
        public const string TEMP_FOLDER = ".\\Data\\Temp";
        #endregion

        #region Variables and Enums
        private MainWindow instance;

        // Download Stuff
        private WebClient downloadWebClient;
        private Queue<FileDetails> downloadQueue;
        private List<string> cancelledDownloadItems;
        private DateTime downloadStartTime; // Remaining Time Calculation

        // States
        private DowngradeStep currentDowngradeStep;
        private Prerequisites currentPrerequisite;

        // Other
        private FileIniDataParser iniParser;

        private ModDetails currentModToInstall;
        private OptionalComponentInfo currentOptionalComponentToInstall;

        private bool errored, backupErrored, prerequisiteErrored;
        private bool canDownloadFiles;
        private bool alreadyFinished;
        private int progress;

        // Enums
        public enum DowngradeStep
        {
            Backup,
            Download, // Can Download: Prerequisites, Old and New Vladivostok, No EFLC Music in IV Fix and selected Mods.
            Prerequisites,
            RadioDowngrade,
            RadioVladivostokDowngrade,
            RadioNoEFLCMusicInIVFix,
            GameDowngrade,
            ModInstall,
            OptionalComponentsInstall
        }
        public enum Prerequisites
        {
            VisualCPlusPlus,
            DirectXExtract,
            DirectX,
            GFWL
        }
        #endregion

        #region Constructor
        public DowngradingUC(MainWindow window)
        {
            instance = window;
            downloadQueue = new Queue<FileDetails>();
            cancelledDownloadItems = new List<string>();
            iniParser = new FileIniDataParser();

            InitializeComponent();
        }
        public DowngradingUC()
        {
            downloadQueue = new Queue<FileDetails>();
            cancelledDownloadItems = new List<string>();
            iniParser = new FileIniDataParser();

            InitializeComponent();
        }
        #endregion

        #region Methods

        private void AddLogItem(LogType type, string str, bool includeTimeStamp = true, bool printInListBox = true)
        {
            Dispatcher.Invoke(() =>
            {
                string logTime = string.Format("{0}", DateTime.Now.ToString("HH:mm:ss"));

                string logText = "";
                if (includeTimeStamp)
                    logText = string.Format("[{0}] [{1}] {2}", logTime, type.ToString(), str);
                else 
                    logText = string.Format("[{0}] {1}", type.ToString(), str);

                // Add log to StatusListBox
                if (printInListBox)
                    StatusListbox.Items.Add(logText);

                // Add log to log file
                if (includeTimeStamp)
                    Core.AddLogItem(type, string.Format("[{0}] {1}", logTime, str));
                else
                    Core.AddLogItem(type, str);

                // Auto scroll StatusListBox to last item
                StatusListbox.SelectedIndex = StatusListbox.Items.Count - 1;
                StatusListbox.ScrollIntoView(StatusListbox.SelectedItem);
            });
        }

        private void ClearReadOnly(DirectoryInfo parentDirectory)
        {
            if (parentDirectory == null)
                return;
            if (!parentDirectory.Exists)
                return;

            // Remove the read-only attribute if it is set
            if ((parentDirectory.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                parentDirectory.Attributes &= ~FileAttributes.ReadOnly;

            foreach (FileInfo fi in parentDirectory.GetFiles())
            {
                // Remove the read-only attribute if it is set
                if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    fi.Attributes &= ~FileAttributes.ReadOnly;
            }
            foreach (DirectoryInfo di in parentDirectory.GetDirectories())
            {
                ClearReadOnly(di);
            }
        }
        private void RenameFile(string oldFileName, string newFileName)
        {
            try
            {
                string oldPath = string.Format("{0}\\{1}", DowngradingInfo.IVWorkingDirectoy, oldFileName);
                string newPath = string.Format("{0}\\{1}", DowngradingInfo.IVWorkingDirectoy, newFileName);

                if (!File.Exists(oldPath))
                {
                    AddLogItem(LogType.Warning, string.Format("Could not find file '{0}' to rename to '{1}'", oldFileName, newFileName));
                    return;
                }

                // Check if file with the target name already exists
                if (File.Exists(newPath))
                    File.Delete(newPath);

                File.Move(oldPath, newPath);
                AddLogItem(LogType.Info, string.Format("Renamed file '{0}' to '{1}'", oldFileName, newFileName));
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("An error occured while trying to rename file '{0}' to '{1}'. Details: {2}", oldFileName, newFileName, ex.Message));
            }
        }

        private void UpdateCurrentStep(DowngradeStep step)
        {
            // Log
            Core.AddLogItem(LogType.Info, string.Format("Changing step from '{0}' to '{1}'.", currentDowngradeStep, step));

            currentDowngradeStep = step;

            Dispatcher.Invoke(() =>
            {
                instance.ChangeActionButtonVisiblity(false, currentDowngradeStep == DowngradeStep.Download, false, true);

                switch (currentDowngradeStep)
                {
                    case DowngradeStep.Download:
                        CurrentStepLabel.Text = "Current Step: Downloading files";
                        break;
                    case DowngradeStep.Backup:
                        CurrentStepLabel.Text = "Current Step: Backing up original game files";
                        break;
                    case DowngradeStep.Prerequisites:
                        CurrentStepLabel.Text = "Current Step: Installing Prerequisites";
                        break;
                    case DowngradeStep.RadioDowngrade:
                        CurrentStepLabel.Text = "Current Step: Radio Downgrade";
                        break;
                    case DowngradeStep.RadioVladivostokDowngrade:
                        CurrentStepLabel.Text = "Current Step: Vladivostok Downgrade";
                        break;
                    case DowngradeStep.RadioNoEFLCMusicInIVFix:
                        CurrentStepLabel.Text = "Current Step: No EFLC Music In IV Fix Install";
                        break;
                    case DowngradeStep.GameDowngrade:
                        CurrentStepLabel.Text = "Current Step: Game Downgrade";
                        break;
                    case DowngradeStep.ModInstall:
                        CurrentStepLabel.Text = "Current Step: Mod Install";
                        break;
                    case DowngradeStep.OptionalComponentsInstall:
                        CurrentStepLabel.Text = "Current Step: Installing optional mod components";
                        break;
                }
            });
        }
        private void UpdateStatusText(string txt)
        {
            Dispatcher.Invoke(() => StatusLabel.Text = txt);
        }
        private void ChangeProgressBarIndeterminateState(bool isIndeterminate)
        {
            Dispatcher.Invoke(() => DowngradeProgressBar.IsIndeterminate = isIndeterminate);
        }
        private void UpdateStatusProgressBar(bool isIndeterminate, int progress, int totalFiles)
        {
            Dispatcher.Invoke(() =>
            {
                if (isIndeterminate)
                {
                    DowngradeProgressBar.IsIndeterminate = true;
                }
                else
                {
                    DowngradeProgressBar.IsIndeterminate = false;
                    DowngradeProgressBar.Maximum = totalFiles;
                    DowngradeProgressBar.Value = progress;
                }
            });
        }
        
        private void Finish()
        {
            if (alreadyFinished)
            {
                Debugger.Log(0, "Downgrading Process", "Was trying to call Finish() twice.\n");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                // Remove read-only attributes
                RemoveReadOnlyAttribute(true);

                // Create LaunchGTAIV.exe
                CreateLaunchGTAIVExecutable();

                // Perfom last steps
                if (DowngradingInfo.DowngradeTo == "1040")
                    DeleteDLCsFor1040();

                if (Core.IsInSimpleMode)
                {
                    instance.taskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    instance.GetMainProgressBar().IsIndeterminate = false;
                    instance.GetMainProgressBar().Value = 0;

                    instance.NextStep(4);

                    return;
                }
                else
                {
                    instance.taskbarItemInfo.ProgressValue = 100;
                    instance.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    instance.GetMainProgressBar().IsIndeterminate = false;
                    instance.GetMainProgressBar().Value = 100;
                    instance.GetMainProgressBar().Foreground = Brushes.DarkGreen;
                    DowngradeProgressBar.Foreground = Brushes.Green;
                }

                AddLogItem(LogType.Info, "Finshed!");
                CurrentStepLabel.Text = "Current Step: Finished";
                StatusLabel.Text = "Finshed! Click on 'Next' to continue.";

                instance.ChangeActionButtonEnabledState(true, true, true, true);
            });

            alreadyFinished = true;
        }

        #region PreChecks
        private void RemoveOldFolders()
        {
            try
            {
                string pluginsFolder = string.Format("{0}\\plugins", DowngradingInfo.IVWorkingDirectoy);
                if (Directory.Exists(pluginsFolder))
                {
                    Directory.Delete(pluginsFolder, true);

                    // Check if deletion was successfull
                    if (!Directory.Exists(pluginsFolder))
                        AddLogItem(LogType.Info, "Deleted plugins folder.");
                    else
                        AddLogItem(LogType.Warning, "plugins folder was not deleted! Please delete manually.");
                }

                string scriptsFolder = string.Format("{0}\\scripts", DowngradingInfo.IVWorkingDirectoy);
                if (Directory.Exists(scriptsFolder))
                {
                    Directory.Delete(scriptsFolder, true);

                    // Check if deletion was successfull
                    if (!Directory.Exists(scriptsFolder))
                        AddLogItem(LogType.Info, "Deleted scripts folder.");
                    else
                        AddLogItem(LogType.Warning, "scripts folder was not deleted! Please delete manually.");
                }
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("An error occured while trying to remove old folders! Details: {0}", ex.Message));
            }
        }
        private void RemoveSettings()
        {
            try
            {
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string settingsFilePath = string.Format("{0}\\Rockstar Games\\GTA IV\\Settings\\SETTINGS.CFG", localAppDataPath);

                if (!File.Exists(settingsFilePath))
                    return;

                File.Delete(settingsFilePath);

                // Check if deletion was successful
                if (!File.Exists(settingsFilePath))
                    AddLogItem(LogType.Info, "Deleted settings.cfg file from LocalAppData.");
                else
                    AddLogItem(LogType.Warning, "settings.cfg file was not deleted!");
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("An error occured while trying to delete settings.cfg file! Details: {0}", ex.Message));
            }
        }
        private void CreateXLivelessAddonFolders()
        {
            if (DowngradingInfo.DowngradeTo != "1040")
                return;

            try
            {
                string savesDir =       DowngradingInfo.IVWorkingDirectoy + "\\saves";
                string settingsDir =    DowngradingInfo.IVWorkingDirectoy + "\\settings";

                // Create saves folder
                Directory.CreateDirectory(savesDir);
                if (Directory.Exists(savesDir))
                    AddLogItem(LogType.Info, "Created saves folder for XLivelessAddon.");

                // Create settings folder
                Directory.CreateDirectory(settingsDir);
                if (Directory.Exists(settingsDir))
                    AddLogItem(LogType.Info, "Created settings folder for XLivelessAddon.");
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("An error occured while trying to create XLivelessAddon folders! Details: {0}", ex.Message));
            }
        }
        private void KillAnyGTAProcessIfRunning()
        {
            try
            {
                Process gtaivProcess =          Process.GetProcessesByName("GTAIV").FirstOrDefault();
                Process gtaEncoderProcess =     Process.GetProcessesByName("gtaEncoder").FirstOrDefault();
                Process gta4BrowserProcess =    Process.GetProcessesByName("gta4Browser").FirstOrDefault();
                
                // Check if no process is running
                if (gtaEncoderProcess == null
                    && gta4BrowserProcess == null
                    && gtaivProcess == null)
                {
                    return;
                }

                AddLogItem(LogType.Info, "Found some GTA IV processes which are still running. Trying to kill them...");

                // Some processes are still running
                if (gtaivProcess != null)
                {
                    gtaivProcess.Kill();

                    if (gtaivProcess.WaitForExit(3000))
                    {
                        AddLogItem(LogType.Info, "Killed GTAIV Process.");

                        gtaivProcess.Dispose();
                        gtaivProcess = null;
                    }
                }
                if (gtaEncoderProcess != null)
                {
                    gtaEncoderProcess.Kill();

                    if (gtaEncoderProcess.WaitForExit(3000))
                    {
                        AddLogItem(LogType.Info, "Killed gtaEncoder Process.");

                        gtaEncoderProcess.Dispose();
                        gtaEncoderProcess = null;
                    }
                }
                if (gta4BrowserProcess != null)
                {
                    gta4BrowserProcess.Kill();

                    if (gta4BrowserProcess.WaitForExit(3000))
                    {
                        AddLogItem(LogType.Info, "Killed gta4Browser Process.");

                        gta4BrowserProcess.Dispose();
                        gta4BrowserProcess = null;
                    }
                }

                AddLogItem(LogType.Info, "Any currently opened GTA IV Process got killed.");
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("Failed to kill any opened GTA IV Processes. Details: {0}", ex.Message));
            }
        }
        #endregion

        #region Backup
        private void StartCreatingBackup()
        {
            string sourcePath = DowngradingInfo.IVWorkingDirectoy;
            string targetPath = DowngradingInfo.IVTargetBackupDirectory;
            bool createZIP = DowngradingInfo.CreateBackupInZipFile;

            UpdateCurrentStep(DowngradeStep.Backup);

            Task.Run(() =>
            {
                AddLogItem(LogType.Info, "Creating a backup...");

                try
                {
                    string[] directorys = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
                    string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    int total = directorys.Length + files.Length;

                    if (createZIP)
                    {
                        string zipFileName = string.Format("{0}\\GTAIV.Backup.{1}.{2}{3}{4}.zip", targetPath, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                        // Update status progress bar
                        UpdateStatusProgressBar(true, 0, 0);
                        UpdateStatusText(string.Format("Backing up game files to file {0}", Path.GetFileName(zipFileName)));

                        // ZIP
                        ZipFile.CreateFromDirectory(sourcePath, zipFileName);
                    }
                    else
                    {
                        // Create all of the directories
                        foreach (string dirPath in directorys)
                        {
                            progress++;

                            // Update status progress bar
                            UpdateStatusProgressBar(false, progress, total);
                            UpdateStatusText(string.Format("Copying directory: {0}, Progress: {1}/{2}", Path.GetDirectoryName(dirPath), progress, total));

                            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                        }

                        // Copy all the files & Replaces any files with the same name
                        foreach (string newPath in files)
                        {
                            progress++;

                            // Update status progress bar
                            UpdateStatusProgressBar(false, progress, total);
                            UpdateStatusText(string.Format("Copying file: {0}, Progress: {1}/{2}", Path.GetFileName(newPath), progress, total));

                            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddLogItem(LogType.Error, string.Format("Failed to create backup! Details: {0}", ex.Message));
                    backupErrored = true;
                }

            }).ContinueWith(r =>
            {
                if (!backupErrored)
                    AddLogItem(LogType.Info, "Backup created!");

                // Reset status progress bar
                UpdateStatusProgressBar(false, 0, 0);

                Dispatcher.Invoke(() =>
                {
                    if (canDownloadFiles)
                    {
                        // Download Files
                        StartDownloads();
                    }
                    else // Continue with something else
                    {
                        if (DowngradingInfo.InstallPrerequisites)
                        {
                            // Install Prerequisites
                            StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                        }
                        else
                        {
                            if (DowngradingInfo.ConfigureForGFWL)
                            {
                                // Install GFWL Prerequisites
                                StartInstallPrerequisites(Prerequisites.GFWL);
                            }
                            else
                            {
                                // Continue with Radio/Game downgrade
                                if (DowngradingInfo.WasAnyRadioDowngraderSelected())
                                    BeginExtractionProcess(DowngradeStep.RadioDowngrade);
                                else
                                    BeginExtractionProcess(DowngradeStep.GameDowngrade);
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
            try
            {

                // Game Downgrading
                string archiveName = string.Format("{0}.zip", DowngradingInfo.DowngradeTo);
                string filePath = GetFileLocationInTempFolder(archiveName);

                if (!File.Exists(filePath))
                {
                    if (!Core.IsInOfflineMode)
                        downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                    else
                        return ShowMissingFileWarning(archiveName);
                }
                else
                {

                    // Check if already existing zip file is corrupted or not
                    if (IsZipFileCorrupted(filePath))
                    {
                        if (Core.IsInOfflineMode)
                            return ShowCorruptedFileWarning(archiveName);

                        downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        AddLogItem(LogType.Info, string.Format("{0} is already downloaded but the file is corrupted. Adding to download queue to redownload.", archiveName));
                    }
                    else
                        AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));

                }

                // Radio Downgrading
                if (DowngradingInfo.WasAnyRadioDowngraderSelected())
                {
                    archiveName = string.Format("{0}.zip", DowngradingInfo.SelectedRadioDowngrader);
                    filePath = GetFileLocationInTempFolder(archiveName);

                    if (!File.Exists(filePath))
                    {
                        if (!Core.IsInOfflineMode)
                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        else
                            return ShowMissingFileWarning(archiveName);
                    }
                    else
                    {
                        // Check if already existing zip file is corrupted or not
                        if (IsZipFileCorrupted(filePath))
                        {
                            if (Core.IsInOfflineMode)
                                return ShowCorruptedFileWarning(archiveName);

                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                            AddLogItem(LogType.Info, string.Format("{0} is already downloaded but the file is corrupted. Adding to download queue to redownload.", archiveName));
                        }
                        else
                        {
                            AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));
                        }
                    }

                    // Vladivostok Type
                    if (DowngradingInfo.IsSelectedRadioDowngraderSneeds())
                    {
                        archiveName = string.Format("{0}.zip", DowngradingInfo.SelectedVladivostokType);
                        filePath = GetFileLocationInTempFolder(archiveName);

                        if (!File.Exists(filePath))
                        {
                            if (!Core.IsInOfflineMode)
                                downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                            else
                                return ShowMissingFileWarning(archiveName);
                        }
                        else
                        {
                            // Check if already existing zip file is corrupted or not
                            if (IsZipFileCorrupted(filePath))
                            {
                                if (Core.IsInOfflineMode)
                                    return ShowCorruptedFileWarning(archiveName);

                                downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                                AddLogItem(LogType.Info, string.Format("{0} is already downloaded but the file is corrupted. Adding to download queue to redownload.", archiveName));
                            }
                            else
                            {
                                AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));
                            }
                        }
                    }
                }

                // Radio Downgrading - No EFLC Music in IV Fix
                if (DowngradingInfo.InstallNoEFLCMusicInIVFix)
                {
                    archiveName = "NoEpisodeMusicInIV.zip";
                    filePath = GetFileLocationInTempFolder(archiveName);

                    if (!File.Exists(filePath))
                    {
                        if (!Core.IsInOfflineMode)
                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        else
                            return ShowMissingFileWarning(archiveName);
                    }
                    else
                    {

                        // Check if already existing zip file is corrupted or not
                        if (IsZipFileCorrupted(filePath))
                        {
                            if (Core.IsInOfflineMode)
                                return ShowCorruptedFileWarning(archiveName);

                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                            AddLogItem(LogType.Info, string.Format("{0} is already downloaded but the file is corrupted. Adding to download queue to redownload.", archiveName));
                        }
                        else
                            AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));

                    }
                }

                // Add mods and optional mod components if offline mode is not activated
                if (!Core.IsInOfflineMode)
                {
                    ModDetails[] selectedMods = DowngradingInfo.SelectedMods.ToArray();
                    for (int i = 0; i < selectedMods.Length; i++)
                        downloadQueue.Enqueue(selectedMods[i].FileDetails);

                    OptionalComponentInfo[] selectedOptionalComponents = DowngradingInfo.SelectedOptionalComponents.ToArray();
                    for (int i = 0; i < selectedOptionalComponents.Length; i++)
                        downloadQueue.Enqueue(selectedOptionalComponents[i].FileDetails);
                }

                // Prerequisites
                if (DowngradingInfo.InstallPrerequisites)
                {

                    // DirectX
                    archiveName = "directx_Jun2010_redist.exe";
                    filePath = GetFileLocationInTempFolder(archiveName);

                    if (!File.Exists(filePath))
                    {
                        if (!Core.IsInOfflineMode)
                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        else
                            return ShowMissingFileWarning(archiveName);
                    }
                    else
                    {
                        AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));
                    }

                    // Visual C++
                    archiveName = "VisualCppRedist_AIO_x86_x64.exe";
                    filePath = GetFileLocationInTempFolder(archiveName);

                    if (!File.Exists(filePath))
                    {
                        if (!Core.IsInOfflineMode)
                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        else
                            return ShowMissingFileWarning(archiveName);
                    }
                    else
                    {
                        AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));
                    }

                }
                if (DowngradingInfo.ConfigureForGFWL)
                {

                    // gfwlivesetup.exe
                    archiveName = "gfwlivesetup.exe";
                    filePath = GetFileLocationInTempFolder(archiveName);

                    if (!File.Exists(filePath))
                    {
                        if (!Core.IsInOfflineMode)
                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        else
                            return ShowMissingFileWarning(archiveName);
                    }
                    else
                    {
                        AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));
                    }

                    // xliveredist.msi
                    archiveName = "xliveredist.msi";
                    filePath = GetFileLocationInTempFolder(archiveName);

                    if (!File.Exists(filePath))
                    {
                        if (!Core.IsInOfflineMode)
                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        else
                            return ShowMissingFileWarning(archiveName);
                    }
                    else
                    {
                        AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));
                    }

                    // wllogin_32.msi / wllogin_64.msi
                    archiveName = Environment.Is64BitOperatingSystem == false ? "wllogin_32.msi" : "wllogin_64.msi";
                    filePath = GetFileLocationInTempFolder(archiveName);

                    if (!File.Exists(filePath))
                    {
                        if (!Core.IsInOfflineMode)
                            downloadQueue.Enqueue(Core.GetDowngradeFileByFileName(archiveName).FileDetails);
                        else
                            return ShowMissingFileWarning(archiveName);
                    }
                    else
                    {
                        AddLogItem(LogType.Info, string.Format("{0} is already downloaded. Skipping.", archiveName));
                    }

                }

                // Let user know what is going to be downloaded
                FileDetails[] items = downloadQueue.ToArray();
                for (int i = 0; i < items.Length; i++)
                {
                    FileDetails item = items[i];
                    AddLogItem(LogType.Info, string.Format("Added '{0}' to download queue. URL: {1}", item.Name, item.DownloadURL), true, true);
                }

                AddLogItem(LogType.Info, string.Format("{0} items were added to download queue.", downloadQueue.Count));
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Warning, string.Format("Failed to add an item to the download queue! Details: {0}", ex.Message));
            }

            return downloadQueue.Count > 0;
        }
        private void StartDownloads()
        {
            try
            {
                UpdateCurrentStep(DowngradeStep.Download);

                AddLogItem(LogType.Info, "Starting downloads...");

                // Get next download item in queue and log
                FileDetails downloadItem = downloadQueue.Dequeue();

                AddLogItem(LogType.Info, string.Format("Downloading {0}...", downloadItem.Name));

                // Create temp folders
                CreateTempFolder();

                // Start downloading
                downloadStartTime = DateTime.Now;
                downloadWebClient.DownloadFileAsync(new Uri(downloadItem.DownloadURL), GetFileLocationInTempFolder(downloadItem.Name), downloadItem);
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("Error while trying to start downloading stuff from the queue! Continuing. Details: {0}", ex.Message));

                if (DowngradingInfo.InstallPrerequisites) // Install Prerequisites
                {
                    StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                }
                else
                {
                    if (DowngradingInfo.ConfigureForGFWL) // Install GFWL Prerequisites
                    {
                        StartInstallPrerequisites(Prerequisites.GFWL);
                    }
                    else // Continue with Radio/Game downgrade
                    {
                        if (DowngradingInfo.WasAnyRadioDowngraderSelected())
                            BeginExtractionProcess(DowngradeStep.RadioDowngrade);
                        else
                            BeginExtractionProcess(DowngradeStep.GameDowngrade);
                    }
                }
            }
        }
        #endregion

        #region Prerequisites
        private void StartInstallPrerequisites(Prerequisites current)
        {
            prerequisiteErrored = false;

            currentPrerequisite = current;
            UpdateCurrentStep(DowngradeStep.Prerequisites);

            UpdateStatusProgressBar(true, 0, 0);
            UpdateStatusText("Installing Prerequisites. This process could take a few minutes...");

            Task.Run(() =>
            {

                // Get and set file name for selected prerequisite
                string fileName = string.Empty;

                switch (currentPrerequisite)
                {
                    case Prerequisites.VisualCPlusPlus:
                        fileName = GetFileLocationInTempFolder("VisualCppRedist_AIO_x86_x64.exe");
                        break;
                    case Prerequisites.DirectXExtract:
                        fileName = GetFileLocationInTempFolder("directx_Jun2010_redist.exe");
                        break;
                    case Prerequisites.DirectX:
                        fileName = string.Format("{0}\\DXSETUP.exe", CreateTempFolder("DirectX"));
                        break;
                    case Prerequisites.GFWL:
                        fileName = GetFileLocationInTempFolder("gfwlivesetup.exe");
                        break;
                }

                // Check if file exists
                if (!File.Exists(fileName))
                {
                    AddLogItem(LogType.Warning, string.Format("Could not find installation file for prerequisite {0}! Skipping.", currentPrerequisite));
                    return;
                }

                // Create and start installer process
                using (Process p = new Process())
                {
                    p.EnableRaisingEvents = true;
                    p.StartInfo.FileName = fileName;

                    switch (currentPrerequisite)
                    {
                        case Prerequisites.VisualCPlusPlus:
                            {
                                AddLogItem(LogType.Info, "Installing Visual C++ Redistributables...");
                                p.StartInfo.Arguments = "/ai /gm2";
                            }
                            break;
                        case Prerequisites.DirectXExtract:
                            {
                                AddLogItem(LogType.Info, "Extracting DirectX setup files...");
                                p.StartInfo.Arguments = string.Format("/Q /C /T:\"{0}\"", Path.GetFullPath(CreateTempFolder("DirectX")));
                            }
                            break;
                        case Prerequisites.DirectX:
                            {
                                AddLogItem(LogType.Info, "Installing DirectX...");
                                p.StartInfo.Arguments = "/silent";
                            }
                            break;
                        case Prerequisites.GFWL:
                            {
                                AddLogItem(LogType.Info, "Installing Games for Windows Live...");
                                // This does not have a silent install option
                            }
                            break;
                    }

                    try
                    {
                        p.Start();
                        p.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        AddLogItem(LogType.Error, string.Format("An error occured while installing prerequisite '{0}'. Details: {1}", currentPrerequisite, ex.Message));
                        prerequisiteErrored = true;
                    }
                }
            }).ContinueWith(result =>
            {
                switch (currentPrerequisite)
                {
                    case Prerequisites.VisualCPlusPlus:
                        if (!prerequisiteErrored)
                            AddLogItem(LogType.Info, "Visual C++ Redistributables installed!");

                        StartInstallPrerequisites(Prerequisites.DirectXExtract);
                        break;

                    case Prerequisites.DirectXExtract:
                        if (!prerequisiteErrored)
                            AddLogItem(LogType.Info, "DirectX setup files extracted");

                        StartInstallPrerequisites(Prerequisites.DirectX);
                        break;
                    case Prerequisites.DirectX:
                        if (!prerequisiteErrored)
                            AddLogItem(LogType.Info, "DirectX installed!");

                        UpdateStatusProgressBar(false, 0, 0);

                        // Install GFWL if selected or continue with Radio Downgrade
                        if (DowngradingInfo.ConfigureForGFWL)
                            StartInstallPrerequisites(Prerequisites.GFWL);
                        else
                            BeginExtractionProcess(DowngradeStep.RadioDowngrade);

                        break;

                    case Prerequisites.GFWL:
                        if (!prerequisiteErrored)
                            AddLogItem(LogType.Info, "Games for Windows Live installed!");

                        UpdateStatusProgressBar(false, 0, 0);

                        BeginExtractionProcess(DowngradeStep.RadioDowngrade);
                        break;
                }
            });
        }
        #endregion

        #region Radio
        private void StartRadioDowngrade()
        {
            string radioDowngradeInstallFile = string.Format("{0}\\install.bat", DowngradingInfo.IVWorkingDirectoy);
            if (File.Exists(radioDowngradeInstallFile))
            {
                try
                {

                    ChangeProgressBarIndeterminateState(true);

                    // Parse install.bat file
                    string[] lines = File.ReadAllLines(radioDowngradeInstallFile);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        if (line.StartsWith("@echo"))
                            continue;

                        string[] commands = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        switch (commands[0].ToLower())
                        {
                            // jptch.exe
                            case "jptch":

                                string arg1 = commands[1];
                                string arg2 = commands[2];
                                string arg3 = commands[3];

                                string argFullPath1 = string.Format("{0}\\{1}", DowngradingInfo.IVWorkingDirectoy, commands[1]);
                                string argFullPath2 = string.Format("{0}\\{1}", DowngradingInfo.IVWorkingDirectoy, commands[2]);

                                // Check if files exist
                                if (!File.Exists(argFullPath1))
                                {
                                    AddLogItem(LogType.Warning, string.Format("Could not find file {0} for patching! Skipping file. Radio Downgrader might not work as expected.", Path.GetFileName(argFullPath1)));
                                    continue;
                                }
                                if (!File.Exists(argFullPath2))
                                {
                                    AddLogItem(LogType.Warning, string.Format("Could not find file {0} for patching! Skipping file. Radio Downgrader might not work as expected.", Path.GetFileName(argFullPath2)));
                                    continue;
                                }

                                // Log
                                AddLogItem(LogType.Info, string.Format("Patching file: {0}", Path.GetFileName(commands[1])));

                                // Start patching
                                if (!JPatchFile(arg1, arg2, arg3))
                                    return;

                                break;

                            // Rename file
                            case "rename":

                                string directoryName = Path.GetDirectoryName(commands[1].Replace('"', ' ').Replace(" ", ""));

                                string oldFileName = Path.GetFileName(commands[1].Replace('"', ' ').Replace(" ", ""));
                                string newFileName = commands[2].Replace('"', ' ').Replace(" ", "");

                                string oldPath = string.Format("{0}\\{1}\\{2}", DowngradingInfo.IVWorkingDirectoy, directoryName, oldFileName);
                                string newPath = string.Format("{0}\\{1}\\{2}", DowngradingInfo.IVWorkingDirectoy, directoryName, newFileName);

                                // Check if file exist
                                if (!File.Exists(oldPath))
                                {
                                    AddLogItem(LogType.Warning, string.Format("Could not find file {0} for renaming! Skipping file. Radio Downgrader might not work as expected.", oldFileName));
                                    continue;
                                }

                                // Log
                                AddLogItem(LogType.Info, string.Format("Renaming file {0} to {1}", oldFileName, newFileName));

                                // Rename file
                                File.Move(oldPath, newPath);

                                break;

                            // Delete file
                            case "del":

                                string fileName = commands[1].Replace('"', ' ').Replace(" ", "");
                                string justFileName = Path.GetFileName(fileName);
                                string fileToDelete = string.Format("{0}\\{1}", DowngradingInfo.IVWorkingDirectoy, fileName);

                                // Check if file exist
                                if (!File.Exists(fileToDelete))
                                {
                                    AddLogItem(LogType.Warning, string.Format("Could not find file {0} for deletion! Skipping file.", justFileName));
                                    continue;
                                }

                                // Log
                                AddLogItem(LogType.Info, string.Format("Deleting file: {0}", justFileName));

                                // Delete file
                                File.Delete(fileToDelete);

                                break;
                        }
                    }

                    FinishRadioDowngrader(true);

                }
                catch (Exception ex)
                {
                    AddLogItem(LogType.Error, string.Format("An error occured while trying to downgrade the radio! Details: {0}", ex.Message));

                    FinishRadioDowngrader(false);
                }
            }
            else
            {
                AddLogItem(LogType.Error, "Could not downgrade the radio! File install.bat does not exists. Continuing without radio downgrade.");

                FinishRadioDowngrader(false);
            }
        }
        private bool JPatchFile(string arg1, string arg2, string arg3)
        {
            try
            {
                // Get jptch.exe file location
                string jptchExecutable = string.Format("{0}\\jptch.exe", DowngradingInfo.IVWorkingDirectoy);

                // Start jptch
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = jptchExecutable;
                info.WorkingDirectory = DowngradingInfo.IVWorkingDirectoy;
                info.Arguments = string.Format("{0} {1} {2}", arg1, arg2, arg3);
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                Process.Start(info).WaitForExit();

                return true;
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("An error occured while trying jpatch radio file! Continuing without radio downgrader. Details: {0}", ex.Message));
                FinishRadioDowngrader(false);
            }

            return false;
        }
        private void FinishRadioDowngrader(bool succeeded)
        {
            if (succeeded)
                AddLogItem(LogType.Info, "Radio downgrade finished!");

            UpdateStatusProgressBar(false, 0, 0);

            Dispatcher.Invoke(() =>
            {
                if (DowngradingInfo.IsSelectedRadioDowngraderSneeds())
                {
                    BeginExtractionProcess(DowngradeStep.RadioVladivostokDowngrade);
                }
                else
                {
                    if (DowngradingInfo.InstallNoEFLCMusicInIVFix)
                        BeginExtractionProcess(DowngradeStep.RadioNoEFLCMusicInIVFix);
                    else
                        BeginExtractionProcess(DowngradeStep.GameDowngrade);
                }
            });
        }
        #endregion

        #region Extraction
        private void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            progress = 0;

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);

                progress++;
                UpdateStatusText(string.Format("Extracting file {0}, Progress: {1}/{2}", file.Name, progress, archive.Entries.Count));
                UpdateStatusProgressBar(false, progress, archive.Entries.Count);
            }
        }
        private void BeginExtractionProcess(DowngradeStep state)
        {
            UpdateCurrentStep(state);

            Task.Run(() =>
            {
                string fileLoc = string.Empty;

                // Get the location of the archive based on the current step
                switch (state)
                {

                    // Game Downgrade
                    case DowngradeStep.GameDowngrade:
                        {
                            fileLoc = GetFileLocationInTempFolder(string.Format("{0}.zip", DowngradingInfo.DowngradeTo));
                            AddLogItem(LogType.Info, "Extracting files for game downgrader...");
                        }
                        break;

                    // Radio Downgrade
                    case DowngradeStep.RadioDowngrade:
                        {
                            if (DowngradingInfo.WasAnyRadioDowngraderSelected())
                            {
                                fileLoc = GetFileLocationInTempFolder(string.Format("{0}.zip", DowngradingInfo.SelectedRadioDowngrader));
                                AddLogItem(LogType.Info, "Extracting files for radio downgrader...");
                            }
                            else
                            {
                                AddLogItem(LogType.Info, "Skipping radio downgrading step as no radio downgrader was selected.");
                                FinishRadioDowngrader(false);
                                return;
                            }
                        }
                        break;
                    case DowngradeStep.RadioVladivostokDowngrade:
                        {
                            fileLoc = GetFileLocationInTempFolder(string.Format("{0}.zip", DowngradingInfo.SelectedVladivostokType));
                            AddLogItem(LogType.Info, "Extracting Vladivostok Downgrade files...");
                        }
                        break;
                    case DowngradeStep.RadioNoEFLCMusicInIVFix:
                        {
                            fileLoc = GetFileLocationInTempFolder("NoEpisodeMusicInIV.zip");
                            AddLogItem(LogType.Info, "Extracting files for No EFLC Music In IV Fix...");
                        }
                        break;

                    // Mod Install
                    case DowngradeStep.ModInstall:
                        {
                            fileLoc = GetFileLocationInTempFolder(currentModToInstall.FileDetails.Name);
                            AddLogItem(LogType.Info, string.Format("{0} is currently being extracted and installed...", currentModToInstall.FileDetails.Name));
                        }
                        break;
                    case DowngradeStep.OptionalComponentsInstall:
                        fileLoc = GetFileLocationInTempFolder(currentOptionalComponentToInstall.FileDetails.Name);
                        break;
                }

                // Some checks
                if (string.IsNullOrEmpty(fileLoc))
                {
                    AddLogItem(LogType.Warning, "!!! fileLoc is empty - This should never be the case. Please send the log file to the developer. !!!");
                    AddLogItem(LogType.Warning, string.Format("Current State: {0}", state), true, false);
                    return;
                }
                if (!File.Exists(fileLoc))
                {
                    AddLogItem(LogType.Warning, string.Format("File '{0}' was not found and could not be extracted. Skipping.", Path.GetFileName(fileLoc)));
                    return;
                }
                if (IsZipFileCorrupted(fileLoc))
                {
                    AddLogItem(LogType.Warning, string.Format("Skipping file '{0}' because it appears to be corrupted!", Path.GetFileName(fileLoc)));
                    return;
                }

                // Begin extracting archive
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(fileLoc))
                    {
                        ExtractToDirectory(archive, DowngradingInfo.IVWorkingDirectoy, true);
                    }
                }
                catch (Exception ex)
                {
                    errored = true;

                    AddLogItem(LogType.Error, string.Format("An error occured while extracting '{0}'! Details: {1}", Path.GetFileName(fileLoc), ex.Message));
                    instance.ChangeStep(Steps.Error, new List<object>() { ex, string.Format("Crashed at step: {0}", currentDowngradeStep.ToString()) });
                }

            }).ContinueWith(r =>
            {
                if (errored)
                    return;

                switch (currentDowngradeStep)
                {
                    case DowngradeStep.RadioDowngrade:

                        AddLogItem(LogType.Info, "Radio downgrade files extracted!");
                        AddLogItem(LogType.Info, "Starting radio downgrade.");

                        UpdateStatusText("Radio is currently getting downgraded...");

                        StartRadioDowngrade();

                        break;
                    case DowngradeStep.RadioVladivostokDowngrade:

                        AddLogItem(LogType.Info, "Vladivostok Downgrade files extracted and installed!");

                        if (DowngradingInfo.InstallNoEFLCMusicInIVFix)
                            BeginExtractionProcess(DowngradeStep.RadioNoEFLCMusicInIVFix);
                        else
                            BeginExtractionProcess(DowngradeStep.GameDowngrade);

                        break;
                    case DowngradeStep.RadioNoEFLCMusicInIVFix:

                        AddLogItem(LogType.Info, "No EFLC Music In IV Fix files extracted and installed!");
                        BeginExtractionProcess(DowngradeStep.GameDowngrade);

                        break;
                    case DowngradeStep.GameDowngrade:

                        AddLogItem(LogType.Info, "Game downgrade files extracted and installed!");

                        if (DowngradingInfo.SelectedMods.Count != 0)
                        {
                            AddLogItem(LogType.Info, "Extracting and installing selected mods and optional components...");
                            StartInstallingMods();
                        }
                        else
                        {
                            Finish();
                        }

                        break;
                    case DowngradeStep.ModInstall:

                        if (!cancelledDownloadItems.Contains(currentModToInstall.FileDetails.Name))
                        {
                            // Rename dinput8.dll to xlive.dll if current mod is an ASI Loader and if the user does not want to configure for GFWL
                            if (currentModToInstall.IsASILoader)
                            {
                                if (!DowngradingInfo.ConfigureForGFWL)
                                    RenameFile("dinput8.dll", "xlive.dll");
                            }

                            // Execute post install actions of current mod
                            ExecutePostInstallActionsOfCurrentMod();
                        }

                        // Reset current mod to be installed
                        currentModToInstall = null;

                        if (DowngradingInfo.SelectedMods.Count != 0)
                        {
                            StartInstallingMods();
                        }
                        else
                        {
                            if (DowngradingInfo.SelectedOptionalComponents.Count != 0)
                                StartInstallingOptionalModComponents();
                            else
                                Finish();
                        }

                        break;
                    case DowngradeStep.OptionalComponentsInstall:

                        // Reset current optional to be installed
                        currentOptionalComponentToInstall = null;

                        StartInstallingOptionalModComponents();

                        break;
                }
            });
        }
        #endregion
        
        #region Mods
        private void ExecutePostInstallActionsOfCurrentMod()
        {
            if (currentModToInstall == null)
                return;
            if (currentModToInstall.PostInstallActions.Count == 0)
                return;

            AddLogItem(LogType.Info, string.Format("Executing {0} post install action(s) of mod '{1}'...", currentModToInstall.PostInstallActions.Count, currentModToInstall.FileDetails.Name));

            try
            {
                for (int i = 0; i < currentModToInstall.PostInstallActions.Count; i++)
                {
                    PostInstallAction postInstallAction = currentModToInstall.PostInstallActions[i];

                    if (postInstallAction.Action == null)
                        continue;

                    switch (postInstallAction.Type)
                    {
                        case PostInstallActionType.EditIniFile:
                            {
                                EditIniFileAction editIniFileAction = (postInstallAction.Action as JObject).ToObject<EditIniFileAction>();

                                if (editIniFileAction.EditEntries.Count == 0)
                                    break;

                                AddLogItem(LogType.Info, string.Format("Executing 'Edit Ini File' post install action of mod '{0}' for file '{1}'", currentModToInstall.FileDetails.Name, editIniFileAction.TargetFileName));

                                // Build file path
                                string path = string.Format("{0}\\{1}", DowngradingInfo.IVWorkingDirectoy, editIniFileAction.TargetFileName);

                                // Check if file exists
                                if (!File.Exists(path))
                                {
                                    AddLogItem(LogType.Warning, string.Format("Could not open file to edit '{0}' for mod '{1}'! The file does not exists. Skipping.", editIniFileAction.TargetFileName, currentModToInstall.FileDetails.Name));
                                    continue;
                                }

                                // Check if the path is within the expected directory
                                if (!new DirectoryInfo(path).FullName.Contains(DowngradingInfo.IVWorkingDirectoy))
                                {
                                    AddLogItem(LogType.Warning, string.Format("Mod '{0}' tried to edit a file outside of the GTA IV directory! Skipping. File: {1}", currentModToInstall.FileDetails.Name, editIniFileAction.TargetFileName));
                                    break;
                                }

                                // Try open file
                                IniData iniData = iniParser.ReadFile(path);

                                if (iniData == null)
                                {
                                    AddLogItem(LogType.Warning, string.Format("Could not open file to edit '{0}' for mod '{1}'! Skipping.", editIniFileAction.TargetFileName, currentModToInstall.FileDetails.Name));
                                    break;
                                }

                                iniData.Configuration.AssigmentSpacer = string.Empty;

                                // Edit each entry
                                for (int e = 0; e < editIniFileAction.EditEntries.Count; e++)
                                {
                                    IniFileSection editSection = editIniFileAction.EditEntries[e];

                                    // If section is empty, edit global section instead
                                    if (string.IsNullOrWhiteSpace(editSection.Section))
                                    {
                                        if (iniData.Global.ContainsKey(editSection.Key))
                                            iniData.Global[editSection.Key] = editSection.NewValue;
                                    }
                                    else
                                    {
                                        if (iniData.Sections.ContainsSection(editSection.Section))
                                            iniData[editSection.Section][editSection.Key] = editSection.NewValue;
                                    }
                                }

                                // Save file
                                //iniParser.WriteFile(path, iniData); // <- This does not remove empty lines from the ini file - Maybe there's a property for that?

                                // This removes empty lines from the ini file because apparently they could break stuff (Atleast in ZolikaPatch)
                                // A better way of doing this would be to override the ini file parser and add that functionality there. But this works for now.
                                File.WriteAllLines(path, iniData.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("An error occured while trying to execute post install actions of mod '{0}'! Details: {1}", currentModToInstall.FileDetails.Name, ex.Message));
            }
        }
        private void StartInstallingMods()
        {
            if (DowngradingInfo.SelectedMods.Count != 0)
            {
                currentModToInstall = DowngradingInfo.SelectedMods.Dequeue();
                BeginExtractionProcess(DowngradeStep.ModInstall);
            }
            else
            {
                Finish();
            }
        }
        private void StartInstallingOptionalModComponents()
        {
            if (DowngradingInfo.SelectedOptionalComponents.Count != 0)
            {
                // TODO: Prevent installing optional components when the parent mod did not install due to corrupted archive or something
                currentOptionalComponentToInstall = DowngradingInfo.SelectedOptionalComponents.Dequeue();
                BeginExtractionProcess(DowngradeStep.OptionalComponentsInstall);
            }
            else
            {
                Finish();
            }
        }
        #endregion

        #region FinalThings
        private void DeleteDLCsFor1040()
        {
            try
            {
                string tbogtDir = DowngradingInfo.IVWorkingDirectoy + "\\TBoGT";
                string tladDir = DowngradingInfo.IVWorkingDirectoy + "\\TLAD";
                if (Directory.Exists(tbogtDir))
                {
                    Directory.Delete(tbogtDir, true);
                    if (!Directory.Exists(tbogtDir))
                        AddLogItem(LogType.Info, "Deleted unnecessary TBoGT DLC Folder because GTA IV 1040 can't load DLCs.");
                }
                if (Directory.Exists(tladDir))
                {
                    Directory.Delete(tladDir, true);
                    if (!Directory.Exists(tladDir))
                        AddLogItem(LogType.Info, "Deleted unnecessary TLAD DLC Folder because GTA IV 1040 can't load DLCs.");
                }
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("An eerror occured while trying to delete unnecessary DLC folders! Details: {0}", ex.Message));
            }
        }
        private void RemoveReadOnlyAttribute(bool again)
        {
            if (!Core.IsAppRunningWithAdminPrivileges)
                return;

            ChangeProgressBarIndeterminateState(true);

            if (again)
                AddLogItem(LogType.Info, "Trying to remove any read-only attributes inside the GTA IV directory again...");
            else
                AddLogItem(LogType.Info, "Trying to remove any read-only attributes inside the GTA IV directory...");

            ClearReadOnly(new DirectoryInfo(DowngradingInfo.IVWorkingDirectoy));

            AddLogItem(LogType.Info, "Removed attributes.");
            ChangeProgressBarIndeterminateState(false);
        }
        private void CreateLaunchGTAIVExecutable()
        {
            try
            {
                string playGTAIV = string.Format("{0}\\PlayGTAIV.exe", DowngradingInfo.IVWorkingDirectoy);
                string launchGTAIV = string.Format("{0}\\LaunchGTAIV.exe", DowngradingInfo.IVWorkingDirectoy);

                if (File.Exists(launchGTAIV))
                {
                    AddLogItem(LogType.Warning, "A LaunchGTAIV.exe file already exists in the directory.");
                    return;
                }

                File.Copy(playGTAIV, launchGTAIV);
                AddLogItem(LogType.Info, "Created copy of PlayGTAIV.exe named \"LaunchGTAIV.exe\".");
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("Error while trying to create a copy of PlayGTAIV.exe! Details: {0}", ex.Message));
            }
        }
        #endregion

        #endregion

        #region Functions
        private string CreateTempFolder(string subFolder = null)
        {
            try
            {
                string tmpFolder = string.Concat(TEMP_FOLDER, "\\", subFolder);

                if (!Directory.Exists(tmpFolder))
                    Directory.CreateDirectory(tmpFolder);

                return tmpFolder;
            }
            catch (Exception ex)
            {
                Core.AddLogItem(LogType.Error, string.Format("An error occured while trying to create temp folder! Details: {0}", ex.Message));
                instance.ShowErrorScreen(ex);
            }

            return null;
        }
        private string GetFileLocationInTempFolder(string fileName)
        {
            return string.Format(".\\Data\\Temp\\{0}", fileName);
        }
        private bool IsZipFileCorrupted(string filePath)
        {
            try
            {
                using (ZipArchive arch = ZipFile.OpenRead(filePath))
                    return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool ShowMissingFileWarning(string file)
        {
            instance.ShowStandaloneWarningScreen("Downgrading file missing", string.Format("Could not continue downgrading because the file {1} is missing.{0}" +
                "Place all the required files for your downgrade in the 'Data -> Temp' folder and try again.", Environment.NewLine, file));
            return false;
        }
        private bool ShowCorruptedFileWarning(string file)
        {
            instance.ShowStandaloneWarningScreen("Downgrading file appears to be corrupted", string.Format("Could not continue downgrading because the file {1} appears to be corrupted.{0}" +
                "Either try do redowngrade, or download the file manually and place it in the 'Data -> Temp' folder and try again.", Environment.NewLine, file));
            return false;
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep();
        }
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            if (downloadWebClient == null)
                return;
            if (!downloadWebClient.IsBusy)
                return;

            switch (MessageBox.Show("Canceling the current download may lead to unforeseen issues. Only proceed if you believe the download is stuck. Are you sure you want to cancel?", "Cancel current download", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            {
                case MessageBoxResult.Yes:

                    if (currentDowngradeStep == DowngradeStep.Download)
                    {
                        try
                        {
                            downloadWebClient.CancelAsync();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    break;
            }
        }

        private void DownloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            FileDetails downloadItem = (FileDetails)e.UserState;

            try
            {
                if (e.Cancelled)
                {
                    AddLogItem(LogType.Warning, string.Format("Download was cancelled for {0}!", downloadItem.Name));
                    cancelledDownloadItems.Add(downloadItem.Name);
                }
                if (e.Error != null)
                    AddLogItem(LogType.Error, string.Format("An error occured while downloading item {0}. Details: {1}", downloadItem.Name, e.Error.Message));

                // Download
                Task.Run(() =>
                {
                    // Download remaining files or continue with downgrading
                    if (downloadQueue.Count != 0)
                    {

                        // Get next download item in queue
                        FileDetails nextDownloadItem = downloadQueue.Dequeue();

                        AddLogItem(LogType.Info, string.Format("Downloading {0}...", nextDownloadItem.Name));

                        // Start downloading
                        downloadStartTime = DateTime.Now;
                        Dispatcher.Invoke(() => downloadWebClient.DownloadFileAsync(new Uri(nextDownloadItem.DownloadURL), GetFileLocationInTempFolder(nextDownloadItem.Name), nextDownloadItem));

                    }
                    else // Continue with downgrading
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AddLogItem(LogType.Info, "Download of files completed!");

                            if (DowngradingInfo.InstallPrerequisites) // Install Prerequisites
                            {
                                StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                            }
                            else
                            {
                                if (DowngradingInfo.ConfigureForGFWL) // Install GFWL Prerequisites
                                {
                                    StartInstallPrerequisites(Prerequisites.GFWL);
                                }
                                else // Continue with Radio/Game downgrade
                                {
                                    if (DowngradingInfo.WasAnyRadioDowngraderSelected())
                                        BeginExtractionProcess(DowngradeStep.RadioDowngrade);
                                    else
                                        BeginExtractionProcess(DowngradeStep.GameDowngrade);
                                }
                            }
                        });
                    }

                });
            }
            catch (Exception ex)
            {
                AddLogItem(LogType.Error, string.Format("[Try Catch] An error occured while downloading item {0}. Details: {1}", downloadItem.Name, ex.Message));
                instance.ShowErrorScreen(ex);
            }
        }
        private void DownloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            FileDetails currentDownloadItem = (FileDetails)e.UserState;

            TimeSpan timeSpent = DateTime.Now - downloadStartTime;
            int secondsRemaining = (int)(timeSpent.TotalSeconds / e.BytesReceived * (currentDownloadItem.SizeInBytes - e.BytesReceived));
            TimeSpan estTime = TimeSpan.FromSeconds(secondsRemaining);

            double percentageDownloaded = (double)e.BytesReceived / currentDownloadItem.SizeInBytes * 100;
            DowngradeProgressBar.Value = percentageDownloaded;

            StatusLabel.Text = string.Format("Downloaded {0} of {1} ({2}%). Estimated time remaining: {3}",
                FileHelper.GetExactFileSizeAdvanced(e.BytesReceived),
                FileHelper.GetExactFileSizeAdvanced(currentDownloadItem.SizeInBytes),
                Math.Round(percentageDownloaded, 0),
                estTime.ToString(@"hh\:mm\:ss"));
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
            instance.BackButton.Content = "Back";

            // Lists
            if (downloadQueue != null)
            {
                downloadQueue.Clear();
                downloadQueue = null;
            }
            if (cancelledDownloadItems != null)
            {
                cancelledDownloadItems.Clear();
                cancelledDownloadItems = null;
            }
            
            // Destroy WebClient
            if (downloadWebClient != null)
            {
                downloadWebClient.DownloadProgressChanged -= DownloadClient_DownloadProgressChanged;
                downloadWebClient.DownloadFileCompleted -= DownloadClient_DownloadFileCompleted;

                if (downloadWebClient.IsBusy)
                    downloadWebClient.CancelAsync();

                downloadWebClient.Dispose();
                downloadWebClient = null;
            }

            // Other
            iniParser = null;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(false, false, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, false);

            instance.BackButton.Content = "Skip current download";

            // Init WebClient
            downloadWebClient = new WebClient();
            downloadWebClient.DownloadProgressChanged += DownloadClient_DownloadProgressChanged;
            downloadWebClient.DownloadFileCompleted += DownloadClient_DownloadFileCompleted;

            AddLogItem(LogType.Info, "- - - Starting Downgrading Process - - -");

            // Populate download queue
            canDownloadFiles = PopulateDownloadQueueList();

            // Change progressbar states
            instance.taskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            instance.GetMainProgressBar().IsIndeterminate = true;

            // Do pre stuff
            RemoveOldFolders();
            RemoveSettings();
            CreateXLivelessAddonFolders();
            KillAnyGTAProcessIfRunning();

            // Removing read-only attributes
            RemoveReadOnlyAttribute(false);

            // Start things
            try
            {
                if (DowngradingInfo.WantsToCreateBackup) // Create Backup
                {
                    StartCreatingBackup();
                }
                else
                {
                    if (canDownloadFiles) // Download Files
                    {
                        StartDownloads();
                    }
                    else // Continue without downloading files
                    {
                        if (DowngradingInfo.InstallPrerequisites) // Install Prerequisites
                        {
                            StartInstallPrerequisites(Prerequisites.VisualCPlusPlus);
                        }
                        else
                        {
                            if (DowngradingInfo.ConfigureForGFWL) // Install GFWL Prerequisites
                            {
                                StartInstallPrerequisites(Prerequisites.GFWL);
                            }
                            else // Continue with Radio/Game downgrade
                            {
                                if (DowngradingInfo.WasAnyRadioDowngraderSelected())
                                    BeginExtractionProcess(DowngradeStep.RadioDowngrade);
                                else
                                    BeginExtractionProcess(DowngradeStep.GameDowngrade);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                instance.ShowErrorScreen(ex);
            }
        }

    }
}
