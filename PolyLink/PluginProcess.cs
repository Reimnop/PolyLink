using System;
using System.Collections.Concurrent;
using Il2CppSystem.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using PolyLink.Common.Packet;
using Rewired;
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
        
        hubConnection.On<StartGamePacket>("StartGame", packet =>
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
                    Log.Error($"Level with ID {packet.LevelId} not found!");
                    return;
                }

                VGPlayerManager.inst.players.Clear();
                foreach (var playerInfo in packet.Players)
                {
                    Log.Info($"Player: {playerInfo.DisplayName}");
                    var vgPlayerData = new VGPlayerManager.VGPlayerData
                    {
                        PlayerID = playerInfo.Id,
                        ControllerID = 0
                    };
                    VGPlayerManager.inst.players.Add(vgPlayerData);
                }
                
                SaveManager.inst.CurrentArcadeLevel = level;
                SceneManager.inst.LoadScene("Arcade Level");
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