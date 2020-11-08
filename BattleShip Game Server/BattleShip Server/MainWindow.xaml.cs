//--------------------------------------------------------------------------------------------
//File:   MainWindown.cs
//Desc:   This program defines a class MainWindow which contains startup logic for the server, 
//          while establishing connection between the GUI and its model.
//---------------------------------------------------------------------------------------------


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
using System.Windows.Threading;

namespace BattleShip_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BServerCom server;
        DispatcherTimer Pending;
        public MainWindow()
        {
            InitializeComponent();

            server = new BServerCom();
            DataContext = server;

            Pending = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0, 0, 0, 200)
            };
        }

        void LogMessages()
        {
            txtRR.Text = server.Messages;
            txtRR.ScrollToEnd();
        }
        
        //Establishes a connection to the server when a client attempts connection
        void EstConnection()
        {
            try
            {  
                if (server.BServer.Pending())
                {
                    var player = new Player(server.BServer);
                    server.Commun(player.PlayerClient);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            txtGame.SetBinding(TextBox.TextProperty, "StrNames");
            txtRR.SetBinding(TextBox.TextProperty, "Messages");
            Task.Run(() =>
            {
                do
                {
                    Task.Run(() => EstConnection());
                } while (true);
            });
        }
    }
}
