using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GTAIVDowngrader.Controls {
    public partial class CommandlineItem : UserControl {

        #region Properties
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
        #endregion

        #region Events
        public delegate void InsertDelegate(string argName);
        public event InsertDelegate Insert;
        #endregion

        #region Constructor
        public CommandlineItem()
        {
            InitializeComponent();
        }
        #endregion

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Insert?.Invoke(Title);
        }
    }
}
