using HarmonyLib;
using PolyLink.Util;

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
}