namespace IL2TelemetryRelay.Motion;

public record MotionData : Event
{
    public readonly float Yaw;
    public readonly float Pitch;
    public readonly float Roll;
    public readonly float SpinX;
    public readonly float SpinY;
    public readonly float SpinZ;
    public readonly float AccX;
    public readonly float AccY;
    public readonly float AccZ;

    public MotionData(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
        // Heading (Radians) 0 is North
        Yaw   = BitConverter.ToSingle(packet, offset);
        // Pitch (Radians) 0 is level
        Pitch = BitConverter.ToSingle(packet, offset + 4);
        // Roll (Radians) 0 is level
        Roll  = BitConverter.ToSingle(packet, offset + 8);
        // Roll rate (Radians/s)
        SpinX = BitConverter.ToSingle(packet, offset + 12);
        // Pitch rate (Radians/s)
        SpinY = BitConverter.ToSingle(packet, offset + 16);
        // Yaw rate (Radians/s)
        SpinZ = BitConverter.ToSingle(packet, offset + 20);
        // In m/s
        AccX  = BitConverter.ToSingle(packet, offset + 24);
        AccY  = BitConverter.ToSingle(packet, offset + 28);
        AccZ  = BitConverter.ToSingle(packet, offset + 32);
    }
}