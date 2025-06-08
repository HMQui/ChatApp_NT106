using ChatApp.Server.Services;
using ChatApp.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 20 * 1024 * 1024;
    options.StreamBufferCapacity = 20 * 1024 * 1024;
});

builder.Services.AddControllers();

builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<S3Service>();


var app = builder.Build();

app.MapControllers();
app.MapHub<StatusAccountHub>("/socket/status");
app.MapHub<ChatOneOnOneHub>("/socket/chat-single");
app.MapHub<NotificationHub>("/socket/notification");
app.MapHub<VoiceCallHub>("/socket/voice-call");


app.Run();
