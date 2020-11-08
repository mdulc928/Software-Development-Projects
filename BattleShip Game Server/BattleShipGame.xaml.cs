//--------------------------------------------------------------------------------------------
//File:   BattleShipGame.xaml.cs
//Desc:   This program defines a class BattleShipGame which contains methods and attributes for
//          gui display of the game BattleShip.
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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;

namespace Battleship
{
    /// <summary>
    /// Interaction logic for BattleShipGame.xaml
    /// </summary>
    public partial class BattleShipGame : Window
    {
        Game game;
        OceanGrid humans;
        OceanGrid aI;

        DispatcherTimer Checktimer;
        DispatcherTimer DecremTimer;

        public int GSize { get; set; }                      //Contains size of grids;
        public bool CheatOn { get; set; }                   //always true unless the player unchecks box in MainWindow

        //Initializes the BattleShip Window with all the respective variables and different timers.
        public BattleShipGame(int size)
        {
            InitializeComponent();
            
            GSize = size;
            game = new Game(GSize);
            DataContext = game;

            game.Play("Start.wav");

            Checktimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            Checktimer.Tick += Timer_CheckHuman;

            DecremTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1000)
            };
            DecremTimer.Tick += DecremTimer_Decrem;
        }

        //Adds the cells to the both the human and ai stack panels
        public void AddCells(StackPanel pnl)
        {
            int size = GSize;

            for (int r = 0; r < size; r++)
            {
                StackPanel pnlSea = new StackPanel();
                pnlSea.Orientation = Orientation.Horizontal;
                pnlSea.HorizontalAlignment = HorizontalAlignment.Center;
                pnlSea.VerticalAlignment = VerticalAlignment.Center;

                for (int c = 0; c < size; c++)
                {
                    Button b = new Button();                    

                    b.Height = 240 / size;
                    b.Width = b.Height;

                    b.Click += B_Attack;

                    pnlSea.Children.Add(b);
                }

                pnl.Children.Add(pnlSea);
            }
        }

        // Contains logic to place the ships in the human and AI grids.
        public void GridPlace()
        {
            List<OceanGrid> oceans = new List<OceanGrid> { humans, aI };
            foreach (OceanGrid ocean in oceans)
            {
                for (int r = 0; r < game.Size; r++)
                {
                    for (int c = 0; c < game.Size; c++)
                    {
                        if (ocean.BoardLoc[r, c] == OceanGrid.States.Ship)
                        {
                            StackPanel panel;
                            if (ocean == aI)
                            {
                                panel = (StackPanel)pnlAI.Children[r + 1];
                                Button button = (Button)panel.Children[c];
                                if (CheatOn == true)
                                {
                                    ActivateBtn(button);
                                }
                            }
                            else
                            {
                                panel = (StackPanel)pnlHuman.Children[r + 1];
                                Button button = (Button)panel.Children[c];
                                ActivateBtn(button);
                            }
                        }

                    }
                }
            }
        }

        //Cell button event handler that calls Attack (see def in Game.cs) with its coordinates in the grid,
        //updating the state of the cells based off the results the Attack returns
        public void B_Attack(object sender, RoutedEventArgs e)
        {

            Button b = (Button)sender;

            if(VerifyBtn(b) == true)
            {

                Array[] results = game.Attack(GetBtnCoords(pnlAI, b)[0], GetBtnCoords(pnlAI, b)[1]);
                if(game.IsGameOver == false)
                {
                    bool[] human = (bool[])results[0];
                    bool[] ai = (bool[])results[1];
                    int[] coords = (int[])results[2];

                    if (human[0] == true)
                    {
                        HitBtn(b);
                    }

                    if (ai[0] == true)
                    {
                        Button hBtn = GetBtn(pnlHuman, coords[0], coords[1]);
                        HitBtn(hBtn);
                    }
                    else
                    {
                        HighLightBtn(pnlHuman);
                    }
                    Debug.WriteLine("Human");
                    game.UpdateState(humans);
                    Debug.WriteLine("AI");
                    game.UpdateState(aI);
                }
                else
                {
                    GameOverScrn();
                }
            }
            else
            {
                MessageBox.Show("Wrong Grid");
            }
        }

        //Timer tick event handler that fires ten times a second, checking whether or not the Human has attacked
        private void Timer_CheckHuman(object sender, EventArgs e)
        {
            if (game.IsGameOver != true)
            {
                if (game.HAttacked == true)
                {
                    DecremTimer.Stop();
                    game.TimeLim = 5;
                    DecremTimer.Start();
                    game.HAttacked = false;
                }
            }
            else
            {
                GameOverScrn();
            }

        }

        //DecremTimer tick event handler that fires every second that calls TimedAttack (see comments for TimedAttack in Game.cs) 
        //updates the Human's grid that at the place where it was attacked
        private void DecremTimer_Decrem(object sender, EventArgs e)
        {
            Array[] AIResults = game.TimedAttack();

            if (game.IsGameOver != true)
            {
                if (AIResults != null)
                {
                    bool[] ai = (bool[])AIResults[0];
                    int[] coords = (int[])AIResults[1];

                    if (ai[0] == true)
                    {
                        Button hBtn = GetBtn(pnlHuman, coords[0], coords[1]);
                        HitBtn(hBtn);
                    }
                    else
                    {
                        HighLightBtn(pnlHuman);
                    }
                }
            }
            else
            {
                GameOverScrn();
                DecremTimer.Stop();
            }
        }

        //HighLights the button in its, while clearing the one that was formerly highlighted (used when the button contains the coordinates of a Ship)
        void HitBtn(Button b)
        {
            int[] oldCoords = new int[2];

            if (game.Attacked.Count == 1)
            {
                oldCoords = (int[])game.Attacked[0];
            }
            else if (game.Attacked.Count > 0)
            {
                oldCoords = (int[])game.Attacked[game.Attacked.Count - 1];
            }
            if (oldCoords != null)
            {
                Button oldB = GetBtn(pnlHuman, oldCoords[0], oldCoords[1]);
                oldB.ClearValue(BackgroundProperty);
                oldB.ClearValue(BorderBrushProperty);
            }

            b.BorderBrush = Brushes.Red;
            b.Background = Brushes.LightPink;
            b.Foreground = Brushes.Red;
            b.BorderThickness = new Thickness(2);

            b.Content = "x";
            b.FontWeight = FontWeights.ExtraBold;

            b.HorizontalContentAlignment = HorizontalAlignment.Center;
            b.VerticalContentAlignment = VerticalAlignment.Center;
        }

        //HighLights a button of the stackPanel in its params that is currently selected, while clearing the former one (used for AI missed attacks) 
        void HighLightBtn(StackPanel panel)
        {
            int[] oldCoords = new int[2];
            int[] newCoords = new int[2];

            if (game.Attacked.Count == 1)
            {
                oldCoords = (int[])game.Attacked[0];
                newCoords = oldCoords;
            }
            else
            {
                oldCoords = (int[])game.Attacked[game.Attacked.Count - 2];
                newCoords = (int[])game.Attacked[game.Attacked.Count - 1];
            }

            Button oldB = GetBtn(panel, oldCoords[0], oldCoords[1]);
            Button newB = GetBtn(panel, newCoords[0], newCoords[1]);

            oldB.ClearValue(BackgroundProperty);
            oldB.ClearValue(BorderBrushProperty);

            newB.Background = Brushes.LightGreen;
            newB.BorderBrush = Brushes.DarkGreen;
        }

        //Highlights button passed in the params(used for initial state of grids with Ships)
        void ActivateBtn(Button b)
        {
            b.BorderBrush = Brushes.LightBlue;
            b.Background = Brushes.LightSlateGray;
            b.BorderThickness = new Thickness(2);
        }

        //Gets the coordinates of the button inside the Stackpanel in its params and returns it as an array (used to retrieve the coords of the button that was attacked)
        int[] GetBtnCoords(StackPanel players, Button b)
        {
            StackPanel panel = (StackPanel)b.Parent;

            int x = players.Children.IndexOf(panel) - 1;
            int y = panel.Children.IndexOf(b);

            int[] coords = new int[] { x, y };
            return coords;
        }

        //Returns a reference to the button object with the coordinates inside the StackPanel specified in the parameters
        Button GetBtn(StackPanel panel, int x, int y)
        {
            StackPanel child = (StackPanel)panel.Children[x + 1];
            Button b = (Button)child.Children[y];
            return b;

        }

        //Verify that the Button in its params is not in the AI grid, returning true if it is not and false otherwise
        bool VerifyBtn(Button b)
        {
            StackPanel nested = (StackPanel)b.Parent;

            bool gBtn = false;

            if (pnlAI.Children.IndexOf(nested) != -1)
            {
                gBtn = true;
            }

            return gBtn;
        }

        //Removes everything on the "Ocean", displaying the winner of the game
        void GameOverScrn()
        {

            pnlOcean.Children.RemoveRange(0, pnlOcean.Children.Count);
            pnlOcean.VerticalAlignment = VerticalAlignment.Center;

            TextBlock gameOver = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                Text = game.Message,
                FontSize = 35,
                FontFamily = new FontFamily("Castellar"),
            };

            Image winner = new Image()
            {
                Source = new BitmapImage(new Uri("/" + game.Winner + ".jpeg", UriKind.Relative)),
            };

            pnlOcean.Children.Add(winner);
            pnlOcean.Children.Add(gameOver);

            game.Play(game.EndSound);

            Checktimer.Stop();
            DecremTimer.Stop();

            game = null;
        }

        //Startup code for the BattleShip window 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            humans = game.Human;
            aI = game.AI;
            AddCells(pnlHuman);
            AddCells(pnlAI);

            Checktimer.Start();
            DecremTimer.Start();
        }
    }
}