namespace ChatApp.Common.DTOs
{
    public class GroupMembersDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
