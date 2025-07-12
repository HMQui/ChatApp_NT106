using ChatApp.Server.DTOs;
using ChatApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Common.DTOs;
using ChatApp.Common.DAO;

namespace ChatApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly S3Service _s3Service;

        public GroupsController(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromForm] CreateGroupRequest request)
        {
            try
            {
                string avatarUrl = null;

                if (request.Avatar != null && request.Avatar.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await request.Avatar.CopyToAsync(ms);
                        var bytes = ms.ToArray();

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Avatar.FileName);

                        avatarUrl = await _s3Service.UploadImageAsync(bytes, fileName);
                    }
                }

                var newGroup = new GroupDTO
                {
                    GroupName = request.GroupName,
                    CreatedBy = request.CreatorEmail,
                    Avatar_URL = avatarUrl,
                    CreatedAt = DateTime.Now
                };

                int newGroupId = GroupDAO.Instance.CreateGroup(newGroup);

                if (newGroupId > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Group created successfully",
                        groupId = newGroupId,
                        avatarUrl = avatarUrl
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Failed to create group in database"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error while creating group: " + ex.Message
                });
            }
        }
    }
}
