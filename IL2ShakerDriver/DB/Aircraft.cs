namespace IL2ShakerDriver.DB;

internal struct Aircraft
{
    public string        Name;
    public float[]?      EngineHarmonics;
    public List<RawGun>? Guns;
}