using HarmonyLib;

namespace PolyLink.Patch;

[HarmonyPatch(typeof(GameManager))]
public class GameManagerPatch
{
    [HarmonyPatch(nameof(GameManager.CheckpointCheck))]
    [HarmonyPrefix]
    public static bool CancelCheckpoint()
    {
        return false;
    }
}