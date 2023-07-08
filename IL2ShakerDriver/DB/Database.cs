using System.Numerics;
using YamlDotNet.Serialization;

namespace IL2ShakerDriver.DB;

internal class Database
{
    private readonly Dictionary<GunID, Gun>      _guns            = new();
    private readonly Dictionary<string, Vector4> _engineHarmonics = new();
    private          Gun                         _defaultGun;
    private          Vector4                     _defaultEngine;

    public void LoadFiles()
    {
        Logging.At(this).Information("Loading aircraft data...");

        using TextReader reader = new StreamReader(Directory.GetCurrentDirectory() + @"\Planes.yaml");

        var aircrafts = new Deserializer().Deserialize<List<Aircraft>>(reader);

        float biggestGun  = 0;
        float smallestGun = float.MaxValue;

        foreach (var aircraft in aircrafts)
        {
            Logging.At(this).Verbose("Loading {Name}...", aircraft.Name);

            if (aircraft.Name == "Default")
            {
                _defaultEngine = new Vector4(aircraft.EngineHarmonics);
                var gun = aircraft.Guns[0];
                _defaultGun = new Gun("Default", gun.RPM, 0, 0);
                continue;
            }

            if (aircraft.EngineHarmonics == null)
                _engineHarmonics.Add(aircraft.Name, _defaultEngine);
            else
                _engineHarmonics.Add(aircraft.Name, new Vector4(aircraft.EngineHarmonics));

            if (aircraft.Guns == null)
                continue;
            {
                for (int i = 0; i < aircraft.Guns.Count; i++)
                {
                    var gun = aircraft.Guns[i];

                    if (gun.Mass.Length != gun.Velocity.Length || gun.Mass.Length == 0)
                    {
                        Logging.At(this)
                           .Warning("{Name} - Invalid data - there must be at least one of both mass and velocity and the counts must match",
                                    aircraft.Name);
                        continue;
                    }

                    for (int j = 0; j < gun.Indexes.Length; j++)
                    {
                        int index = gun.Indexes[j];
                        if (index < 0)
                        {
                            Logging.At(this).Warning("{Name} - Invalid index ({Index}) - indexes must be >= 0",
                                                     aircraft.Name, index);
                            break;
                        }

                        for (int k = 0; k < gun.Mass.Length; k++)
                        {
                            float velocity = gun.Velocity[k];
                            float mass     = gun.Mass[k];
                            if (velocity is <= 0 or > 3000)
                            {
                                Logging.At(this)
                                   .Warning("{Name}:{GunName} - Invalid velocity ({Velocity}) - must be > 0 and < 3000",
                                            aircraft.Name, gun.Name, velocity);
                                continue;
                            }

                            if (mass is <= 0 or > 10)
                            {
                                Logging.At(this)
                                   .Warning("{Name}:{GunName} - Invalid mass ({Mass}) - must be > 0 and < 10",
                                            aircraft.Name, gun.Name, mass);
                                continue;
                            }

                            var gunID  = new GunID(aircraft.Name, index, velocity, mass);
                            var newGun = new Gun(gun.Name, gun.RPM, velocity, mass);

                            if (newGun.KineticEnergy > biggestGun)
                                biggestGun = newGun.KineticEnergy;
                            if (newGun.KineticEnergy < smallestGun)
                                smallestGun = newGun.KineticEnergy;

                            _guns.Add(gunID, newGun);
                        }
                    }
                }
            }
        }

        // Logging.At(this).Debug("Biggest KE: {Big}    Smallest KE: {Small}", biggestGun, smallestGun);
        Logging.At(this).Information("Data for {Count} aircraft loaded", aircrafts.Count);
    }

    public Gun GetGun(GunID gunID)
    {
        if (_guns.TryGetValue(gunID, out var gun))
            return gun;
        Logging.At(this).Warning("{GunID} not found in gun DB - using default settings, RPM will be incorrect", gunID);
        return new Gun("Unknown", _defaultGun.RPM, gunID.Velocity, gunID.Mass);
    }

    public Vector4 GetEngineHarmonics(string aircraftName)
    {
        if (_engineHarmonics.TryGetValue(aircraftName, out var harmonics))
            return harmonics;
        Logging.At(this).Warning("{Name} not found in engine DB - using default engine", aircraftName);
        return _defaultEngine;
    }
}