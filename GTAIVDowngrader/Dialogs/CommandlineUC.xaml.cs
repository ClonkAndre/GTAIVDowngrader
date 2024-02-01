using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using GTAIVDowngrader.Classes;
using GTAIVDowngrader.Controls;

namespace GTAIVDowngrader.Dialogs
{
    public partial class CommandlineUC : UserControl
    {

        #region Variables
        private MainWindow instance;

        private double VRAM = 1024;
        #endregion

        #region Methods
        private void AppendText(string str, bool doLineBreak = true)
        {
            if (doLineBreak)
                CustomCommandlineTextBox.AppendText(str + Environment.NewLine);
            else
                CustomCommandlineTextBox.AppendText(str);
        }

        private void AddCommandLineArgumentsToList()
        {
            if (Core.CommandLineArguments.Count != 0)
            {
                for (int i = 0; i < Core.CommandLineArguments.Count; i++)
                {
                    CommandLineArgument cla = Core.CommandLineArguments[i];
                    CommandlineItem item = new CommandlineItem();
                    item.Insert += Item_Insert;
                    item.Margin = new Thickness(0,5,0,0);
                    item.Title = cla.ArgumentName;
                    item.Description = cla.ArgumentDescription;

                    switch (cla.Category)
                    {
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
        private void Item_Insert(string argName)
        {
            if (CustomCommandlineTextBox.Text.Contains(argName))
            {
                Core.Notification.ShowNotification(NotificationType.Info, 3000, "Argument already added", "This argument already exists in the custom commandline.", "ALREADY_ADDED");
                return;
            }

            switch (argName)
            {
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
                    break;
                default:
                    AppendText(argName);
                    break;
            }
        }

        private void CreateCommandlineWithoutAvailableVidMem()
        {
            string path = string.Format("{0}\\commandline.txt", Core.CurrentDowngradingInfo.IVWorkingDirectoy);
            StringBuilder builder = new StringBuilder();

            try
            {
                if (File.Exists(path))
                {
                    switch (MessageBox.Show("There is already a commandline.txt in the GTA IV root directory. Override existing file?", "Override?", MessageBoxButton.YesNo, MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            builder.AppendLine("-nomemrestrict");
                            builder.AppendLine("-norestrictions");
                            if (AlsoIncludeWindowedCheckbox.IsChecked.Value)    builder.AppendLine("-windowed");
                            if (AlsoIncludeNoPreCacheCheckbox.IsChecked.Value)  builder.AppendLine("-noprecache");
                            File.WriteAllText(path, builder.ToString());
                            break;
                    }
                }
                else
                {
                    builder.AppendLine("-nomemrestrict");
                    builder.AppendLine("-norestrictions");
                    if (AlsoIncludeWindowedCheckbox.IsChecked.Value)    builder.AppendLine("-windowed");
                    if (AlsoIncludeNoPreCacheCheckbox.IsChecked.Value)  builder.AppendLine("-noprecache");
                    File.WriteAllText(path, builder.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error while creating commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Constructor
        public CommandlineUC()
        {
            InitializeComponent();
            AddCommandLineArgumentsToList();
        }
        public CommandlineUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
            AddCommandLineArgumentsToList();
        }
        #endregion

        #region Events
        private void Instance_SkipButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep();
        }
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep(2);
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            try
            {
                string path = string.Format("{0}\\commandline.txt", Core.CurrentDowngradingInfo.IVWorkingDirectoy);

                if (File.Exists(path))
                {
                    // Create commandline
                    if (RecommendedCommandlineRadioButton.IsChecked.Value)
                    {
                        if (AlsoIncludeAvailableVidMemCheckbox.IsChecked.Value)
                            return;
                        else
                            CreateCommandlineWithoutAvailableVidMem();
                    }
                    else if (CustomCommandlineRadioButton.IsChecked.Value)
                    {
                        switch (MessageBox.Show("There is already a commandline.txt in the GTA IV root directory. Override existing file?", "Override?", MessageBoxButton.YesNo, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                File.WriteAllText(path, CustomCommandlineTextBox.Text);
                                break;
                        }
                    }
                }
                else
                {
                    // Create commandline
                    if (RecommendedCommandlineRadioButton.IsChecked.Value)
                    {
                        if (AlsoIncludeAvailableVidMemCheckbox.IsChecked.Value)
                            return;
                        else
                            CreateCommandlineWithoutAvailableVidMem();
                    }
                    else if (CustomCommandlineRadioButton.IsChecked.Value)
                    {
                        File.WriteAllText(path, CustomCommandlineTextBox.Text);
                    }
                }

                instance.NextStep();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error while creating commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.SkipButtonClicked -= Instance_SkipButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.SkipButtonClicked += Instance_SkipButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, true, true);
            instance.ChangeActionButtonEnabledState(true, true, true, false);

            // 1040 only arguments
            if (Core.CurrentDowngradingInfo.DowngradeTo == GameVersion.v1040)
            {
                AlsoIncludeNoPreCacheCheckbox.Visibility = Visibility.Visible;
                AlsoIncludeNoPreCacheCheckbox.IsChecked = true;
            }

            if (CustomCommandlineRadioButton.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text))
                    instance.ChangeActionButtonEnabledState(true, true, true, false);
                else
                    instance.ChangeActionButtonEnabledState(true, true, true, true);
            }
        }

        private void RecommendedCommandlineRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (RecommendedCommandlineRadioButton.IsChecked.Value)
                instance.ChangeActionButtonEnabledState(true, true, true, true);
        }
        private void CustomCommandlineRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CustomCommandlineRadioButton.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text))
                    instance.ChangeActionButtonEnabledState(true, true, true, false);
                else
                    instance.ChangeActionButtonEnabledState(true, true, true, true);

                CustomCommandlineStackPanel.Visibility = Visibility.Visible;
            }
            else
                CustomCommandlineStackPanel.Visibility = Visibility.Hidden;
        }

