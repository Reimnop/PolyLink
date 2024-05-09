using HarmonyLib;
using PolyLink.Util;

namespace PolyLink.Patch;

[HarmonyPatch(typeof(VGPlayer))]
public class VGPlayerPatch
{
    [HarmonyPatch(nameof(VGPlayer.OnChildTriggerEnter))]
    [HarmonyPrefix]
    public static bool CancelPlayerTriggerEnter()
    {
        return false;
    }
    
    [HarmonyPatch(nameof(VGPlayer.OnChildTriggerStay))]
    [HarmonyPrefix]
    public static bool CancelPlayerTriggerStay()
    {
        return false;
    }
}