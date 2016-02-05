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

namespace SnifferApp.Logic.UI
{
    /// <summary>
    /// Interaction logic for SelectedPacketLine.xaml
    /// </summary>
    public partial class SelectedPacketLine : UserControl
    {
        public string packetType;
        public string packetName;
        private SelectedPacketLine()
        {
            InitializeComponent();
        }

        public SelectedPacketLine(string type, string fieldName) : this()
        {
            packetType = type;
            packetName = fieldName;
        }
    }
}
