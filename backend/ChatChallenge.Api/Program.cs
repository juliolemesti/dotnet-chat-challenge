using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using ChatChallenge.Infrastructure.Data;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Infrastructure.Repositories;
using ChatChallenge.Api.Services;
using ChatChallenge.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<ChatDbContext>(options =>
  options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register JWT service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Password service
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Register Stock Bot service (placeholder for future RabbitMQ integration)
builder.Services.AddScoped<IStockBotService, StockBotService>();

// Register Stock API service with HttpClient
builder.Services.AddHttpClient<IStockApiService, StockApiService>();

// Register Message Broker service (in-memory implementation)
builder.Services.AddSingleton<IMessageBrokerService, InMemoryMessageBrokerService>();

// Register SignalR notification service as Singleton
builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();

// Register Stock Bot Background Service
builder.Services.AddHostedService<StockBotBackgroundService>();

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure SignalR authentication
    options.Events = new JwtBearerEvents
    {
      OnMessageReceived = context =>
      {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
        {
          context.Token = accessToken;
        }
        return Task.CompletedTask;
      },
      OnAuthenticationFailed = context =>
      {
        if (context.Request.Path.StartsWithSegments("/chathub"))
        {
          Console.WriteLine($"SignalR JWT Authentication failed: {context.Exception.Message}");
        }
        return Task.CompletedTask;
      },
      OnTokenValidated = context =>
      {
        if (context.Request.Path.StartsWithSegments("/chathub"))
        {
          var userName = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
          if (string.IsNullOrEmpty(userName))
          {
            context.Fail("Username claim is required for SignalR connections");
          }
        }
        return Task.CompletedTask;
      }
    };
});

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend",
    policy =>
    {
      policy.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
  DbInitializer.Initialize(context);
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.Run();
