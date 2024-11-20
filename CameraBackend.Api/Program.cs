var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors( options => {
    options.AddPolicy("NextJsPolicy", policy => {
        policy.WithOrigins( "http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Call CORS functionality
app.UseCors("NextJsPolicy");

app.MapGet("/", () => "Hello World!");



app.Run();
