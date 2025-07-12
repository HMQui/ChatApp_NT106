using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Common.DTOs
{
    public class GroupMessagesDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string SenderEmail { get; set; }
        public string SenderNickname { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
