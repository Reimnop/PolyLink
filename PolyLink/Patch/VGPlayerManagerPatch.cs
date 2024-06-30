using HarmonyLib;
using UnityEngine;

namespace PolyLink.Patch;

[HarmonyPatch(typeof(VGPlayerManager))]
public class VGPlayerManagerPatch
{
    [HarmonyPatch(nameof(VGPlayerManager.SpawnPlayers))]
    [HarmonyPrefix]
    public static void ReplaceActions(ref Il2CppSystem.Action<int, Vector3> _hitAction, ref Il2CppSystem.Action<Vector3> _deathAction)
    {
        _deathAction = (Il2CppSystem.Action<Vector3>)OnPlayerDeath;
    }
    
    private static void OnPlayerDeath(Vector3 position)
    {
        Log.Info($"Player died at {position}");
    }
}