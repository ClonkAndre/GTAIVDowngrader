namespace GTAIVDowngrader.JsonObjects {
    public class DowngradeInformation {

        #region Variables
        // File Details
        public string FileName;
        public long FileSize;

        // Mod Details
        public string Type;
        public string ForVersion;
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
            return string.Format("Type: {0}, Version: {1}, FileName: {2}, URL: {3}", Type, ForVersion, FileName, DownloadURL);
        }
        #endregion

    }
}
