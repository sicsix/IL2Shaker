using System.Numerics;

namespace IL2TelemetryRelay.Events;

public record WheelDataEvent : Event
{
    public readonly int     Index;
    public readonly Vector3 Offset;

    public WheelDataEvent(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        Index = BitConverter.ToUInt16(packet, offset);
        // Second index, appears to be same as first
        // ushort index1 = BitConverter.ToUInt16(packet, offset + 2); 
        Offset = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset + 4), // Offset forwards+ backwards-
            Y = BitConverter.ToSingle(packet, offset + 8), // Offset up+ down-
            Z = BitConverter.ToSingle(packet, offset + 12) // Offset left- right+
        };
    }
}