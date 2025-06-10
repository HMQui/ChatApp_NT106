using System;

namespace ChatApp.Common.DTOs
{
    public class GroupMemberDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public UserDTO User { get; set; }
    }
} 