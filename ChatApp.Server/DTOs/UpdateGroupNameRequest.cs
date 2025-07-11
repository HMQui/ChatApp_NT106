namespace ChatApp.Server.DTOs
{
    public class UpdateGroupNameRequest
    {
        public int GroupId { get; set; }
        public string NewGroupName { get; set; } = null!;
    }
}
