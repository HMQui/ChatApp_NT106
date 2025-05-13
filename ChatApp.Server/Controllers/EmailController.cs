using Microsoft.AspNetCore.Mvc;
using ChatApp.Server.Services;
using ChatApp.Server.Models;
using System.Threading.Tasks;

namespace ChatApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : Controller
    {
        private readonly EmailService _emailService;
        public IActionResult Index()
        {
            return View();
        }

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Hello World");
        }

        [HttpPost("send/verifycode")]
        public async Task<IActionResult> SendVerifyCode([FromBody] SendVerifyCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.VerifyCode))
            {
                return BadRequest("Email and verify code are required.");
            }

            string subject = "Zola: Verification Code";
            string body = $"Your verification code is: {request.VerifyCode}";

            await _emailService.SendEmailAsync(request.Email, subject, body);

            return Ok(new { message = "Verification code sent successfully." });
        }
    }
}
