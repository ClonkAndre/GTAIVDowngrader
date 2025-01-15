#if FILE_EDITOR_PROJ
using Newtonsoft.Json;
#endif

namespace GTAIVDowngrader.Classes.Json.Modification
{
    public class OptionalComponentInfo
    {

        #region Variables

        // Details
        public FileDetails FileDetails;

        public string Title;
#if FILE_EDITOR_PROJ
        [JsonIgnore()] public string EditTitle;
#endif
        public string Description;

        // Other
        public bool CheckedByDefault;

        #endregion

        #region Constructor
        public OptionalComponentInfo()
        {
            FileDetails = new FileDetails();
            Title = "";
#if FILE_EDITOR_PROJ
            EditTitle = "";
#endif
            Description = "";
        }
        #endregion

        #region Methods
#if FILE_EDITOR_PROJ
        public void PrepareForEditor()
        {
            EditTitle = Title;
            FileDetails.PrepareForEditor();
        }
#endif
        #endregion

    }
}
