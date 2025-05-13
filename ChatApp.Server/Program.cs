using ChatApp.Server.Services;
using ChatApp.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<CloudinaryService>();
var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "chat-app-5b314-firebase-adminsdk-fbsvc-63357f4f2d.json");
var bucketName = "chat-app-5b314.appspot.com";

builder.Services.AddSingleton<FirebaseStorageService>(provider =>
    new FirebaseStorageService(credentialsPath, bucketName));

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});

builder.Services.AddSingleton<S3Service>();


var app = builder.Build();

app.MapControllers();
app.MapHub<StatusAccountHub>("/socket/status");
app.MapHub<ChatOneOnOneHub>("/socket/chat-single");

app.Run();
