using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PolyLink.Server.Controller;
using PolyLink.Server.Service;
using PolyLink.Server.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "Enter your token: `Bearer YOUR_TOKEN`",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

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
app.UseHttpsRedirection();
app.MapHub<GameHub>("/game");

var gameServer = new GameServer(app);
await gameServer.RunAsync(CancellationToken.None);