using System.Numerics;

namespace IL2TelemetryRelay.State;

public record StateData : Event
{
    public int     EngineCount;
    public Vector4 RPM;
    public Vector4 IntakeManifoldPressurePa;

    public int     LandingGearCount;
    public Vector4 LandingGearPosition;
    public Vector4 LandingGearPressure;

    public float IndicatedAirSpeedMetresSecond;

    // Longitudinal, Vertical, Lateral
    public Vector3 Acceleration;

    public float StallBuffetFrequency;
    public float StallBuffetAmplitude;

    public float AboveGroundLevelMetres;

    public float FlapsPosition;
    public float AirBrakePosition;

    public Vector4 Val2;
    public float   Val3;
    public float   Val7;

    public bool Paused;

    public StateData(uint tick, byte[] packet, int offset) : base(tick, packet, offset)
    {
    }
}