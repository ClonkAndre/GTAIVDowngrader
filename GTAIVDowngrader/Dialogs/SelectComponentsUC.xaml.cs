using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Newtonsoft.Json;

using GTAIVDowngrader.Controls;
using CCL;

namespace GTAIVDowngrader.Dialogs
{
    public partial class SelectComponentsUC : UserControl
    {

        #region Variables
        private MainWindow instance;

        private WebClient downloadWebClient;
        private List<JsonObjects.ModInformation> allMods;

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

            public bool IVSDKDotNetSelected;
            public int IVSDKDotNetModsSelected;
            public List<string> IVSDKDotNetModsSelectedNames;
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

                IVSDKDotNetSelected = false;
                IVSDKDotNetModsSelected = 0;
                IVSDKDotNetModsSelectedNames = new List<string>();
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
            try
            {
                Clear();
                ChangeLoadingPageState(true, "Retrieving all mods");

                if (!Core.IsInOfflineMode)
                {

                    string dLink = "https://raw.githubusercontent.com/ClonkAndre/GTAIVDowngraderOnline_Files/main/v2.0_and_up/modInfos.json";

                    if (Core.UseAlternativeDownloadLinks)
                    {
                        dLink = "ftp://195.201.179.80/IVDowngraderFiles/modInfos.json";
                        Core.AddLogItem(LogType.Warning, "Using alternative download link for modInfos.json! Mods might be outdated!");
                        Core.Notification.ShowNotification(NotificationType.Warning, 10100, "Outdated Mods Information", string.Format("The downgrader used an alternative download link to the list of all mods, this list might be outdated.{0}" +
                            "Please make sure to manually update your mods after the downgrade.", Environment.NewLine));

                        string filePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\modInfos.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                        downloadWebClient.DownloadFileAsync(new Uri(dLink), filePath);
                    }
                    else
                    {
                        downloadWebClient.DownloadStringAsync(new Uri(dLink));
                    }

                }
                else
                    ChangeLoadingPageState(true, string.Format("Could not retrieve all mods.{0}" +
                        "The downgrader is running in offline mode.{0}" +
                        "Please download the mods that you want manually after the downgrade.", Environment.NewLine), false);
            }
            catch (Exception ex)
            {
                ChangeLoadingPageState(true, string.Format("An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, ex.Message), false);
            }
        }
        public void AddModsToContainer()
        {
            Dispatcher.Invoke(() => {

                // Add mods to container
                for (int i = 0; i < allMods.Count; i++)
                {
                    JsonObjects.ModInformation mod = allMods[i];

                    // Check if mod can be added to container
                    if (!mod.ShowInDowngrader)
                        continue;

                    if (Core.CDowngradingInfo.ConfigureForGFWL)
                    {
                        if (!mod.CompatibleWithGFWL)
                            continue;
                    }
                    else
                    {
                        if (mod.Title == "dsound")
                            continue;
                        if (mod.Title.Contains("ZolikaPatch") && mod.Title.Contains("GFWL"))
                            continue;
                        if (mod.Title.Contains("ZMenuIV") && mod.Title.Contains("GFWL"))
                            continue;
                    }

                    if (!mod.ForVersions.Contains(ModVersion.All))
                    {
                        if (!mod.ForVersions.Contains((ModVersion)(int)Core.CDowngradingInfo.DowngradeTo))
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
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsChecked)
                {
                    if (item.ModInfo.IsASILoader)
                        modCheck.ASILoadersSelected++;
                    if (item.ModInfo.IsScriptHook)
                        modCheck.ScriptHookSelected = true;
                    if (item.ModInfo.IsScriptHookHook)
                        modCheck.ScriptHookHookSelected = true;
                    if (item.ModInfo.IsScriptHookDotNet)
                        modCheck.ScriptHookDotNetSelected = true;
                    if (item.ModInfo.IsIVSDKDotNet)
                        modCheck.IVSDKDotNetSelected = true;

                    if (item.ModInfo.RequiresASILoader)
                    {
                        modCheck.ASIModsSelected++;
                        modCheck.ASIModsSelectedNames.Add(item.ModInfo.Title);
                    }
                    if (item.ModInfo.RequiresScriptHook)
                    {
                        modCheck.ScriptHookModsSelected++;
                        modCheck.ScriptHookModsSelectedNames.Add(item.ModInfo.Title);
                    }
                    if (item.ModInfo.RequiresScriptHookDotNet)
                    {
                        modCheck.ScriptHookDotNetModsSelected++;
                        modCheck.ScriptHookDotNetModsSelectedNames.Add(item.ModInfo.Title);
                    }
                    if (item.ModInfo.RequiresIVSDKDotNet)
                    {
                        modCheck.IVSDKDotNetModsSelected++;
                        modCheck.IVSDKDotNetModsSelectedNames.Add(item.ModInfo.Title);
                    }
                }
            }
        }
        public void DoModCheck()
        {
            if (modCheck != null)
            {

                bool areAnyErrorsAvailable = false;

                errorText = new StringBuilder();
                errorText.AppendLine("The following issues need to be fixed so that we can continue.");
                errorText.AppendLine();

                if (modCheck.ASIModsSelected >= 1 || modCheck.ASILoadersSelected > 1)
                {
                    if (modCheck.ASILoadersSelected == 0)
                    {
                        errorText.AppendLine("- - - ASI Loader - - -");
                        if (modCheck.ASIModsSelected > 1)
                        {
                            errorText.AppendLine(string.Format("You've selected {0} ASI Mods but you haven't selected any ASI Loader. Please select an ASI Loader from the mod list.", modCheck.ASIModsSelected.ToString()));
                            errorText.AppendLine();
                            errorText.Append("Selected ASI Mods: ");
                        }
                        else
                        {
                            errorText.AppendLine(string.Format("You've selected {0} ASI Mod but you haven't selected any ASI Loader. Please select an ASI Loader from the mod list.", modCheck.ASIModsSelected.ToString()));
                            errorText.AppendLine();
                            errorText.Append("Selected ASI Mod: ");
                        }
                        for (int i = 0; i < modCheck.ASIModsSelectedNames.Count; i++)
                        {
                            if (i == (modCheck.ASIModsSelectedNames.Count - 1))
                                errorText.Append(modCheck.ASIModsSelectedNames[i]);
                            else
                                errorText.Append(modCheck.ASIModsSelectedNames[i] + ", ");
                        }
                        errorText.AppendLine();
                        areAnyErrorsAvailable = true;
                    }
                    else if (modCheck.ASILoadersSelected > 1)
                    {
                        errorText.AppendLine("- - - ASI Loader - - -");
                        errorText.AppendLine(string.Format("You've selected {0} ASI Loaders but you only need one.", modCheck.ASILoadersSelected.ToString()));
                        errorText.AppendLine();
                        areAnyErrorsAvailable = true;
                    }
                }

                if (modCheck.ScriptHookModsSelected >= 1)
                {
                    // Empty for now...
                }

                if (modCheck.ScriptHookDotNetModsSelected >= 1)
                {
                    // Empty for now...
                }

                if (modCheck.IVSDKDotNetModsSelected >= 1)
                {
                    // Empty for now...
                }

                if (areAnyErrorsAvailable)
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
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            if (Core.CDowngradingInfo.DowngradeTo == GameVersion.v1040)
                instance.PreviousStep(3);
            else
                instance.PreviousStep(1);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            // Add all checked mods to list
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsChecked)
                    Core.CDowngradingInfo.SelectedMods.Add(item.ModInfo);
            }

            // Add all optional mod components to list
            for (int i = 0; i < ModListStackPanel.Children.Count; i++)
            {
                ModItem item = (ModItem)ModListStackPanel.Children[i];
                if (item.IsChecked)
                {
                    if (item.ModInfo.OptionalComponents == null)
                        continue;

                    for (int o = 0; o < item.OptionalsWrapPanel.Children.Count; o++)
                    {
                        CheckBox cBox = item.OptionalsWrapPanel.Children[o] as CheckBox;

                        if (cBox.IsChecked.Value)
                            Core.CDowngradingInfo.SelectedOptionalComponents.Add((JsonObjects.OptionalComponentInfo)cBox.Tag);
                    }
                }
            }

            // Set if user wants to install prerequisites
            Core.CDowngradingInfo.SetInstallPrerequisites(InstallPrerequisitesCheckBox.IsChecked.Value);

            // Final step
            if (Core.IsInOfflineMode || allMods.Count == 0)
            {
                instance.ShowMessageDialogScreen("Important Note",
                    string.Format("It is HIGHLY recommended to install the Ultimate ASI Loader (xlive.dll) and ZolikaPatch to your game otherwise your game probably won't launch.{0}" +
                    "To download the Ultimate ASI Loader and ZolikaPatch, just click on the buttons next to the Continue button.{0}" +
                    "We can't guarantee for a stable and functional downgrade if those 2 components are not installed.", Environment.NewLine),
                    Steps.S9_Confirm,
                    null,
                    "Ultimate ASI Loader",
                    () => Web.AskUserToGoToURL(new Uri("https://github.com/ThirteenAG/Ultimate-ASI-Loader/releases")),
                    "ZolikaPatch",
                    () => Web.AskUserToGoToURL(new Uri("https://gtaforums.com/topic/955449-iv-zolikapatch/")));
            }
            else
                instance.NextStep();
        }

        private void DownloadWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                    return;

                if (e.Error == null)
                {
                    string result = e.Result;
                    if (!string.IsNullOrWhiteSpace(result))
                    {

                        allMods = JsonConvert.DeserializeObject<List<JsonObjects.ModInformation>>(result);

#if DEBUG
                        Console.WriteLine("- - - Mods - - -");
                        allMods.ForEach(m => Console.WriteLine(m.ToString())); // Print to console
#endif
                       
                        if (allMods.Count != 0)
                            AddModsToContainer(); // Add all mods to mods container

                        ChangeLoadingPageState(false, "");

                    }
                    else
                    {
                        ChangeLoadingPageState(true, "An unknown error occured while trying to retrieve all mods.", false);
                    }
                }
                else
                {
                    ChangeLoadingPageState(true, string.Format("(1) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, e.Error.Message), false);
                }
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

                if (e.Error == null)
                {
                    // Read file
                    string filePath = string.Format("{0}\\Red Wolf Interactive\\IV Downgrader\\DownloadedData\\modInfos.json", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

                    if (!File.Exists(filePath))
                    {
                        ChangeLoadingPageState(true, string.Format("(1) An error occured while trying to retrieve all mods."), false);
                        return;
                    }

                    allMods = JsonConvert.DeserializeObject<List<JsonObjects.ModInformation>>(File.ReadAllText(filePath));

#if DEBUG
                    Console.WriteLine("- - - Mods - - -");
                    allMods.ForEach(m => Console.WriteLine(m.ToString())); // Print to console
#endif

                    if (allMods.Count != 0)
                        AddModsToContainer(); // Add all mods to mods container

                    ChangeLoadingPageState(false, "");
                }
                else
                {
                    ChangeLoadingPageState(true, string.Format("(2) An error occured while trying to retrieve all mods.{0}{1}", Environment.NewLine, e.Error.Message), false);
                }
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
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            // Init WebClient
            downloadWebClient = new WebClient();

            if (Core.UseAlternativeDownloadLinks)
                downloadWebClient.Credentials = new NetworkCredential("ivdowngr", "7MY4qi2a8g");

            downloadWebClient.DownloadStringCompleted += DownloadWebClient_DownloadStringCompleted;
            downloadWebClient.DownloadFileCompleted += DownloadWebClient_DownloadFileCompleted;

            // Set ToolTip for InstallPrerequisites CheckBox
            if (Core.CDowngradingInfo.ConfigureForGFWL)
            {
                InstallPrerequisitesCheckBox.ToolTip = string.Format("This will download and install DirectX June 2010 SDK and Visual C++ 2005 SP1 which is required for GTA IV and most mods.{0}" +
                    "It will also install the Prerequisites required by GFWL because you checked the 'Configure this downgrade for GFWL' checkbox earlier.{0}",
                    "You can uncheck this if you are sure that you already have everything.", Environment.NewLine);
            }
            else
            {
                InstallPrerequisitesCheckBox.ToolTip = string.Format("This will download and install DirectX June 2010 SDK and Visual C++ 2005 SP1 which is required for GTA IV and most mods.{0}" +
                    "You can uncheck this if you are sure that you already have those.", Environment.NewLine);
            }

            // Remove previously selected mods and optionals so they aren't twice in the list
            Core.CDowngradingInfo.SelectedMods.Clear();
            Core.CDowngradingInfo.SelectedOptionalComponents.Clear();

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
            if (errorText != null)
                MessageBox.Show(errorText.ToString(), "Problems", MessageBoxButton.OK);
        }

    }
}
