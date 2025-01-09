using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;
using CCL;

using GTAIVDowngrader.Controls;
using GTAIVDowngrader.Classes;
using GTAIVDowngrader.Classes.Json.Modification;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SelectComponentsUC : UserControl
    {

        #region Variables
        private MainWindow instance;

        private WebClient downloadWebClient;
        private List<ModDetails> allMods;

        private StringBuilder modCheckResultBuilder;
        #endregion

        #region Methods
        private void ChangeLoadingPageState(bool visible, string text, bool progressBarVisible = true)
        {
            if (visible)
            {
                StatusLabel.Text = text;

                if (progressBarVisible)
                    DownloadStatusProgressBar.Visibility = Visibility.Visible;
                else
                    DownloadStatusProgressBar.Visibility = Visibility.Collapsed;

                StatusGrid.Visibility = Visibility.Visible;
            }
            else
            {
                StatusGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void Clear()
        {
            if (modCheckResultBuilder != null)
            {
                modCheckResultBuilder.Clear();
                modCheckResultBuilder = null;
            }

            FilterTextBox.Clear();
            ModListStackPanel.Children.Clear();
            allMods.Clear();
            SelectAllButton.IsEnabled = false;
            DeselectAllButton.IsEnabled = false;
        }

        private void RetrieveMods()
        {
            try
            {
                Clear();
                ChangeLoadingPageState(true, "Retrieving all mods");

                // Check offline mode state
                if (Core.IsInOfflineMode)
                {
                    ChangeLoadingPageState(true, string.Format("Could not retrieve all mods.{0}" +
                        "The downgrader is running in offline mode, and is therefor not allowed to download anything from the internet.{0}" +
                        "Please download the mods that you want manually after the downgrade.", Environment.NewLine), false);

                    return;
                }

                // The download link for downloading the mod stuff
                downloadWebClient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/refs/heads/v2.2/modDetails.json"));
            }
            catch (Exception ex)
            {
                ChangeLoadingPageState(true, string.Format("An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, ex.Message), false);
            }
        }
        private void AddModsToContainer()
        {
            Dispatcher.Invoke(() =>
            {

                // Sort mods by title
                allMods = allMods.OrderBy(x => x.Title).ToList();

                // Add mods to container
                for (int i = 0; i < allMods.Count; i++)
                {
                    ModDetails mod = allMods[i];

                    // Check if mod can be added to container
                    if (!mod.ShowInDowngrader)
                        continue;

                    if (DowngradingInfo.ConfigureForGFWL)
                    {
                        // Do not show mods which are NOT compatible with GFWL
                        if (!mod.CompatibleWithGFWL)
                            continue;
                    }

                    // Skip if mod was already added
                    if (WasModWithUniqueNameAlreadyAdded(mod.UniqueName))
                        continue;

                    // Check if mod is compatible with selected downgrading version
                    if (mod.ForGameVersion.Count != 0)
                    {
                        if (!mod.ForGameVersion.Contains(DowngradingInfo.DowngradeTo))
                            continue;
                    }

                    // Add mod item
                    ModItem modItem = new ModItem(mod);
                    modItem.CheckedChanged += ModItem_CheckedChanged;

                    if (i != allMods.Count - 1)
                        modItem.Margin = new Thickness(3, 3, 3, 3);
                    else 
                        modItem.Margin = new Thickness(3, 3, 3, 3);

                    ModListStackPanel.Children.Add(modItem);
                }

                // Re-enable buttons
                SelectAllButton.IsEnabled = true;
                DeselectAllButton.IsEnabled = true;

                // Calculate size of already checked mods
                CalculateModsSize();

            });
        }
        private void ModItem_CheckedChanged(ModItem sender, bool newState)
        {
            DoModCheck();
            CalculateModsSize();
        }

        private void SelectAllModsInList()
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsEnabled)
                    item.IsChecked = true;
            }
        }
        private void DeselectAllModsInList()
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsEnabled)
                    item.IsChecked = false;
            }
        }

        private void AppendNewLineToModCheck()
        {
            if (modCheckResultBuilder == null)
                modCheckResultBuilder = new StringBuilder();

            modCheckResultBuilder.AppendLine();
        }
        private void AppendStringToModCheck(string str, params object[] args)
        {
            if (modCheckResultBuilder == null)
                modCheckResultBuilder = new StringBuilder();

            modCheckResultBuilder.AppendLine(string.Format(str, args));
        }
        private void AppendBooleanToModCheck(string str, bool value)
        {
            AppendStringToModCheck(str, value ? "Yes" : "No");
        }
        private void DoModCheck()
        {
            if (modCheckResultBuilder != null)
                modCheckResultBuilder.Clear();

            // Get the selected mods
            List<ModDetails> selectedMods = GetSelectedMods();

            if (selectedMods.Count == 0)
            {
                ProblemsButton.Visibility = Visibility.Hidden;
                instance.ChangeActionButtonEnabledState(true, true, true, true);
                return;
            }

            // Check the selected ASI Loaders, Mod Loaders and Dependencies
            int selectedAsiLoaders =        selectedMods.Count(x => x.IsASILoader);
            bool selectedScriptHook =       selectedMods.Any(x => x.IsScriptHook);
            bool selectedScriptHookDotNet = selectedMods.Any(x => x.IsScriptHookDotNet);
            bool selectedIVSDKDotNet =      selectedMods.Any(x => x.IsIVSDKDotNet);

            AppendStringToModCheck("- - - Mod Check - - -");
            AppendStringToModCheck("ASI Loaders selected: {0}",         selectedAsiLoaders);
            AppendBooleanToModCheck("ScriptHook selected: {0}",         selectedScriptHook);
            AppendBooleanToModCheck("ScriptHookDotNet selected: {0}",   selectedScriptHookDotNet);
            AppendBooleanToModCheck("IV-SDK .NET selected: {0}",        selectedIVSDKDotNet);

            AppendNewLineToModCheck();

            // Check the selected ASI Mods
            bool wasThereAnyAsiModProblem = false;
            List<ModDetails> selectedAsiMods = selectedMods.Where(x => x.HasASIModDetails()).ToList();

            AppendStringToModCheck("- - - ASI Mod Check Result - - -");

            // Did the user select any ASI Mods?
            if (selectedAsiMods.Count > 0)
            {
                // Did the user select an ASI Loader?
                if (selectedAsiLoaders > 0)
                {
                    // Yes, now check the possible dependencies
                    selectedAsiMods.ForEach(x =>
                    {
                        if (x.ASIModDetails.ForScriptHook && !selectedScriptHook)
                        {
                            // Let user know about the missing dependency
                            AppendStringToModCheck("- {0} requires ScriptHook to work!", x.Title);

                            wasThereAnyAsiModProblem = true;
                        }
                    });
                }
                else
                {
                    // No, let user know about it
                    AppendStringToModCheck("You've selected {0} ASI Mod(s) but no ASI Loader! " +
                        "Please select atleast one ASI Loader from the list to fix this issue.", selectedAsiMods.Count);

                    wasThereAnyAsiModProblem = true;
                }
            }
            else
            {
                wasThereAnyAsiModProblem = false;
            }

            if (!wasThereAnyAsiModProblem)
                AppendStringToModCheck("No problems detected.");

            // Check the selected .NET Mods
            bool wasThereAnyDotNetModProblem = false;
            List<ModDetails> selectedDotNetMods = selectedMods.Where(x => x.HasDotNetModDetails()).ToList();

            AppendNewLineToModCheck();
            AppendStringToModCheck("- - - .NET Mod Check Result - - -");

            // Check the possible dependencies
            selectedDotNetMods.ForEach(x =>
            {
                if (x.DotNetModDetails.ForScriptHookDotNet && !selectedScriptHookDotNet)
                {
                    // Let user know about the missing dependency
                    AppendStringToModCheck("- {0} requires ScriptHookDotNet to work!", x.Title);

                    wasThereAnyDotNetModProblem = true;
                }
                else if (x.DotNetModDetails.ForIVSDKDotNet && !selectedIVSDKDotNet)
                {
                    // Let user know about the missing dependency
                    AppendStringToModCheck("- {0} requires IV-SDK .NET to work!", x.Title);

                    wasThereAnyDotNetModProblem = true;
                }
            });

            if (!wasThereAnyDotNetModProblem)
                AppendStringToModCheck("No problems detected.");

            // Was there any problem?
            if (wasThereAnyAsiModProblem || wasThereAnyDotNetModProblem)
            {
                ProblemsButton.Visibility = Visibility.Visible;
                instance.ChangeActionButtonEnabledState(true, true, true, false);
            }
            else
            {
                ProblemsButton.Visibility = Visibility.Hidden;
                instance.ChangeActionButtonEnabledState(true, true, true, true);
            }
        }

        private void CalculateModsSize()
        {
            long size = 0;
            int selectedMods = 0;

            // Mods
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];

                if (item.IsChecked)
                {
                    selectedMods++;
                    size += item.ModInfo.FileDetails.SizeInBytes;

                    if (item.ModInfo.OptionalComponents == null)
                        continue;

                    for (int o = 0; o < item.OptionalsWrapPanel.Children.Count; o++)
                    {
                        CheckBox cBox = item.OptionalsWrapPanel.Children[o] as CheckBox;

                        if (cBox.IsChecked.Value)
                            size += ((OptionalComponentInfo)cBox.Tag).FileDetails.SizeInBytes;
                    }
                }
            }

            SelectedModsInfoLabel.Text = string.Format("Selected {0} mod(s). Total Size: {1}", selectedMods, FileHelper.GetExactFileSizeAdvanced(size));
        }
        #endregion

        #region Functions
        private bool WasModWithUniqueNameAlreadyAdded(string uniqueName)
        {
            if (string.IsNullOrWhiteSpace(uniqueName))
                return false;

            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];

                if (item.ModInfo.UniqueName == uniqueName)
                    return true;
            }

            return false;
        }
        private List<ModDetails> GetSelectedMods()
        {
            List<ModDetails> selectedMods = new List<ModDetails>();

            // Add all checked mods to list
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];

                if (item.IsChecked)
                    selectedMods.Add(item.ModInfo);
            }

            return selectedMods;
        }
        private List<OptionalComponentInfo> GetSelectedOptionalComponents()
        {
            List<OptionalComponentInfo> selectedOptionalComponents = new List<OptionalComponentInfo>();

            // Add all checked mods to list
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];

                if (item.IsChecked)
                {
                    if (item.ModInfo.OptionalComponents == null)
                        continue;

                    for (int o = 0; o < item.OptionalsWrapPanel.Children.Count; o++)
                    {
                        CheckBox componentCheckBox = item.OptionalsWrapPanel.Children[o] as CheckBox;

                        if (componentCheckBox.IsChecked.Value)
                            selectedOptionalComponents.Add((OptionalComponentInfo)componentCheckBox.Tag);
                    }
                }
            }

            return selectedOptionalComponents;
        }
        #endregion

        #region Constructor
        public SelectComponentsUC(MainWindow window)
        {
            instance = window;
            allMods = new List<ModDetails>();
            InitializeComponent();
        }
        public SelectComponentsUC()
        {
            allMods = new List<ModDetails>();
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            if (DowngradingInfo.DowngradeTo == "1040")
                instance.PreviousStep(3);
            else
                instance.PreviousStep(1);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            // Add all checked mods to list
            DowngradingInfo.SelectedMods.Clear();
            DowngradingInfo.AddSelectedMods(GetSelectedMods());

            // Add all optional mod components to list
            DowngradingInfo.SelectedOptionalComponents.Clear();
            DowngradingInfo.AddSelectedOptionalComponents(GetSelectedOptionalComponents());

            // Set if user wants to install prerequisites
            DowngradingInfo.SetInstallPrerequisites(InstallPrerequisitesCheckBox.IsChecked.Value);

            // Final step
            if (allMods.Count == 0)
            {
                instance.ShowMessageDialogScreen("Important Note",
                    string.Format("It is HIGHLY recommended to install the Ultimate ASI Loader and ZolikaPatch to your game, as otherwise your game probably won't launch.{0}" +
                    "To download the Ultimate ASI Loader and ZolikaPatch, just click on the buttons below.{0}" +
                    "We can't guarantee for a stable and functional downgrade if those 2 components aren't installed.", Environment.NewLine),
                    Steps.S9_Confirm,
                    null,
                    "Ultimate ASI Loader",
                    () => Web.AskUserToGoToURL(new Uri("https://github.com/ThirteenAG/Ultimate-ASI-Loader/releases")),
                    "ZolikaPatch",
                    () => Web.AskUserToGoToURL(new Uri("https://gtaforums.com/topic/955449-iv-zolikapatch/")),
                    () => // Next button override
                    {
                        if (Core.IsInSimpleMode)
                        {
                            Core.LogDowngradingInfos();
                            instance.NextStep(1);
                        }
                        else
                        {
                            instance.NextStep();
                        }
                    });
            }
            else
            {
                if (Core.IsInSimpleMode)
                {
                    Core.LogDowngradingInfos();
                    instance.NextStep(1);
                }
                else
                {
                    instance.NextStep();
                }
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                UIElement element = ModListStackPanel.Children[i];

                if (string.IsNullOrWhiteSpace(FilterTextBox.Text))
                {
                    element.Visibility = Visibility.Visible;
                }
                else
                {
                    ModItem item = (ModItem)element;

                    if (item.Title.ToLower().Contains(FilterTextBox.Text.ToLower()))
                    {
                        element.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        element.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void DownloadWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                    return;

                if (e.Error != null)
                {
                    ChangeLoadingPageState(true, string.Format("(1) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, e.Error.Message), false);
                    return;
                }

                // Get the result and do validation
                string result = e.Result;

                if (string.IsNullOrWhiteSpace(result))
                {
                    ChangeLoadingPageState(true, "An unknown error occured while trying to retrieve all mods.", false);
                    return;
                }

                // Parse mods
                allMods = JsonConvert.DeserializeObject<List<ModDetails>>(result);

#if DEBUG
                // Debug
                Console.WriteLine("- - - Mods - - -");
                allMods.ForEach(m => Console.WriteLine(m.ToString())); // Print to console
#endif

                if (allMods.Count != 0)
                    AddModsToContainer(); // Add all mods to mods container

                ChangeLoadingPageState(false, "");
            }
            catch (Exception ex)
            {
                ChangeLoadingPageState(true, string.Format("(2) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, ex.Message), false);
            }
        }
        private void DownloadWebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                    return;

                if (e.Error != null)
                {
                    ChangeLoadingPageState(true, string.Format("(2) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, e.Error.Message), false);
                    return;
                }

                // Read file
                string filePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\modInfos.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

                if (!File.Exists(filePath))
                {
                    ChangeLoadingPageState(true, string.Format("(1) An error occured while trying to retrieve all mods."), false);
                    return;
                }

                // Parse mods
                allMods = JsonConvert.DeserializeObject<List<ModDetails>>(File.ReadAllText(filePath));

#if DEBUG
                // Debug
                Console.WriteLine("- - - Mods - - -");
                allMods.ForEach(m => Console.WriteLine(m.ToString())); // Print to console
#endif

                if (allMods.Count != 0)
                    AddModsToContainer(); // Add all mods to mods container

                ChangeLoadingPageState(false, "");
            }
            catch (Exception ex)
            {
                ChangeLoadingPageState(true, string.Format("(3) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, ex.Message), false);
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;

            // Destroy WebClient
            downloadWebClient.DownloadStringCompleted -= DownloadWebClient_DownloadStringCompleted;
            downloadWebClient.DownloadFileCompleted -= DownloadWebClient_DownloadFileCompleted;
            downloadWebClient.CancelAsync();
            downloadWebClient.Dispose();
            downloadWebClient = null;

            Clear();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Core.IsInSimpleMode)
            {
                instance.NextButton.Content = "Downgrade";
                NextInstruction.Text = "To downgrade, press the 'Downgrade' button.";
            }

            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            // Init WebClient
            downloadWebClient = new WebClient();
            downloadWebClient.DownloadStringCompleted += DownloadWebClient_DownloadStringCompleted;
            downloadWebClient.DownloadFileCompleted += DownloadWebClient_DownloadFileCompleted;

            // Set ToolTip for InstallPrerequisites CheckBox
            if (DowngradingInfo.ConfigureForGFWL)
            {
                InstallPrerequisitesCheckBox.ToolTip = string.Format("This will download and install DirectX June 2010 SDK and the all-in-one Visual C++ 2005-2022 Redistributable.{0}" +
                    "These are required for GTA IV and some mods.{0}" +
                    "It will also install the Prerequisites required by GFWL because you checked the 'Configure this downgrade for GFWL' checkbox earlier.{0}" +
                    "You can uncheck this if you are sure that you already have everything.", Environment.NewLine);
            }
            else
            {
                InstallPrerequisitesCheckBox.ToolTip = string.Format("This will download and install DirectX June 2010 SDK and the all-in-one Visual C++ 2005-2022 Redistributable.{0}" +
                    "These are required for GTA IV and some mods.{0}" +
                    "You can uncheck this if you are sure that you already have those.", Environment.NewLine);
            }

            // Remove previously selected mods and optionals so they aren't twice in the list
            DowngradingInfo.SelectedMods.Clear();
            DowngradingInfo.SelectedOptionalComponents.Clear();

            // Download stuff
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
            if (modCheckResultBuilder != null)
                MessageBox.Show(modCheckResultBuilder.ToString(), "Problems", MessageBoxButton.OK);
        }

    }
}
