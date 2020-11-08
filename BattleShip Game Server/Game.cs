//--------------------------------------------------------------------------------------------
//File:   Game.cs
//Desc:   This program defines a class Game which contains the data for the game BattleShip.
//---------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Media;

namespace Battleship
{
    public class Game : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> Players { get; set; }

        public OceanGrid Human;
        public OceanGrid AI;
        public int timeLimit;
        private int hShips;
        private int aiShips;

        SoundPlayer soundPlayer;
        static Random limit = new Random();

        public List<Array> Attacked { get; set; }                               //List containing the coordinates the AI has attacked
        public bool HAttacked { get; set; }
        public int TimeLim
        {
            get
            {
                return timeLimit;
            }
            set
            {
                timeLimit = value;
                SetProperty("TimeLim");
            }
        }
        public int Size { get; set; }
        public int NumShips { get; set; }
        public int HShips
        {
            get
            {
                return hShips;
            }
            set
            {
                hShips = value;
                SetProperty("HShips");
            }
        }
        public int AIShips
        {
            get
            {
                return aiShips;
            }
            set
            {
                aiShips = value;
                SetProperty("AIShips");
            }
        }
        public bool IsGameOver { get; set; }
        public string Winner { get; set; }                      //Declared winner of the game
        public string Message { get; set; }                     //Message to display when game ends;
        public string EndSound { get; set; }                    //Sound to play when game ends;

        //Constructor for Game class
        public Game(int size)
        {
            Size = size;
            NumShips = 5;
            Human = new OceanGrid(size);
            AI = new OceanGrid(size);


            Place();
            Attacked = new List<Array>();
            timeLimit = 5;
            HShips = Human.Ships.Count;
            AIShips = AI.Ships.Count;

            Players = new List<string>();
        }

        //Checks in the grid in the params for a ship object with the coordinates x and y, and returns true if not found, false if found
        public bool ValidatePosition(OceanGrid grid, int x, int y)
        {
            return !grid.GetShipCoord(x, y);
        }

        //verifies that the numbers picked are valid positions and places ships on the grids
        public void Place()
        {
            List<OceanGrid> grids = new List<OceanGrid> { Human, AI };

            Random ornt = new Random(1);
            Random len = new Random();

            foreach (OceanGrid grid in grids)
            {
                List<int> lengths = new List<int>();

                int two = 0;

                while (grid.Ships.Count < NumShips)
                {
                    int x = limit.Next(Size);
                    int y = limit.Next(Size);
                    int length = len.Next(5);
                    int orient;


                    bool validPos = false;

                    while (validPos == false)
                    {
                        if (length > 1)
                        {
                            orient = ornt.Next(1, 3);
                            validPos = grid.TestLoc(x, y, length, orient);

                            if (length == 2 && two != 2)
                            {
                                two++;
                                lengths.Add(length);
                            }
                            else if (!lengths.Contains(length))
                            {
                                lengths.Add(length);
                            }
                            else
                            {
                                validPos = false;
                            }

                            if (validPos == true)
                            {
                                grid.AddShip(x, y, length, orient);
                            }
                        }
                        else if (ValidatePosition(grid, x, y) == true)
                        {
                            grid.AddShip(x, y);
                            validPos = true;
                        }

                        x = limit.Next(Size);
                        y = limit.Next(Size);
                        length = len.Next(5);
                    }

                    UpdateState(grid);
                }
            }

        }

        //Calls the Attack method (see definition in OceanGrid.cs) in the either of the grids, and returns a array containing the results of the both attacks
        // and the coordinates the AI attacked.
        public Array[] Attack(int x, int y)
        {
            Array[] results = new Array[3];

            bool[] hCheck = AI.Attack(x, y);

            if (hCheck[0] == true)
            {
                Play("Hit.wav");
            }
            else
            {
                Play("Miss.wav");
            }

            EndGame();

            HAttacked = true;
            AIShips = AI.Ships.Count;

            Array[] AIResults = AIAttack();
            HShips = Human.Ships.Count;

            int[] aiCoords = (int[])AIResults[1];
            bool[] aiCheck = (bool[])AIResults[0];

            results[0] = hCheck;
            results[1] = aiCheck;
            results[2] = aiCoords;

            EndGame();

            return results;
        }

