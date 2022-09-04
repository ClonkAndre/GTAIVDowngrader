using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

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
        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Insert?.Invoke(Title);
        }

    }
}
