using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTAIVDowngrader.Controls {
    public partial class ModItem : UserControl {

        #region Variables and Properties
        // Variables
        public JsonObjects.ModInformation ModInfo;

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
        public ModItem(JsonObjects.ModInformation modInfo)
        {
            InitializeComponent();
            ModInfo = modInfo;
            Title = ModInfo.Title;
            Description = ModInfo.Description;
            IsChecked = ModInfo.CheckedByDefault;
        }
        public ModItem()
        {
            InitializeComponent();
        }
        #endregion

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsEnabled) {
                MainGrid.Background = "#804f4f4f".ToBrush();
                TitleLabel.Foreground = Brushes.White;
                DescLabel.Foreground = Brushes.White;
            }
            else {
                MainGrid.Background = "#80212121".ToBrush();
                TitleLabel.Foreground = Brushes.DarkGray;
                DescLabel.Foreground = Brushes.DarkGray;
            }
        }

        private void CheckedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckedChanged?.Invoke(this, CheckedCheckBox.IsChecked.Value);
        }

    }
}
