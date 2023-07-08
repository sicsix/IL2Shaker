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
        Yaw   = BitConverter.ToSingle(packet, offset);
        Pitch = BitConverter.ToSingle(packet, offset + 4);
        Roll  = BitConverter.ToSingle(packet, offset + 8);
        SpinX = BitConverter.ToSingle(packet, offset + 12);
        SpinY = BitConverter.ToSingle(packet, offset + 16);
        SpinZ = BitConverter.ToSingle(packet, offset + 20);
        AccX  = BitConverter.ToSingle(packet, offset + 24);
        AccY  = BitConverter.ToSingle(packet, offset + 28);
        AccZ  = BitConverter.ToSingle(packet, offset + 32);
    }
}