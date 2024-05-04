using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using PolyLink.Common.Model;

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

var profile = (await step1Response.Content.ReadFromJsonAsync<Profile>())!;

Console.WriteLine("Profile info:");
Console.WriteLine($"ID: {profile.Id}");
Console.WriteLine($"Name: {profile.Name}");
Console.WriteLine($"Display Name: {profile.DisplayName}");
Console.WriteLine($"Login Token: {profile.LoginToken}");

Console.WriteLine("2. Connecting to SignalR");
var connection = new HubConnectionBuilder()
    .WithUrl($"http://{targetUrl}/game", options =>
    {
        options.Headers.Add("Authorization", $"Bearer {profile.LoginToken}");
    })
    .Build();

connection.On<string, string>("ReceiveMessage", (user, message) =>
    Console.WriteLine($"{user} says: {message}"));

await connection.StartAsync();

while (true)
{
    Console.Write("Enter message: ");
    var message = Console.ReadLine();
    await connection.SendAsync("SendMessage", message);
}
