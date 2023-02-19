namespace GTAIVDowngrader.JsonObjects {
    public class DowngradeInformation {

        #region Variables
        // File Details
        public string FileName;
        public long FileSize;
        public bool NeedsToBeDecompressed;

        // Mod Details
        public string Type;
        public string DownloadURL;
        #endregion

        #region Constructor
        public DowngradeInformation()
        {

        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("Type: {0}, FileName: {1}, FileSize: {2}, URL: {3}", Type, FileName, FileSize.ToString(), DownloadURL);
        }
        #endregion

    }
}
