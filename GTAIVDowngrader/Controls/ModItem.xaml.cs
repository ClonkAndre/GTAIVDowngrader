using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using CCL;

using GTAIVDowngrader.Classes.Json.Modification;

namespace GTAIVDowngrader.Controls
{
    public partial class ModItem : UserControl
    {

        #region Variables and Properties
        // Variables
        public ModDetails ModInfo;

        // Properties
        public string Title
        {
            get { return TitleLabel.Text; }
            set { TitleLabel.Text = value; }
        }
        public string Description
        {
            get { return DescLabel.Text; }
            set { DescLabel.Text = value; }
        }
        public bool IsChecked
        {
            get { return CheckedCheckBox.IsChecked.Value; }
            set { CheckedCheckBox.IsChecked = value; }
        }
        #endregion

        #region Events
        public delegate void CheckedChangedDelegate(ModItem sender, bool newState);
        public event CheckedChangedDelegate CheckedChanged;
        #endregion

        #region Constructor
        public ModItem(ModDetails modInfo)
        {
            InitializeComponent();

            // Set mod infos
            ModInfo = modInfo;
            Title = ModInfo.Title;
            Description = ModInfo.Description;
            IsChecked = ModInfo.CheckedByDefault;

            // Set warning and/or web page
            if (!string.IsNullOrWhiteSpace(ModInfo.WarningMessage))
            {
                WarningImage.ToolTip = ModInfo.WarningMessage;
                WarningImage.Visibility = Visibility.Visible;
            }
            if (!string.IsNullOrWhiteSpace(ModInfo.OfficialModWebPage))
                WebImage.Visibility = Visibility.Visible;

            // Set optionals components
            if (ModInfo.OptionalComponents != null)
            {
                // Clear container
                OptionalsWrapPanel.Children.Clear();
               
                // Add optional components to container
                for (int i = 0; i < ModInfo.OptionalComponents.Count; i++)
                {
                    OptionalComponentInfo info = ModInfo.OptionalComponents[i];

                    // Create checkbox
                    CheckBox cBox = new CheckBox();
                    cBox.Foreground = Brushes.White;
                    cBox.Tag = info;
                    cBox.Content = info.Title;
                    cBox.ToolTip = info.Description;
                    cBox.Margin = new Thickness(0, 0, 5, 0);

                    if (info.CheckedByDefault)
                        cBox.IsChecked = true;

                    // Add checkbox to optional components container
                    OptionalsWrapPanel.Children.Add(cBox);
                }

                // Show content
                OptionalsContent.Visibility = Visibility.Visible;
            }
        }
        public ModItem()
        {
            InitializeComponent();
        }
        #endregion

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsEnabled)
            {
                MainGrid.Background = "#804f4f4f".ToBrush();
                TitleLabel.Foreground = Brushes.White;
                DescLabel.Foreground = Brushes.White;
            }
            else
            {
                MainGrid.Background = "#80212121".ToBrush();
                TitleLabel.Foreground = Brushes.DarkGray;
                DescLabel.Foreground = Brushes.DarkGray;
            }
        }

        private void CheckedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckedChanged?.Invoke(this, CheckedCheckBox.IsChecked.Value);

            // Disable the OptionalsWrapPanel if checkbox is not checked
            OptionalsWrapPanel.IsEnabled = CheckedCheckBox.IsChecked.Value;

            // Uncheck all optional components
            if (!CheckedCheckBox.IsChecked.Value)
            {
                for (int i = 0; i < OptionalsWrapPanel.Children.Count; i++)
                {
                    CheckBox cBox = OptionalsWrapPanel.Children[i] as CheckBox;
                    cBox.IsChecked = false;
                }
            }
        }
        private void WebImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Web.AskUserToGoToURL(new Uri(ModInfo.OfficialModWebPage));
        }

    }
}
