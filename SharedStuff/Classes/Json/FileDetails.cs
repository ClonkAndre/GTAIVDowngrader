#if FILE_EDITOR_PROJ
using Newtonsoft.Json;
#endif

namespace GTAIVDowngrader.Classes.Json
{
    public class FileDetails
    {

        #region Variables

        // Details
        public string Name;
#if FILE_EDITOR_PROJ
        [JsonIgnore()] public string EditName;
#endif
        public string DownloadURL;
        public long SizeInBytes;

        #endregion

        #region Constructor
#if FILE_EDITOR_PROJ
        // copy constructor
        public FileDetails(FileDetails fileDetails)
        {
            Name = fileDetails.Name;
            DownloadURL = fileDetails.DownloadURL;
            SizeInBytes = fileDetails.SizeInBytes;
        }
#endif
        // default constructor
        public FileDetails()
        {
            Name = "";
#if FILE_EDITOR_PROJ
            EditName = "";
#endif
            DownloadURL = "";
            SizeInBytes = 0;
        }
        #endregion

        #region Methods
#if FILE_EDITOR_PROJ
        public void PrepareForEditor()
        {
            EditName = Name;
        }
#endif
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