        //Calls AIAttack when the TimeLimit for the Human to move has ended, returning the results of the attack and the coordinated that were attacked in an array of arrays 
        public Array[] TimedAttack()
        {
            if (TimeLim == 0)
            {
                Array[] AIResults = AIAttack();

                EndGame();
                if (IsGameOver == true)
                {
                    return null;
                }

                TimeLim = 5;
                return AIResults;
            }
            else
            {
                TimeLim--;
            }
            return null;
        }

        //Computes a location for the AI to attack and returns the results of the attack and the coordinates that were attacked in an array of arrays
        public Array[] AIAttack()
        {
            Array[] AIResults = new Array[2];

            Random aiAttack = new Random();

            int[] aiCoords = new int[2];
            bool[] aiCheck = new bool[2];

            while (aiCheck[1] == false)
            {
                int aiX = aiAttack.Next(Size);
                int aiY = aiAttack.Next(Size);
                aiCoords[0] = aiX;
                aiCoords[1] = aiY;

                aiCheck = Human.Attack(aiX, aiY);
            }

            HShips = Human.Ships.Count;

            AIResults[0] = aiCheck;
            AIResults[1] = aiCoords;

            if (aiCheck[0] == false)
            {
                Attacked.Add(aiCoords);
                Play("Miss.wav");
            }
            else if (aiCheck[0] == true)
            {
                Play("Hit.wav");
            }

            return AIResults;

        }

        //Plays the sound from the source passed in its parameters.
        public void Play(string sound)
        {
            soundPlayer = new SoundPlayer(sound);
            soundPlayer.Play();
        }

        //Verifies that Grids still contain ships, setting IsGameOver to true if one of them doesn't and supplying the appropriate Message and Winner of the game. 
        public void EndGame()
        {
            if (Human.Ships.Count == 0)
            {
                EndSound = "Lost.wav";
                IsGameOver = true;
                Message = "TOO BAD. I WON!!!";
                Winner = "Computer";
            }

            if (AI.Ships.Count == 0)
            {
                EndSound = "Win.wav";
                IsGameOver = true;
                Message = "NICE. YOU WON!!!";
                Winner = "Human";
            }
        }

        //Shows the state of the grid in passed into its parameter
        public string UpdateState(OceanGrid ocean)
        {
            Debug.WriteLine("Board:");

            StringBuilder fnlState = new StringBuilder();

            for (int x = 0; x < Size; x++)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("\r");
                for (int y = 0; y < Size; y++)
                {
                    switch (ocean.BoardLoc[x, y])
                    {
                        case OceanGrid.States.Ship:
                            builder.Append('X');
                            break;
                        case OceanGrid.States.Hit:
                            builder.Append('*');
                            break;
                        case OceanGrid.States.Missed:
                            builder.Append('O');
                            break;
                        default:
                            builder.Append('~');
                            break;
                    }
                    //if (ocean.BoardLoc[x, y] == OceanGrid.States.Ship)
                    //{
                    //    builder.Append('X');
                    //}
                    //else
                    //{
                    //    builder.Append('~');
                    //}
                }

                string state = builder.ToString();
                fnlState.AppendFormat("{0}\r\n", state);
                Debug.WriteLine(state);

            }
            Debug.WriteLine("\n");

            return fnlState.ToString();

        }

        //Event handler for the ProperyChanged event Notifying the object bound of the change in the source in its parameters. 
        protected void SetProperty(string source)
        {
            PropertyChangedEventHandler handle = PropertyChanged;
            if (handle != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(source));
            }
        }
    }
}
