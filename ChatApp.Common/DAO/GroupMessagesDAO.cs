using ChatApp.Common.DTOs;
using System.Data;

namespace ChatApp.Common.DAO
{
    public class GroupMessagesDAO
    {
        private static GroupMessagesDAO instance;
        public static GroupMessagesDAO Instance
        {
            get { if (instance == null) instance = new GroupMessagesDAO(); return instance; }
            private set { instance = value; }
        }
        private GroupMessagesDAO() { }

        public List<GroupMessagesDTO> GetMessagesByGroupId(int groupId)
        {
            List<GroupMessagesDTO> messages = new List<GroupMessagesDTO>();

            string query = @"
                SELECT 
                    GM.id,
                    GM.group_id,
                    GM.sender_email,
                    GM.message,
                    GM.message_type,
                    GM.sent_at,
                    GM.is_deleted,
                    M.nick_name AS sender_nickname
                FROM 
                    GroupMessages GM
                LEFT JOIN 
                    GroupMembers M ON GM.sender_email = M.user_email AND GM.group_id = M.group_id
                WHERE 
                    GM.group_id = @param0
                    AND GM.is_deleted = 0
                ORDER BY 
                    GM.sent_at ASC";

            DataTable result = DataProvider.Instance.ExcuteQuery(query, new object[] { groupId });

            foreach (DataRow row in result.Rows)
            {
                GroupMessagesDTO message = new GroupMessagesDTO
                {
                    Id = (int)row["id"],
                    GroupId = (int)row["group_id"],
                    SenderEmail = row["sender_email"].ToString(),
                    SenderNickname = row["sender_nickname"].ToString(),
                    Message = row["message"].ToString(),
                    MessageType = row["message_type"].ToString(),
                    SentAt = (DateTime)row["sent_at"],
                    IsDeleted = Convert.ToBoolean(row["is_deleted"])
                };

                messages.Add(message);
            }

            return messages;
        }

        public int AddMessage(GroupMessagesDTO message)
        {
            string query = @"
                INSERT INTO GroupMessages 
                (group_id, sender_email, message, message_type, sent_at, is_deleted)
                OUTPUT INSERTED.id
                VALUES (@param0, @param1, @param2, @param3, @param4, @param5)";

            object result = DataProvider.Instance.ExcuteScalar(query, new object[]
            {
                message.GroupId,
                message.SenderEmail,
                message.Message,
                message.MessageType,
                message.SentAt,
                message.IsDeleted ? 1 : 0
            });

            return result != null ? Convert.ToInt32(result) : -1;
        }

        public bool DeleteMessage(int messageId)
        {
            string query = @"
                UPDATE GroupMessages
                SET is_deleted = 1
                WHERE id = @param0";

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(query, new object[] { messageId });

            return rowsAffected > 0;
        }

        public GroupMessagesDTO GetMessageById(int messageId)
        {
            string query = @"
                SELECT 
                    GM.id,
                    GM.group_id,
                    GM.sender_email,
                    GM.message,
                    GM.message_type,
                    GM.sent_at,
                    GM.is_deleted,
                    M.nick_name AS sender_nickname
                FROM 
                    GroupMessages GM
                INNER JOIN 
                    GroupMembers M ON GM.sender_email = M.user_email AND GM.group_id = M.group_id
                WHERE 
                    GM.id = @param0";

            DataTable result = DataProvider.Instance.ExcuteQuery(query, new object[] { messageId });

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                return new GroupMessagesDTO
                {
                    Id = (int)row["id"],
                    GroupId = (int)row["group_id"],
                    SenderEmail = row["sender_email"].ToString(),
                    SenderNickname = row["sender_nickname"].ToString(),
                    Message = row["message"].ToString(),
                    MessageType = row["message_type"].ToString(),
                    SentAt = (DateTime)row["sent_at"],
                    IsDeleted = Convert.ToBoolean(row["is_deleted"])
                };
            }

            return null;
        }
    }
}