using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace ChallengerModeTest.Hub
{
    public class MenuHub : Microsoft.AspNet.SignalR.Hub
    {
        public int LoginUser(string Username)
        {
            UserHolder.User u = new UserHolder.User(Username, Context.ConnectionId);
            UserHolder.UserList.Add(u);
            Clients.All.refreshList(UserHolder.UserList);

            return u.Id;
        }

        public void LogoutUser(int id,int gameId)
        {
            UserHolder.RemoveUser(id);

            if (gameId != -1) //If the player was in a game.
            {
                string cs = GameHolder.ResolveGame(id, gameId);

                if (cs != "")
                {
                    Clients.Client(cs).matchDone("Opponent left the game, you win!");
                }
            }

            Clients.All.refreshList(UserHolder.UserList);
        }

        public void InviteUser(int id,int gameId)
        {
            UserHolder.User u = UserHolder.GetUser(id);

            if (GameHolder.GetGame(gameId).PlayerTwo == null) //So to not send invite when game is full.
            {
                Clients.Client(u.GetConId()).recInvite(gameId, u.Username + " invited you to a game!");
            }
        }
        public void DeclineInvite(int UserId,int GameId)
        {
            UserHolder.User host = GameHolder.GetGame(GameId).PlayerOne;
            UserHolder.User invitedUser = UserHolder.GetUser(UserId);
            Clients.Client(host.GetConId()).declinedInvite(invitedUser.Username+" declined your invite!");
        }
        /// <summary>
        /// Posts the latest match to all players.
        /// </summary>
        public void ViewLatestMatch()
        {
            if (GameHolder.HistoryList.Count > 0)
            {
                Clients.All.latestMatch(
                    JsonConvert.SerializeObject(GameHolder.HistoryList[GameHolder.HistoryList.Count - 1]));
            }
        }
    }
    /// <summary>
    /// Used to keep a persisent list of users. (Simulates the behaviour of a SQL implementation)
    /// </summary>
    public static class UserHolder
    {
        public static List<User> UserList = new List<User>();
        private static int PrevId = -1; //Used to keep track of given ids.

        /// <summary>
        /// Simulates the sql behaviour of giving out ids.
        /// </summary>
        /// <returns> An id for the next user.</returns>
        private static int GetId()
        {
            PrevId++;
            return PrevId;
        }

        public static void RemoveUser(int UserId)
        {
            UserList.RemoveAll(x => x.Id == UserId);
        }

        public static User GetUser(int UserId)
        {
            return UserList.FirstOrDefault(x => x.Id == UserId);
        }

        public class User
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("username")]
            public string Username { get; set; }
            [JsonProperty("game")]
            public int GameId { get; set; }
            private string conId { get; set; }

            public string GetConId()
            {
                return conId;
            }

            public User(string username,string connectionId)
            {
                Id = GetId();
                Username = username;
                GameId = 0;
                conId = connectionId;
            }
        }
    }
}