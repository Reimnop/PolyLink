using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Parctan.Modding.Localization;
using PolyLink.Ui;

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
        
        // Register i18n files
        LocalizationManager.Register(Assembly.GetExecutingAssembly(), "Resources.I18n", "PolyLink");
        
        // Register main menu UI
        Menu.Register();

        // AddComponent<PluginProcess>();
        
        // SceneLoaderPatch.RegisterLoader("PolyLink", async () =>
        // {
        //     SteamClient.Init(AppId);
        //     await Loader.LoadAsync();
        // });
    }
}