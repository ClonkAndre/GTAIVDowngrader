using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using GTAIVDowngrader.Classes;

namespace GTAIVDowngrader.Dialogs
{
    public partial class MultiplayerUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public MultiplayerUC(MainWindow mainWindow)
        {
            instance = mainWindow;
            InitializeComponent();
        }
        public MultiplayerUC()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            instance.PreviousStep();
        }
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            DowngradingInfo.SetConfigureForGFWL(ConfigureForGFWLCheckBox.IsChecked.Value);

            if (ConfigureForGFWLCheckBox.IsChecked.Value)
            {
                instance.ShowMessageDialogScreen("Product Key Information",
                    string.Format("In order to play Games for Windows Live (GFWL) Multiplayer, you will need a Product Key!{0}" +
                    "If you need help with GFWL, it is recommended to join the Grand Theft RevIVal Discord server.{0}" +
                    "In this server, you can find Product Keys for GFWL and they also host regular multiplayer events.", Environment.NewLine),
                    Steps.S7_SelectRadioDwngrd,
                    null,
                    "Back",
                    () => instance.ChangeStep(Steps.S6_Multiplayer),
                    "Join Grand Theft RevIVal Discord",
                    () => CCL.Web.AskUserToGoToURL(new Uri("https://discord.gg/gtrf")));
            }
            else
                instance.NextStep();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;

            instance.ChangeActionButtonVisiblity(true, true, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            if (Core.Is420())
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char2.png", UriKind.Relative));
            if (Core.IsPrideMonth)
                bgChar.Source = new BitmapImage(new Uri("..\\Resources\\chars\\char9.png", UriKind.Relative));
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
