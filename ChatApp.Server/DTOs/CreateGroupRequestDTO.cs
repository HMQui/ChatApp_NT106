namespace ChatApp.Server.DTOs
{
    public class CreateGroupRequest
    {
        public string CreatorEmail { get; set; }
        public string GroupName { get; set; }
        public IFormFile Avatar { get; set; }
    }
}
