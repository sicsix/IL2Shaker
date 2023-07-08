using System.Numerics;

namespace IL2TelemetryRelay.Events;

public record HitEvent : Event
{
    public Vector3 Offset;
    public Vector3 Force;

    public HitEvent(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        Offset = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset),
            Y = BitConverter.ToSingle(packet, offset + 4),
            Z = BitConverter.ToSingle(packet, offset + 8)
        };

        // Appears to be forces applied in each direction
        Force = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset + 12),
            Y = BitConverter.ToSingle(packet, offset + 16),
            Z = BitConverter.ToSingle(packet, offset + 20)
        };
    }
}