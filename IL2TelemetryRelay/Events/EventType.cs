namespace IL2TelemetryRelay.Events;

internal enum EventType : ushort
{
    VehicleName  = 0,
    EngineData   = 1,
    GunData      = 2,
    WheelData    = 3,
    BombRelease  = 4,
    RocketLaunch = 5,
    Event6       = 6,
    Hit          = 7,
    Damage       = 8,
    GunFired     = 9,
    Event13      = 13,
    CurrentSeat  = 14
}