        private void CustomCommandlineTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomCommandlineRadioButton.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text))
                    instance.ChangeActionButtonEnabledState(true, true, true, false);
                else
                    instance.ChangeActionButtonEnabledState(true, true, true, true);
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
            try
            {
                string path = ".\\Data\\saved_commandline.txt";
                if (!string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text))
                {
                    if (Directory.Exists(".\\Data"))
                    {
                        if (File.Exists(path))
                        {
                            switch (MessageBox.Show("This will override the already saved commandline file. Do you want to override the saved commandline?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                            {
                                case MessageBoxResult.Yes:
                                    break;
                                case MessageBoxResult.No:
                                    return;
                            }
                        }

                        File.WriteAllText(path, CustomCommandlineTextBox.Text);
                        if (File.Exists(path))
                            MessageBox.Show("Commandline saved! You can now press on 'Load saved commandline' to load your saved commandline so that you don't have to write a new one everytime again.", "Commandline saved", MessageBoxButton.OK, MessageBoxImage.Information);
                        else
                            MessageBox.Show("Unknown error while saving.", "Unknown error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Can't save empty commandline!", "Empty commandline", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error while creating commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadSavedCommandlineButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = ".\\Data\\saved_commandline.txt";
                if (File.Exists(path))
                {
                    if (!string.IsNullOrWhiteSpace(CustomCommandlineTextBox.Text))
                    {
                        switch (MessageBox.Show("This will override the current commandline. Are you sure that you want to load your saved commandline?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                        {
                            case MessageBoxResult.Yes:
                                CustomCommandlineTextBox.Text = File.ReadAllText(path);
                                break;
                        }
                    }
                    else
                    {
                        CustomCommandlineTextBox.Text = File.ReadAllText(path);
                    }
                }
                else
                {
                    MessageBox.Show("There is currently no commandline saved. Press on 'Save current commandline' to save the current commandline.", "Nothing saved", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error while loading commandline. Details: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
