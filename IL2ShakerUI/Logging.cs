using System;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;

namespace IL2ShakerUI;

internal static class Logging
{
    public static readonly LoggingLevelSwitch LevelSwitch = new();

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