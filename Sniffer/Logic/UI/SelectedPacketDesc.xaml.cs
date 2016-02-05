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
    /// Interaction logic for SelectedPacketDesc.xaml
    /// </summary>
    public partial class SelectedPacketDesc : UserControl
    {
        private SelectedPacketDesc()
        {
            InitializeComponent();
        }

        public SelectedPacketDesc(SelectedPacketLine p) : this()
        {
            DataTypeName.Content = p.packetType;
            var name = "(" + p.packetName + "): ";
            switch (p.packetType)
            {
                case "b": //byte
                    name += byte.Parse(p.Content.Content.ToString(), System.Globalization.NumberStyles.HexNumber);
                    break;
                case "s": //short
                    name += short.Parse(p.Content.Content.ToString(), System.Globalization.NumberStyles.HexNumber);
                    break;
                case "d": //int
                    name += int.Parse(p.Content.Content.ToString(), System.Globalization.NumberStyles.HexNumber);
                    break;
                case "l": //long
                    name += long.Parse(p.Content.Content.ToString(), System.Globalization.NumberStyles.HexNumber);
                    break;
                case "f": //float
                    name += float.Parse(p.Content.Content.ToString(), System.Globalization.NumberStyles.HexNumber);
                    break;
                case "str": //string
                case "unk":
                    FieldName.Content = p.Content.Content;
                    break;
            }
            FieldName.Content = name;
        }
    }
}
