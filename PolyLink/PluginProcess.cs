using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using PolyLink.Common.Packet;
using PolyLink.Patch;
using PolyLink.Util;
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
            Log.Info($"Received StartGame packet with level ID: {packet.LevelId}");
            
            actions.Enqueue(() =>
            {
                var level = ArcadeLevelDataManager.Inst.GetSteamLevel(new PublishedFileId
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

                VGPlayerManager.Inst.players.Clear();
                foreach (var playerInfo in packet.Players)
                {
                    Log.Info($"Player: {playerInfo.DisplayName}");
                    var vgPlayerData = new VGPlayerManager.VGPlayerData
                    {
                        PlayerID = playerInfo.Id,
                        ControllerID = packet.LocalPlayerId == playerInfo.Id ? 0 : -1
                    };
                    VGPlayerManager.Inst.players.Add(vgPlayerData);
                }
                
                SaveManager.Inst.CurrentArcadeLevel = level;
                SceneManager.Inst.LoadScene("Arcade Level");
            });
        });
        
        hubConnection.On<SetPlayerHealthPacket>("SetPlayerHealth", packet =>
        {
            Log.Info("Received SetPlayerHealth packet");
            
            actions.Enqueue(() =>
            {
                var playerData = VGPlayerManager.Inst.players
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
                    player.HitEvent?.Invoke(player.Health, player.Player_Wrapper.position);

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
                var playerData = VGPlayerManager.Inst.players
                    .ToEnumerable()
                    .FirstOrDefault(x => x.PlayerID == packet.PlayerId);
                if (playerData == null)
                {
                    Log.Error("Player not found!");
                    return;
                }

                var player = playerData.PlayerObject;
                player.Health = 0;
                
                // yes pidge, your naming conventions are still bad
                // please fix them asap
                player.DeathEvent?.Invoke(player.Player_Wrapper.position);
                player.PlayerDeath();
                player.PlayerDeathAnimation();
            });
        });

        hubConnection.On<RewindToCheckpointPacket>("RewindToCheckpoint", packet =>
        {
            Log.Info("Received RewindToCheckpoint packet");
            
            actions.Enqueue(() =>
            {
                GameManager.Inst.RewindToCheckpoint(packet.CheckpointIndex);
            });
        });
        
        hubConnection.On<ActivateCheckpointPacket>("ActivateCheckpoint", packet =>
        {
            Log.Info("Received ActivateCheckpoint packet");
            
            actions.Enqueue(() =>
            {
                var gameManager = GameManager.Inst;
                if (gameManager.checkpointsActivated[packet.CheckpointIndex])
                    return;
                ActivateCheckpoint(packet.CheckpointIndex);
            });
        });

        hubConnection.StartAsync().Wait();
        Log.Info("SignalR connected!");
        
        // Initialize events
        GameManagerPatch.CheckpointCrossed += checkpointIndex =>
        {
            Log.Info($"Checkpoint crossed: {checkpointIndex}");
            ActivateCheckpoint(checkpointIndex);
            hubConnection.SendAsync("ActivateCheckpoint", new ActivateCheckpointPacket
            {
                CheckpointIndex = checkpointIndex
            });
        };
    }

    private void ActivateCheckpoint(int checkpointIndex)
    {
        var gameManager = GameManager.Inst;
        gameManager.playingCheckpointAnimation = true;
        VGPlayerManager.Inst.RespawnPlayers();
        gameManager.StartCoroutine(gameManager.PlayCheckpointAnimation(checkpointIndex));
    }

    private void Update()
    {
        while (actions.TryDequeue(out var action))
            action();
        
        // Sync player position
        // Get local player
        var localPlayer = VGPlayerManager.Inst.players
            .ToEnumerable()
            .FirstOrDefault(x => x.PlayerID == LazySingleton<MultiplayerManager>.Instance.LocalPlayerId);
        if (localPlayer == null)
            return;
        if (localPlayer.PlayerObject == null)
            return;
        
        // Send player position
        var position = localPlayer.PlayerObject.Player_Wrapper.position;
        hubConnection.SendAsync("UpdatePlayerPosition", new C2SUpdatePlayerPositionPacket
        {
            X = position.x,
            Y = position.y
        });
    }
}