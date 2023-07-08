namespace IL2TelemetryRelay.State;

internal enum StateType : ushort
{
    RPM                           = 0,
    IntakeManifoldPressurePa      = 1,
    Val2                          = 2,
    Val3                          = 3,
    LandingGearPosition           = 4,
    LandingGearPressure           = 5,
    IndicatedAirspeedMetresSecond = 6,
    Val7                          = 7,
    Acceleration                  = 8,
    StallBuffet                   = 9,
    AboveGroundLevelMetres        = 10,
    FlapsPosition                 = 11,
    AirBrakePosition              = 12
}