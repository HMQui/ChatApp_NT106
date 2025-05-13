using ChatApp.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Common.DAO
{
    public class FriendDAO
    {
        private static FriendDAO instance;
        public static FriendDAO Instance
        {
            get { if (instance == null) instance = new FriendDAO(); return instance; }
            private set { instance = value; }
        }
        private FriendDAO() { }

        public List<UserDTO> GetFriends(string emailUser)
        {
            List<UserDTO> friendsList = new List<UserDTO>();

            string query1 = "SELECT id FROM Users WHERE email = @param0";
            var parameters1 = new object[] { emailUser };
            var result1 = DataProvider.Instance.ExcuteQuery(query1, parameters1);
            if (result1.Rows.Count == 0) return friendsList;

            string idUser = result1.Rows[0]["id"].ToString();

            string query2 = "SELECT friend_id FROM Friends WHERE user_id = @param0 AND status = @param1";
            var parameters2 = new object[] { idUser, "accepted" };
            var result2 = DataProvider.Instance.ExcuteQuery(query2, parameters2);

            foreach (DataRow row in result2.Rows)
            {
                string friendId = row["friend_id"].ToString();

                string userQuery = "SELECT * FROM Users WHERE id = @param0";
                var userResult = DataProvider.Instance.ExcuteQuery(userQuery, new object[] { friendId });

                if (userResult.Rows.Count > 0)
                {
                    DataRow userRow = userResult.Rows[0];
                    friendsList.Add(new UserDTO
                    {
                        Id = Convert.ToInt32(userRow["id"]),
                        Email = userRow["email"].ToString(),
                        FullName = userRow["full_name"].ToString(),
                        AvatarUrl = userRow["avatar_url"] == DBNull.Value ? null : userRow["avatarUrl"].ToString(),
                        Status = userRow["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(userRow["created_at"]),
                        IsVerified = Convert.ToBoolean(userRow["is_verified"])
                    });
                }
            }

            return friendsList;
        }

    }
}
