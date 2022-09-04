using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GTAIVDowngrader.Controls {
    public partial class NotificationItem : UserControl {

        #region Variables and Properties
        // Variables
        private string _additionalInfos;
        private int _showTime;
        private bool timeRanOut, mouseIsOverNotification, isCurrentlyFadingOut, blockAutoClose;

        // Properties
        public string Title
        {
            get { return TitleLabel.Text; }
            set { TitleLabel.Text = value; }
        }
        public string Description
        {
            get { return DescriptionLabel.Text; }
            set { DescriptionLabel.Text = value; }
        }
        public string AdditionnalInformations
        {
            get { return _additionalInfos; }
            private set { _additionalInfos = value; }
        }
        public SolidColorBrush NotificationColor
        {
            get { return (SolidColorBrush)NotificationBorder.Background; }
            set { NotificationBorder.Background = value; }
        }
        public Color NotificationBorderEffectColor
        {
            get { return NotificationBorderEffect.Color; }
            set { NotificationBorderEffect.Color = value; }
        }
        public int ShowTime
        {
            get { return _showTime; }
            set { _showTime = value; }
        }
        #endregion

        #region Events
        public event EventHandler DeleteEvent;
        #endregion

        #region Constructor
        public NotificationItem(int showTime, string tile, string description, string additionalInfos = "")
        {
            timeRanOut = false;
            mouseIsOverNotification = false;
            isCurrentlyFadingOut = false;
            blockAutoClose = false;
            InitializeComponent();
            ShowTime = showTime;
            Title = tile;
            Description = description;
            AdditionnalInformations = additionalInfos;
        }
        public NotificationItem()
        {
            timeRanOut = false;
            mouseIsOverNotification = false;
            isCurrentlyFadingOut = false;
            blockAutoClose = false;
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void FadeIn(bool useAnimation)
        {
            if (useAnimation) {
                this.Visibility = Visibility.Visible;
                this.Opacity = 0.0;
                DoubleAnimation dA = new DoubleAnimation {
                    From = 0.0,
                    To = 1.0,
                    FillBehavior = FillBehavior.HoldEnd,
                    Duration = new Duration(TimeSpan.FromSeconds(0.15))
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(dA);
                Storyboard.SetTarget(dA, this);
                Storyboard.SetTargetProperty(dA, new PropertyPath(OpacityProperty));
                storyboard.Begin();
            }
            else {
                this.Visibility = Visibility.Visible;
                this.Opacity = 1.0;
            }
        }
        public void FadeOut(bool useAnimation)
        {
            isCurrentlyFadingOut = true;
            if (useAnimation) {
                this.Opacity = 1.0;
                DoubleAnimation dA = new DoubleAnimation {
                    From = 1.0,
                    To = 0.0,
                    FillBehavior = FillBehavior.HoldEnd,
                    Duration = new Duration(TimeSpan.FromSeconds(0.15))
                };

                var storyboard = new Storyboard();
                storyboard.Completed += Storyboard_Completed;
                storyboard.Children.Add(dA);
                Storyboard.SetTarget(dA, this);
                Storyboard.SetTargetProperty(dA, new PropertyPath(OpacityProperty));
                storyboard.Begin();
            }
            else {
                this.Opacity = 0.0;
                Storyboard_Completed(this, EventArgs.Empty);
            }
        }
        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (this.Opacity < 0.5) {
                this.Visibility = Visibility.Collapsed;
                this.Opacity = 0.0;
                DeleteEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        public void ShowNotifiction()
        {
            try {
                Task.Run(() => {
                    Dispatcher.Invoke(() => { FadeIn(true); });
                    Thread.Sleep(_showTime);
                    timeRanOut = true;
                    if (!blockAutoClose) {
                        if (!mouseIsOverNotification) Dispatcher.Invoke(() => { FadeOut(true); });
                    }
                });
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!isCurrentlyFadingOut) mouseIsOverNotification = true;
        }
        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseIsOverNotification = false;
            if (timeRanOut) FadeOut(true);
        }

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) {
                blockAutoClose = true;
                FadeOut(true);
            }
        }

    }
}
