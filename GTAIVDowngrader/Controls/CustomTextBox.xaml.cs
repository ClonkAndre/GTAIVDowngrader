using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GTAIVDowngrader.Controls {
    public partial class CustomTextBox : UserControl {

        #region Variables and Properties
        // Variables
        private BrushConverter brushConverter;
        private SolidColorBrush focusColor;
        private SolidColorBrush unfocusedColor;
        private SolidColorBrush textColor;
        private bool showPassword;

        // Properties
        public string WatermarkText
        {
            get { return WatermarkLabel.Text; }
            set { WatermarkLabel.Text = value; }
        }
        public string Text
        {
            get { return InputTextBox.Text; }
            set { InputTextBox.Text = value; }
        }
        public int MaxLength
        {
            get { return InputTextBox.MaxLength; }
            set { InputTextBox.MaxLength = value; }
        }
        public bool ShowPassword
        {
            get { return showPassword; }
            set {
                showPassword = value;
                if (showPassword) {
                    InputTextBox.Visibility = Visibility.Visible;
                    PasswordInputTextBox.Visibility = Visibility.Hidden;
                }
                else {
                    InputTextBox.Visibility = Visibility.Hidden;
                    PasswordInputTextBox.Visibility = Visibility.Visible;
                }
            }
        }
        public SolidColorBrush FocusColor
        {
            get { return focusColor; }
            set {
                focusColor = value;
            }
        }
        public SolidColorBrush UnfocusedColor
        {
            get { return unfocusedColor; }
            set {
                unfocusedColor = value;
            }
        }
        public SolidColorBrush TextColor
        {
            get { return textColor; }
            set {
                textColor = value;
                InputTextBox.Foreground = textColor;
                PasswordInputTextBox.Foreground = textColor;
            }
        }
        #endregion

        #region Events
        public event EventHandler<TextChangedEventArgs> TextChanged;
        #endregion

        #region Methods
        private void SetSelection(PasswordBox passwordBox, int start, int length)
        {
            passwordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                                 .Invoke(passwordBox, new object[] { start, length });
        }
        #endregion

        #region Constructor
        public CustomTextBox()
        {
            InitializeComponent();
            brushConverter = new BrushConverter();
            FocusColor = (SolidColorBrush)brushConverter.ConvertFrom("#007ACC");
            UnfocusedColor = (SolidColorBrush)brushConverter.ConvertFrom("#454545");
            ShowPassword = true;
        }
        #endregion

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InputTextBox.Text != "") {
                WatermarkLabel.Visibility = Visibility.Hidden;
            }
            else {
                WatermarkLabel.Visibility = Visibility.Visible;
            }
            TextChanged?.Invoke(this, e);
            PasswordInputTextBox.Password = InputTextBox.Text;
        }
        private void PasswordInputTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = PasswordInputTextBox.Password;
            SetSelection(PasswordInputTextBox, PasswordInputTextBox.Password.Length, 0);
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            LineRectangle.Fill = FocusColor;
        }
        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            LineRectangle.Fill = UnfocusedColor;
        }
    }
}
