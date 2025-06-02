using ChatApp.Common.DTOs;
using System.Data;

namespace ChatApp.Common.DAO
{
    public class MessageDAO
    {
        private static MessageDAO instance;
        public static MessageDAO Instance
        {
            get { if (instance == null) instance = new MessageDAO(); return instance; }
            private set { instance = value; }
        }
        private MessageDAO() { }

        public List<MessageDTO> GetMessagesOneOnOne(string senderEmail, string receiverEmail)
        {
            string query = @"
            SELECT 
                m.id,
                sender.email AS SenderEmail,
                receiver.email AS ReceiverEmail,
                m.group_id AS GroupId,
                m.message AS Message,
                m.message_type AS MessageType,
                m.sent_at AS SentAt
            FROM Messages m
            JOIN Users sender ON m.sender_id = sender.id
            JOIN Users receiver ON m.receiver_id = receiver.id
            WHERE 
                (sender.email = @param0 AND receiver.email = @param1)
                OR
                (sender.email = @param1 AND receiver.email = @param0)
            ORDER BY m.sent_at ASC;
            ";

            var parameters = new object[] { senderEmail, receiverEmail };
            var dataTable = DataProvider.Instance.ExcuteQuery(query, parameters);

            List<MessageDTO> messages = new List<MessageDTO>();

            foreach (DataRow row in dataTable.Rows)
            {
                MessageDTO message = new MessageDTO
                {
                    Id = Convert.ToInt32(row["Id"]),
                    SenderEmail = row["SenderEmail"].ToString(),
                    ReceiverEmail = row["ReceiverEmail"].ToString(),
                    GroupId = row["GroupId"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["GroupId"]),
                    Message = row["Message"].ToString(),
                    MessageType = row["MessageType"].ToString(),
                    SentAt = Convert.ToDateTime(row["SentAt"]),
                };
                messages.Add(message);
            }

            return messages;
        }

        public bool InsertMessage(string senderEmail, string receiverEmail, string message, string messageType, DateTime sentAt, int? groupId = null)
        {
            string query = @"
            INSERT INTO Messages (sender_id, receiver_id, group_id, message, message_type, sent_at)
            VALUES (
                (SELECT id FROM Users WHERE email = @param0),
                (SELECT id FROM Users WHERE email = @param1),
                @param2,
                @param3,
                @param4,
                @param5
            );
            ";

            object[] parameters = new object[]
            {
                senderEmail,
                receiverEmail,
                (object?)groupId ?? DBNull.Value,
                message,
                messageType,
                sentAt
            };

            int result = DataProvider.Instance.ExcuteNonQuery(query, parameters);
            return result > 0;
        }

        public MessageDTO GetSingleMessage(Dictionary<string, object> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                throw new ArgumentException("Conditions must not be empty.");

            var whereClauses = new List<string>();
            var parameters = new List<object>();
            int index = 0;

            foreach (var kvp in conditions)
            {
                whereClauses.Add($"{kvp.Key} = @param{index}");
                parameters.Add(kvp.Value);
                index++;
            }

            string whereClause = string.Join(" AND ", whereClauses);

            string query = $@"
            SELECT TOP 1
                m.id,
                sender.email AS SenderEmail,
                receiver.email AS ReceiverEmail,
                m.group_id AS GroupId,
                m.message AS Message,
                m.message_type AS MessageType,
                m.sent_at AS SentAt,
                m.file_data AS FileData,
                m.file_name AS FileName
            FROM Messages m
            JOIN Users sender ON m.sender_id = sender.id
            JOIN Users receiver ON m.receiver_id = receiver.id
            WHERE {whereClause}
            ORDER BY m.sent_at ASC;
            ";
            Console.WriteLine(query);

            var dataTable = DataProvider.Instance.ExcuteQuery(query, parameters.ToArray());

            if (dataTable.Rows.Count == 0)
                return null;

            DataRow row = dataTable.Rows[0];
            return new MessageDTO
            {
                Id = Convert.ToInt32(row["Id"]),
                SenderEmail = row["SenderEmail"].ToString(),
                ReceiverEmail = row["ReceiverEmail"].ToString(),
                GroupId = row["GroupId"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["GroupId"]),
                Message = row["Message"].ToString(),
                MessageType = row["MessageType"].ToString(),
                SentAt = Convert.ToDateTime(row["SentAt"]),
            };
        }
        public List<MessageDTO> GetMessages(Dictionary<string, object> conditions)
        {
            string query = @"
            SELECT 
                m.id,
                sender.email AS SenderEmail,
                receiver.email AS ReceiverEmail,
                m.group_id AS GroupId,
                m.message AS Message,
                m.message_type AS MessageType,
                m.sent_at AS SentAt,
            FROM Messages m
            JOIN Users sender ON m.sender_id = sender.id
            JOIN Users receiver ON m.receiver_id = receiver.id
            WHERE 1 = 1
            ";

            List<object> parameters = new List<object>();
            int index = 0;

            foreach (var condition in conditions)
            {
                query += $" AND m.{condition.Key} = @param{index}";
                parameters.Add(condition.Value);
                index++;
            }

            var dataTable = DataProvider.Instance.ExcuteQuery(query, parameters.ToArray());

            List<MessageDTO> messages = new List<MessageDTO>();

            foreach (DataRow row in dataTable.Rows)
            {
                MessageDTO message = new MessageDTO
                {
                    Id = Convert.ToInt32(row["Id"]),
                    SenderEmail = row["SenderEmail"].ToString(),
                    ReceiverEmail = row["ReceiverEmail"].ToString(),
                    GroupId = row["GroupId"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["GroupId"]),
                    Message = row["Message"].ToString(),
                    MessageType = row["MessageType"].ToString(),
                    SentAt = Convert.ToDateTime(row["SentAt"]),
                };
                messages.Add(message);
            }

            return messages;
        }

    }
}
