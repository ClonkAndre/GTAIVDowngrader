using GTAIVDowngrader.JsonObjects;

namespace GTAIVDowngrader.Classes
{
    internal class FileDownload
    {
        #region Properties
        public string FileName { get; private set; }
        public string DownloadURL { get; private set; }
        public long FileSize { get; private set; }
        public bool NeedsToBeDecompressed { get; private set; }
        #endregion

        #region Constructor
        public FileDownload(DowngradeInformation info)
        {
            FileName = info.FileName;
            FileSize = info.FileSize;
            DownloadURL = info.DownloadURL;
            NeedsToBeDecompressed = info.NeedsToBeDecompressed;
        }
        public FileDownload(OptionalComponentInfo info)
        {
            FileName = info.FileName;
            FileSize = info.FileSize;
            DownloadURL = info.DownloadURL;
            NeedsToBeDecompressed = false;
        }
        public FileDownload(ModInformation info)
        {
            FileName = info.FileName;
            FileSize = info.FileSize;
            DownloadURL = info.DownloadURL;
            NeedsToBeDecompressed = false;
        }
        #endregion
    }
}
