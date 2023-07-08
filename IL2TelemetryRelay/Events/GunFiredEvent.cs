namespace IL2TelemetryRelay.Events;

public record GunFiredEvent : Event
{
    public readonly int Index;

    public GunFiredEvent(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        Index = packet[offset];
    }
}