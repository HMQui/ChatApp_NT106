using ChatApp.Common.DTOs;
using System.Data;
using System.Text;

namespace ChatApp.Common.DAO
{
    public class GroupMembersDAO
    {
        private static GroupMembersDAO instance;
        public static GroupMembersDAO Instance
        {
            get { if (instance == null) instance = new GroupMembersDAO(); return instance; }
            private set { instance = value; }
        }
        private GroupMembersDAO() { }

        public List<GroupMembersDTO> GetMembersByGroupId(int groupId)
        {
            List<GroupMembersDTO> members = new List<GroupMembersDTO>();

            string query = @"
                SELECT 
                    GM.id,
                    GM.group_id,
                    GM.nick_name,
                    GM.user_email AS email,
                    U.avatar_url AS avatar,
                    GM.joined_at
                FROM 
                    GroupMembers GM
                INNER JOIN 
                    Users U ON GM.user_email = U.email
                WHERE 
                    GM.group_id = @param0
                ORDER BY 
                    GM.joined_at";

            DataTable result = DataProvider.Instance.ExcuteQuery(query, new object[] { groupId });

            foreach (DataRow row in result.Rows)
            {
                GroupMembersDTO member = new GroupMembersDTO
                {
                    Id = (int)row["id"],
                    GroupId = (int)row["group_id"],
                    NickName = row["nick_name"].ToString(),
                    Email = row["email"].ToString(),
                    Avatar = row["avatar"] != DBNull.Value ? row["avatar"].ToString() : null,
                    JoinedAt = (DateTime)row["joined_at"]
                };

                members.Add(member);
            }

            return members;
        }

