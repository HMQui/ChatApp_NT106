using ChatApp.Common.DTOs;
using System.Data;

namespace ChatApp.Common.DAO
{
    public class GroupDAO
    {
        private static GroupDAO instance;
        public static GroupDAO Instance
        {
            get { if (instance == null) instance = new GroupDAO(); return instance; }
            private set { instance = value; }
        }
        private GroupDAO() { }

        public GroupDTO CreateGroup(string groupName, int createdBy)
        {
            string query = @"
                INSERT INTO Groups (group_name, created_by, created_at)
                OUTPUT INSERTED.id, INSERTED.group_name, INSERTED.created_by, INSERTED.created_at
                VALUES (@param0, @param1, GETDATE())";

            object[] parameters = { groupName, createdBy };
            DataTable dt = DataProvider.Instance.ExcuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new GroupDTO
                {
                    Id = Convert.ToInt32(row["id"]),
                    GroupName = row["group_name"].ToString(),
                    CreatedBy = Convert.ToInt32(row["created_by"]),
                    CreatedAt = Convert.ToDateTime(row["created_at"])
                };
            }

            return null;
        }

        public bool AddMember(int groupId, int userId)
        {
            string query = @"
                INSERT INTO GroupMembers (group_id, user_id, joined_at)
                VALUES (@param0, @param1, GETDATE())";

            object[] parameters = { groupId, userId };
            int result = DataProvider.Instance.ExcuteNonQuery(query, parameters);
            return result > 0;
        }

        public List<GroupDTO> GetUserGroups(int userId)
        {
            string query = @"
                SELECT g.* 
                FROM Groups g
                JOIN GroupMembers gm ON g.id = gm.group_id
                WHERE gm.user_id = @param0";

            object[] parameters = { userId };
            DataTable dt = DataProvider.Instance.ExcuteQuery(query, parameters);

            List<GroupDTO> groups = new List<GroupDTO>();
            foreach (DataRow row in dt.Rows)
            {
                groups.Add(new GroupDTO
                {
                    Id = Convert.ToInt32(row["id"]),
                    GroupName = row["group_name"].ToString(),
                    CreatedBy = Convert.ToInt32(row["created_by"]),
                    CreatedAt = Convert.ToDateTime(row["created_at"])
                });
            }

            return groups;
        }

        public List<GroupMemberDTO> GetGroupMembers(int groupId)
        {
            string query = @"
                SELECT gm.*, u.*
                FROM GroupMembers gm
                JOIN Users u ON gm.user_id = u.id
                WHERE gm.group_id = @param0";

            object[] parameters = { groupId };
            DataTable dt = DataProvider.Instance.ExcuteQuery(query, parameters);

            List<GroupMemberDTO> members = new List<GroupMemberDTO>();
            foreach (DataRow row in dt.Rows)
            {
                members.Add(new GroupMemberDTO
                {
                    Id = Convert.ToInt32(row["id"]),
                    GroupId = Convert.ToInt32(row["group_id"]),
                    UserId = Convert.ToInt32(row["user_id"]),
                    JoinedAt = Convert.ToDateTime(row["joined_at"]),
                    User = new UserDTO
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Email = row["email"].ToString(),
                        FullName = row["full_name"].ToString(),
                        AvatarUrl = row["avatar_url"] == DBNull.Value ? null : row["avatar_url"].ToString(),
                        Status = row["status"].ToString(),
                        CreatedAt = Convert.ToDateTime(row["created_at"]),
                        IsVerified = Convert.ToBoolean(row["is_verified"])
                    }
                });
            }

            return members;
        }

        public bool RemoveMember(int groupId, int userId)
        {
            string query = @"
                DELETE FROM GroupMembers 
                WHERE group_id = @param0 AND user_id = @param1";

            object[] parameters = { groupId, userId };
            int result = DataProvider.Instance.ExcuteNonQuery(query, parameters);
            return result > 0;
        }

        public bool UpdateGroupInfo(int groupId, string groupName)
        {
            string query = @"
                UPDATE Groups 
                SET group_name = @param0
                WHERE id = @param1";

            object[] parameters = { groupName, groupId };
            int result = DataProvider.Instance.ExcuteNonQuery(query, parameters);
            return result > 0;
        }
    }
} 