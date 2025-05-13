namespace ChatApp.Server.Models
{
    public class SendVerifyCodeRequest
    {
        public string Email { get; set; }
        public string VerifyCode { get; set; }
    }
}
