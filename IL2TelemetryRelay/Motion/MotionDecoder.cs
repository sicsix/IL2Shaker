namespace IL2TelemetryRelay.Motion;

internal static class MotionDecoder
{
    internal static Event Decode(byte[] packet)
    {
        uint tick = BitConverter.ToUInt32(packet, 4);
        return new MotionData(tick, packet, 8);
    }
}