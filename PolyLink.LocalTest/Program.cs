using System.Net.Http.Json;
using System.Net.WebSockets;
using PolyLink.Common.Model;
using PolyLink.Common.Networking;
using PolyLink.Common.Packet;

const string targetUrl = "localhost:5291";

using var httpClient = new HttpClient();

Console.WriteLine("1. Setting up Profile");
Console.Write("Enter your name: ");
var profileName = Console.ReadLine();
Console.Write("Enter your display name: ");
var profileDisplayName = Console.ReadLine();

var step1Response = await httpClient.PostAsJsonAsync($"http://{targetUrl}/Profile", new ProfileInfo
{
    Name = profileName!,
    DisplayName = profileDisplayName!
});

if (!step1Response.IsSuccessStatusCode)
{
    Console.WriteLine("Failed to create profile!");
    return;
}

var profile = await step1Response.Content.ReadFromJsonAsync<Profile>();

Console.WriteLine("2. Connecting to WebSocket");
var ws = new ClientWebSocket();
ws.Options.SetRequestHeader("Authorization", $"Bearer {profile!.LoginToken}");
await ws.ConnectAsync(new Uri($"ws://{targetUrl}/Game/connect"), CancellationToken.None);

Console.WriteLine("Connected to WebSocket!");

var connectionHandler = new ConnectionHandler(ws, PacketRegistry.Default);
connectionHandler.PacketReceived = async (_, packet) =>
{
    if (packet is S2CHeartbeatPacket)
    {
        Console.WriteLine("Received heartbeat packet! Sending response...");
        await connectionHandler.SendAsync(new C2SHeartbeatPacket());
    }
};

while (!ws.CloseStatus.HasValue)
{
    await connectionHandler.ReceiveAsync();
}
