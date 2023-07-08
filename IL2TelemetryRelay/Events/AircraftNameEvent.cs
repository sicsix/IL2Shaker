using System.Text;

namespace IL2TelemetryRelay.Events;

public record AircraftNameEvent : Event
{
    public readonly string Name;

    public AircraftNameEvent(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        byte nameLength = packet[offset];
        Name = Encoding.ASCII.GetString(packet, offset + 1, nameLength).TrimEnd('\0');
    }
}