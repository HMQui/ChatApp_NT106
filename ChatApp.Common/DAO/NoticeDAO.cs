using ChatApp.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Data;

namespace ChatApp.Common.DAO
{
    public class NoticeDAO
    {
        private static NoticeDAO instance;
        public static NoticeDAO Instance
        {
            get { if (instance == null) instance = new NoticeDAO(); return instance; }
            private set { instance = value; }
        }
        private NoticeDAO() { }

        public int AddNotice(NoticeDTO notice)
        {
            string query = @"
                INSERT INTO Notices 
                (email, title, message, created_at)
                OUTPUT INSERTED.id
                VALUES (@param0, @param1, @param2, @param3)";

            object result = DataProvider.Instance.ExcuteScalar(query, new object[]
            {
                notice.Email,
                notice.Title,
                notice.Message,
                notice.CreatedAt
            });

            return result != null ? Convert.ToInt32(result) : -1;
        }

        public bool MarkAsSeen(int noticeId, string email)
        {
            string query = @"
                UPDATE Notices 
                SET is_seen = 1,
                    seen_at = @param1
                WHERE id = @param0 AND email = @param2";

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(query, new object[]
            {
                noticeId,
                DateTime.Now,
                email
            });

            return rowsAffected > 0;
        }

        public bool SoftDeleteNotice(int noticeId, string email)
        {
            string query = @"
                UPDATE Notices 
                SET is_deleted = 1,
                    deleted_at = @param1
                WHERE id = @param0 AND email = @param2";

            int rowsAffected = DataProvider.Instance.ExcuteNonQuery(query, new object[]
            {
                noticeId,
                DateTime.Now,
                email
            });

            return rowsAffected > 0;
        }

        public List<NoticeDTO> GetNoticesByEmail(string email, bool includeDeleted = false)
        {
            List<NoticeDTO> notices = new List<NoticeDTO>();

            string query = @"
                SELECT id, email, title, message, is_seen, is_deleted, 
                       created_at, seen_at, deleted_at
                FROM Notices
                WHERE email = @param0";

            if (!includeDeleted)
            {
                query += " AND is_deleted = 0";
            }

            query += " ORDER BY created_at DESC";

            DataTable result = DataProvider.Instance.ExcuteQuery(query, new object[] { email });

            foreach (DataRow row in result.Rows)
            {
                notices.Add(new NoticeDTO
                {
                    Id = (int)row["id"],
                    Email = row["email"].ToString(),
                    Title = row["title"].ToString(),
                    Message = row["message"].ToString(),
                    IsSeen = Convert.ToBoolean(row["is_seen"]),
                    IsDeleted = Convert.ToBoolean(row["is_deleted"]),
                    CreatedAt = (DateTime)row["created_at"],
                    SeenAt = row["seen_at"] != DBNull.Value ? (DateTime?)row["seen_at"] : null,
                    DeletedAt = row["deleted_at"] != DBNull.Value ? (DateTime?)row["deleted_at"] : null
                });
            }

            return notices;
        }

        public NoticeDTO GetNoticeById(int noticeId)
        {
            string query = @"
                SELECT id, email, title, message, is_seen, is_deleted, 
                       created_at, seen_at, deleted_at
                FROM Notices
                WHERE id = @param0";

            DataTable result = DataProvider.Instance.ExcuteQuery(query, new object[] { noticeId });

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                return new NoticeDTO
                {
                    Id = (int)row["id"],
                    Email = row["email"].ToString(),
                    Title = row["title"].ToString(),
                    Message = row["message"].ToString(),
                    IsSeen = Convert.ToBoolean(row["is_seen"]),
                    IsDeleted = Convert.ToBoolean(row["is_deleted"]),
                    CreatedAt = (DateTime)row["created_at"],
                    SeenAt = row["seen_at"] != DBNull.Value ? (DateTime?)row["seen_at"] : null,
                    DeletedAt = row["deleted_at"] != DBNull.Value ? (DateTime?)row["deleted_at"] : null
                };
            }

            return null;
        }
    }
}