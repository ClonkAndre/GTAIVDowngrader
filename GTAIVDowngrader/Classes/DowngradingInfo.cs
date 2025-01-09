using System.Collections.Generic;
using System.IO;
using System.Linq;

using GTAIVDowngrader.Classes.Json.Modification;

namespace GTAIVDowngrader.Classes
{
    internal static class DowngradingInfo
    {
        #region Properties
        public static string IVExecutablePath { get; private set; }
        public static string IVWorkingDirectoy { get; private set; }
        public static string IVTargetBackupDirectory { get; private set; }
        public static string GeneratedMD5Hash { get; private set; }
        public static string NewGTAIVTargetLocation { get; private set; }

        public static string DowngradeTo { get; private set; }
        public static string SelectedRadioDowngrader { get; private set; }
        public static string SelectedVladivostokType { get; private set; }

        public static bool ConfigureForGFWL { get; private set; }
        public static bool InstallNoEFLCMusicInIVFix { get; private set; }
        public static bool InstallPrerequisites { get; private set; }
        public static bool WantsToCreateBackup { get; private set; }
        public static bool CreateBackupInZipFile { get; private set; }
        public static bool GTAIVInstallationGotMovedByDowngrader { get; private set; }

        public static Queue<ModDetails> SelectedMods;
        public static Queue<OptionalComponentInfo> SelectedOptionalComponents;
        #endregion

        #region Methods
        public static void Init()
        {
            SelectedMods = new Queue<ModDetails>();
            SelectedOptionalComponents = new Queue<OptionalComponentInfo>();
        }
        public static void Cleanup()
        {
            IVExecutablePath = string.Empty;
            IVWorkingDirectoy = string.Empty;
            IVTargetBackupDirectory = string.Empty;
            GeneratedMD5Hash = string.Empty;
            NewGTAIVTargetLocation = string.Empty;

            DowngradeTo = string.Empty;
            SelectedRadioDowngrader = string.Empty;
            SelectedVladivostokType = string.Empty;

            ConfigureForGFWL = false;
            InstallNoEFLCMusicInIVFix = false;
            InstallPrerequisites = false;
            WantsToCreateBackup = false;
            CreateBackupInZipFile = false;
            GTAIVInstallationGotMovedByDowngrader = false;

            SelectedMods.Clear();
            SelectedOptionalComponents.Clear();
        }

        public static void SetPath(string executablePath)
        {
            IVExecutablePath = executablePath;
            IVWorkingDirectoy = Path.GetDirectoryName(executablePath);
        }
        public static void SetTargetBackupPath(string backupPath)
        {
            IVTargetBackupDirectory = backupPath;
        }
        public static void SetGeneratedMD5Hash(string hash)
        {
            GeneratedMD5Hash = hash;
        }
        public static void SetNewGTAIVTargetLocation(string location)
        {
            NewGTAIVTargetLocation = location;
        }

        public static void SetDowngradeVersion(string version)
        {
            DowngradeTo = version;
        }
        public static void SetRadioDowngrader(string radioDowngrader)
        {
            SelectedRadioDowngrader = radioDowngrader;
        }
        public static void SetVladivostokType(string type)
        {
            SelectedVladivostokType = type;
        }

        public static void SetConfigureForGFWL(bool value)
        {
            ConfigureForGFWL = value;
        }
        public static void SetInstallNoEFLCMusicInIVFix(bool value)
        {
            InstallNoEFLCMusicInIVFix = value;
        }
        public static void SetInstallPrerequisites(bool value)
        {
            InstallPrerequisites = value;
        }
        public static void SetWantsToCreateBackup(bool value)
        {
            WantsToCreateBackup = value;
        }
        public static void SetCreateBackupInZipFile(bool value)
        {
            CreateBackupInZipFile = value;
        }
        public static void SetGTAIVInstallationGotMovedByDowngrader(bool value)
        {
            GTAIVInstallationGotMovedByDowngrader = value;
        }

        public static void AddSelectedMods(List<ModDetails> mods)
        {
            mods = mods.OrderBy(x => x.ForceToBeLastInInstallQueue).ToList();
            mods.ForEach(x => SelectedMods.Enqueue(x));
        }
        public static void AddSelectedOptionalComponents(List<OptionalComponentInfo> optionalComponents)
        {
            optionalComponents.ForEach(x => SelectedOptionalComponents.Enqueue(x));
        }
        #endregion

        #region Functions
        public static bool WasAnyRadioDowngraderSelected()
        {
            return !string.IsNullOrWhiteSpace(SelectedRadioDowngrader);
        }
        public static bool IsSelectedRadioDowngraderSneeds()
        {
            return SelectedRadioDowngrader == "SneedsRadioDowngrader";
        }
        public static bool IsSelectedRadioDowngraderLegacy()
        {
            return SelectedRadioDowngrader == "LegacyRadioDowngrader";
        }
        #endregion
    }
}
