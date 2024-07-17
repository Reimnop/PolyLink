using HarmonyLib;
using PolyLink.Util;
using Rewired;

namespace PolyLink.Patch;

public delegate void PlayerCollideEventHandler(VGPlayer sender);

[HarmonyPatch(typeof(VGPlayer))]
public class VGPlayerPatch
{
    public static event PlayerCollideEventHandler? PlayerCollide;
    
    [HarmonyPatch(nameof(VGPlayer.OnChildTriggerEnter))]
    [HarmonyPrefix]
    public static bool CancelPlayerTriggerEnter(ref VGPlayer __instance)
    {
        PlayerCollide?.Invoke(__instance);
        return false;
    }
    
    [HarmonyPatch(nameof(VGPlayer.OnChildTriggerStay))]
    [HarmonyPrefix]
    public static bool CancelPlayerTriggerStay(ref VGPlayer __instance)
    {
        PlayerCollide?.Invoke(__instance);
        return false;
    }
    
    [HarmonyPatch(nameof(VGPlayer.Update))]
    [HarmonyPrefix]
    public static bool FreezePlayer(ref VGPlayer __instance)
    {
        var localPlayerId = LazySingleton<MultiplayerManager>.Instance.LocalPlayerId;
        if (__instance.PlayerID == localPlayerId)
            return true;
        
        // Freeze player if player is not local player
        __instance.RotatePlayer();
        return false;
    }
    
    [HarmonyPatch($"get_{nameof(VGPlayer.RPlayer)}")]
    [HarmonyPrefix]
    public static bool CancelPlayerRPlayer(ref VGPlayer __instance, ref Rewired.Player? __result)
    {
        // Force keyboard input for local player
        var localPlayerId = LazySingleton<MultiplayerManager>.Instance.LocalPlayerId;
        if (__instance.PlayerID == localPlayerId)
        {
            __result = ReInput.players.GetPlayer(0);
            return false;
        }
        
        return true;
    }
}