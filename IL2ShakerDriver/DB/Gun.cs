namespace IL2ShakerDriver.DB;

internal record struct Gun(string Name, float RPM, float Velocity, float Mass)
{
    public readonly string Name                = Name;
    public readonly float  RPM                 = RPM;
    public readonly float  Velocity            = Velocity;
    public readonly float  Mass                = Mass;
    public readonly float  KineticEnergy       = 0.5f * Mass * Velocity * Velocity / 1000f;
    public readonly float  SqrKineticEnergy    = MathF.Sqrt(0.5f                  * Mass * Velocity * Velocity / 1000f);
    public readonly int    SamplesBetweenShots = (int)(SimClock.SampleRate / (RPM / 60f));
}