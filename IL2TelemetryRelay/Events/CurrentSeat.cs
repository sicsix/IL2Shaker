namespace IL2TelemetryRelay.Events;

public record CurrentSeat : Event
{
    public readonly uint   Seat;
    public readonly ushort Ushort0;

    public CurrentSeat(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        // Triggers on seat change     
        // Seems to be all 1s (4294967295) if pilot or co-pilot, and 1023 if any gunner
        // Not sure what the ushort value represents, but seems to be 1 or 2
        Seat    = BitConverter.ToUInt32(packet, offset);
        Ushort0 = BitConverter.ToUInt16(packet, offset + 4);
    }
}