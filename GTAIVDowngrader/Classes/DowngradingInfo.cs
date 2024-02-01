using System;
using System.Collections.Generic;
using System.IO;

using GTAIVDowngrader.JsonObjects;

namespace GTAIVDowngrader.Classes
{
    internal class DowngradingInfo
    {
        #region Properties
        public string IVExecutablePath { get; private set; }
        public string IVWorkingDirectoy { get; private set; }
        public string IVTargetBackupDirectory { get; private set; }
        public string ReceivedMD5Hash { get; private set; }
        public string RelatedMD5Hash { get; private set; }
        public string NewGTAIVTargetLocation { get; private set; }

        public GameVersion DowngradeTo { get; private set; }
        public RadioDowngrader SelectedRadioDowngrader { get; private set; }
        public VladivostokTypes SelectedVladivostokType { get; private set; }

        public bool ConfigureForGFWL { get; private set; }
        public bool InstallNoEFLCMusicInIVFix { get; private set; }
        public bool InstallPrerequisites { get; private set; }
        public bool WantsToCreateBackup { get; private set; }
        public bool CreateBackupInZipFile { get; private set; }
        public bool GTAIVInstallationGotMovedByDowngrader { get; private set; }

        public List<ModInformation> SelectedMods;
        public List<OptionalComponentInfo> SelectedOptionalComponents;
        #endregion

        #region Constructor
        public DowngradingInfo()
        {
            SelectedMods = new List<ModInformation>();
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
        public void SetReceivedMD5Hash(string hash)
        {
            ReceivedMD5Hash = hash;
        }
        public void SetRelatedMD5Hash(string hash)
        {
            RelatedMD5Hash = hash;
        }
        public void SetNewGTAIVTargetLocation(string location)
        {
            NewGTAIVTargetLocation = location;
        }

        public void SetDowngradeVersion(GameVersion version)
        {
            DowngradeTo = version;
        }
        public void SetRadioDowngrader(RadioDowngrader radioDowngrader)
        {
            SelectedRadioDowngrader = radioDowngrader;
        }
        public void SetVladivostokType(VladivostokTypes type)
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
    }
}
