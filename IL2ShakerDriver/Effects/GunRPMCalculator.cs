using IL2ShakerDriver.DB;

namespace IL2ShakerDriver.Effects;

internal class GunRPMCalculator
{
    private const int SampleSize = 200;

    private          string    _aircraftName;
    private readonly List<Gun> _guns;
    private readonly bool[]    _calculating   = new bool[16];
    private readonly int[]     _indexes       = new int[16];
    private readonly float[][] _timeDiffs     = new float[16][];
    private readonly uint[]    _lastFiredTick = new uint[16];
    private readonly float[]   _gunRPM        = new float[16];

    public GunRPMCalculator(List<Gun> guns)
    {
        _guns = guns;
        for (int i = 0; i < _timeDiffs.Length; i++)
        {
            _timeDiffs[i] = new float[SampleSize];
        }
    }

    public void SetAircraftName(string aircraftName)
    {
        _aircraftName = aircraftName;
    }

    public void UpdateFireRate(uint tick, int index)
    {
        if (!_calculating[index])
        {
            Logging.At(this)
               .Debug("Gun {Idx}: Calculating rate of fire - hold down trigger for a few seconds then release", index);
            _calculating[index] = true;
        }

        if (_lastFiredTick[index] == 0)
        {
            _lastFiredTick[index] = tick;
            return;
        }

        _timeDiffs[index][_indexes[index]] = (tick - _lastFiredTick[index]) * 0.020f;

        _indexes[index]       = (_indexes[index] + 1) % SampleSize;
        _lastFiredTick[index] = tick;
    }

    public void CalculateIfStopped(SimTime simTime, int index)
    {
        uint lastFiredTick = _lastFiredTick[index];
        if (lastFiredTick == 0)
            return;

        // If we haven't fired a shot for 1 second call it done
        if ((int)(simTime.Tick - lastFiredTick) > SimClock.UpdateRate * 2)
            CalculateRPM(index);
    }

    private void CalculateRPM(int index)
    {
        float sum   = 0;
        int   count = 0;
        for (int i = 0; i < _timeDiffs[index].Length; i++)
        {
            float diff = _timeDiffs[index][i];
            _timeDiffs[index][i] = 0f;
            if (diff == 0)
                continue;
            sum += diff;
            count++;
        }

        _indexes[index] = 0;
        float rpm = 60 / (sum / count);
        _gunRPM[index] = rpm;
        Logging.At(this).Debug("Gun {Index}: RPM {BwShots:F1}", index, rpm);
        _calculating[index]   = false;
        _lastFiredTick[index] = 0;

        bool calculatingComplete = true;
        for (int i = 0; i < _calculating.Length; i++)
        {
            if (!_calculating[i])
                continue;
            calculatingComplete = false;
            break;
        }

        if (calculatingComplete)
            LogGunYAML();
    }

    private void LogGunYAML()
    {
        string gunData = string.Empty;
        for (int i = 1; i < _guns.Count; i++)
        {
            gunData = CheckAndAddGuns(gunData, _guns[i], i);
        }

        gunData = CheckAndAddFinalGuns(gunData);

        Logging.At(this).Debug("Gun YAML data:\r\n\r\n- Name: {Aircraft}\r\n  Guns:\r\n{GunData}\r\n\r\n",
                               _aircraftName, gunData);
    }

    private Gun _currGun;
    private int _gunStartIndex;
    private int _gunCount;

    private string CheckAndAddGuns(string gunData, Gun gun, int index)
    {
        if (_gunCount == 0 || _currGun == gun)
        {
            _gunCount++;
            _currGun = gun;
            return gunData;
        }

        gunData = AddGuns(gunData, index - 1);

        _currGun       = gun;
        _gunCount      = 1;
        _gunStartIndex = index;

        return gunData;
    }

    private string CheckAndAddFinalGuns(string gunData)
    {
        if (_gunCount == 0)
        {
            _gunStartIndex = 0;
            return gunData;
        }

        gunData        = AddGuns(gunData, _guns.Count - 1);
        _currGun       = default;
        _gunCount      = 0;
        _gunStartIndex = 0;
        return gunData;
    }

    private string AddGuns(string gunData, int index)
    {
        float  rpm     = _gunRPM[index];
        string indexes = string.Empty;
        for (int i = _gunStartIndex; i <= index; i++)
        {
            if (i != _gunStartIndex)
                indexes += $", {i}";
            else
                indexes += $"{i}";
        }

        gunData += "    - Name: PUT_NAME_HERE\r\n"
                 + $"      RPM: {(int)Math.Round(rpm)}\r\n"
                 + $"      Indexes: [ {indexes} ]\r\n"
                 + $"      Velocity: [ {_currGun.Velocity:G9} ]\r\n"
                 + $"      Mass: [ {_currGun.Mass:G9} ]\r\n";
        return gunData;
    }
}