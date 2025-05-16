using ChatApp.Server.Services;
using ChatApp.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<S3Service>();


var app = builder.Build();

app.MapControllers();
app.MapHub<StatusAccountHub>("/socket/status");
app.MapHub<ChatOneOnOneHub>("/socket/chat-single");
app.MapHub<NotificationHub>("/socket/notification");

app.Run();
