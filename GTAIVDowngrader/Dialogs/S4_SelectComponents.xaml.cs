using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using GTAIVDowngrader.Controls;
using INIController;

namespace GTAIVDowngrader.Dialogs {
    public partial class S4_SelectComponents : UserControl {

        #region Variables
        private MainWindow instance;

        private List<ModItem> modItems;
        public List<ModItem> selectedMods;

        private ModCheck modCheck;
        private StringBuilder errorText;
        #endregion

        #region Classes
        public class ModCheck
        {
            #region Variables
            public int ASILoadersSelected;
            public int ASIModsSelected;
            public List<string> ASIModsSelectedNames;

            public bool ScriptHookSelected;
            public int ScriptHookModsSelected;
            public List<string> ScriptHookModsSelectedNames;

            public bool ScriptHookHookSelected;

            public bool ScriptHookDotNetSelected;
            public int ScriptHookDotNetModsSelected;
            public List<string> ScriptHookDotNetModsSelectedNames;
            #endregion

            #region Constructor
            public ModCheck()
            {
                ASILoadersSelected = 0;
                ASIModsSelected = 0;
                ASIModsSelectedNames = new List<string>();

                ScriptHookSelected = false;
                ScriptHookModsSelected = 0;
                ScriptHookModsSelectedNames = new List<string>();

                ScriptHookHookSelected = false;

                ScriptHookDotNetSelected = false;
                ScriptHookDotNetModsSelected = 0;
                ScriptHookDotNetModsSelectedNames = new List<string>();
            }
            #endregion

            #region Overrides
            public override string ToString()
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("- - - ASI - - -");
                str.AppendLine("ASI Loaders selected: " + ASILoadersSelected.ToString());
                str.AppendLine("ASI Mods selected: " + ASIModsSelected.ToString());
                str.AppendLine();
                str.AppendLine("- - - ScriptHook - - -");
                str.AppendLine("ScriptHook selected: " + ScriptHookSelected.ToString());
                str.AppendLine("ScriptHook Mods selected: " + ScriptHookModsSelected.ToString());
                str.AppendLine();
                str.AppendLine("- - - ScriptHookHook - - -");
                str.AppendLine("ScriptHookHook selected: " + ScriptHookHookSelected.ToString());
                str.AppendLine();
                str.AppendLine("- - - ScriptHookDotNet - - -");
                str.AppendLine("ScriptHookDotNet selected: " + ScriptHookDotNetSelected.ToString());
                str.AppendLine("ScriptHookDotNet Mods selected: " + ScriptHookDotNetModsSelected.ToString());
                return str.ToString();
            }
            #endregion
        }
        #endregion

