using HarmonyLib;
using PolyLink.Util;

namespace PolyLink.Patch;

[HarmonyPatch(typeof(VGPlayer))]
public class VGPlayerPatch
{
    [HarmonyPatch(nameof(VGPlayer.OnChildTriggerEnter))]
    [HarmonyPrefix]
    public static bool CancelPlayerTriggerEnter(ref VGPlayer __instance)
    {
        var multiplayerManager = LazySingleton<MultiplayerManager>.Instance;
        return multiplayerManager.LocalPlayerId == __instance.PlayerID;
    }
    
    [HarmonyPatch(nameof(VGPlayer.OnChildTriggerStay))]
    [HarmonyPrefix]
    public static bool CancelPlayerTriggerStay(ref VGPlayer __instance)
    {
        var multiplayerManager = LazySingleton<MultiplayerManager>.Instance;
        return multiplayerManager.LocalPlayerId == __instance.PlayerID;
    }
}