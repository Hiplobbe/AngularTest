using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.Win32;

namespace ChallengerModeTest.Hub
{
    public class GameHub : Microsoft.AspNet.SignalR.Hub
    {
        public int StartGame(int HostId)
        {
            try
            {
                UserHolder.User u = UserHolder.GetUser(HostId);
                if (u != null)
                {
                    GameHolder.Game g = new GameHolder.Game(u);
                    return g.GameId;
                }
            }
            catch (Exception e)
            {
                //Add wanted logger here
            }

            return -1;
        }
        public void MakeMove(int UserId, int GameId,int TileNumber)
        {
            GameHolder.Game game = GameHolder.GetGame(GameId);

            if (game.CheckLegalMove(UserId, TileNumber))
            {
                game.MakeMove(TileNumber); //Makes the move on the board.

                if (game.CheckWinner()) //Check if game is over.
                {
                    GameHolder.RemoveGame(GameId);
                    GameHolder.GameHistory hist = GameHolder.GetHistory(GameId);

                    //Messages the players that the game is over.
                    Clients.Client(game.PlayerOne.GetConId()).matchDone(hist.WMessage);
                    Clients.Client(game.PlayerTwo.GetConId()).matchDone(hist.WMessage);
                }
                else
                {
                    UpdateBoard(game);
                }
            }
        }
        public void AcceptInvite(int UserId, int GameId,int OldGameId)
        {
            if(OldGameId != -1) //If the player was already in a game.
            {
                string cs = GameHolder.ResolveGame(UserId, GameId);

                if (cs != "")
                {
                    Clients.Client(cs).matchDone("Opponent left the game, you win!");
                }
            }
            //TODO: Check if game is already full.
            GameHolder.Game game = GameHolder.PlayerJoin(UserHolder.GetUser(UserId), GameId);

            UpdateBoard(game);
        }
        public void UpdateBoard(GameHolder.Game game)
        {
            Clients.Client(game.PlayerOne.GetConId()).updateBoard(game.ToJson());
            Clients.Client(game.PlayerTwo.GetConId()).updateBoard(game.ToJson());
        }
    }
}