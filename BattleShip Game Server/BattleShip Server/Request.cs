//--------------------------------------------------------------------------------------------
//File:   Request.cs
//Desc:   This program defines a base class Request and the subclasses used to proccess the 
//          requests sent by player client.
//---------------------------------------------------------------------------------------------

using Battleship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_Server
{
    //Base class for handling all requests after the initial handshake
    public abstract class Request
    {      
        public abstract Response Execute(Game g);

        //Contains logic to deserialize the string request, returning a Request object base on the rqst TYPE;
        public static Request Deserialize(string rqst)
        {
            string[] request = rqst.Split(' ');
            string com = request[0];
            switch(com)
            {
                case DemoRequest.TYPE:
                    return new DemoRequest();
                case AttackRequest.TYPE:
                    AttackRequest attack = new AttackRequest(null, null);
                    return attack.Deserialize(request);
                case GameStateRequest.TYPE:
                    return new GameStateRequest();
                default:
                    throw new Exception("Not a command.");
            } 
        }
    }

    //Contains code for handling the demonstration of the server functionality
    public class DemoRequest: Request
    {
        public const string TYPE = "demo";

        //Implement the Execute method, but does not use the game in its params
        public override Response Execute(Game g)
        {
            return new Demo();
        }
    }

    //Contains logic for handling the Attack Requests
    public class AttackRequest: Request
    {
        public const string TYPE = "Attack";

        public string Row { get; set; }
        public string Col { get; set; }
        public string[] Request { get; set; }

        //constructor for the AttackRequest class
        public AttackRequest(string x, string y)
        {
            Row = x;
            Col = y;
        }

        //initializes an instance of the AttackRequest with the array in its params
        public Request Deserialize(string[] rqst)
        {
            Request = rqst;
            
            if(Request.Length == 3)
            {
                return new AttackRequest(Request[1], Request[2]);
            }
            return new AttackRequest(null, null);
        }

        //Executes the Attack command on the Game in its params, returning a new AttackResponse object 
        public override Response Execute(Game g)
        {
            string result = "invalid";
            string vldPlace = "0 1 2 3 4 5 6 7 8 9";

            string[] reqs = new string[] { Row, Col };


            if (vldPlace.Contains(reqs[0]) && vldPlace.Contains(reqs[1]))
            {
                int[] HCoords = new int[2] { Convert.ToInt32(reqs[0]), Convert.ToInt32(reqs[1]) };
                Array[] atResults = g.Attack(HCoords[0], HCoords[1]);

                if (!g.IsGameOver)
                {
                    bool[] hResults = (bool[])atResults[0];
                    int[] aiCoords = (int[])atResults[2];

                    string hResult = ProccessResults(hResults);

                    return new AttackResponse(hResult, aiCoords);                        
                }
            }
            return new AttackResponse(result, null);
        }

        //Proccesses the results of the human attack, then returns the result
        string ProccessResults(bool[] hResults)
        {
            string hResult = "invalid";

            if (hResults[0] == true)
            {
                hResult = "hit";
            }
            else if (hResults[0] == false)
            {
                hResult = "missed";
            }
            if (hResults[1] == false)
            {
                hResult = "dup";
            }

            return hResult;
        }

    }

    //Contains logic for handling the GameState requests
    public class GameStateRequest: Request
    {
        public const string TYPE = "GameState";

        //Executes the GameState command on the Game in its params, then returns an instance of the GameStateResponse class
        public override Response Execute(Game g)
        {
            string status = "active";
            if (g.IsGameOver == true)
            {
                status = "ended " + g.Winner;
            }

            return new GameStateResponse(status, g);
        }
    }

}
