using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GTAIVDowngrader.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class CommandlineUC : UserControl {

        #region Variables
        private MainWindow instance;

        private List<CommandLineArgument> commandLineArguments;
        private double VRAM = 1024;
        #endregion

        #region Struct
        private struct CommandLineArgument
        {
            #region Properties
            public int Category { get; private set; }
            public string ArgumentName { get; private set; }
            public string ArgumentDescription { get; private set; }
            #endregion

            #region Constructor
            public CommandLineArgument(int category, string aName, string aDesc)
            {
                Category = category;
                ArgumentName = aName;
                ArgumentDescription = aDesc;
            }
            #endregion
        }
        #endregion

        #region Methods
        private void AddCommandLineArguments()
        {
            // Graphics
            commandLineArguments.Add(new CommandLineArgument(0, "-renderquality", "Sets the render quality of the game. (0-4)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-shadowdensity", "Sets the shadow density of the game. (0-16)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-texturequality", "Sets the texture quality of the game. (0-2)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-viewdistance", "Sets the view distance of the game (0-99)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-detailquality", "Sets the detail quality of the game. (0-99)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-novblank", "Disables wait for vblank (No Vsync)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-norestrictions", "Do not limit graphics settings"));
            commandLineArguments.Add(new CommandLineArgument(0, "-width", "Sets the width of the main render window (default is 800)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-height", "Sets the height of the main render window (default is 600)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-safemode", "Runs the graphics in the lowest setting possible"));
            commandLineArguments.Add(new CommandLineArgument(0, "-frameLimit", "Limits frame to interval of refresh rate (ex. If refreshrate is 60HZ –frameLimit 1 = Locks down to 60HZ)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-refreshrate", "Sets the refresh rate of the main render window"));
            commandLineArguments.Add(new CommandLineArgument(0, "-fullscreen", "Forces fullscreen mode"));
            commandLineArguments.Add(new CommandLineArgument(0, "-windowed", "Forces windowed mode"));
            commandLineArguments.Add(new CommandLineArgument(0, "-availablevidmem", "Sets the amount of physical Video Memory (ex. -availablevidmem 1024.0 with 1024 being your amount of VRAM because 1024MB = 1GB)"));
            commandLineArguments.Add(new CommandLineArgument(0, "-percentvidmem", "Sets the percentage of video memory to make available to GTA"));
            // Audio
            commandLineArguments.Add(new CommandLineArgument(1, "-fullspecaudio", "Forces high-end CPU audio footprint"));
            commandLineArguments.Add(new CommandLineArgument(1, "-minspecaudio", "Forces low-end CPU audio footprint"));
            // System
            commandLineArguments.Add(new CommandLineArgument(2, "-noprecache", "Disables precache of resources"));
            commandLineArguments.Add(new CommandLineArgument(2, "-nomemrestrict", "Disables memory restrictions"));
        }
        private void AddCommandLineArgumentsToList()
        {
            if (commandLineArguments.Count != 0) {
                for (int i = 0; i < commandLineArguments.Count; i++) {
                    CommandLineArgument cla = commandLineArguments[i];
                    CommandlineItem item = new CommandlineItem();
                    item.Insert += Item_Insert;
                    item.Margin = new Thickness(0,5,0,0);
                    item.Title = cla.ArgumentName;
                    item.Description = cla.ArgumentDescription;
                    switch (cla.Category) {
                        case 0:
                            GraphicsArgsStackPanel.Children.Add(item);
                            break;
                        case 1:
                            AudioArgsStackPanel.Children.Add(item);
                            break;
                        case 2:
                            SystemArgsStackPanel.Children.Add(item);
                            break;
                    }
                }
            }
        }

        private void AppendText(string str, bool doLineBreak = true)
        {
            if (doLineBreak) {
                CustomCommandlineTextBox.AppendText(str + Environment.NewLine);
            }
            else {
                CustomCommandlineTextBox.AppendText(str);
            }
        }

        private void Item_Insert(string argName)
        {
            if (CustomCommandlineTextBox.Text.Contains(argName)) {
                MessageBox.Show("This argument already exists in the custom commandline!", "Argument already added", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            switch (argName) {
                case "-renderquality":
                    AppendText(string.Format("{0} 0", argName));
                    break;
                case "-shadowdensity":
                    AppendText(string.Format("{0} 0", argName));
                    break;
                case "-texturequality":
                    AppendText(string.Format("{0} 0", argName));
                    break;
                case "-viewdistance":
                    AppendText(string.Format("{0} 0", argName));
                    break;
                case "-detailquality":
                    AppendText(string.Format("{0} 0", argName));
                    break;
                case "-width":
                    AppendText(string.Format("{0} 800", argName));
                    break;
                case "-height":
                    AppendText(string.Format("{0} 600", argName));
                    break;
                case "-frameLimit":
                    AppendText(string.Format("{0} 0", argName));
                    break;
                case "-availablevidmem":
                    AppendText(string.Format("{0} {1}.0", argName, VRAM.ToString()));
                    //if (vram >= 2048) {
                    //    AppendText(string.Format("{0} 2048.0", argName));
                    //}
                    //else {
                    //    AppendText(string.Format("{0} {1}.0", argName, Math.Round(vram, 0).ToString()));
                    //}
                    break;
                default:
                    AppendText(argName);
                    break;
            }
        }

        private void CreateCommandlineWithoutAvailableVidMem()
        {
            string path = string.Format("{0}\\commandline.txt", instance.s1_SelectIVExe.IVInstallDirectory);
            StringBuilder builder = new StringBuilder();

            try {
                if (File.Exists(path)) {
                    switch (MessageBox.Show("There is already a commandline.txt in the GTA IV root directory. Override existing file?", "Override?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                        case MessageBoxResult.Yes:
                            builder.AppendLine("-nomemrestrict");
                            builder.AppendLine("-norestrictions");
                            if (AlsoIncludeWindowedCheckbox.IsChecked.Value) builder.AppendLine("-windowed");
                            if (AlsoIncludeNoPreCacheCheckbox.IsChecked.Value) builder.AppendLine("-noprecache");
                            File.WriteAllText(path, builder.ToString());
                            break;
                    }
                }
                else {
                    builder.AppendLine("-nomemrestrict");
                    builder.AppendLine("-norestrictions");
                    if (AlsoIncludeWindowedCheckbox.IsChecked.Value) builder.AppendLine("-windowed");
                    if (AlsoIncludeNoPreCacheCheckbox.IsChecked.Value) builder.AppendLine("-noprecache");
                    File.WriteAllText(path, builder.ToString());
                }
            }
            catch (Exception ex) {
                MessageBox.Show(string.Format("Error while creating commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Constructor
        public CommandlineUC()
        {
            InitializeComponent();

            commandLineArguments = new List<CommandLineArgument>();
            AddCommandLineArguments();
            AddCommandLineArgumentsToList();

            //double _vram = Helper.GetVRAM();
            //if (_vram >= 2048) {
            //    VRAM = 2048;
            //}
            //else {
            //    VRAM = Math.Round(_vram, 0);
            //}
        }
        public CommandlineUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();

            commandLineArguments = new List<CommandLineArgument>();
            AddCommandLineArguments();
            AddCommandLineArgumentsToList();

            //double _vram = Helper.GetVRAM();
            //if (_vram >= 2048) {
            //    VRAM = 2048;
            //}
            //else {
            //    VRAM = Math.Round(_vram, 0);
            //}
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            VRAMQuestionGrid.Visibility = Visibility.Hidden;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (instance.s2_SelectDwngrdVersion.selectedVersion == GameVersion.v1040) {
                AlsoIncludeNoPreCacheCheckbox.Visibility = Visibility.Visible;
                AlsoIncludeNoPreCacheCheckbox.IsChecked = true;
            }

            if (CustomCommandlineRadioButton.IsChecked.Value) {
                if (string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text)) {
                    NextButton.IsEnabled = false;
                }
                else {
                    NextButton.IsEnabled = true;
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            instance.ShowExitMsg();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            instance.PreviousStep();
        }
        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            instance.NextStep();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                string path = string.Format("{0}\\commandline.txt", instance.s1_SelectIVExe.IVInstallDirectory);

                if (File.Exists(path)) {
                    // Create commandline
                    if (RecommendedCommandlineRadioButton.IsChecked.Value) {
                        if (AlsoIncludeAvailableVidMemCheckbox.IsChecked.Value) {
                            VRAMQuestionGrid.Visibility = Visibility.Visible;
                            return;
                        }
                        else {
                            CreateCommandlineWithoutAvailableVidMem();
                        }
                    }
                    else if (CustomCommandlineRadioButton.IsChecked.Value) {
                        switch (MessageBox.Show("There is already a commandline.txt in the GTA IV root directory. Override existing file?", "Override?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                            case MessageBoxResult.Yes:
                                File.WriteAllText(path, CustomCommandlineTextBox.Text);
                                break;
                        }
                    }
                }
                else {
                    // Create commandline
                    if (RecommendedCommandlineRadioButton.IsChecked.Value) {
                        if (AlsoIncludeAvailableVidMemCheckbox.IsChecked.Value) {
                            VRAMQuestionGrid.Visibility = Visibility.Visible;
                            return;
                        }
                        else {
                            CreateCommandlineWithoutAvailableVidMem();
                        }
                    }
                    else if (CustomCommandlineRadioButton.IsChecked.Value) {
                        File.WriteAllText(path, CustomCommandlineTextBox.Text);
                    }
                }

                instance.NextStep();
            }
            catch (Exception ex) {
                MessageBox.Show(string.Format("Error while creating commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecommendedCommandlineRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (RecommendedCommandlineRadioButton.IsChecked.Value) NextButton.IsEnabled = true;
        }
        private void CustomCommandlineRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CustomCommandlineRadioButton.IsChecked.Value) {
                if (string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text)) {
                    NextButton.IsEnabled = false;
                }
                else {
                    NextButton.IsEnabled = true;
                }
                CustomCommandlineStackPanel.Visibility = Visibility.Visible;
            }
            else {
                CustomCommandlineStackPanel.Visibility = Visibility.Hidden;
            }
        }

        private void CustomCommandlineTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomCommandlineRadioButton.IsChecked.Value) {
                if (string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text)) {
                    NextButton.IsEnabled = false;
                }
                else {
                    NextButton.IsEnabled = true;
                }
            }
        }

        private void OpenArgumentListButton_Click(object sender, RoutedEventArgs e)
        {
            ArgsListGrid.Visibility = Visibility.Visible;
        }
        private void CloseArgsListButton_Click(object sender, RoutedEventArgs e)
        {
            ArgsListGrid.Visibility = Visibility.Hidden;
        }

        private void SaveCurrentCommandlineButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                string path = ".\\Downgrader\\saved_commandline.txt";
                if (!string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text)) {
                    if (Directory.Exists(".\\Downgrader")) {
                        if (File.Exists(path)) {
                            switch (MessageBox.Show("This will override the already saved commandline file. Do you want to override the saved commandline?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning)) {
                                case MessageBoxResult.Yes:
                                    break;
                                case MessageBoxResult.No:
                                    return;
                            }
                        }

                        File.WriteAllText(path, CustomCommandlineTextBox.Text);
                        if (File.Exists(path)) {
                            MessageBox.Show("Commandline saved! You can now press on 'Load saved commandline' to load your saved commandline so that you don't have to write a new one everytime again.", "Commandline saved", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else {
                            MessageBox.Show("Unknown error while saving.", "Unknown error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else {
                    MessageBox.Show("Can't save empty commandline!", "Empty commandline", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(string.Format("Error while creating commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadSavedCommandlineButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                string path = ".\\Downgrader\\saved_commandline.txt";
                if (File.Exists(path)) {
                    if (!string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text)) {
                        switch (MessageBox.Show("This will override the current commandline. Are you sure that you want to load your saved commandline?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning)) {
                            case MessageBoxResult.Yes:
                                CustomCommandlineTextBox.Text = File.ReadAllText(path);
                                break;
                        }
                    }
                    else {
                        CustomCommandlineTextBox.Text = File.ReadAllText(path);
                    }
                }
                else {
                    MessageBox.Show("There is currently no commandline saved. Press on 'Save current commandline' to save the current commandline.", "Nothing saved", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(string.Format("Error while loading commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region VRAM Question Grid
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            switch (MessageBox.Show(string.Format("This link takes you to {0} ({1}). Do you want to go there?", uri.Host, uri.ToString()), "Open link?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                case MessageBoxResult.Yes:
                    Process.Start(uri.ToString());
                    break;
            }
        }
        private void WhereDoIFindVRAMAmountButton_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("https://www.techwalla.com/articles/how-to-find-out-how-much-vram-you-have");
            switch (MessageBox.Show(string.Format("This takes you to {0} ({1}). Do you want to go there?", uri.Host, uri.ToString()), "Open link?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                case MessageBoxResult.Yes:
                    Process.Start(uri.ToString());
                    break;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            VRAMQuestionGrid.Visibility = Visibility.Hidden;
        }
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            string path = string.Format("{0}\\commandline.txt", instance.s1_SelectIVExe.IVInstallDirectory);

            if (string.IsNullOrWhiteSpace(VRAMAmountTextbox.Text)) {
                ErrorLabel.Text = "Please enter the amount of VRAM your GPU has";
                return;
            }
            if (!int.TryParse(VRAMAmountTextbox.Text, out int result) || VRAMAmountTextbox.Text.Contains("+") || VRAMAmountTextbox.Text.Contains("-")) {
                ErrorLabel.Text = "Please only enter numbers!";
                return;
            }
            if (result == 0) {
                ErrorLabel.Text = "VRAM can't be 0!";
                return;
            }

            if (File.Exists(path)) {
                // Create commandline
                switch (MessageBox.Show("There is already a commandline.txt in the GTA IV root directory. Override existing file?", "Override?", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                    case MessageBoxResult.Yes:
                        StringBuilder builder = new StringBuilder();

                        if (VRAMAmountTextbox.Text.Length <= 1) { // GB
                            if (result >= 4) {
                                builder.AppendLine("-availablevidmem 4096.0");
                            }
                            else {
                                builder.AppendLine(string.Format("-availablevidmem {0}.0", (result * 1024).ToString()));
                            }
                        }
                        else { // MB
                            if (result > 4096) {
                                ErrorLabel.Text = "VRAM can't be over 4096! (So just use 4096 if your VRAM is bigger than that)";
                                return;
                            }
                            else {
                                builder.AppendLine(string.Format("-availablevidmem {0}.0", result.ToString()));
                            }
                        }

                        builder.AppendLine("-nomemrestrict");
                        builder.AppendLine("-norestrictions");
                        if (AlsoIncludeWindowedCheckbox.IsChecked.Value) builder.AppendLine("-windowed");
                        if (AlsoIncludeNoPreCacheCheckbox.IsChecked.Value) builder.AppendLine("-noprecache");
                        File.WriteAllText(path, builder.ToString());
                        break;
                }
            }
            else {
                StringBuilder builder = new StringBuilder();

                if (VRAMAmountTextbox.Text.Length <= 1) { // GB
                    if (result >= 4) {
                        builder.AppendLine("-availablevidmem 4096.0");
                    }
                    else {
                        builder.AppendLine(string.Format("-availablevidmem {0}.0", (result * 1024).ToString()));
                    }
                }
                else { // MB
                    if (result > 4096) {
                        ErrorLabel.Text = "VRAM can't be over 4096! (So just use 4096 if your VRAM is bigger than that)";
                        return;
                    }
                    else {
                        builder.AppendLine(string.Format("-availablevidmem {0}.0", result.ToString()));
                    }
                }

                builder.AppendLine("-nomemrestrict");
                builder.AppendLine("-norestrictions");
                if (AlsoIncludeWindowedCheckbox.IsChecked.Value) builder.AppendLine("-windowed");
                if (AlsoIncludeNoPreCacheCheckbox.IsChecked.Value) builder.AppendLine("-noprecache");
                File.WriteAllText(path, builder.ToString());
            }

            instance.NextStep();
        }
        #endregion

    }
}
