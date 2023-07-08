using System.Numerics;

namespace IL2TelemetryRelay.Events;

public record Event6 : Event
{
    public Vector3 Vec0;
    public Vector3 Vec1;

    public Event6(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        // Seems to be 2 x float3 ??
        Vec0 = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset),
            Y = BitConverter.ToSingle(packet, offset + 4),
            Z = BitConverter.ToSingle(packet, offset + 8)
        };

        Vec1 = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset + 12),
            Y = BitConverter.ToSingle(packet, offset + 16),
            Z = BitConverter.ToSingle(packet, offset + 20)
        };
    }
}