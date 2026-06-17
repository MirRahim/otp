using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OtpSystem.API.Extensions;
using OtpSystem.API.Middlewares;
using OtpSystem.Application.Interfaces;
using OtpSystem.Application.Services;
using OtpSystem.Application.Services.OtpService;
using OtpSystem.Infrastructure.Cache;
using OtpSystem.Infrastructure.Messaging;
using OtpSystem.Infrastructure.Persistence;
using OtpSystem.Infrastructure.Persistence.Repositories;
using OtpSystem.Infrastructure.Resilience;
using OtpSystem.Infrastructure.Sms;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Repositories
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Tenant context — scoped to request
builder.Services.AddScoped<TenantContext>();

// ── Auth ──────────────────────────────────────────────────────────────
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

// ── Redis ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!
    )
);
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// ── SMS ───────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<SmsIrService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SmsConfiguration:SMS.ir:Uri"]!);
    client.DefaultRequestHeaders.Add(
        "x-api-key",
        builder.Configuration["SmsConfiguration:SMS.ir:API_TOKEN"]
    );
}).AddSmsResiliencePipeline();

builder.Services.AddScoped<FallbackSmsService>();
builder.Services.AddScoped<SmsSenderService>();
builder.Services.AddScoped<ISmsService, SmsSenderService>();

// ── RabbitMQ ──────────────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SmsConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"]!);
            h.Password(builder.Configuration["RabbitMq:Password"]!);
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddScoped<IMessagePublisher, MassTransitPublisher>();

// ── Application Services ──────────────────────────────────────────────
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// ── Infrastructure ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddRateLimiting();
builder.Services.AddAppHealthChecks(builder.Configuration);

// ── Swagger ───────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OTP System", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// ── CORS ──────────────────────────────────────────────────────────────
const string defaultCorsPolicyName = "DefaultCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(defaultCorsPolicyName, policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ═════════════════════════════════════════════════════════════════════
var app = builder.Build();
// ═════════════════════════════════════════════════════════════════════

app.UseSwagger();
app.UseSwaggerUI();

// Order matters — do not change
app.UseCors(defaultCorsPolicyName);         // 1. CORS
app.UseRouting();                           // 2. Routing
app.UseRateLimiter();                       // 3. Rate Limiting
app.UseMiddleware<IdempotencyMiddleware>(); // 4. Idempotency
app.UseMiddleware<TenantResolutionMiddleware>(); 
app.UseAuthentication();                    // 5. Authentication
app.UseAuthorization();                     // 6. Authorization

app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// ── Health Checks ─────────────────────────────────────────────────────
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds + "ms",
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds + "ms",
                description = e.Value.Description,
                tags = e.Value.Tags
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("infrastructure")
});

app.Run();