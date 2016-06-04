using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using Newtonsoft.Json;

namespace AngularTest.Hub
{
    public static class GameHolder
    {
        public static List<Game> GameList = new List<Game>();

        public static List<GameHistory> HistoryList = new List<GameHistory>();

        private static int PrevId = -1; //Used to mimic sql behaviour.

        public static GameHistory GetHistory(int GameId)
        {
            GameHistory history = HistoryList.FirstOrDefault(x => x.GameId == GameId);

            return history;
        }

        public static Game PlayerJoin(UserHolder.User User, int GameId)
        {
            Game game = GetGame(GameId);

            game.PlayerJoin(User);

            return game;
        }

        public static Game GetGame(int GameId)
        {
            return GameList.FirstOrDefault(x => x.GameId == GameId);
        }
        /// <summary>
        /// Resolves a game if a players decides to leave.
        /// </summary>
        /// <param name="LeaverId">The leavers user id.</param>
        /// <param name="GameId">The game that the player left.</param>
        /// <returns>The player who is lefts connection string.</returns>
        public static string ResolveGame(int LeaverId, int GameId)
        {
            Game game = GetGame(GameId);
            if (game == null)
            {
                if (game.PlayerTwo == null) //If a game wasn't even started, skip this.
                {
                    if (game.PlayerOne.Id == LeaverId)
                    {
                        HistoryList.Add(game.ConcludeGame(2, "Opponent left the game!"));

                        return game.PlayerTwo.GetConId();
                    }
                    else if (game.PlayerTwo.Id == LeaverId)
                    {
                        HistoryList.Add(game.ConcludeGame(1, "Opponent left the game!"));

                        return game.PlayerOne.GetConId();
                    }
                }

                RemoveGame(GameId);
            }

            return "";
        }

        public static void RemoveGame(int GameId)
        {
            GameList.RemoveAll(x => x.GameId == GameId);
        }

        public class Game
        {
            [JsonProperty("gameId")]
            public int GameId { get; set; }
            private int HostId { get; set; }
            [JsonProperty("tiles")]
            public int[] Tiles { get; set; } //The tiles of the board. 
            [JsonProperty("currentPlayer")]
            public sbyte CurrentPlayer { get; set; } //Keeps track of whos turn it is.(Should be either 1 or 2)
            [JsonProperty("playerOne")]
            public UserHolder.User PlayerOne { get; set; }
            [JsonProperty("playerTwo")]
            public UserHolder.User PlayerTwo { get; set; }
            private string[] TurnsMade { get; set; } //Keeps track of the moves made, to later be saved.

            private int Turn = 1; //Keeps track of what turn it is.

            /// <summary>
            /// Checks if a move is legal.
            /// </summary>
            /// <param name="UserId">The user id for the one making the move.</param>
            /// <param name="TileNumber">The tile to be taken.</param>
            /// <returns></returns>
            public bool CheckLegalMove(int UserId, int TileNumber)
            {
                if (Tiles[TileNumber] == 0)
                {
                    if (PlayerOne.Id == UserId && CurrentPlayer == 1)
                    {
                        return true;
                    }
                    else if (PlayerTwo.Id == UserId && CurrentPlayer == 2)
                    {
                        return true;
                    }
                }

                return false;
            }
            /// <summary>
            /// Makes the wanted move and returns the new board.
            /// </summary>
            /// <param name="TileNumber"> The tile to be taken.</param>
            /// <param name="Player"> If player one or player two is making the move .(Should be either 1 or 2)</param>
            /// <returns></returns>
            public int[] MakeMove(int TileNumber)
            {
                Tiles[TileNumber] = CurrentPlayer;

                TurnsMade[Turn - 1] = "Player " + CurrentPlayer + " took tile " + (TileNumber + 1); //Save the turn to history.

                if (CurrentPlayer == 1)
                {
                    CurrentPlayer = 2;
                }
                else
                {
                    CurrentPlayer = 1;
                }

                Turn++;
                return Tiles;
            }
            /// <summary>
            /// Checks if anyone has won, ends game if winner is found.
            /// </summary>
            /// <returns> True if winner was found. Or if its a draw.</returns>
            public bool CheckWinner()
            {
                GameHistory history = null;

                #region Horizontal
                if (Tiles[0] == Tiles[1] && Tiles[1] == Tiles[2] && Tiles[0] != 0) //Top
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[0] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                else if (Tiles[3] == Tiles[4] && Tiles[4] == Tiles[5] && Tiles[3] != 0) //Middle
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[3] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                else if (Tiles[6] == Tiles[7] && Tiles[7] == Tiles[8] && Tiles[6] != 0) //Bottom
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[6] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                #endregion
                #region Vertical
                if (Tiles[0] == Tiles[3] && Tiles[3] == Tiles[6] && Tiles[0] != 0) //Left
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[0] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                else if (Tiles[1] == Tiles[4] && Tiles[4] == Tiles[7] && Tiles[1] != 0) //Middle
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[1] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                else if (Tiles[2] == Tiles[5] && Tiles[5] == Tiles[8] && Tiles[2] != 0) //Right
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[2] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                #endregion
                #region Diagonal
                if (Tiles[0] == Tiles[4] && Tiles[4] == Tiles[8] && Tiles[0] != 0)
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[0] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                else if (Tiles[2] == Tiles[4] && Tiles[4] == Tiles[6] && Tiles[2] != 0)
                {
                    history = ConcludeGame(Convert.ToSByte(Tiles[0]), "Player " + Tiles[2] + " won the game");
                    HistoryList.Add(history);
                    return true;
                }
                #endregion

                if (Turn >= 9) //Draw
                {
                    history = ConcludeGame(0, "Draw!");
                    HistoryList.Add(history);
                    return true;
                }

                return false;
            }
            public void PlayerJoin(UserHolder.User NewUser)
            {
                if (PlayerTwo == null) //If game is not full.
                {
                    PlayerTwo = NewUser;
                }
            }
            public GameHistory ConcludeGame(sbyte PlayerNumber,string lastMessage)
            {
                if (PlayerNumber == 1)
                {
                    return new GameHistory(GameId,PlayerOne.Username,TurnsMade,lastMessage);
                }
                else if (PlayerNumber == 2)
                {
                    return new GameHistory(GameId, PlayerTwo.Username, TurnsMade, lastMessage);
                }
                else
                {
                    return new GameHistory(GameId, "", TurnsMade, lastMessage);
                }
            }
            /// <summary>
            /// Serializes the game object into json.
            /// </summary>
            /// <returns>Json string of the game.</returns>
            public string ToJson()
            {
                return JsonConvert.SerializeObject(this);
            }
            public Game(UserHolder.User Host)
            {
                GameId = GetId();
                HostId = Host.Id;

                PlayerOne = Host;

                Tiles = new int[9];
                CurrentPlayer = 1; //TODO: Randomise starting player?

                TurnsMade = new string[9];

                GameList.Add(this);
            }
            private int GetId()
            {
                PrevId++;
                return PrevId;
            }
        }

        public class GameHistory
        {
            public int GameId { get; set; }
            public string WinningPlayer { get; set; }
            public string[] TurnsMade { get; set; }
            public string WMessage { get; set; }
            public GameHistory(int gameId,string WinPlayer,string[] turnsmade,string WinningMessage)
            {
                GameId = gameId;
                WinningPlayer = WinPlayer;
                TurnsMade = turnsmade;
                WMessage = WinningMessage;
            }
        }
    }
}