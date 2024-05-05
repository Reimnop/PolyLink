using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace PolyLink;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public static Plugin Instance { get; private set; } = null!;
    
    public override void Load()
    {
        Instance = this;
        
        // Plugin startup logic
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
}