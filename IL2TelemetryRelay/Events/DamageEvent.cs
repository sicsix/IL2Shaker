using System.Numerics;

namespace IL2TelemetryRelay.Events;

public record DamageEvent : Event
{
    public readonly Vector3 Offset;
    public readonly float   Float0;

    public DamageEvent(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        // This event is spammed on being hit, perhaps a damaging hit?
        // Offset lines up very closely with a received HitEvent from the previous tick
        // Maybe the volume of these events indicates extent of damage?
        // No other way to look at it unless there's a bug in IL-2 that causes this to be spammed
        Offset = new Vector3
        {
            X = BitConverter.ToSingle(packet, offset),
            Y = BitConverter.ToSingle(packet, offset + 4),
            Z = BitConverter.ToSingle(packet, offset + 8)
        };
        // Seems to depend on what hit by 2.8f, 3.8f for small cal guns, 59.6 for running into the ground
        // Might not be a float but sure looks like the bits in front are the sign/exponent
        // Bit patterns are either:
        // 0100 0000_0011 0011_0011 0011_0011 0011
        // or
        // 0100 0000_0111 0011_0011 0011_0011 0011
        // Could indicate damage type?
        Float0 = BitConverter.ToSingle(packet, offset + 12);
    }
}