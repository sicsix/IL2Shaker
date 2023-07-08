namespace IL2ShakerDriver.DB;

internal record GunID(string AircraftName, int Index, float Velocity, float Mass)
{
    public readonly string AircraftName = AircraftName;
    public readonly int    Index        = Index;
    public readonly float  Velocity     = Velocity;
    public readonly float  Mass         = Mass;

    public override string ToString()
    {
        return
            $"GunID {{ AircraftName = {AircraftName}, Index = {Index}, Velocity = {Velocity:G9}, Mass = {Mass:G9} }}";
    }
}