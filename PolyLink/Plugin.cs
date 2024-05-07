using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace PolyLink;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public static Plugin Instance { get; private set; } = null!;
    
    private Harmony harmony = null!;
    
    public override void Load()
    {
        Instance = this;
        
        harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        AddComponent<PluginProcess>();
    }
}