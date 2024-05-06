using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR.Client;
using PolyLink.Common.Packet;
using Steamworks.Data;
using UnityEngine;

namespace PolyLink;

public class PluginProcess : MonoBehaviour
{
    private HubConnection hubConnection = null!;

    private readonly ConcurrentQueue<Action> actions = [];
    
    private void Start()
    {
        // Initialize SignalR
        hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5291/game?name=reimnop&displayName=Reimnop")
            .Build();
        
        hubConnection.On<SwitchToArcadePacket>("SwitchToArcade", packet =>
        {
            Log.Info($"Received SwitchToArcade packet with level ID: {packet.LevelId}");
            
            actions.Enqueue(() =>
            {
                var level = ArcadeLevelDataManager.inst.GetSteamLevel(new PublishedFileId
                {
                    Value = packet.LevelId
                });
                
                if (level == null)
                {
                    Log.Warn($"Level with ID {packet.LevelId} not found!");
                    return;
                }
                
                GameManager2.inst.StartCoroutine(GameManager2.inst.LoadGame(level));
            });
        });

        hubConnection.StartAsync().Wait();
        Log.Info("SignalR connected!");
    }

    private void Update()
    {
        while (actions.TryDequeue(out var action))
            action();
    }
}