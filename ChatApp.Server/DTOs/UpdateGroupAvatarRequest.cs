namespace ChatApp.Server.DTOs
{
    public class UpdateGroupAvatarRequest
    {
        public int GroupId { get; set; }
        public byte[] ImageBytes { get; set; } = null!;
    }
}
