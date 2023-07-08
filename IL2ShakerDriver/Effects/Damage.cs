using System.Numerics;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class Damage : Effect
{
    private uint    _damageTick;
    private Vector3 _damageOffset;
    private uint    _count;
    private float   _strength;

    public Damage(ISampleProvider source, Audio audio) : base(source, audio)
    {
        // TODO This is a stub at the moment. Need to figure out what these damage packets actually mean
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
    }

    protected override void OnEventDataReceived(Event eventData)
    {
        if (eventData is DamageEvent damagedEvent)
        {
            if (_damageTick != 0 && damagedEvent.Tick != _damageTick)
                Logging.At(this).Debug("Never cleared previous damage");

            if (_damageTick != 0 && _damageOffset != damagedEvent.Offset)
                Logging.At(this).Debug("Two different damage events in same tic");

            _damageTick   = damagedEvent.Tick;
            _damageOffset = damagedEvent.Offset;
            _strength     = damagedEvent.Float0;
            _count++;
        }
    }

    protected override void OnUpdate(SimTime simTime)
    {
        if (simTime.Tick > _damageTick && _strength != 0)
        {
            Logging.At(this)
               .Debug("Damaged {{ Tick = {Tick}, Offset = {Offset}, Strength = {Strength}, Count = {Count} }}",
                      _damageTick, _damageOffset, _strength, _count);
            _damageTick   = 0;
            _damageOffset = Vector3.Zero;
            _strength     = 0;
            _count        = 0;
        }
    }
}