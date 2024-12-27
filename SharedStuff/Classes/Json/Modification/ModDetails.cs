using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace GTAIVDowngrader.Classes.Json.Modification
{
    public class ModDetails
    {
        #region Variables

        // Details
        public FileDetails FileDetails;
        public List<string> ForGameVersion;

        public string Title;
        [JsonIgnore()] public string EditTitle;
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

        #endregion

        #region Constructor
        public ModDetails(ModDetails instance)
        {
            FileDetails = instance.FileDetails;
            ForGameVersion = instance.ForGameVersion.ToList();

            Title = instance.Title;
            EditTitle = instance.EditTitle;
            Description = instance.Description;
            WarningMessage = instance.WarningMessage;
            OfficialModWebPage = instance.OfficialModWebPage;

            IsASILoader = instance.IsASILoader;
            IsScriptHook = instance.IsScriptHook;
            ASIModDetails = new ASIModDetails(instance.ASIModDetails);
            DotNetModDetails = new DotNetModDetails(instance.DotNetModDetails);

            CompatibleWithGFWL = instance.CompatibleWithGFWL;
            ShowInDowngrader = instance.ShowInDowngrader;
            CheckedByDefault = instance.CheckedByDefault;

            OptionalComponents = instance.OptionalComponents.ToList();
        }
        public ModDetails()
        {
            FileDetails = new FileDetails();
            ForGameVersion = new List<string>();
            OptionalComponents = new List<OptionalComponentInfo>();

            Title = "";
            EditTitle = "";
            Description = "";
            WarningMessage = "";
            OfficialModWebPage = "";

            ShowInDowngrader = true;
        }
        #endregion

        #region Methods
        public void PrepareForEditor()
        {
            EditTitle = Title;
            FileDetails.PrepareForEditor();
            OptionalComponents.ForEach(x => x.PrepareForEditor());
        }
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
                FileDetails, // 0
                ForGameVersion.Count, // 1
                Title, // 2
                ASIModDetails.ToString(), // 3
                DotNetModDetails.ToString(), // 4
                CompatibleWithGFWL, // 5
                ShowInDowngrader, // 6
                CheckedByDefault, // 7
                OptionalComponents.Count); // 8
        }
        #endregion
    }
}
