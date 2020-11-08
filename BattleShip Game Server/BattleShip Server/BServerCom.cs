//--------------------------------------------------------------------------------------------
//File:   BServerCom.cs
//Desc:   This program defines a class BServerCom which handles communication between the game 
//          the player client.
//---------------------------------------------------------------------------------------------


using Battleship;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip_Server
{
    class BServerCom : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string LOCK = "Lock";

        public const int Port = 6500;
        public TcpListener BServer;
        bool connected = false;

        private string gameNames;                                      //Contains the string representation of the game names
        public string StrNames
        {
            get
            {
                return gameNames;
            }
            set
            {
                gameNames += value;
                SetProperty("StrNames");
            }
        }
        Dictionary<string, Game> games = new Dictionary<string, Game>(); //Dictionay to hold the game instances
        public Dictionary<string, Game> Games
        {
            get
            {
                return games;
            }
            set
            {
                games = value;
                SetProperty("Games");
            }
        }
        public Dictionary<string, Game>.KeyCollection GameNames;            //holds a collection of the names of the games that have been created.

        private StringBuilder messages = new StringBuilder();               //stringbuilder used by the Log Messages
        public string Messages
        {
            get
            {
                return messages.ToString();
            }
            set
            {
                messages.AppendLine(value);
                SetProperty("Messages");
            }
        }

        //Constructor for the BServer class, initializing the GameNames Property
        public BServerCom()
        {
            BServer = new TcpListener(IPAddress.Any, Port);
            BServer.Start();

            GameNames = new Dictionary<string, Game>.KeyCollection(games);
        }        

        //Handles communication between client in its param and the listener.
        public void Commun(TcpClient player)
        {
            string address = player.Client.RemoteEndPoint.ToString();
            LogMsg("Connection request from: " + address);

            connected = true;
            try
            {
                using (NetworkStream conctn = player.GetStream())
                {
                    Game game;

                    StreamReader reader = new StreamReader(conctn);
                    StreamWriter writer = new StreamWriter(conctn);

                    writer.WriteLine("Welcome to Battleship. What game do you wish to join? ");
                    writer.Flush();

                    do
                    {
                        if (player.Available > 0)
                        {
                            string gameName = reader.ReadLine();

                            lock (LOCK)
                            {
                                if (!GameNames.Contains(gameName))
                                {
                                    game = new Game(10);

                                    Games.Add(gameName, game);
                                    game.Players.Add(address);
                                    StrNames = gameName + "\n";
                                }
                                else
                                {
                                    game = Games[gameName];
                                    game.Players.Add(address);
                                }
                            }                            

                            writer.WriteLine(GameState(game));
                            writer.Flush();

                            while (connected)
                            {
                                string request = reader.ReadLine();
                                string response = null;

                                while (request != null)
                                {
                                    LogMsg(gameName + " Request: " + request);
                                    try
                                    {                                        
                                        lock (LOCK)
                                        {
                                            var requestQ = Request.Deserialize(request);
                                            var responseQ = requestQ.Execute(game);
                                            response = responseQ.Serialize();
                                        }
                                    }                             
                                    catch
                                    {
                                        response = null;
                                    }                                    

                                    writer.WriteLine(response);
                                    writer.Flush();
                                    LogMsg("Response:\n" + response);

                                    request = reader.ReadLine();
                                }
                            }
                        }
                    } while(player != null);

                    LogMsg("Player disconnected.");
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Check: " + ex.Message);
            }
        }

        //Retries the state of the game in its params and returns a string representation of the boards
        public string GameState(Game g)
        {
            string status = "active";
            if (g.IsGameOver == true)
            {
                status = "ended " + g.Winner;
            }
            string states = LogMsg("GameStateResponse " + status + "\n" + g.UpdateState(g.Human) + "---\n" + g.UpdateState(g.AI) + "\n");
            return states;
        }

        //Loads the message in its params to the Messages property and returns the message in its params
        string LogMsg(string msg)
        {
            Messages = msg + "\n";
            return msg;
        }

        protected void SetProperty(string source)
        {
            PropertyChangedEventHandler handle = PropertyChanged;
            if (handle != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(source));
            }
        }
    }

    class Player : IDisposable
    {
        public StreamReader Reader { get; }

        public StreamWriter Writer { get; }

        public TcpClient PlayerClient { get; set; }

        private NetworkStream stream;

        public Player(TcpListener listener)
        {
            PlayerClient = listener.AcceptTcpClient();
            stream = PlayerClient.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);

        }

        public void Dispose()
        {
            PlayerClient.Close();
        }
    }
}
