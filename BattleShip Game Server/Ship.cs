//--------------------------------------------------------------------------------------------
//File:   Ship.cs
//Desc:   This program defines instance variables and constructors for a class Ship.
//---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship
{
    public class Ship
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Length { get; set; }
        public int Orient { get; set; }
        public int ActlSize { get; set; }                       //Actual size of ship

        //constructor for Ship that designates a postion upon instantiation
        public Ship(int x, int y)
        {
            X = x;
            Y = y;
        }

        //Overload of the constructor of the Ship that takes a length and an orientation(1 = Horizontal, 2=Vertical)
        public Ship(int x, int y, int length, int orient)
        {
            X = x;
            Y = y;
            Length = length;
            Orient = orient;
            ActlSize = length;
        }

        //Returns the starting coordinates of a ship (unused as of yet)
        public int[] GetCoords()
        {
            int[] coords = new int[] { X, Y };
            return coords;
        }
    }
}
