namespace ChatApp.Common.DTOs
{
    public class GroupDTO
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string CreatedBy { get; set; }
        public string Avatar_URL { get; set; }
        public DateTime CreatedAt { get; set; }
        public int IsDeleted { get; set; } = 0;
    }
}
