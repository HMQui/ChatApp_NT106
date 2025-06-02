namespace ChatApp.Common.DTOs
{
    public class UserFriendDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string FriendStatus { get; set; }
    }
}
