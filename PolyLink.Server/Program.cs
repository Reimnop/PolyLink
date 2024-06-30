using Microsoft.EntityFrameworkCore;
using PolyLink.Server.Controller;
using PolyLink.Server.Service;
using PolyLink.Server.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddDbContext<PolyLinkContext>(opt => opt.UseInMemoryDatabase("PolyLink"));
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<GameHub>("/game");

var gameServer = new GameServer(app);
await gameServer.RunAsync();