using System.Net.Http.Json;
using System.Net.WebSockets;
using PolyLink.LocalTest;

var targetUrl = "localhost:5291";

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

while (!ws.CloseStatus.HasValue)
{
    var buffer = new byte[1024];
    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    var receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
    Console.WriteLine($"Received message: {receivedMessage}");

    if (receivedMessage == "heartbeat!")
    {
        Console.WriteLine("Since previous message was a heartbeat, sending a response");
        await ws.SendAsync(new ArraySegment<byte>("response!"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
