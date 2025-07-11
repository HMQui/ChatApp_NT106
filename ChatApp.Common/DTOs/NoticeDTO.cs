using System;

namespace ChatApp.Common.DTOs
{
    public class NoticeDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsSeen { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SeenAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}