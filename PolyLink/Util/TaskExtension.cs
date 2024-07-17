using System.Threading.Tasks;

namespace PolyLink.Util;

public static class TaskExtension
{
    public static Il2CppSystem.Threading.Tasks.Task ToIl2Cpp(this Task task)
    {
        return Il2CppSystem.Threading.Tasks.Task.Run((Il2CppSystem.Action)(() => task.Wait()));
    }
    
    public static Task ToTask(this Il2CppSystem.Threading.Tasks.Task task)
    {
        return Task.Run(task.Wait);
    }
}