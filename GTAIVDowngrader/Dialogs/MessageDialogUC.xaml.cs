using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs
{
    public partial class MessageDialogUC : UserControl
    {

        #region Variables
        private MainWindow instance;
        private Steps continueWithStep;
        private List<object> nextStepArgs;
        private Action backButtonAction, skipButtonAction, nextButtonAction;
        #endregion

        #region Methods
        public void SetMessage(string title, string desc, Steps continueWith, List<object> args, string backButtonText, Action bba, string skipButtonText, Action sba)
        {
            Dispatcher.Invoke(() => {

                TitleLabel.Text = title;
                DescriptionLabel.Text = desc;
                continueWithStep = continueWith;
                nextStepArgs = args;
                backButtonAction = bba;
                skipButtonAction = sba;

                if (backButtonAction != null)
                    instance.BackButton.Content = backButtonText;
                if (skipButtonAction != null)
                    instance.SkipButton.Content = skipButtonText;

                instance.ChangeActionButtonVisiblity(true, backButtonAction != null, skipButtonAction != null, true);

            });
        }
        public void OverrideNextButtonClick(Action withAction)
        {
            nextButtonAction = withAction;
        }
        public void ResetOverridenNextButtonClick()
        {
            nextButtonAction = null;
        }
        #endregion

        #region Constructor
        public MessageDialogUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        public MessageDialogUC()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Instance_NextButtonClicked(object sender, EventArgs e)
        {
            if (nextButtonAction != null)
                nextButtonAction?.Invoke();
            else
                instance.ChangeStep(continueWithStep, nextStepArgs);
        }
        private void Instance_BackButtonClicked(object sender, EventArgs e)
        {
            backButtonAction?.Invoke();
        }
        private void Instance_SkipButtonClicked(object sender, EventArgs e)
        {
            skipButtonAction?.Invoke();
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked -= Instance_NextButtonClicked;
            instance.BackButtonClicked -= Instance_BackButtonClicked;
            instance.SkipButtonClicked -= Instance_SkipButtonClicked;

            instance.NextButton.Content = "Next";
            instance.BackButton.Content = "Back";
            instance.SkipButton.Content = "Skip";
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            instance.NextButtonClicked += Instance_NextButtonClicked;
            instance.BackButtonClicked += Instance_BackButtonClicked;
            instance.SkipButtonClicked += Instance_SkipButtonClicked;

            instance.NextButton.Content = "Continue";

            //instance.ChangeActionButtonVisiblity(true, false, false, true);
            instance.ChangeActionButtonEnabledState(true, true, true, true);
        }

    }
}
