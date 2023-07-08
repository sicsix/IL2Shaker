# IL2Shaker

IL2Shaker is a bass shaker driver exclusively for [IL-2 Sturmovik: Great Battles](https://il2sturmovik.com/).

The goal is to provide high quality effects tailored specifically to IL2 using as much of the telemetry data as possible
to generate effects on the fly at run-time. 
It is designed for ButtKicker and Dayton style transducers but may work with other setups.

IL2Shaker currently has these effects modelled:
* Engine vibrations modelled using crankshaft and camshaft frequencies and harmonics, varying with the current RPM and each engine is modelled individually in multi-engine aircraft.
* Simulation time is modelled to provide accurately spaced gun fire according to each guns RPM. Frequency and volume of gun fire effects are determined by the kinetic energy of the projectile - big guns are deep and powerful.
* Stall buffet effects simulate stalling with the amplitude and frequency of the vibrations varying according to conditions.
* Bumps to the landing gear during takeoff and landing.
* Landing gear and flaps extension and retraction.
* Received hits - bigger hits mean more volume and lower frequencies.
* Impacts both midair and ground.

## Usage

First, edit `startup.cfg` in the `data/` folder in your IL-2 installation directory to include the following:

```
[KEY = telemetrydevice]
    addr = "127.0.0.1"
    decimation = 1
    enable = true
    port = 29373
[END]
```

Decimation must be set to 1 to enable high frequency telemetry, any other number will result in the program failing to work.

After this, download and extract the latest release from [releases](https://github.com/sicsix/IL2Shaker/releases), run `IL2Shaker.exe`, then select the correct audio output.
Adjust volumes to suit your equipment and taste.

## Contributing

Pull requests are welcome.

## License

[GPL 3.0](https://choosealicense.com/licenses/gpl-3.0/)