        public List<GroupMembersDTO> GetFilteredGroupMembers(int groupId, Dictionary<string, object> filter)
        {
            List<GroupMembersDTO> members = new List<GroupMembersDTO>();

            // Câu truy vấn cơ bản
            StringBuilder queryBuilder = new StringBuilder(@"
            SELECT 
                GM.id,
                GM.group_id,
                GM.nick_name,
                GM.user_email AS email,
                U.avatar_url AS avatar,
                GM.joined_at
            FROM 
                GroupMembers GM
            INNER JOIN 
                Users U ON GM.user_email = U.email
            WHERE 
                GM.group_id = @param0");

            // Danh sách tham số
            List<object> parameters = new List<object> { groupId };
            int paramIndex = 1; // Bắt đầu từ @param1 vì @param0 đã dùng cho groupId

            // Thêm điều kiện filter nếu có
            if (filter != null && filter.Count > 0)
            {
                foreach (var kvp in filter)
                {
                    switch (kvp.Key.ToLower())
                    {
                        case "nickname":
                            if (kvp.Value != null)
                            {
                                queryBuilder.Append(" AND GM.nick_name LIKE @param" + paramIndex);
                                parameters.Add("%" + kvp.Value.ToString() + "%");
                                paramIndex++;
                            }
                            break;

                        case "email":
                            if (kvp.Value != null)
                            {
                                queryBuilder.Append(" AND GM.user_email LIKE @param" + paramIndex);
                                parameters.Add("%" + kvp.Value.ToString() + "%");
                                paramIndex++;
                            }
                            break;

                        case "joinedafter":
                            if (kvp.Value is DateTime)
                            {
                                queryBuilder.Append(" AND GM.joined_at >= @param" + paramIndex);
                                parameters.Add((DateTime)kvp.Value);
                                paramIndex++;
                            }
                            break;

                        case "joinedbefore":
                            if (kvp.Value is DateTime)
                            {
                                queryBuilder.Append(" AND GM.joined_at <= @param" + paramIndex);
                                parameters.Add((DateTime)kvp.Value);
                                paramIndex++;
                            }
                            break;
                    }
                }
            }

            // Sắp xếp
            queryBuilder.Append(" ORDER BY GM.joined_at");

            // Thực thi truy vấn
            DataTable result = DataProvider.Instance.ExcuteQuery(queryBuilder.ToString(), parameters.ToArray());

            // Chuyển đổi kết quả
            foreach (DataRow row in result.Rows)
            {
                GroupMembersDTO member = new GroupMembersDTO
                {
                    Id = (int)row["id"],
                    GroupId = (int)row["group_id"],
                    NickName = row["nick_name"].ToString(),
                    Email = row["email"].ToString(),
                    Avatar = row["avatar"] != DBNull.Value ? row["avatar"].ToString() : null,
                    JoinedAt = (DateTime)row["joined_at"]
                };
                members.Add(member);
            }

            return members;
        }

        public GroupMembersDTO AddMemberToGroup(int groupId, string userEmail, string nickName)
        {
            // Thêm thành viên vào nhóm
            string insertQuery = @"
            INSERT INTO GroupMembers 
            (group_id, user_email, nick_name, joined_at)
            VALUES (@param0, @param1, @param2, @param3)";

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(insertQuery,
                new object[] { groupId, userEmail, nickName, DateTime.Now });

            if (rowsAffected > 0)
            {
                // Lấy thông tin thành viên vừa thêm
                string selectQuery = @"
                SELECT 
                    GM.id,
                    GM.group_id,
                    GM.nick_name,
                    GM.user_email,
                    U.avatar_url AS avatar,
                    GM.joined_at
                FROM 
                    GroupMembers GM
                JOIN 
                    Users U ON GM.user_email = U.email
                WHERE 
                    GM.group_id = @param0 
                    AND GM.user_email = @param1";

                DataTable result = DataProvider.Instance.ExcuteQuery(selectQuery,
                    new object[] { groupId, userEmail });

                if (result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    return new GroupMembersDTO
                    {
                        Id = (int)row["id"],
                        GroupId = (int)row["group_id"],
                        NickName = row["nick_name"].ToString(),
                        Email = row["user_email"].ToString(),
                        Avatar = row["avatar"] != DBNull.Value ? row["avatar"].ToString() : null,
                        JoinedAt = (DateTime)row["joined_at"]
                    };
                }
            }

            return null;
        }

        public bool RemoveMemberFromGroup(int groupId, string userEmail)
        {
            string query = @"
                DELETE FROM GroupMembers 
                WHERE group_id = @param0 AND user_email = @param1";

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(query,
                new object[] { groupId, userEmail });

            return rowsAffected > 0;
        }

        public bool IsMemberInGroup(int groupId, string userEmail)
        {
            string query = @"
                SELECT COUNT(1) 
                FROM GroupMembers 
                WHERE group_id = @param0 AND user_email = @param1";

            object result = DataProvider.Instance.ExcuteScalar(query,
                new object[] { groupId, userEmail });

            return Convert.ToInt32(result) > 0;
        }

        public List<UserDTO> GetFriendsNotInGroup(int groupId, string adminEmail)
        {
            List<UserDTO> friends = new List<UserDTO>();

            string query = @"
            SELECT U.id, U.email, U.full_name, U.avatar_url, U.status, U.created_at, U.is_verified
            FROM Friends F
            JOIN Users U ON F.friend_id = U.id
            WHERE F.user_id = (SELECT id FROM Users WHERE email = @param0)
            AND F.status = 'accepted'
            AND NOT EXISTS (
                SELECT 1 FROM GroupMembers GM 
                WHERE GM.group_id = @param1 
                AND GM.user_email = U.email
            )";

            DataTable result = DataProvider.Instance.ExcuteQuery(query,
                new object[] { adminEmail, groupId });

            foreach (DataRow row in result.Rows)
            {
                friends.Add(new UserDTO
                {
                    Id = (int)row["id"],
                    Email = row["email"].ToString(),
                    FullName = row["full_name"].ToString(),
                    AvatarUrl = row["avatar_url"] != DBNull.Value ? row["avatar_url"].ToString() : null,
                    Status = row["status"].ToString(),
                    CreatedAt = (DateTime)row["created_at"],
                    IsVerified = Convert.ToBoolean(row["is_verified"])
                });
            }

            return friends;
        }
    }
}