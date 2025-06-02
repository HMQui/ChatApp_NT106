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

            string query2 = "SELECT friend_id FROM Friends WHERE user_id = @param0 AND status IN (@param1, @param2, @param3)";
            var parameters2 = new object[] { idUser, "accepted", "blocked", "block" };
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

        public List<UserFriendDTO> GetFriendsWithStatus(string emailUser)
        {
            List<UserFriendDTO> friendsList = new List<UserFriendDTO>();

            string query1 = "SELECT id FROM Users WHERE email = @param0";
            var parameters1 = new object[] { emailUser };
            var result1 = DataProvider.Instance.ExcuteQuery(query1, parameters1);
            if (result1.Rows.Count == 0) return friendsList;

            string idUser = result1.Rows[0]["id"].ToString();

            string query2 = "SELECT friend_id, status FROM Friends WHERE user_id = @param0 AND status IN (@param1, @param2, @param3)";
            var parameters2 = new object[] { idUser, "accepted", "blocked", "block" };
            var result2 = DataProvider.Instance.ExcuteQuery(query2, parameters2);

            foreach (DataRow row in result2.Rows)
            {
                string friendId = row["friend_id"].ToString();
                string friendStatus = row["status"].ToString();

                string userQuery = "SELECT * FROM Users WHERE id = @param0";
                var userResult = DataProvider.Instance.ExcuteQuery(userQuery, new object[] { friendId });

                if (userResult.Rows.Count > 0)
                {
                    DataRow userRow = userResult.Rows[0];
                    friendsList.Add(new UserFriendDTO
                    {
                        Id = Convert.ToInt32(userRow["id"]),
                        Email = userRow["email"].ToString(),
                        FullName = userRow["full_name"].ToString(),
                        AvatarUrl = userRow["avatar_url"] == DBNull.Value ? null : userRow["avatar_url"].ToString(),
                        Status = userRow["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(userRow["created_at"]),
                        IsVerified = Convert.ToBoolean(userRow["is_verified"]),
                        FriendStatus = friendStatus // Gán status từ bảng Friends
                    });
                }
            }

            return friendsList;
        }

        public List<UserFriendDTO> GetFriendsRequest(string emailUser)
        {
            List<UserFriendDTO> friendsList = new List<UserFriendDTO>();

            string query1 = "SELECT id FROM Users WHERE email = @param0";
            var parameters1 = new object[] { emailUser };
            var result1 = DataProvider.Instance.ExcuteQuery(query1, parameters1);
            if (result1.Rows.Count == 0) return friendsList;

            string idUser = result1.Rows[0]["id"].ToString();

            string query2 = "SELECT friend_id, status FROM Friends WHERE user_id = @param0 AND status IN (@param1, @param2)";
            var parameters2 = new object[] { idUser, "send_request", "receive_request", };
            var result2 = DataProvider.Instance.ExcuteQuery(query2, parameters2);

            foreach (DataRow row in result2.Rows)
            {
                string friendId = row["friend_id"].ToString();
                string friendStatus = row["status"].ToString();

                string userQuery = "SELECT * FROM Users WHERE id = @param0";
                var userResult = DataProvider.Instance.ExcuteQuery(userQuery, new object[] { friendId });

                if (userResult.Rows.Count > 0)
                {
                    DataRow userRow = userResult.Rows[0];
                    friendsList.Add(new UserFriendDTO
                    {
                        Id = Convert.ToInt32(userRow["id"]),
                        Email = userRow["email"].ToString(),
                        FullName = userRow["full_name"].ToString(),
                        AvatarUrl = userRow["avatar_url"] == DBNull.Value ? null : userRow["avatar_url"].ToString(),
                        Status = userRow["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(userRow["created_at"]),
                        IsVerified = Convert.ToBoolean(userRow["is_verified"]),
                        FriendStatus = friendStatus // Gán status từ bảng Friends
                    });
                }
            }

            return friendsList;
        }

        public bool UnblockFriendByEmail(string email1, string email2)
        {
            string getIdQuery = "SELECT id FROM Users WHERE email = @param0";

            object userIdObj = DataProvider.Instance.ExcuteScalar(getIdQuery, new object[] { email1 });
            object friendIdObj = DataProvider.Instance.ExcuteScalar(getIdQuery, new object[] { email2 });

            if (userIdObj == null || friendIdObj == null)
                return false; // Một trong hai email không tồn tại

            int userId = Convert.ToInt32(userIdObj);
            int friendId = Convert.ToInt32(friendIdObj);

            string updateQuery = @"
            UPDATE friends
            SET status = 'accepted'
            WHERE 
                (user_id = @param0 AND friend_id = @param1 AND status = 'block')
                OR
                (user_id = @param1 AND friend_id = @param0 AND status = 'blocked')";

            int result = DataProvider.Instance.ExcuteNonQuery(updateQuery, new object[] { userId, friendId });

            return result > 0;
        }

        public bool BlockFriendByEmail(string email1, string email2)
        {
            string getIdQuery = "SELECT id FROM Users WHERE email = @param0";

            object userIdObj = DataProvider.Instance.ExcuteScalar(getIdQuery, new object[] { email1 });
            object friendIdObj = DataProvider.Instance.ExcuteScalar(getIdQuery, new object[] { email2 });

            if (userIdObj == null || friendIdObj == null)
                return false; // Một trong hai email không tồn tại

            int userId = Convert.ToInt32(userIdObj);
            int friendId = Convert.ToInt32(friendIdObj);

            string updateQuery = @"
            UPDATE friends
            SET status = CASE 
                            WHEN user_id = @param0 AND friend_id = @param1 THEN 'block'
                            WHEN user_id = @param1 AND friend_id = @param0 THEN 'blocked'
                         END
            WHERE 
                (user_id = @param0 AND friend_id = @param1)
                OR
                (user_id = @param1 AND friend_id = @param0)";

            int result = DataProvider.Instance.ExcuteNonQuery(updateQuery, new object[] { userId, friendId });

            return result > 0;
        }

        public bool UnfriendByEmail(string email1, string email2)
        {
            string getIdQuery = "SELECT id FROM Users WHERE email = @param0";

            object userIdObj = DataProvider.Instance.ExcuteScalar(getIdQuery, new object[] { email1 });
            object friendIdObj = DataProvider.Instance.ExcuteScalar(getIdQuery, new object[] { email2 });

            if (userIdObj == null || friendIdObj == null)
                return false; // Một trong hai email không tồn tại

            int userId = Convert.ToInt32(userIdObj);
            int friendId = Convert.ToInt32(friendIdObj);

            string deleteQuery = @"
            DELETE FROM friends
            WHERE 
                (user_id = @param0 AND friend_id = @param1)
                OR
                (user_id = @param1 AND friend_id = @param0)";

                    int result = DataProvider.Instance.ExcuteNonQuery(deleteQuery, new object[] { userId, friendId });

                    return result > 0;
        }

        public bool SendFriendRequest(string userEmail, string friendEmail)
        {
            // Lấy ID của userEmail
            string queryUserId = "SELECT id FROM Users WHERE email = @param0";
            var userIdTable = DataProvider.Instance.ExcuteQuery(queryUserId, new object[] { userEmail });
            if (userIdTable.Rows.Count == 0) return false;
            int userId = Convert.ToInt32(userIdTable.Rows[0]["id"]);

            // Lấy ID của friendEmail
            string queryFriendId = "SELECT id FROM Users WHERE email = @param0";
            var friendIdTable = DataProvider.Instance.ExcuteQuery(queryFriendId, new object[] { friendEmail });
            if (friendIdTable.Rows.Count == 0) return false;
            int friendId = Convert.ToInt32(friendIdTable.Rows[0]["id"]);

            // Thêm bản ghi send_request
            string insertSend = "INSERT INTO Friends (user_id, friend_id, status) VALUES (@param0, @param1, 'send_request')";
            DataProvider.Instance.ExcuteNonQuery(insertSend, new object[] { userId, friendId });

            // Thêm bản ghi receive_request
            string insertReceive = "INSERT INTO Friends (user_id, friend_id, status) VALUES (@param0, @param1, 'receive_request')";
            DataProvider.Instance.ExcuteNonQuery(insertReceive, new object[] { friendId, userId });

            return true;
        }

        public void AcceptFriendRequest(string userEmail, string friendEmail)
        {
            string query = @"
            DECLARE @UserId INT, @FriendId INT;

            SELECT @UserId = id FROM Users WHERE email = @param0;
            SELECT @FriendId = id FROM Users WHERE email = @param1;

            UPDATE Friends
            SET status = 'accepted'
            WHERE (user_id = @UserId AND friend_id = @FriendId AND status = 'receive_request')
               OR (user_id = @FriendId AND friend_id = @UserId AND status = 'send_request');
        ";

            object[] parameters = new object[] { userEmail, friendEmail };
            DataProvider.Instance.ExcuteNonQuery(query, parameters);
        }

    }
}
