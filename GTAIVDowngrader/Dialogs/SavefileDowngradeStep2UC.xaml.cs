using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Microsoft.WindowsAPICodePack.Dialogs;

using CCL;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SavefileDowngradeStep2UC : UserControl
    {

        #region Variables
        private MainWindow instance;
        private WebClient webClient;

        private string[] selectedSaveFiles;
        private Queue<string> saveFilesUploadQueue;
        private Queue<SaveFileDownload> saveFilesDownloadQueue;
        #endregion

        #region Structs
        private struct SaveFileDownload
        {
            #region Properties
            public string FileName { get; private set; }
            public string ID { get; private set; }
            public int Slot { get; private set; }
            #endregion

            #region Constructor
            public SaveFileDownload(string fileName, string id, int slot)
            {
                FileName = fileName;
                ID = id;
                Slot = slot;
            }
            #endregion
        }
        #endregion

        #region Methods
        private void ChangeSelectedFilesTextVisibility(bool visible)
        {
            Dispatcher.Invoke(() => {
                SelectedFilesTextBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            });
        }
        private void ChangeButtonEnabledStates(bool enabled)
        {
            Dispatcher.Invoke(() => {
                SelectFilesButton.IsEnabled = enabled;
                DowngradeButton.IsEnabled = enabled;
                instance.ChangeActionButtonEnabledState(true, enabled, true, enabled);
            });
        }
        private void ChangeProgressBarIndeterminateState(bool isIndeterminate)
        {
            Dispatcher.Invoke(() => {
                StatusProgressBar.IsIndeterminate = isIndeterminate;
            });
        }
        private void UpdateStatusText(string status)
        {
            Dispatcher.Invoke(() => {
                StatusTextBlock.Text = status;
            });
        }
        private void UpdateStatusProgressBar(int value)
        {
            Dispatcher.Invoke(() => {
                StatusProgressBar.Value = value;
            });
        }
        #endregion

        #region Constructor
        public SavefileDowngradeStep2UC(MainWindow window)
        {
            instance = window;

            // Lists
            saveFilesUploadQueue = new Queue<string>();
            saveFilesDownloadQueue = new Queue<SaveFileDownload>();

            // WebClient
            webClient = new WebClient();
            webClient.UploadProgressChanged += WebClient_UploadProgressChanged;
            webClient.UploadFileCompleted += WebClient_UploadFileCompleted;
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

            InitializeComponent();
        }
        public SavefileDowngradeStep2UC()
        {
            // Lists
            saveFilesUploadQueue = new Queue<string>();
            saveFilesDownloadQueue = new Queue<SaveFileDownload>();

            // WebClient
            webClient = new WebClient();
            webClient.UploadProgressChanged += WebClient_UploadProgressChanged;
            webClient.UploadFileCompleted += WebClient_UploadFileCompleted;
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep();
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

        private void WebClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive == -1)
            {
                ChangeProgressBarIndeterminateState(true);
                UpdateStatusText(string.Format("Starting upload for {0}", e.UserState.ToString()));
                return;
            }

            ChangeProgressBarIndeterminateState(false);

            string bytesReceived =          FileHelper.GetExactFileSizeAdvanced(e.BytesReceived).ToString();
            string totalBytesToReceived =   FileHelper.GetExactFileSizeAdvanced(e.TotalBytesToReceive).ToString();

            UpdateStatusText(string.Format("Uploading {0} | {1} of {2} uploaded", e.UserState.ToString(), bytesReceived, totalBytesToReceived));
        }
        private void WebClient_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            string saveFileName = e.UserState.ToString();

            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                Core.Notification.ShowNotification(NotificationType.Error, 6500, string.Format("{0} - Upload error... Continuing", saveFileName), e.Error.Message, saveFileName);
                Core.AddLogItem(LogType.Error, string.Format("An error occured while trying to upload {0}. Details: {1}", saveFileName, e.Error.Message));

                // Upload next save file
                if (saveFilesUploadQueue.Count != 0)
                {
                    string nextSaveFileToBeUploaded = saveFilesUploadQueue.Dequeue();
                    webClient.UploadFileAsync(new Uri("https://gtasnp.com/upload/process"), "POST", nextSaveFileToBeUploaded, Path.GetFileNameWithoutExtension(nextSaveFileToBeUploaded));

                    // Log
                    Core.AddLogItem(LogType.Info, string.Format("(1) - Uploading next file {0}", nextSaveFileToBeUploaded));
                }
                else
                {
                    UpdateStatusText("Uploading complete! Continuing with downloading converted saves...");

                    SaveFileDownload nextSaveFileToBeDownloaded = saveFilesDownloadQueue.Dequeue();
                    string downloadUrl = string.Format("https://gtasnp.com/download/file/{0}?downgrade_version=1&slot={1}", nextSaveFileToBeDownloaded.ID, nextSaveFileToBeDownloaded.Slot.ToString());

                    string dir = ".\\Data\\Savegames";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    webClient.DownloadFileAsync(new Uri(downloadUrl), string.Format("{0}\\{1}", dir, nextSaveFileToBeDownloaded.FileName), nextSaveFileToBeDownloaded.FileName);

                    // Log
                    Core.AddLogItem(LogType.Info, string.Format("(1) - Uploading complete! Started downloading file {0}", nextSaveFileToBeDownloaded.FileName));
                }
                
                return;
            }

            // Parse save game info from response
            string[] responseLines = Encoding.ASCII.GetString(e.Result).Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries);

            bool isValidSaveGame = true;
            bool isGTAIVSaveGame = true;
            string saveGameID = string.Empty;
            int slot = 0;

            for (int i = 0; i < responseLines.Length; i++)
            {
                string line = responseLines[i];

                // Checks
                if (!isValidSaveGame)
                {
                    Core.Notification.ShowNotification(NotificationType.Warning, 3000, "Invalid file", string.Format("{0} is not a valid GTA IV save game file.", saveFileName));
                    break;
                }
                if (!isGTAIVSaveGame)
                {
                    Core.Notification.ShowNotification(NotificationType.Warning, 3000, "Wrong game", string.Format("{0} is not a valid GTA IV save game file.", saveFileName));
                    break;
                }

                // Parsing
                if (line.Contains("property"))
                {
                    if (line.Contains("og:url")) // Check if is valid save game file and get save game ID if valid
                    {

                        if (line.Contains("nogame")) // Uploaded file is not a valid save game
                        {
                            isValidSaveGame = false;
                        }
                        else // Try to parse save game ID
                        {
                            string rawID = line.Split(new char[] { '/' })[3];
                            saveGameID = rawID.Remove(rawID.Length - 2, 2);
                        }

                        continue;

                    }
                    if (line.Contains("og:image")) // Check if save game is for GTA IV
                    {
                        isGTAIVSaveGame = line.Contains("gtaiv");
                    }
                }
            }

            // Get slot number
            string slotStrRaw = saveFileName.Remove(0, saveFileName.Length - 2);

            if (slotStrRaw == "00")
            {
                slot = 0;
            }
            else
            {
                slotStrRaw = slotStrRaw.TrimStart('0');
                slot = ParseExtension.Parse(slotStrRaw, 0);
            }

            // Add to download queue if uploaded save game was valid
            if (isValidSaveGame && isGTAIVSaveGame)
                saveFilesDownloadQueue.Enqueue(new SaveFileDownload(saveFileName, saveGameID, slot));

            // Continue with next save file
            if (saveFilesUploadQueue.Count != 0)
            {
                string nextSaveFileToBeUploaded = saveFilesUploadQueue.Dequeue();
                webClient.UploadFileAsync(new Uri("https://gtasnp.com/upload/process"), "POST", nextSaveFileToBeUploaded, Path.GetFileNameWithoutExtension(nextSaveFileToBeUploaded));

                // Log
                Core.AddLogItem(LogType.Info, string.Format("(2) - Uploading next file {0}", nextSaveFileToBeUploaded));
            }
            else
            {
                UpdateStatusText("Uploading complete! Continuing with downloading converted saves...");

                SaveFileDownload nextSaveFileToBeDownloaded = saveFilesDownloadQueue.Dequeue();
                string downloadUrl = string.Format("https://gtasnp.com/download/file/{0}?downgrade_version=1&slot={1}", nextSaveFileToBeDownloaded.ID, nextSaveFileToBeDownloaded.Slot.ToString());

                string dir = ".\\Data\\Savegames";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                webClient.DownloadFileAsync(new Uri(downloadUrl), string.Format("{0}\\{1}", dir, nextSaveFileToBeDownloaded.FileName), nextSaveFileToBeDownloaded.FileName);

                // Log
                Core.AddLogItem(LogType.Info, string.Format("(2) - Uploading complete! Started downloading file {0}", nextSaveFileToBeDownloaded.FileName));
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive == -1)
            {
                ChangeProgressBarIndeterminateState(true);
                UpdateStatusText(string.Format("Starting download for {0}", e.UserState.ToString()));
                return;
            }

            ChangeProgressBarIndeterminateState(false);

            string bytesReceived =          FileHelper.GetExactFileSizeAdvanced(e.BytesReceived).ToString();
            string totalBytesToReceived =   FileHelper.GetExactFileSizeAdvanced(e.TotalBytesToReceive).ToString();

            UpdateStatusText(string.Format("Downloading {0} | {1} of {2} downloaded", e.UserState.ToString(), bytesReceived, totalBytesToReceived));
        }
        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string saveFileName = e.UserState.ToString();

            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                Core.Notification.ShowNotification(NotificationType.Error, 6500, string.Format("{0} - Download error... Continuing", saveFileName), e.Error.Message, saveFileName);
                Core.AddLogItem(LogType.Error, string.Format("An error occured while trying to download {0}. Details: {1}", saveFileName, e.Error.Message));

                // Download next save file
                if (saveFilesDownloadQueue.Count != 0)
                {
                    SaveFileDownload nextSaveFileToBeDownloaded = saveFilesDownloadQueue.Dequeue();
                    string downloadUrl = string.Format("https://gtasnp.com/download/file/{0}?downgrade_version=1&slot={1}", nextSaveFileToBeDownloaded.ID, nextSaveFileToBeDownloaded.Slot.ToString());

                    string dir = ".\\Data\\Savegames";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    webClient.DownloadFileAsync(new Uri(downloadUrl), string.Format("{0}\\{1}", dir, nextSaveFileToBeDownloaded.FileName), nextSaveFileToBeDownloaded.FileName);

                    // Log
                    Core.AddLogItem(LogType.Info, string.Format("(1) - Downloading next file {0}", nextSaveFileToBeDownloaded.FileName));
                }
                else
                {
                    selectedSaveFiles = null;
                    ChangeSelectedFilesTextVisibility(false);
                    UpdateStatusProgressBar(100);
                    ChangeButtonEnabledStates(true);

                    UpdateStatusText("Converting and downloading saves completed! You can now continue.");

                    // Log
                    Core.AddLogItem(LogType.Info, "(1) - Coverting and downloading saves completed!");
                }

                return;
            }

            // Download next save file
            if (saveFilesDownloadQueue.Count != 0)
            {
                SaveFileDownload nextSaveFileToBeDownloaded = saveFilesDownloadQueue.Dequeue();
                string downloadUrl = string.Format("https://gtasnp.com/download/file/{0}?downgrade_version=1&slot={1}", nextSaveFileToBeDownloaded.ID, nextSaveFileToBeDownloaded.Slot.ToString());

                string dir = ".\\Data\\Savegames";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                webClient.DownloadFileAsync(new Uri(downloadUrl), string.Format("{0}\\{1}", dir, nextSaveFileToBeDownloaded.FileName), nextSaveFileToBeDownloaded.FileName);

                // Log
                Core.AddLogItem(LogType.Info, string.Format("(2) - Downloading next file {0}", nextSaveFileToBeDownloaded.FileName));
            }
            else
            {
                selectedSaveFiles = null;
                ChangeSelectedFilesTextVisibility(false);
                UpdateStatusProgressBar(100);
                ChangeButtonEnabledStates(true);
                UpdateStatusText("Converting and downloading saves completed! You can now continue.");

                // Log
                Core.AddLogItem(LogType.Info, "(2) - Converting and downloading saves completed!");
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            if (Core.Is420())
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char2.png", UriKind.Relative));
        }

        private void SelectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
            {
                ofd.Title = "Select save files that should be downgraded";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    selectedSaveFiles = ofd.FileNames.ToArray();
                    
                    if (selectedSaveFiles.Length == 1)
                        SelectedFilesTextBlock.Text = string.Format("{0} save file selected", selectedSaveFiles.Length.ToString());
                    else 
                        SelectedFilesTextBlock.Text = string.Format("{0} save files selected", selectedSaveFiles.Length.ToString());

                    ChangeSelectedFilesTextVisibility(true);
                }
            }
        }
        private void DowngradeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedSaveFiles == null || selectedSaveFiles.Length == 0)
                {
                    Core.Notification.ShowNotification(NotificationType.Warning, 3000, "No save file(s) selected", "Please select atleast one save file.");
                    return;
                }

                // Populate Queue
                for (int i = 0; i < selectedSaveFiles.Length; i++)
                {
                    saveFilesUploadQueue.Enqueue(selectedSaveFiles[i]);
                }

                // Disable UI Buttons
                ChangeButtonEnabledStates(false);

                // Start uploading first save file
                string nextSaveFileToBeUploaded = saveFilesUploadQueue.Dequeue();
                webClient.UploadFileAsync(new Uri("https://gtasnp.com/upload/process"), "POST", nextSaveFileToBeUploaded, Path.GetFileNameWithoutExtension(nextSaveFileToBeUploaded));

                // Log
                Core.AddLogItem(LogType.Info, "- - - GTASnP Save File Downgrading - - -");
                Core.AddLogItem(LogType.Info, string.Format("Starting uploading {0}", nextSaveFileToBeUploaded));
            }
            catch (Exception ex)
            {
                Core.AddLogItem(LogType.Info, "- - - GTASnP Save File Downgrading - - -");
                Core.AddLogItem(LogType.Info, string.Format("Failed to start save file downgrade... Details: {0}", ex.Message));
            }
        }

    }
}
