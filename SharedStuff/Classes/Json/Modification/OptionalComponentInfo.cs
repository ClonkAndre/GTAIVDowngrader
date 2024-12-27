using Newtonsoft.Json;

namespace GTAIVDowngrader.Classes.Json.Modification
{
    public class OptionalComponentInfo
    {

        #region Variables

        // Details
        public FileDetails FileDetails;

        public string Title;
        [JsonIgnore()] public string EditTitle;
        public string Description;

        // Other
        public bool CheckedByDefault;

        #endregion

        #region Constructor
        public OptionalComponentInfo()
        {
            FileDetails = new FileDetails();
            Title = "";
            EditTitle = "";
            Description = "";
        }
        #endregion

        #region Methods
        public void PrepareForEditor()
        {
            EditTitle = Title;
            FileDetails.PrepareForEditor();
        }
        #endregion

    }
}
