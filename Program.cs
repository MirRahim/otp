using otpService.Services.OtpService;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!
    )
);

builder.Services.AddScoped<IOtpService, OtpService>();

var app = builder.Build();

app.MapControllers();

app.Run();