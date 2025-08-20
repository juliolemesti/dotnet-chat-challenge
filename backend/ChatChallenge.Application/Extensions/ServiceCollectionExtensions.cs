using Microsoft.Extensions.DependencyInjection;
using ChatChallenge.Application.Interfaces;
using ChatChallenge.Application.Services;
using ChatChallenge.Application.Hubs;

namespace ChatChallenge.Application.Extensions;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Register all Application layer services
  /// </summary>
  /// <param name="services">The service collection</param>
  /// <returns>The service collection for chaining</returns>
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    // Register Application Services
    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<IStockBotService, StockBotService>();
    services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
    services.AddScoped<IChatService, ChatService>();

    // Register SignalR Hub
    services.AddSignalR();

    return services;
  }
}
