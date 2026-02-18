using Microsoft.AspNetCore.SignalR;

namespace PartsUnlimited.Hubs;

/// <summary>
/// Server-to-client push hub for product announcements.
/// Migrated from SignalR 2.2 (OWIN) to ASP.NET Core SignalR.
/// </summary>
public class AnnouncementHub : Hub
{
    // Clients subscribe to receive new-product notifications.
    // The server calls Clients.All.SendAsync("announcement", product) from StoreManagerController.
}
