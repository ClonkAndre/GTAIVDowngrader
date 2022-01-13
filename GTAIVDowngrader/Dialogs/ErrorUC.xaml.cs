using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class ErrorUC : UserControl {

        #region Variables
        private Exception exception;
        #endregion

        #region Constructor
        public ErrorUC(Exception ex, List<string> additionalInfos = null)
        {
            exception = ex;
            InitializeComponent();
            if (additionalInfos == null) {
                DetailsLabel.Text = string.Format("{1}", Environment.NewLine, exception.ToString());
            }
            else {
                string str = string.Empty;

                for (int i = 0; i < additionalInfos.Count; i++) {
                    str += additionalInfos[i];
                }

                DetailsLabel.Text = string.Format("{1}{0}{0}Additional Informations{0}{2}", Environment.NewLine, exception.ToString(), str);
            }
        }
        #endregion

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DetailsLabel.Text);
        }

    }
}
