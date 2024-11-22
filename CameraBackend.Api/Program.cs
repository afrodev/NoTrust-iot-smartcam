using CameraBackend.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Next.js frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Allow credentials if needed
    });
});

builder.Services.AddScoped<SessionService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure WebSocket
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);

// Use CORS policy
app.UseCors("NextJsPolicy");

app.MapControllers();

app.MapGet("/hello", () => Results.Json(new { message = "Hello from the .NET API!" }));

app.MapGet("/stream", () => Results.Json(new { message = "You are now viewing the security camera stream!" }));

app.Run();