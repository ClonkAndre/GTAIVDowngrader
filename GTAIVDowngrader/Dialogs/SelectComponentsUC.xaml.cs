using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;
using GTAIVDowngrader.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class SelectComponentsUC : UserControl {

        #region Variables
        private MainWindow instance;

        private List<JsonObjects.ModInformation> allMods;

        private ModCheck modCheck;
        private StringBuilder errorText;
        #endregion

        #region Classes
        public class ModCheck {

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
        private void ChangeLoadingPageState(bool visible, string text, bool progressBarVisible = true)
        {
            if (visible) {
                StatusLabel.Text = text;
                if (progressBarVisible) {
                    DownloadStatusProgressBar.Visibility = Visibility.Visible;
                }
                else {
                    DownloadStatusProgressBar.Visibility = Visibility.Collapsed;
                }
                StatusGrid.Visibility = Visibility.Visible;
            }
            else {
                StatusGrid.Visibility = Visibility.Collapsed;
            }
        }

        public void Clear()
        {
            ModListStackPanel.Children.Clear();
            allMods.Clear();
            SelectAllButton.IsEnabled = false;
            DeselectAllButton.IsEnabled = false;
            modCheck = null;
        }

        public void RetrieveMods()
        {
            try {
                Clear();
                ChangeLoadingPageState(true, "Retrieving all mods");
                MainFunctions.downloadWebClient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/main/v1.7_and_up/modInfos.json"));
            }
            catch (Exception ex) {
                ChangeLoadingPageState(true, string.Format("An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, ex.Message), false);
            }
        }
        public void AddModsToContainer()
        {
            Dispatcher.Invoke(() => {

                // Add mods to container
                for (int i = 0; i < allMods.Count; i++) {
                    JsonObjects.ModInformation mod = allMods[i];

                    // Check if mod can be added to container
                    if (!mod.ShowInDowngrader)
                        continue;

                    if (MainFunctions.downgradingInfo.ConfigureForGFWL) {
                        if (!mod.CompatibleWithGFWL) continue;
                    }
                    else {
                        if (mod.Title == "dsound") continue;
                        if (mod.Title.Contains("ZolikaPatch") && mod.Title.Contains("GFWL")) continue;
                        if (mod.Title.Contains("ZMenuIV") && mod.Title.Contains("GFWL")) continue;
                    }

                    if (!mod.ForVersions.Contains(ModVersion.All)) {
                        if (!mod.ForVersions.Contains((ModVersion)(int)MainFunctions.downgradingInfo.DowngradeTo))
                            continue;
                    }

                    // Add mod item
                    ModItem modItem = new ModItem(mod);
                    modItem.CheckedChanged += ModItem_CheckedChanged;
                    if (i != allMods.Count - 1)
                        modItem.Margin = new Thickness(3, 3, 3, 0);
                    else 
                        modItem.Margin = new Thickness(3, 3, 3, 3);

                    ModListStackPanel.Children.Add(modItem);
                }

                // Re-enable buttons
                SelectAllButton.IsEnabled = true;
                DeselectAllButton.IsEnabled = true;

            });
        }
        private void ModItem_CheckedChanged(ModItem sender, bool newState)
        {
            CreateModCheck();
            DoModCheck();
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
                    if (item.ModInfo.IsASILoader)
                        modCheck.ASILoadersSelected++;
                    if (item.ModInfo.IsScriptHook)
                        modCheck.ScriptHookSelected = true;
                    if (item.ModInfo.IsScriptHookHook)
                        modCheck.ScriptHookHookSelected = true;
                    if (item.ModInfo.IsScriptHookDotNet)
                        modCheck.ScriptHookDotNetSelected = true;

                    if (item.ModInfo.RequiresASILoader) {
                        modCheck.ASIModsSelected++;
                        modCheck.ASIModsSelectedNames.Add(item.ModInfo.Title);
                    }
                    if (item.ModInfo.RequiresScriptHook) {
                        modCheck.ScriptHookModsSelected++;
                        modCheck.ScriptHookModsSelectedNames.Add(item.ModInfo.Title);
                    }
                    if (item.ModInfo.RequiresScriptHookDotNet) {
                        modCheck.ScriptHookDotNetModsSelected++;
                        modCheck.ScriptHookDotNetModsSelectedNames.Add(item.ModInfo.Title);
                    }   
                }
            }
        }
        public void DoModCheck()
        {
            if (modCheck != null) {

                bool areAnyErrorsAvailable = false;
                errorText = new StringBuilder();
                errorText.AppendLine("The following issues need to be fixed so that we can continue.");
                errorText.AppendLine();

                if (modCheck.ASIModsSelected >= 1 || modCheck.ASILoadersSelected > 1) {
                    if (modCheck.ASILoadersSelected == 0) {
                        errorText.AppendLine("- - - ASI Loader - - -");
                        if (modCheck.ASIModsSelected > 1) {
                            errorText.AppendLine(string.Format("You've selected {0} ASI Mods but you haven't selected any ASI Loader. Please select an ASI Loader from the mod list.", modCheck.ASIModsSelected.ToString()));
                            errorText.AppendLine();
                            errorText.Append("Selected ASI Mods: ");
                        }
                        else {
                            errorText.AppendLine(string.Format("You've selected {0} ASI Mod but you haven't selected any ASI Loader. Please select an ASI Loader from the mod list.", modCheck.ASIModsSelected.ToString()));
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
                        errorText.AppendLine(string.Format("You've selected {0} ASI Loaders but you only need one.", modCheck.ASILoadersSelected.ToString()));
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
        #endregion

        #region Constructor
        public SelectComponentsUC(MainWindow window)
        {
            instance = window;
            allMods = new List<JsonObjects.ModInformation>();
            InitializeComponent();
        }
        public SelectComponentsUC()
        {
            allMods = new List<JsonObjects.ModInformation>();
            InitializeComponent();
        }
        #endregion

        #region Events
        private void DownloadWebClient_DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            try {
                if (e.Cancelled)
                    return;

                if (e.Error == null) {
                    string result = e.Result;
                    if (!string.IsNullOrWhiteSpace(result)) {

                        Console.WriteLine("- - - Mods - - -");

                        allMods = JsonConvert.DeserializeObject<List<JsonObjects.ModInformation>>(result);
                        if (allMods.Count != 0) {
                            allMods.ForEach(m => Console.WriteLine(m.ToString())); // Print to console
                            AddModsToContainer(); // Add all mods to mods container
                        }

                        ChangeLoadingPageState(false, "");
                    }
                    else {
                        ChangeLoadingPageState(true, "An unknown error occured while trying to retrieve all mods.", false);
                    }
                }
                else {
                    ChangeLoadingPageState(true, string.Format("(1) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, e.Error.Message), false);
                }
            }
            catch (Exception ex) {
                ChangeLoadingPageState(true, string.Format("(2) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, ex.Message), false);
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainFunctions.downloadWebClient.DownloadStringCompleted -= DownloadWebClient_DownloadStringCompleted;
            Clear();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // BottomGrid Colours
            if (MainFunctions.isPrideMonth) {
                if (MainFunctions.wantsToDisableRainbowColours) { // Revert to default Colour
                    BottomGrid.Background = "#B3000000".ToBrush();
                }
                else { // Use Rainbow Colours
                    BottomGrid.Background = MainFunctions.GetRainbowGradientBrush();
                }
            }

            // Remove previously selected mods so they aren't twice in the list
            MainFunctions.downgradingInfo.SelectedMods.Clear();

            // Download stuff
            MainFunctions.downloadWebClient.DownloadStringCompleted += DownloadWebClient_DownloadStringCompleted;
            RetrieveMods();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAllModsInList();
        }
        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllModsInList();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RetrieveMods();
        }
        private void ProblemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (errorText != null) MessageBox.Show(errorText.ToString(), "Problems", MessageBoxButton.OK);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFunctions.downgradingInfo.DowngradeTo == GameVersion.v1040) {
                instance.PreviousStep(3);
            }
            else {
                instance.PreviousStep(1);
            }
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Add all checked mods to list
            for (int i = 0; i < ModListStackPanel.Children.Count; i++) {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsChecked) MainFunctions.downgradingInfo.SelectedMods.Add(item.ModInfo);
            }

            // Set if user wants to install prerequisites
            MainFunctions.downgradingInfo.SetInstallPrerequisites(InstallPrerequisitesCheckBox.IsChecked.Value);

            // Final step
            instance.NextStep();
        }

    }
}
