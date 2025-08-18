using ChatChallenge.Api.Models;

namespace ChatChallenge.Api.Services
{
  public interface ISignalRNotificationService
  {
    Task SendStockResponseToRoomAsync(StockResponseMessage stockResponse);
  }
}
