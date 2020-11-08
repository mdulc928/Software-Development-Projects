//--------------------------------------------------------------------------------------------
//File:   OceanGrid.cs
//Desc:   This program defines the instance variables and methods of the class OceanGrid.
//---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship
{

    public class OceanGrid
    {
        private int numRows;
        public int NumRows { get { return numRows; } }
        int numCols;

        private int NumShips { get; set; }                           //Keeps track of the number of ships on the Ocean
        public List<Ship> Ships = new List<Ship>();                  //Container for ships in Grid

        public enum States { Vacant, Ship, Hit, Missed }              //Enumerates values for the state of the Ocean grids
        private States[,] boardLoc;
        public States[,] BoardLoc                                    //Return the state of board
        {
            get { return boardLoc; }
        }

        //Initializes OceanGrid class with the number of rows it should contain specified in the params.
        public OceanGrid(int rows)
        {
            numRows = rows;
            numCols = numRows;
            boardLoc = new States[numRows, numCols];
            PlaceVacant();
            NumShips = 5;
        }


        //this method places Vacant States in BoardLoc
        public void PlaceVacant()
        {
            NumShips = Ships.Count;

            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    boardLoc[r, c] = States.Vacant;
                }
            }
        }

        //Adds a ship object with the coordinates specified in the params to a list containing ship objects, updating the state of the index with the coordinates to Ship
        public void AddShip(int x, int y)
        {
            Ship ship = new Ship(x, y);
            Ships.Add(ship);

            boardLoc[ship.X, ship.Y] = States.Ship;
        }

        //Overload of AddShip that takes a length and an orientation (1 = Horizontal, 2 = Veritical)
        public void AddShip(int x, int y, int length, int orient)
        {
            if (orient == 1)
            {
                for (int c = 0; c < length; c++)
                {
                    int currY = y + c;
                    BoardLoc[x, currY] = States.Ship;
                }
            }
            else if (orient == 2)
            {
                for (int r = 0; r < length; r++)
                {
                    int currX = x + r;
                    BoardLoc[currX, y] = States.Ship;

                }
            }

            Ship ship = new Ship(x, y, length, orient);
            Ships.Add(ship);
        }

        //Checks whether the cells in BoardLoc within the range of the length of the Ship already has a ship, returning true if the cells are Vacant, false if not
        public bool TestLoc(int x, int y, int length, int orient)
        {
            bool valid = true;

            if (orient == 1)
            {
                if (y + length <= numCols)
                {
                    for (int c = 0; c < length; c++)
                    {
                        int currY = y + c;
                        if (BoardLoc[x, currY] == States.Ship)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                else
                {
                    valid = false;
                }

            }
            else if (orient == 2)
            {
                if (x + length <= numRows)
                {
                    for (int r = 0; r < length; r++)
                    {
                        int currX = x + r;
                        if (BoardLoc[currX, y] == States.Ship)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                else
                {
                    valid = false;
                }
            }

            return valid;
        }

        //Changes the state of the cell in BoardLoc with the coordinates specified in the params, returning an array containing 2 boolean values,
        //the first index is true if a Ship was hit, false if not; the second index is false if the cell is already Hit, and true if not.
        public bool[] Attack(int x, int y)
        {
            bool landed = false;
            bool validAttack = true;

            if (BoardLoc[x, y] == States.Ship)
            {
                Hit(x, y);
                landed = true;
            }
            else if (BoardLoc[x, y] == States.Vacant)
            {
                Miss(x, y);
            }
            else if (BoardLoc[x, y] == States.Hit)
            {
                validAttack = false;
            }

            bool[] stats = new bool[] { landed, validAttack };
            return stats;
        }

        //Changes the state of the cell with coordinates specified in the parameters to Hit
        public void Hit(int x, int y)
        {
            //NumShips -= 1;
            boardLoc[x, y] = States.Hit;

            foreach (Ship s in Ships)
            {
                if (s.Orient == 1)
                {
                    int extension = s.Y + s.Length - 1;
                    if (s.Y == 0)
                    {
                        extension = s.Y + s.Length;
                    }

                    if (extension >= y && s.X == x)
                    {
                        DecremShip(s);
                        break;
                    }
                }
                else if (s.Orient == 2)
                {
                    int extension = s.X + s.Length - 1;
                    if (s.X == 0)
                    {
                        extension = s.X + s.Length;
                    }
                    if (extension >= x && s.Y == y)
                    {
                        DecremShip(s);
                        break;
                    }
                }
                else if (s.Orient == 0 && s.X == x && s.Y == y)
                {
                    Ships.Remove(s);
                    NumShips--;
                    break;
                }
            }

        }

        //Changes the state of the cell with coordinates specified in the parameters to Missed
        public void Miss(int x, int y)
        {
            boardLoc[x, y] = States.Missed;
        }

        //Decrements the actual size of the Ship in its params, removing the Ship object from the list of Ships once the size is equal to zero 
        void DecremShip(Ship s)
        {
            s.ActlSize--;

            if (s.ActlSize == 0)
            {
                Ships.Remove(s);
                NumShips--;
            }
        }

        //Compares the coordinates of the ships in the list Ships to see if there values are equal to. 
        //Returns true if values match, false if they don't
        public bool GetShipCoord(int x, int y)
        {
            if (boardLoc[x, y] == States.Ship)
            {
                return true;
            }

            return false;
        }
    }
}
