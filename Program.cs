using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using otpService.Services;
using otpService.Services.Sms;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SmartScheme";
    options.DefaultChallengeScheme = "SmartScheme";
})
.AddPolicyScheme("SmartScheme", "JWT or Cookie", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            return JwtBearerDefaults.AuthenticationScheme;

        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/forbidden";
    options.Cookie.Name = "AuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});


builder.Services.AddScoped<TokenService>();
//builder.Services.TryAdd(ServiceDescriptor.Singleton<ILoggerService, MongoLoggerService>());
builder.Services.AddScoped<SMSService>();



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

//builder.Services.AddHangfire(
//    config => config.UseSimpleAssemblyNameTypeSerializer().
//    UseRecommendedSerializerSettings().UseMemoryStorage()
//);
//builder.Services.AddHangfireServer();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var recurringJobManager = scope.ServiceProvider
//        .GetRequiredService<IRecurringJobManager>();

//    recurringJobManager.AddOrUpdate<TrialEndingSMSJob>(
//        "trial-sms",
//        job => job.Execute(),
//        "0 9 * * *",
//        new RecurringJobOptions
//        {
//            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran")
//        }
//    );
//}

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
