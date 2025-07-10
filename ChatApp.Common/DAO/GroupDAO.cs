using ChatApp.Common.DTOs;
using System.Collections.Generic;

namespace ChatApp.Common.DAO
{
    public class GroupDAO
    {
        private static readonly GroupDAO _instance = new GroupDAO();
        private List<GroupDTO> _groups; // Danh sách nhóm mẫu

        private GroupDAO()
        {
            // Khởi tạo dữ liệu mẫu với avatar và trạng thái
            _groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    GroupId = "G1",
                    GroupName = "Group A",
                    AvatarUrl = "https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg",
                    Status = "online"
                },
                new GroupDTO
                {
                    GroupId = "G2",
                    GroupName = "Group B",
                    AvatarUrl = "https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg",
                    Status = "offline"
                },
                new GroupDTO
                {
                    GroupId = "G3",
                    GroupName = "Group C",
                    AvatarUrl = "https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg",
                    Status = "online"
                }
            };
        }

        public static GroupDAO Instance
        {
            get { return _instance; }
        }

        public List<GroupDTO> GetGroups(string email)
        {
            return _groups;
        }
    }

    public class GroupDTO
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string AvatarUrl { get; set; } // URL ảnh nhóm
        public string Status { get; set; } // Trạng thái nhóm (active/inactive)
    }
}