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
