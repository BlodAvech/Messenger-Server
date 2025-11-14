var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<DatabaseManager>();
var app = builder.Build();
app.MapControllers();

app.Urls.Add("http://0.0.0.0:5000");

app.Run();
