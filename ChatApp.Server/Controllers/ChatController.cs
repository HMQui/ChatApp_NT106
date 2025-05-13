// Ví d? cho ChatController.cs
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost]
        public IActionResult SendMessage([FromBody] string message)
        {
            return Ok($"Received: {message}");
        }
    }
}