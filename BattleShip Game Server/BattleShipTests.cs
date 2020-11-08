//--------------------------------------------------------------------------------------------
//File:   BattleShipTest.cs
//Desc:   This program defines a class BattleShipTests which contains test for the method
//               PlaceShips.
//---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Battleship;

[TestFixture]
public class BattleShipTests
{
    [Test]
    public void PlaceShips_ChecksShipsAmount_ShipAmountCorrect()
    {
        OceanGrid grid = new OceanGrid(5);

        for (int i = 0; i < grid.NumRows; i++)
        {
            grid.AddShip(i, i);
        }

        Assert.IsTrue(grid.Ships.Count == 5);        
    }

    [Test]
    public void PlaceShips_ChecksShipPosition_ShipPositionIsCorrect()
    {
        OceanGrid grid = new OceanGrid(5);

        for(int i = 0; i < grid.Ships.Count; i++)
        {
            grid.AddShip(i, i);
        }

        for(int e = 0; e < grid.Ships.Count; e++)
        {
            Assert.IsTrue(grid.BoardLoc[e, e] == OceanGrid.States.Ship);       
        }
    }

    //[Test]
    //public void PlaceAi_AddsCorrectNumberOfShipstoAiGrid_CorrectAmountOfShips()
    //{
    //    Game game = new Game(5);
    //    for (int i = 0; i < 1000; i++)
    //    {
    //        game.PlaceAi();
    //        Assert.IsTrue(game.AI.Ships.Count == 5);
    //        game.AI.Ships.Clear();
    //    }
    //}
}
