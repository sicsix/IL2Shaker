using System.Runtime.CompilerServices;
using Serilog;

namespace IL2TelemetryRelay;

internal static class Logging
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger At(object caller)
    {
        return At(caller.GetType());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger At(Type type)
    {
        return Log.ForContext("Class", $"[{type.Name}]");
    }
}