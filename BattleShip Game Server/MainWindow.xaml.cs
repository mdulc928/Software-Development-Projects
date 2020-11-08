//--------------------------------------------------------------------------------------------
//File:   MainWindow.xaml.cs
//Desc:   This program defines a MainWindow which contains methods and attribute for the title
//          of the game BattleShip.
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

namespace Battleship
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int gridSize;

        //Textblock that shows the value of the slider
        public TextBlock txtSldrVal = new TextBlock()
        {
            Height = 25,
            Width = 25,
            FontSize = 18,
            Margin = new Thickness(0, -50, 5, 12),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        //Initials MainWindow
        public MainWindow()
        {
            InitializeComponent();
        }

        //create an instance of the GameWindow when button is clicked and display it
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var battleWin = new BattleShipGame(gridSize);
            battleWin.Show();
            battleWin.CheatOn = (bool)Cheater.IsChecked;
            battleWin.GridPlace();
        }

        //checks for the value of the slider and assigns it to the grid Size for the game
        private void Sldr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ValueType value = sldrRow.Value;
            gridSize = Convert.ToInt32(value);
            txtSldrVal.Text = Convert.ToString(gridSize);
        }

        //add the textblock containing the slider value upon the Window loading
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pnlRows.Children.Add(txtSldrVal);
        }
    }

}

