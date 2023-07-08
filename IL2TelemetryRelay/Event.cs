using System.Diagnostics.CodeAnalysis;

namespace IL2TelemetryRelay;

public record Event
{
    public readonly uint Tick;

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    protected Event(uint tick, byte[] packet, int offset)
    {
        Tick = tick;
    }
}