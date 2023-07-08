using System.Numerics;

namespace IL2TelemetryRelay.Events;

public record RocketLaunchEvent : Event
{
    public readonly Vector3 Offset;
    public readonly float   Mass;
    public readonly ushort  Type; // Appears to always be 1 to indicate rocket

    public RocketLaunchEvent(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        Offset = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset),
            Y = BitConverter.ToSingle(packet, offset + 4),
            Z = BitConverter.ToSingle(packet, offset + 8)
        };
        Mass = BitConverter.ToSingle(packet, offset + 12);
        Type = BitConverter.ToUInt16(packet, offset + 16);
    }
}