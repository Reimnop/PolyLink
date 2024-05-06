namespace PolyLink;

public static class Log
{
    public static void Info(string message)
    {
        Plugin.Instance.Log.LogInfo(message);
    }
    
    public static void Warn(string message)
    {
        Plugin.Instance.Log.LogWarning(message);
    }
    
    public static void Error(string message)
    {
        Plugin.Instance.Log.LogError(message);
    }
}