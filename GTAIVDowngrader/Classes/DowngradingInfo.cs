using System.Collections.Generic;
using System.IO;

using GTAIVDowngrader.Classes.Json.Modification;

namespace GTAIVDowngrader.Classes
{
    internal class DowngradingInfo
    {
        #region Properties
        public string IVExecutablePath { get; private set; }
        public string IVWorkingDirectoy { get; private set; }
        public string IVTargetBackupDirectory { get; private set; }
        public string GeneratedMD5Hash { get; private set; }
        public string NewGTAIVTargetLocation { get; private set; }

        public string DowngradeTo { get; private set; }
        public string SelectedRadioDowngrader { get; private set; }
        public string SelectedVladivostokType { get; private set; }

        public bool ConfigureForGFWL { get; private set; }
        public bool InstallNoEFLCMusicInIVFix { get; private set; }
        public bool InstallPrerequisites { get; private set; }
        public bool WantsToCreateBackup { get; private set; }
        public bool CreateBackupInZipFile { get; private set; }
        public bool GTAIVInstallationGotMovedByDowngrader { get; private set; }

        public List<ModDetails> SelectedMods;
        public List<OptionalComponentInfo> SelectedOptionalComponents;
        #endregion

        #region Constructor
        public DowngradingInfo()
        {
            SelectedMods = new List<ModDetails>();
            SelectedOptionalComponents = new List<OptionalComponentInfo>();
        }
        #endregion

        #region Methods
        public void SetPath(string executablePath)
        {
            IVExecutablePath = executablePath;
            IVWorkingDirectoy = Path.GetDirectoryName(executablePath);
        }
        public void SetTargetBackupPath(string backupPath)
        {
            IVTargetBackupDirectory = backupPath;
        }
        public void SetGeneratedMD5Hash(string hash)
        {
            GeneratedMD5Hash = hash;
        }
        public void SetNewGTAIVTargetLocation(string location)
        {
            NewGTAIVTargetLocation = location;
        }

        public void SetDowngradeVersion(string version)
        {
            DowngradeTo = version;
        }
        public void SetRadioDowngrader(string radioDowngrader)
        {
            SelectedRadioDowngrader = radioDowngrader;
        }
        public void SetVladivostokType(string type)
        {
            SelectedVladivostokType = type;
        }

        public void SetConfigureForGFWL(bool value)
        {
            ConfigureForGFWL = value;
        }
        public void SetInstallNoEFLCMusicInIVFix(bool value)
        {
            InstallNoEFLCMusicInIVFix = value;
        }
        public void SetInstallPrerequisites(bool value)
        {
            InstallPrerequisites = value;
        }
        public void SetWantsToCreateBackup(bool value)
        {
            WantsToCreateBackup = value;
        }
        public void SetCreateBackupInZipFile(bool value)
        {
            CreateBackupInZipFile = value;
        }
        public void SetGTAIVInstallationGotMovedByDowngrader(bool value)
        {
            GTAIVInstallationGotMovedByDowngrader = value;
        }
        #endregion

        #region Functions
        public bool WasAnyRadioDowngraderSelected()
        {
            return !string.IsNullOrWhiteSpace(SelectedRadioDowngrader);
        }
        public bool IsSelectedRadioDowngraderSneeds()
        {
            return SelectedRadioDowngrader == "SneedsRadioDowngrader";
        }
        public bool IsSelectedRadioDowngraderLegacy()
        {
            return SelectedRadioDowngrader == "LegacyRadioDowngrader";
        }
        #endregion
    }
}
