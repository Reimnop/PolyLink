using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Steamworks;

namespace PolyLink;

public static class Loader
{
    public static async Task LoadAsync()
    {
        var name = SteamClient.Name;
        var id = SteamClient.SteamId.Value;
        Log.Info($"Your PolyLink auth username is '{name}' (id: {id})");
            
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"https://polylink.parctan.com/game?clientId={id}&displayName={name}")
            .Build();
            
        await hubConnection.StartAsync();
        
        Log.Info("WebSocket connected; PolyLink is ready!");
    }
}