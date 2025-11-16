var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<DatabaseManager>(); 
builder.Services.AddSingleton<ChatService>(); 
builder.Services.AddSingleton<Logger>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var chatService = scope.ServiceProvider.GetRequiredService<ChatService>();
    var logger = scope.ServiceProvider.GetRequiredService<Logger>();
}


app.MapControllers();
app.Urls.Add("http://0.0.0.0:5000");
app.Run();
