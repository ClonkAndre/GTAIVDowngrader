using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GTAIVDowngrader.Dialogs
{
    public partial class WelcomeUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public WelcomeUC()
        {
            InitializeComponent();
        }
        public WelcomeUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void SetSecretStuff()
        {
            if (Core.Is420())
            {
                RedWolfLogo.Source = new BitmapImage(new Uri("..\\Resources\\Misc\\cbsLeaf.png", UriKind.Relative));
                RedWolfLogo.ToolTip = "Happy 420!";
            }
            if (Core.IsClonksBirthday())
            {
                RedWolfLogo.Source = new BitmapImage(new Uri("..\\Resources\\Misc\\clonk.png", UriKind.Relative));
                RedWolfLogo.ToolTip = "Wish ItsClonkAndre a happy birthday!";
            }
            if (Core.IsIVDowngraderReleaseDay())
            {
                RedWolfLogo.Source = new BitmapImage(new Uri("..\\Resources\\ivDowngraderLogo.png", UriKind.Relative));
                RedWolfLogo.ToolTip = "On this day, the first version of the GTA IV Downgrader was released!";
            }
            if (Core.IsIVLauncherReleaseDay())
            {
                RedWolfLogo.Source = new BitmapImage(new Uri("..\\Resources\\Misc\\ivLauncherLogo.png", UriKind.Relative));
                RedWolfLogo.ToolTip = "On this day, the first version of the GTA IV Launcher was released!";
            }
            if (Core.IsIVSDKDotNetReleaseDay())
            {
                RedWolfLogo.Source = new BitmapImage(new Uri("..\\Resources\\Misc\\ivsdknet.png", UriKind.Relative));
                RedWolfLogo.ToolTip = "On this day, the first version of IV-SDK .NET was released!";
            }
            if (Core.IsPrideMonth)
            {
                RedWolfLogo.Source = new BitmapImage(new Uri("..\\Resources\\Misc\\pride.png", UriKind.Relative));
                RedWolfLogo.ToolTip = string.Format("Life your live how YOU want it! YOUR body, YOUR choice.{0}" +
                    "Happy Pride!", Environment.NewLine);
            }
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            instance.NextStep();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Core.IsInSimpleMode)
            {
                instance.NextStep();
                return;
            }

            instance.NextButtonClicked += Instance_NextButtonClicked;

            instance.ChangeActionButtonVisiblity(true, false, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);

            // Do some secret stuff!
            SetSecretStuff();

            DowngraderVersionLabel.Text = string.Format("Version {0}", Core.TheUpdateChecker.CurrentVersion);
        }

        private void CheckForUpdatesHyperLink_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Core.TheUpdateChecker.CheckForUpdatesAsync(false, true);
#else
            Core.TheUpdateChecker.CheckForUpdatesAsync(false);
#endif
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Core.AskUserToOpenURL(e.Uri);
        }

    }
}
