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

namespace GTAIVDowngrader.Controls
{
    public partial class TintImage : UserControl
    {

        #region Properties
        public ImageSource Image
        {
            get {
                return TheImage.Source;
            }
            set {
                TheImage.Source = value;
                TheTintImage.ImageSource = value;
            }
        }
        public SolidColorBrush TintColor
        {
            get {
                return TheTintColor;
            }
            set {
                //TheTintColor = value;
                TheTintColor.Color = value.Color;
            }
        }
        public double TintAmount
        {
            get {
                return TheTintRectangle.Opacity;
            }
            set {
                TheTintRectangle.Opacity = value;
            }
        }
        #endregion

        public TintImage()
        {
            InitializeComponent();
        }

    }
}
