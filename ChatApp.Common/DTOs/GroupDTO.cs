using System;
using System.Collections.Generic;

namespace ChatApp.Common.DTOs
{
    public class GroupDTO
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GroupMemberDTO> Members { get; set; }
    }
} 