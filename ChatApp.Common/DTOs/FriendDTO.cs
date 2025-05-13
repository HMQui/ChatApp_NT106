using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Common.DTOs
{
    public class FriendDTO
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int friend_id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string status { get; set; }
    }
}
