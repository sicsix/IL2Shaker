using IL2TelemetryRelay;
using IL2TelemetryRelay.Motion;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal abstract class Effect : ISampleProvider
{
    public bool       Enabled    { get; set; }
    public Volume     Volume     { get; private set; }
    public WaveFormat WaveFormat { get; }

    protected readonly ISampleProvider Source;
    protected readonly Audio           Audio;
    protected readonly object          Lock = new();

    protected Effect(ISampleProvider source, Audio audio)
    {
        WaveFormat = source.WaveFormat;
        Audio      = audio;
        Source     = source;

        Listener.EventDataReceived  += EventDataReceived;
        Listener.MotionDataReceived += MotionDataReceived;
        Listener.StateDataReceived  += StateDataReceived;
    }

    public void UpdateSettings(IEffectSettings settings)
    {
        lock (Lock)
        {
            bool  switchedState = settings.Enabled != Enabled;
            float previousdB    = Volume.dB;

            Enabled = settings.Enabled;
            SetDecibelsFromMaster(settings.Value);

            if (switchedState || previousdB != Volume.dB)
            {
                Logging.At(this).Verbose("{State}: {Rel}dB relative ({Vol}dB)", Enabled ? "Enabled" : "Disabled",
                                         settings.Value, Volume.dB);
            }

            OnSettingsUpdated();
        }
    }

    protected virtual void OnSettingsUpdated()
    {
    }

    private void SetDecibelsFromMaster(float db)
    {
        Volume = new Volume(Audio.MasterVolume, db);
    }

    private protected float GetAmplitude(float db)
    {
        return GetVolume(db).Amplitude;
    }

    private protected Volume GetVolume(float db)
    {
        return new Volume(Volume, db);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samples = Source.Read(buffer, offset, count);

        if (!Enabled)
            return samples;

        lock (Lock)
        {
            Write(buffer, offset, count);
        }

        return samples;
    }

    protected abstract void Write(float[] buffer, int offset, int count);

    private void EventDataReceived(Event eventData)
    {
        lock (Lock)
        {
            OnEventDataReceived(eventData);
        }
    }

    protected virtual void OnEventDataReceived(Event eventData)
    {
    }

    private void MotionDataReceived(MotionData motionData)
    {
        lock (Lock)
        {
            OnMotionDataReceived(motionData);
        }
    }

    protected virtual void OnMotionDataReceived(MotionData motionData)
    {
    }

    private void StateDataReceived(StateData stateData)
    {
        lock (Lock)
        {
            OnStateDataReceived(stateData);
        }
    }

    protected virtual void OnStateDataReceived(StateData stateData)
    {
    }

    // Attenuation is in decibels/m
    private const float AttenLow     = 0.05f;
    private const float AttenHigh    = 0.3f;
    private const float AttenStartHz = 10f;
    private const float AttenEndHz   = 120f;

    protected static float Attenuate(float freq, float amp, float distance)
    {
        float clampedFreq = Math.Max(Math.Min(freq, AttenEndHz), AttenStartHz) - AttenStartHz;
        float coefficient = AttenLow + (AttenHigh - AttenLow) * (clampedFreq / (AttenEndHz - AttenStartHz));
        float attenuation = MathF.Exp(-coefficient * distance);
        float amplitude   = attenuation * amp;
        return amplitude;
    }
}