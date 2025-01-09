using System.Collections.Generic;

#if FILE_EDITOR_PROJ
using Newtonsoft.Json;
#endif

namespace GTAIVDowngrader.Classes.Json.PostInstallActions
{
    public class IniFileSection
    {

        #region Variables
        public string Section;
        public string Key;
        public string NewValue;
        #endregion

        #region Constructor
        public IniFileSection()
        {
            Section = "";
            Key = "";
            NewValue = "";
        }
        #endregion

    }

    public class EditIniFileAction
    {

        #region Variables
        public string TargetFileName;

#if FILE_EDITOR_PROJ
        [JsonIgnore()] public string EditTargetFileName;
#endif

        public List<IniFileSection> EditEntries;
#endregion

        #region Constructor
        public EditIniFileAction()
        {
            TargetFileName = "";

#if FILE_EDITOR_PROJ
            EditTargetFileName = "";
#endif

            EditEntries = new List<IniFileSection>();
        }
        #endregion

#if FILE_EDITOR_PROJ
        public void PrepareForEditor()
        {
            EditTargetFileName = TargetFileName;
        }
#endif

    }
}
