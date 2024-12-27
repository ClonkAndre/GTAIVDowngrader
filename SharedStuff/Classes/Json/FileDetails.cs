using Newtonsoft.Json;

namespace GTAIVDowngrader.Classes.Json
{
    public class FileDetails
    {

        #region Variables

        // Details
        public string Name;
        [JsonIgnore()] public string EditName;
        public string DownloadURL;
        public long SizeInBytes;

        #endregion

        #region Constructor
        public FileDetails()
        {
            Name = "";
            EditName = "";
            DownloadURL = "";
        }
        #endregion

        #region Methods
        public void PrepareForEditor()
        {
            EditName = Name;
        }
        #endregion

        #region Functions
        public bool AreDetailsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(DownloadURL);
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("Name: {0}, SizeInBytes: {1}, DownloadURL: {2}", Name, SizeInBytes, DownloadURL);
        }
        #endregion

    }
}
