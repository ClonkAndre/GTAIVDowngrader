using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTAIVDowngrader.Controls {
    public partial class ModItem : UserControl {

        #region Variables and Properties
        private BrushConverter brushConverter;

        private string _modFilename;
        public string ModFilename
        {
            get { return _modFilename; }
            set { _modFilename = value; }
        }

        private ModVersion _modVersion;
        public ModVersion ModVersion
        {
            get { return _modVersion; }
            set { _modVersion = value; }
        }

        private bool _isASILoader;
        public bool IsASILoader
        {
            get { return _isASILoader; }
            set { _isASILoader = value; }
        }

        private bool _isScriptHook;
        public bool IsScriptHook
        {
            get { return _isScriptHook; }
            set { _isScriptHook = value; }
        }

        private bool _isScriptHookHook;
        public bool IsScriptHookHook
        {
            get { return _isScriptHookHook; }
            set { _isScriptHookHook = value; }
        }

        private bool _isScriptHookDotNet;
        public bool IsScriptHookDotNet
        {
            get { return _isScriptHookDotNet; }
            set { _isScriptHookDotNet = value; }
        }

        private bool _requiresASILoader;
        public bool RequiresASILoader
        {
            get { return _requiresASILoader; }
            set { _requiresASILoader = value; }
        }

        private bool _requiresScriptHook;
        public bool RequiresScriptHook
        {
            get { return _requiresScriptHook; }
            set { _requiresScriptHook = value; }
        }

        private bool _requiresScriptHookHook;
        public bool RequiresScriptHookHook
        {
            get { return _requiresScriptHookHook; }
            set { _requiresScriptHookHook = value; }
        }

        private bool _requiresScriptHookDotNet;
        public bool RequiresScriptHookDotNet
        {
            get { return _requiresScriptHookDotNet; }
            set { _requiresScriptHookDotNet = value; }
        }

        public string ModName
        {
            get { return TitleLabel.Text; }
            set { TitleLabel.Text = value; }
        }
        public string ModDescription
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
        public ModItem(string filename, string title, string desc, bool isChecked = false)
        {
            brushConverter = new BrushConverter();
            InitializeComponent();
            ModFilename = filename;
            TitleLabel.Text = title;
            DescLabel.Text = desc;
            IsChecked = isChecked;
        }
        public ModItem()
        {
            InitializeComponent();
            Margin = new Thickness(0, 5, 0, 0);
        }
        #endregion

        private void CheckedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckedChanged?.Invoke(this, CheckedCheckBox.IsChecked.Value);
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsEnabled) {
                MainGrid.Background = (SolidColorBrush)brushConverter.ConvertFrom("#804f4f4f");
                TitleLabel.Foreground = Brushes.White;
                DescLabel.Foreground = Brushes.White;
            }
            else {
                MainGrid.Background = (SolidColorBrush)brushConverter.ConvertFrom("#80212121");
                TitleLabel.Foreground = Brushes.DarkGray;
                DescLabel.Foreground = Brushes.DarkGray;
            }
        }
    }
}
