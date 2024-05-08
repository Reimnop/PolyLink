﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using PolyLink.Common.Packet;
using PolyLink.Util;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Playables;

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

                var multiplayerManager = LazySingleton<MultiplayerManager>.Instance;
                multiplayerManager.SetData(packet.LocalPlayerId, packet.Players);

                VGPlayerManager.inst.players.Clear();
                foreach (var playerInfo in packet.Players)
                {
                    Log.Info($"Player: {playerInfo.DisplayName}");
                    var vgPlayerData = new VGPlayerManager.VGPlayerData
                    {
                        PlayerID = playerInfo.Id,
                        ControllerID = packet.LocalPlayerId == playerInfo.Id ? 0 : -1
                    };
                    VGPlayerManager.inst.players.Add(vgPlayerData);
                }
                
                SaveManager.inst.CurrentArcadeLevel = level;
                SceneManager.inst.LoadScene("Arcade Level");
            });
        });
        
        hubConnection.On<SetPlayerHealthPacket>("SetPlayerHealth", packet =>
        {
            Log.Info("Received SetPlayerHealth packet");
            
            actions.Enqueue(() =>
            {
                var playerData = VGPlayerManager.inst.players
                    .ToEnumerable()
                    .FirstOrDefault(x => x.PlayerID == packet.PlayerId);
                if (playerData == null)
                {
                    Log.Error("Player not found!");
                    return;
                }

                var player = playerData.PlayerObject;
                
                // Set new player health
                var oldHealth = player.Health;
                player.Health = packet.Health;
                
                if (oldHealth > packet.Health)
                {
                    // pidge please fix your naming conventions
                    player.playerHitEvent?.Invoke(player.Health, player.Player_Wrapper.position);

                    if (packet.PlayHurtAnimation)
                    {
                        player.StartHurtDecay();
                        AudioManager.Inst.ApplyLowPass(0.05f, 0.4f, 1.0f);
                        AudioManager.Inst.PlaySound("HurtPlayer");
                        player.PlayerHitAnimation();
                    }
                }
            });
        });

        hubConnection.On<KillPlayerPacket>("KillPlayer", packet =>
        {
            Log.Info("Received KillPlayer packet");
            
            actions.Enqueue(() =>
            {
                var playerData = VGPlayerManager.inst.players
                    .ToEnumerable()
                    .FirstOrDefault(x => x.PlayerID == packet.PlayerId);
                if (playerData == null)
                {
                    Log.Error("Player not found!");
                    return;
                }

                var player = playerData.PlayerObject;
                player.Health = 0;
                player.playerDeathEvent?.Invoke(player.Player_Wrapper.position);
                player.PlayerDeath();
                player.PlayerDeathAnimation();
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