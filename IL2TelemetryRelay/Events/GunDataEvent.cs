using System.Numerics;

namespace IL2TelemetryRelay.Events;

public record GunDataEvent : Event
{
    public readonly int     Index;
    public readonly Vector3 Offset;
    public readonly float   Mass;
    public readonly float   Velocity;

    public GunDataEvent(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        Index = BitConverter.ToUInt16(packet, offset); // Gun Index
        Offset = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset + 2), // Offset forwards+ backwards-
            Y = BitConverter.ToSingle(packet, offset + 6), // Offset up+ down-
            Z = BitConverter.ToSingle(packet, offset + 10) // Offset left- right+
        };

        Mass     = BitConverter.ToSingle(packet, offset + 14); // In kg
        Velocity = BitConverter.ToSingle(packet, offset + 18); // In m/s
    }
}