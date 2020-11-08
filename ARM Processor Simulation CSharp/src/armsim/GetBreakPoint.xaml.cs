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
using System.Windows.Shapes;

namespace armsim
{
    /// <summary>
    /// Interaction logic for GetBreakPoint.xaml
    /// </summary>
    public partial class GetBreakPoint : Window
    {
        public string BPaddr;
        public bool On = false;
        public GetBreakPoint(string addr)
        {
            InitializeComponent();
            BPaddr = addr;
            On = true;
        }

        private void txtBpAddr_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                BPaddr = txtBpAddr.Text;
                this.Close();
                On = false;

            }else if(e.Key == Key.Escape){ this.Close();  On = false; }

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            BPaddr = txtBpAddr.Text;
            this.Close();
            On = false;
        }
    }
}
