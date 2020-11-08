//--------------------------------------------------------------------------------------------
//File:   Response.cs
//Desc:   This program defines a base class Response and the subclasses used to formulate a 
//          response to send to the player client.
//---------------------------------------------------------------------------------------------

using Battleship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_Server
{
    //base class for Response
    public abstract class Response
    {
        public abstract string Serialize();
    }
    
    //contains the response for the demo of the server
    class Demo: Response
    {
        //Returns a demo of the format of the GameState messages
        public override string Serialize()
        {
            return "GameStateResponse " + "active" + @"
~~~~~X~~~~
~~X~~*~~~~
~O~~~~~~~~
~~~~~~~~~~
~~~~*~~~~~
~X~~~~~~~~
~~~~~~~~~~
~~~~~~~~~~
~~~~~~~~~~
~~~~~~~~~~
---
~~~~~~~~~~
~~~~~~~~~~
~~X~~*~~~~
~O~~~~~~~~
~~~~~X~~~~
~~~~~~~~~~
~~~~*~~~~~
~X~~~~~~~~
~~~~~~~~~~
~~~~~~~~~~
";
        }

    }

    //Contains logic for the Attack Response
    class AttackResponse: Response
    {
        public string HResult { get; set; }
        public int[] AICoords { get; set; }
        public string result;
        
        //Constructor for the AttackResponse class that recieves the human attack results and the coords of the AI
        public AttackResponse(string hResult, int[] coords)
        {
            HResult = hResult;
            AICoords = coords;
            result = "invalid";
        }

        //Serializes the results of the both the human and AI attacks
        public override string Serialize()
        {
            if (HResult == "invalid" || HResult == "dup")
            {
                result = HResult;
            }
            else
            {
                result = HResult + " " + AICoords[0].ToString() + " " + AICoords[1].ToString();
            }

            return "AttackResponse " + result;
        }

    }
    
    //Contains logic for the GameState Response
    class GameStateResponse: Response
    {
        public string Status { get; set; }
        Game Game { get; set; }

        //Contructor for the GameStateResponse class that receives the status(active, ended) of the game and the game itself in the params
        public GameStateResponse(string status, Game g)
        {
            Game = g;
            Status = status;
        }

        //Serializes the state of the Board returning a string
        public override string Serialize()
        {
            string state = "GameStateResponse " + Status + "\n" + Game.UpdateState(Game.Human) + "---\n" + Game.UpdateState(Game.AI) + "\n";
            return state;
        }
    }

}
