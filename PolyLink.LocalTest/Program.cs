using System.Web;
using Microsoft.AspNetCore.SignalR.Client;
using PolyLink.Common.Packet;

const string targetUrl = "localhost:5291";

using var httpClient = new HttpClient();

Console.WriteLine("1. Setting up Profile");
Console.Write("Enter your name: ");
var profileName = Console.ReadLine();
Console.Write("Enter your display name: ");
var profileDisplayName = Console.ReadLine();

Console.WriteLine("2. Connecting to SignalR");
var connection = new HubConnectionBuilder()
    .WithUrl($"http://{targetUrl}/game" +
             $"?name={HttpUtility.UrlEncode(profileName)}" +
             $"&displayName={HttpUtility.UrlEncode(profileDisplayName)}")
    .Build();

connection.On<StartGamePacket>("SwitchToArcade", packet =>
{
    Console.WriteLine($"Received SwitchToArcade packet with level ID: {packet.LevelId}");
});

await connection.StartAsync();

Console.WriteLine("Press any key to exit");
var stop = false;
while (!stop && connection.State != HubConnectionState.Disconnected)
{
    if (Console.KeyAvailable)
        stop = true;
    await Task.Yield();
}

Console.WriteLine("\n3. Disconnecting from SignalR");
await connection.StopAsync();