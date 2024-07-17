using System;
using HarmonyLib;

namespace PolyLink.Patch;

[HarmonyPatch(typeof(UI_Book))]
public class UiBookPatch
{
    public static event Action<UI_Book>? Initialize;
    
    [HarmonyPatch(nameof(UI_Book.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(UI_Book __instance)
    {
        if (__instance.gameObject.name != "Canvas")
            return;

        Initialize?.Invoke(__instance);
    }
}