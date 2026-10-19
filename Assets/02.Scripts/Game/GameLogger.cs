using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameLogger : MonoBehaviour
{
    [Conditional("ENABLE_LOG")]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
}