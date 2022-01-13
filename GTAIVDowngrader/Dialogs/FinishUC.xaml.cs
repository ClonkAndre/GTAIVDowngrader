using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GTAIVDowngrader.Dialogs {
    public partial class FinishUC : UserControl {

        #region Variables
        private MainWindow instance;
        #endregion

        #region Constructor
        public FinishUC()
        {
            InitializeComponent();
        }
        public FinishUC(MainWindow window)
        {
            instance = window;
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LogItemsListBox.Items.Clear();
            for (int i = 0; i < instance.downgradingUC.logItems.Count; i++) {
                DowngradingUC.LogItem item = instance.downgradingUC.logItems[i];
                LogItemsListBox.Items.Add(item.ToString());
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string fileLoc = string.Format("{0}\\PlayGTAIV.exe", instance.s1_SelectIVExe.IVInstallDirectory);
            try {
                if (File.Exists(fileLoc)) {
                    Process process = new Process();
                    process.StartInfo.FileName = fileLoc;
                    process.StartInfo.WorkingDirectory = instance.s1_SelectIVExe.IVInstallDirectory;
                    process.Start();
                    Environment.Exit(0);
                }
                else {
                    MessageBox.Show("Couldn't find PlayGTAIV.exe", "GTAIVDowngrader.exe");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("An error occured while trying to start GTA IV. Details: " + ex.ToString(), "GTAIVDowngrader.exe");
                Environment.Exit(0);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            instance.PreviousStep();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ShowLogCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ShowLogCheckBox.IsChecked.Value) {
                LogItemsListBox.Visibility = Visibility.Visible;
            }
            else {
                LogItemsListBox.Visibility = Visibility.Collapsed;
            }
        }

    }
}
