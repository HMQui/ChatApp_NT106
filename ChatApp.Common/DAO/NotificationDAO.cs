using ChatApp.Common.DTOs;
using System.Data;

namespace ChatApp.Common.DAO
{
    public class NotificationDAO
    {
        private static NotificationDAO instance;
        public static NotificationDAO Instance
        {
            get { if (instance == null) instance = new NotificationDAO(); return instance; }
            private set { instance = value; }
        }

        private NotificationDAO() { }

        public NotificationDTO InsertNotification(string senderEmail, string receiverEmail, string message, string notification_type)
        {
            string query = @"
                INSERT INTO Notifications (sender_email, receiver_email, message, notification_type, created_at)
                OUTPUT INSERTED.id, INSERTED.sender_email, INSERTED.receiver_email, INSERTED.message, INSERTED.notification_type, INSERTED.created_at
                VALUES (@param0, @param1, @param2, @param3, GETDATE())";

            object[] parameters = { senderEmail, receiverEmail, message, notification_type };
            DataTable dt = DataProvider.Instance.ExcuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new NotificationDTO
                {
                    Id = Convert.ToInt32(row["id"]),
                    SenderEmail = row["sender_email"].ToString(),
                    ReceiverEmail = row["receiver_email"].ToString(),
                    Message = row["message"].ToString(),
                    NotificationType = row["notification_type"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["created_at"])
                };
            }

            return null;
        }
    }
}
