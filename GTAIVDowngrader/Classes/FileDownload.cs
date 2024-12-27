using GTAIVDowngrader.Classes.Json;

namespace GTAIVDowngrader.Classes
{
    internal class FileDownload
    {
        #region Properties
        public string FileName;
        public string DownloadURL;
        public long FileSize { get; private set; }
        #endregion

        #region Constructor
        public FileDownload(DowngradeFileDetails info)
        {
            FileName = info.FileName;
            FileSize = info.FileSize;
            DownloadURL = info.DownloadURL;
        }
        public FileDownload(OptionalComponentInfo info)
        {
            FileName = info.FileName;
            FileSize = info.FileSize;
            DownloadURL = info.DownloadURL;
        }
        public FileDownload(ModDetails info)
        {
            FileName = info.FileName;
            FileSize = info.FileSize;
            DownloadURL = info.DownloadURL;
        }
        #endregion
    }
}
