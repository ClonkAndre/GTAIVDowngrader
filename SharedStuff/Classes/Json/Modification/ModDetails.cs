using System.Collections.Generic;
using System.Linq;

#if FILE_EDITOR_PROJ
using Newtonsoft.Json;
#endif

using GTAIVDowngrader.Classes.Json.PostInstallActions;

namespace GTAIVDowngrader.Classes.Json.Modification
{
    public class ModDetails
    {
        #region Variables

        // Details
        public FileDetails FileDetails;
        public List<string> ForGameVersion;

        public string UniqueName;
        public string Title;
#if FILE_EDITOR_PROJ
        [JsonIgnore()] public string EditTitle;
#endif
        public string Description;
        public string WarningMessage;
        public string OfficialModWebPage;

        // Type
        public bool IsASILoader;
        public bool IsScriptHook;
        public bool IsScriptHookDotNet;
        public bool IsIVSDKDotNet;
        public ASIModDetails ASIModDetails;
        public DotNetModDetails DotNetModDetails;

        // Other
        public bool CompatibleWithGFWL;
        public bool ShowInDowngrader;
        public bool CheckedByDefault;
        public bool ForceToBeLastInInstallQueue;

        // Optional Components
        public List<OptionalComponentInfo> OptionalComponents;

        // Post Install Actions
        public List<PostInstallAction> PostInstallActions;

        #endregion

        #region Constructor
#if FILE_EDITOR_PROJ
        // copy constructor
        public ModDetails(ModDetails modDetails)
        {
            FileDetails = new FileDetails(modDetails.FileDetails);
            ForGameVersion = modDetails.ForGameVersion.ToList();

            UniqueName = modDetails.UniqueName;
            Title = modDetails.Title;
            EditTitle = modDetails.EditTitle;
            Description = modDetails.Description;
            WarningMessage = modDetails.WarningMessage;
            OfficialModWebPage = modDetails.OfficialModWebPage;

            IsASILoader = modDetails.IsASILoader;
            IsScriptHook = modDetails.IsScriptHook;
            IsScriptHookDotNet = modDetails.IsScriptHookDotNet;
            IsIVSDKDotNet = modDetails.IsIVSDKDotNet;
            ASIModDetails = new ASIModDetails(modDetails.ASIModDetails);
            DotNetModDetails = new DotNetModDetails(modDetails.DotNetModDetails);

            CompatibleWithGFWL = modDetails.CompatibleWithGFWL;
            ShowInDowngrader = modDetails.ShowInDowngrader;
            CheckedByDefault = modDetails.CheckedByDefault;

            OptionalComponents = modDetails.OptionalComponents.ToList();

            PostInstallActions = modDetails.PostInstallActions.ToList();
        }
#endif
        // default constructor
        public ModDetails()
        {
            FileDetails = new FileDetails();
            ForGameVersion = new List<string>();
            OptionalComponents = new List<OptionalComponentInfo>();
            PostInstallActions = new List<PostInstallAction>();

            UniqueName = "";
            Title = "";
#if FILE_EDITOR_PROJ
            EditTitle = "";
#endif
            Description = "";
            WarningMessage = "";
            OfficialModWebPage = "";

            ShowInDowngrader = true;
        }
        #endregion

        #region Methods
#if FILE_EDITOR_PROJ
        public void PrepareForEditor()
        {
            EditTitle = Title;
            FileDetails.PrepareForEditor();
            OptionalComponents.ForEach(x => x.PrepareForEditor());
            PostInstallActions.ForEach(x => x.PrepareForEditor());
        }
#endif
        #endregion

        #region Functions
        public bool AreDetailsValid()
        {
            return !string.IsNullOrWhiteSpace(Title)
                && !string.IsNullOrWhiteSpace(Description)
                && ForGameVersion.Count != 0;
        }

        public bool HasASIModDetails()
        {
            return ASIModDetails != null;
        }
        public bool HasDotNetModDetails()
        {
            return DotNetModDetails != null;
        }
        public bool HasOptionalComponents()
        {
            return OptionalComponents.Count != 0;
        }

        public bool IsCompatibleWithThisVersion(string version)
        {
            return ForGameVersion.Contains(version);
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format(
                "FileDetails: {0}, " +
                "ForGameVersion Count: {1}, " +
                "Title: {2}, " +
                "ASIModDetails: {3}, " +
                "DotNetModDetails: {4}, " +
                "CompatibleWithGFWL: {5}, " +
                "ShowInDowngrader: {6}, " +
                "CheckedByDefault: {7}, " +
                "OptionalComponents Count: {8}",
                "PostInstallActions Count: {9}",
                FileDetails, // 0
                ForGameVersion.Count, // 1
                Title, // 2
                ASIModDetails == null ? "-" : ASIModDetails.ToString(), // 3
                DotNetModDetails == null ? "-" : DotNetModDetails.ToString(), // 4
                CompatibleWithGFWL, // 5
                ShowInDowngrader, // 6
                CheckedByDefault, // 7
                OptionalComponents.Count, // 8
                PostInstallActions.Count); // 9
        }
        #endregion
    }
}