        #region Methods
        public void LoadMods()
        {
            try {
                ModListStackPanel.Children.Clear();
                modItems.Clear();
                if (Directory.Exists(".\\Downgrader")) {
                    if (Directory.Exists(".\\Downgrader\\Files")) {
                        if (Directory.Exists(".\\Downgrader\\Files\\Mods")) {
                            foreach (string file in Directory.GetFiles(".\\Downgrader\\Files\\Mods")) {
                                if (Path.GetExtension(file).ToLower() == ".ini") {
                                    // Details
                                    string mVersionRaw = Ini.ReadValueFromFile("Details", "ForVersion", "ALL", file);
                                    ModVersion mVersion = ModVersion.All;
                                    switch (mVersionRaw) {
                                        case "1080":
                                            mVersion = ModVersion.v1080;
                                            break;
                                        case "1070":
                                            mVersion = ModVersion.v1070;
                                            break;
                                        case "1040":
                                            mVersion = ModVersion.v1040;
                                            break;
                                        default:
                                            mVersion = ModVersion.All;
                                            break;
                                    }

                                    string mFilename = Ini.ReadValueFromFile("Details", "Filename", "", file);
                                    string mName = Ini.ReadValueFromFile("Details", "Name", "", file);
                                    string mDesc = Ini.ReadValueFromFile("Details", "Description", "", file);
                                    if (string.IsNullOrWhiteSpace(mDesc)) mDesc = "No description provided.";

                                    if (string.IsNullOrWhiteSpace(mFilename) || string.IsNullOrWhiteSpace(mName))
                                        continue;
                                    
                                    bool checkedByDefault = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Details", "CheckedByDefault", "False", file), false);

                                    // Type
                                    bool isASILoader = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Type", "IsASILoader", "False", file), false);
                                    bool isScriptHook = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Type", "IsScriptHook", "False", file), false);
                                    bool isScriptHookHook = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Type", "IsScriptHookHook", "False", file), false);
                                    bool isScriptHookDotNet = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Type", "IsScriptHookDotNet", "False", file), false);

                                    // Requirements
                                    bool requiresASILoader = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Requirements", "RequiresASILoader", "False", file), false);
                                    bool requiresScriptHook = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Requirements", "RequiresScriptHook", "False", file), false);
                                    bool requiresScriptHookDotNet = Helper.ParseExtension.Parse(Ini.ReadValueFromFile("Requirements", "RequiresScriptHookDotNet", "False", file), false);

                                    // Create new mod item
                                    ModItem item = new ModItem(mFilename, mName, mDesc, checkedByDefault);
                                    item.Margin = new Thickness(5, 5, 5, 0);
                                    item.ModVersion = mVersion;
                                    item.IsASILoader = isASILoader;
                                    item.IsScriptHook = isScriptHook;
                                    item.IsScriptHookHook = isScriptHookHook;
                                    item.IsScriptHookDotNet = isScriptHookDotNet;
                                    item.RequiresASILoader = requiresASILoader;
                                    item.RequiresScriptHook = requiresScriptHook;
                                    item.RequiresScriptHookDotNet = requiresScriptHookDotNet;
                                    item.CheckedChanged += Item_CheckedChanged;

                                    if ((int)instance.s2_SelectDwngrdVersion.selectedVersion == (int)mVersion) {
                                        // Add item to list of mods
                                        modItems.Add(item);
                                    }
                                    else if (mVersion == ModVersion.All) {
                                        // Add item to list of mods
                                        modItems.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }

                // Sort items and add them to the mod list
                if (modItems.Count != 0) {
                    modItems = modItems.OrderBy(q => q.ModName).ToList();

                    for (int i = 0; i < modItems.Count; i++) {
                        if (i == modItems.Count - 1) {
                            ModItem item = modItems[i];
                            item.Margin = new Thickness(5);
                            ModListStackPanel.Children.Add(item);
                        }
                        else {
                            ModListStackPanel.Children.Add(modItems[i]);
                        }
                    }

                    SelectAllButton.IsEnabled = true;
                    DeselectAllButton.IsEnabled = true;
                }
            }
            catch (Exception ex) {
                instance.ChangeStep(Steps.Error, new List<object>() { ex });
            }
        }
        private void Item_CheckedChanged(ModItem sender, bool newState)
        {
            CreateModCheck();

            if (modCheck != null) {

                bool areAnyErrorsAvailable = false;
                errorText = new StringBuilder();
                errorText.AppendLine("The following issues need to be fixed so that we can continue.");
                errorText.AppendLine();

                if (modCheck.ASIModsSelected >= 1 || modCheck.ASILoadersSelected > 1) {
                    if (modCheck.ASILoadersSelected == 0) {
                        errorText.AppendLine("- - - ASI Loader - - -");
                        if (modCheck.ASIModsSelected > 1) {
                            errorText.AppendLine(string.Format("You've selected {0} ASI Mods but you haven't selected any ASI Loader. Please select an ASI Loader from the mod list. We recommend xlive.", modCheck.ASIModsSelected.ToString()));
                            errorText.AppendLine();
                            errorText.Append("Selected ASI Mods: ");
                        }
                        else {
                            errorText.AppendLine(string.Format("You've selected {0} ASI Mod but you haven't selected any ASI Loader. Please select an ASI Loader from the mod list. We recommend xlive.", modCheck.ASIModsSelected.ToString()));
                            errorText.AppendLine();
                            errorText.Append("Selected ASI Mod: ");
                        }
                        for (int i = 0; i < modCheck.ASIModsSelectedNames.Count; i++) {
                            if (i == (modCheck.ASIModsSelectedNames.Count - 1)) {
                                errorText.Append(modCheck.ASIModsSelectedNames[i]);
                            }
                            else {
                                errorText.Append(modCheck.ASIModsSelectedNames[i] + ", ");
                            }
                        }
                        errorText.AppendLine();
                        areAnyErrorsAvailable = true;
                    }
                    else if (modCheck.ASILoadersSelected > 1) {
                        errorText.AppendLine("- - - ASI Loader - - -");
                        errorText.AppendLine(string.Format("You've selected {0} ASI Loaders but you only need one. We recommend xlive.", modCheck.ASILoadersSelected.ToString()));
                        errorText.AppendLine();
                        areAnyErrorsAvailable = true;
                    }
                }

                if (modCheck.ScriptHookModsSelected >= 1) {
                    // Empty for now...
                }

                if (modCheck.ScriptHookDotNetModsSelected >= 1) {
                    // Empty for now...
                }

                if (areAnyErrorsAvailable) {
                    ProblemsButton.Visibility = Visibility.Visible;
                    NextButton.IsEnabled = false;
                }
                else {
                    ProblemsButton.Visibility = Visibility.Hidden;
                    NextButton.IsEnabled = true;
                }
            }
        }

        public void SelectPreviousSelectedMods()
        {
            try {
                if (selectedMods.Count != 0) {
                    for (int s = 0; s < selectedMods.Count; s++) {
                        ModItem modItem = selectedMods[s];
                        for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                            ModItem modItemInList = (ModItem)ModListStackPanel.Children[i];
                            if (modItem.IsChecked) {
                                if (modItem.ModName.Equals(modItemInList.ModName)) {
                                    modItemInList.IsChecked = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public void ChangeCheckstateForSpecificModInList(string name, bool checkState, bool applyForOnlyOneMod = true)
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                ModItem modItem = (ModItem)ModListStackPanel.Children[i];
                if (modItem.ModName.ToLower().Equals(name.ToLower())) {
                    modItem.IsChecked = checkState;
                    if (applyForOnlyOneMod) break;
                }
            }
        }
        public void ChangeCheckstateForSpecificModInList(ModType type, bool checkState, bool applyForOnlyOneMod = false)
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                ModItem modItem = (ModItem)ModListStackPanel.Children[i];
                switch (type) {
                    case ModType.ASILoader:
                        if (modItem.IsASILoader) {
                            modItem.IsChecked = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ASIMod:
                        if (modItem.RequiresASILoader) {
                            modItem.IsChecked = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHook:
                        if (modItem.IsScriptHook) {
                            modItem.IsChecked = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookMod:
                        if (modItem.RequiresScriptHook) {
                            modItem.IsChecked = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookHook:
                        if (modItem.IsScriptHookHook) {
                            modItem.IsChecked = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookDotNet:
                        if (modItem.IsScriptHookDotNet) {
                            modItem.IsChecked = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookDotNetMod:
                        if (modItem.RequiresScriptHookDotNet) {
                            modItem.IsChecked = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                }
            }
        }
        public void ChangeEnableStateForSpecificModInList(ModType type, bool checkState, bool applyForOnlyOneMod = false)
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                ModItem modItem = (ModItem)ModListStackPanel.Children[i];
                switch (type) {
                    case ModType.ASILoader:
                        if (modItem.IsASILoader) {
                            modItem.IsEnabled = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ASIMod:
                        if (modItem.RequiresASILoader) {
                            modItem.IsEnabled = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHook:
                        if (modItem.IsScriptHook) {
                            modItem.IsEnabled = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookMod:
                        if (modItem.RequiresScriptHook) {
                            modItem.IsEnabled = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookHook:
                        if (modItem.IsScriptHookHook) {
                            modItem.IsEnabled = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookDotNet:
                        if (modItem.IsScriptHookDotNet) {
                            modItem.IsEnabled = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                    case ModType.ScriptHookDotNetMod:
                        if (modItem.RequiresScriptHookDotNet) {
                            modItem.IsEnabled = checkState;
                            if (applyForOnlyOneMod) break;
                        }
                        break;
                }
            }
        }
        public void SelectAllModsInList()
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsEnabled) item.IsChecked = true;
            }
        }
        public void DeselectAllModsInList()
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsEnabled) item.IsChecked = false;
            }
        }

        public void CreateModCheck()
        {
            modCheck = new ModCheck();
            for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsChecked) {
                    if (item.IsASILoader)
                        modCheck.ASILoadersSelected++;
                    if (item.IsScriptHook)
                        modCheck.ScriptHookSelected = true;
                    if (item.IsScriptHookHook)
                        modCheck.ScriptHookHookSelected = true;
                    if (item.IsScriptHookDotNet)
                        modCheck.ScriptHookDotNetSelected = true;

                    if (item.RequiresASILoader) {
                        modCheck.ASIModsSelected++;
                        modCheck.ASIModsSelectedNames.Add(item.ModName);
                    }
                    if (item.RequiresScriptHook) {
                        modCheck.ScriptHookModsSelected++;
                        modCheck.ScriptHookModsSelectedNames.Add(item.ModName);
                    }
                    if (item.RequiresScriptHookDotNet) {
                        modCheck.ScriptHookDotNetModsSelected++;
                        modCheck.ScriptHookDotNetModsSelectedNames.Add(item.ModName);
                    }   
                }
            }
        }
        #endregion

        #region Constructor
        public S4_SelectComponents()
        {
            InitializeComponent();
            modItems = new List<ModItem>();
            selectedMods = new List<ModItem>();
        }
        public S4_SelectComponents(MainWindow window)
        {
            instance = window;
            InitializeComponent();
            modItems = new List<ModItem>();
            selectedMods = new List<ModItem>();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMods();
            SelectPreviousSelectedMods();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Add selected mods to the selected mods list
            if (modItems.Count != 0) {
                selectedMods.Clear();
                for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                    ModItem item = (ModItem)ModListStackPanel.Children[i];
                    if (item.IsChecked) {
                        selectedMods.Add(item);
                    }
                }
            }

            instance.PreviousStep(1);
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Add selected mods to the selected mods list
            if (modItems.Count != 0) {
                selectedMods.Clear();
                for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                    ModItem item = (ModItem)ModListStackPanel.Children[i];
                    if (item.IsChecked) {
                        selectedMods.Add(item);
                    }
                }
            }
            
            instance.NextStep();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAllModsInList();
        }
        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllModsInList();
        }

        private void ProblemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (errorText != null) MessageBox.Show(errorText.ToString(), "Problems", MessageBoxButton.OK);
        }
    }
}
