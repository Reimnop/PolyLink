using System;
using System.Collections.Concurrent;
using System.Linq;
using Il2CppSystems.SceneManagement;
using Microsoft.AspNetCore.SignalR.Client;
using PolyLink.Common.Packet;
using PolyLink.Patch;
using PolyLink.Util;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace PolyLink;

public class PluginProcess(HubConnection hubConnection) : MonoBehaviour
{
    private readonly ConcurrentQueue<Action> actions = [];

    private void Start()
    {
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
                        PlayerID = playerInfo.Id
                    };
                    VGPlayerManager.Inst.players.Add(vgPlayerData);
                }
                
                SaveManager.Inst.CurrentArcadeLevel = level;
                SceneLoader.Inst.LoadSceneGroup("Arcade_Level");
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
                SetPlayerHealth(player, packet.Health, packet.PlayHurtAnimation);
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
                
                // yes pidge, your naming conventions are still bad
                // please fix them asap
                SetPlayerHealth(player, 0, true);
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

        hubConnection.On<S2CUpdatePlayerPositionPacket>("UpdatePlayerPosition", packet =>
        {
            // Log.Info("Received UpdatePlayerPosition packet");
            
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
                if (!player)
                    return;
                if (!player.Player_Rigidbody)
                    return;
                
                var oldPosition = player.Player_Rigidbody.position;
                var newPosition = new Vector2(packet.X, packet.Y);
                player.Player_Rigidbody.position = newPosition;
                
                var delta = newPosition - oldPosition;
                if (delta.sqrMagnitude > 0.0001f)
                {
                    delta.Normalize();
                    player.p_lastMoveX = delta.x;
                    player.p_lastMoveY = delta.y;
                }
            });
        });
        
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

        VGPlayerPatch.PlayerCollide += player =>
        {
            var multiplayerManager = LazySingleton<MultiplayerManager>.Instance;
            
            // Don't process collision for other players
            if (multiplayerManager.LocalPlayerId != player.PlayerID)
                return;

            if (player.CanTakeDamage)
            {
                // Hurt player
                SetPlayerHealth(player, player.Health - 1, true);
            
                // Send player hurt event
                hubConnection.SendAsync("HurtPlayer");
            }
        };
    }

    private void Update()
    {
        while (actions.TryDequeue(out var action))
            action();
        
        // Sync player position
        var vgPlayerManager = VGPlayerManager.Inst;
        if (vgPlayerManager)
        {
            // Get local player
            var localPlayer = vgPlayerManager.players
                .ToEnumerable()
                .FirstOrDefault(x => x.PlayerID == LazySingleton<MultiplayerManager>.Instance.LocalPlayerId);
            if (localPlayer == null)
                return;
        
            var playerObject = localPlayer.PlayerObject;
            if (!playerObject)
                return;
        
            // Send player position
            var position = playerObject.Player_Rigidbody.position;
            hubConnection.SendAsync("UpdatePlayerPosition", new C2SUpdatePlayerPositionPacket
            {
                X = position.x,
                Y = position.y
            });
        }
    }
    
    private void ActivateCheckpoint(int checkpointIndex)
    {
        var gameManager = GameManager.Inst;
        gameManager.playingCheckpointAnimation = true;
        VGPlayerManager.Inst.RespawnPlayers();
        gameManager.StartCoroutine(gameManager.PlayCheckpointAnimation(checkpointIndex));
    }

    private void SetPlayerHealth(VGPlayer player, int newHealth, bool playHurtAnimation)
    {
        // Set new player health
        var oldHealth = player.Health;
        player.Health = newHealth;
                
        if (oldHealth > newHealth)
        {
            // pidge please fix your naming conventions
            if (newHealth > 0)
                player.HitEvent?.Invoke(player.Health, player.Player_Wrapper.position);
            else
                player.DeathEvent?.Invoke(player.Player_Wrapper.position);

            if (playHurtAnimation)
            {
                if (newHealth > 0)
                {
                    player.StartHurtDecay();
                    AudioManager.Inst.ApplyLowPass(0.05f, 0.4f, 1.0f);
                    AudioManager.Inst.PlaySound("HurtPlayer", "SFX");
                    player.PlayerHitAnimation();
                }
                else
                {
                    player.PlayerDeath();
                    player.PlayerDeathAnimation();
                }
            }
        }
    }
}