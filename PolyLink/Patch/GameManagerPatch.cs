using System;
using HarmonyLib;
using PolyLink.Util;

namespace PolyLink.Patch;

public delegate void CheckpointCrossedEventHandler(int checkpointIndex);

[HarmonyPatch(typeof(GameManager))]
public class GameManagerPatch
{
    public static event CheckpointCrossedEventHandler? CheckpointCrossed;
    
    [HarmonyPatch(nameof(GameManager.CheckpointCheck))]
    [HarmonyPrefix]
    public static bool CancelCheckpoint(ref GameManager __instance)
    {
        var checkpoints = DataManager.inst.gameData.beatmapData.checkpoints;
        if (__instance.checkpointsActivated.Length > 0 && checkpoints.Count > 0)
        {
            var crossedCheckpointIndex = -1;
            foreach (var (i, checkpoint) in checkpoints.ToEnumerable().Indexed())
            {
                if (__instance.CurrentSongTimeSmoothed >= checkpoint.time && !__instance.checkpointsActivated[i])
                {
                    crossedCheckpointIndex = i;
                    break;
                }
                
                if (__instance.CurrentSongTimeSmoothed < checkpoint.time)
                    break;
            }
            
            if (crossedCheckpointIndex != -1)
            {
                // __instance.playingCheckpointAnimation = true;
                // VGPlayerManager.Inst.RespawnPlayers();
                // __instance.StartCoroutine(__instance.PlayCheckpointAnimation(checkpointIndexToActivate));
                CheckpointCrossed?.Invoke(crossedCheckpointIndex);
            }
        }
        return false;
    }
}