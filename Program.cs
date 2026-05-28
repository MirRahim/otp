using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OtpSystem.Application.Interfaces;
using OtpSystem.Application.Services;
using OtpSystem.Application.Services.OtpService;
using OtpSystem.Application.Services.SMSService;
using OtpSystem.Infrastructure.Cache;
using OtpSystem.Infrastructure.Sms;
using StackExchange.Redis;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<SMSService>();
builder.Services.AddScoped<IOtpService, OtpService>();
// SMS providers
builder.Services.AddHttpClient<SmsIrService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SmsConfiguration:SMS.ir:Uri"]);
    client.DefaultRequestHeaders.Add(
        "x-api-key",
        builder.Configuration["SmsConfiguration:SMS.ir:API_TOKEN"]
    );
});

builder.Services.AddScoped<FallbackSmsService>();
builder.Services.AddScoped<SmsSenderService>();

// Register SmsSenderService as the ISmsService implementation
builder.Services.AddScoped<ISmsService, SmsSenderService>();

// Cache
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 500 * 1024 * 1024; });


const string defaultCorsPolicyName = "DefaultCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
       defaultCorsPolicyName,
       policy =>
       {
           policy
           .SetIsOriginAllowed(_ => true)
           //.SetIsOriginAllowedToAllowWildcardSubdomains()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
           //.AllowAnyOrigin();
       }
   );
}
);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache(); // TODO: replace with redis

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!
    )
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors(defaultCorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

//app.UseHangfireDashboard("/jobs");
app.Run();
