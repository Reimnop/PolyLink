using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppSystems.SceneManagement;
using PolyLink.Util;

namespace PolyLink.Patch;

[HarmonyPatch(typeof(SceneLoader))]
public class SceneLoaderPatch
{
    private static readonly List<(string Name, Func<Task> TaskFactory)> loaders = [];
    
    [HarmonyPatch(nameof(SceneLoader.Start))]
    [HarmonyPostfix]
    public static void LoadMod(ref SceneLoader __instance)
    {
        foreach (var (name, taskFactory) in loaders)
        {
            __instance.manager.AddToLoadingTasks(new TaskData
            {
                Name = name,
                Task = taskFactory().ToIl2Cpp()
            });
        }
    }

    public static void RegisterLoader(string name, Func<Task> taskFactory)
    {
        loaders.Add((name, taskFactory));
    }
}