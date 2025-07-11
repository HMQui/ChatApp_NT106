using ChatApp.Common.DTOs;
using System.Data;
using System.Text;

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

        public List<GroupDTO> GetGroupsByUserEmail(string userEmail)
        {
            List<GroupDTO> groups = new List<GroupDTO>();

            string query = @"
                SELECT G.id, G.group_name, G.created_by, G.avatar_url, G.created_at
                FROM Groups G
                INNER JOIN GroupMembers GM ON G.id = GM.group_id
                WHERE GM.user_email = @param0 AND G.is_deleted = 0";

            DataTable result = DataProvider.Instance.ExcuteQuery(query, new object[] { userEmail });

            foreach (DataRow row in result.Rows)
            {
                GroupDTO group = new GroupDTO
                {
                    Id = (int)row["id"],
                    GroupName = row["group_name"].ToString(),
                    CreatedBy = row["created_by"].ToString(),
                    Avatar_URL = row["avatar_url"].ToString(),
                    CreatedAt = (DateTime)row["created_at"]
                };

                groups.Add(group);
            }

            return groups;
        }

        public int CreateGroup(GroupDTO group)
        {
            // Lấy nickname từ bảng Users
            string getFullNameQuery = @"
SELECT full_name
FROM Users
WHERE email = @param0";

            object fullNameObj = DataProvider.Instance.ExcuteScalar(getFullNameQuery, new object[]
            {
        group.CreatedBy
            });

            string nickName = fullNameObj != null ? fullNameObj.ToString() : "Unknown";

            int newGroupId = -1;

            if (string.IsNullOrEmpty(group.Avatar_URL))
            {
                // Trường hợp không có avatar → bỏ avatar_url ra khỏi query
                string insertGroupQuery = @"
INSERT INTO Groups (group_name, created_by, created_at)
OUTPUT INSERTED.id
VALUES (@param0, @param1, @param2)";

                object result = DataProvider.Instance.ExcuteScalar(insertGroupQuery, new object[]
                {
            group.GroupName,
            group.CreatedBy,
            group.CreatedAt
                });

                if (result != null)
                    newGroupId = Convert.ToInt32(result);
            }
            else
            {
                // Trường hợp có avatar
                string insertGroupQuery = @"
INSERT INTO Groups (group_name, created_by, avatar_url, created_at)
OUTPUT INSERTED.id
VALUES (@param0, @param1, @param2, @param3)";

                object result = DataProvider.Instance.ExcuteScalar(insertGroupQuery, new object[]
                {
            group.GroupName,
            group.CreatedBy,
            group.Avatar_URL,
            group.CreatedAt
                });

                if (result != null)
                    newGroupId = Convert.ToInt32(result);
            }

            if (newGroupId <= 0)
            {
                return -1;
            }

            // Thêm creator vào GroupMembers
            string insertMemberQuery = @"
INSERT INTO GroupMembers (group_id, joined_at, nick_name, user_email)
VALUES (@param0, @param1, @param2, @param3)";

            int rows = DataProvider.Instance.ExcuteNonQuery(insertMemberQuery, new object[]
            {
        newGroupId,
        DateTime.Now,
        nickName,
        group.CreatedBy
            });

            if (rows > 0)
            {
                return newGroupId;
            }
            else
            {
                // Trường hợp insert GroupMembers fail
                return -1;
            }
        }

        public List<GroupDTO> GetGroupsWithFilters(Dictionary<string, object> filters)
        {
            List<GroupDTO> groups = new List<GroupDTO>();

            // Xây dựng câu truy vấn cơ bản
            StringBuilder queryBuilder = new StringBuilder(@"
            SELECT G.id, G.group_name, G.created_by, G.avatar_url, G.created_at, G.is_deleted
            FROM Groups G
            WHERE 1=1 AND G.is_deleted = 0");

            // Tạo danh sách tham số
            List<object> parameters = new List<object>();
            int paramIndex = 0;

            // Thêm các điều kiện filter
            if (filters != null && filters.Count > 0)
            {
                foreach (var filter in filters)
                {
                    switch (filter.Key.ToLower())
                    {
                        case "groupname":
                            queryBuilder.Append(" AND G.group_name LIKE @param" + paramIndex);
                            parameters.Add("%" + filter.Value.ToString() + "%");
                            paramIndex++;
                            break;

                        case "createdby":
                            queryBuilder.Append(" AND G.created_by = @param" + paramIndex);
                            parameters.Add(filter.Value.ToString());
                            paramIndex++;
                            break;

                        case "createdafter":
                            queryBuilder.Append(" AND G.created_at >= @param" + paramIndex);
                            parameters.Add(Convert.ToDateTime(filter.Value));
                            paramIndex++;
                            break;

                        case "createdbefore":
                            queryBuilder.Append(" AND G.created_at <= @param" + paramIndex);
                            parameters.Add(Convert.ToDateTime(filter.Value));
                            paramIndex++;
                            break;

                        case "id":
                            queryBuilder.Append(" AND G.id = @param" + paramIndex);
                            parameters.Add(Convert.ToInt32(filter.Value));
                            paramIndex++;
                            break;

                        case "useremail":
                            queryBuilder.Append(" AND EXISTS (SELECT 1 FROM GroupMembers GM WHERE GM.group_id = G.id AND GM.user_email = @param" + paramIndex + ")");
                            parameters.Add(filter.Value.ToString());
                            paramIndex++;
                            break;
                    }
                }
            }

            // Thực thi truy vấn
            DataTable result = DataProvider.Instance.ExcuteQuery(queryBuilder.ToString(), parameters.ToArray());

            // Chuyển đổi kết quả thành danh sách GroupDTO
            foreach (DataRow row in result.Rows)
            {
                GroupDTO group = new GroupDTO
                {
                    Id = (int)row["id"],
                    GroupName = row["group_name"].ToString(),
                    CreatedBy = row["created_by"].ToString(),
                    Avatar_URL = row["avatar_url"] != DBNull.Value ? row["avatar_url"].ToString() : null,
                    CreatedAt = (DateTime)row["created_at"],
                    IsDeleted = row["is_deleted"] != DBNull.Value ? Convert.ToInt32(row["is_deleted"]) : 0
                };

                groups.Add(group);
            }

            return groups;
        }

        public bool UpdateGroupInfo(int groupId, string newGroupName, string newAvatarUrl = null)
        {
            string query;
            object[] parameters;

            if (string.IsNullOrEmpty(newAvatarUrl))
            {
                // Chỉ cập nhật tên nhóm
                query = @"
            UPDATE Groups 
            SET group_name = @param0
            WHERE id = @param1";

                parameters = new object[] { newGroupName, groupId };
            }
            else
            {
                // Cập nhật cả tên nhóm và avatar
                query = @"
            UPDATE Groups 
            SET group_name = @param0, 
                avatar_url = @param1
            WHERE id = @param2";

                parameters = new object[] { newGroupName, newAvatarUrl, groupId };
            }

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteGroup(int groupId)
        {
            string softDeleteGroupQuery = @"
            UPDATE Groups 
            SET is_deleted = 1
            WHERE id = @param0";

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(softDeleteGroupQuery,
                new object[] { groupId });

            return rowsAffected > 0;
        }

        public bool ChangeGroupAvatar(int groupId, string newAvatarUrl)
        {
            string query = @"
        UPDATE Groups 
        SET avatar_url = @param0
        WHERE id = @param1";

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(query,
                new object[] { newAvatarUrl, groupId });

            return rowsAffected > 0;
        }
    }
}