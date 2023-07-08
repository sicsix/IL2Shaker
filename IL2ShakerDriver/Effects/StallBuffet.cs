using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class StallBuffet : Effect
{
    private const    float         FrequencyMultiplier = 4f;
    private const    float         AmplitudeMultiplier = 3.5f;
    private const    float         TransitionTime      = 0.05f;
    private readonly WaveGenerator _waveGenerator      = new();

    private float _maxAmp;

    public StallBuffet(ISampleProvider source, Audio audio) : base(source, audio)
    {
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
        if (Audio.SimSpeed != SimSpeed.x1)
            _waveGenerator.SetTarget(0, 0, TransitionTime);
        _waveGenerator.Write(buffer, offset, count, Audio.SimClock.Time);
    }

    protected override void OnStateDataReceived(StateData stateData)
    {
        float nextFreq = stateData.StallBuffetFrequency * FrequencyMultiplier;
        float nextAmp  = stateData.StallBuffetAmplitude * AmplitudeMultiplier;

        if (stateData.StallBuffetAmplitude > _maxAmp)
        {
            _maxAmp = stateData.StallBuffetAmplitude;
            // Logging.At(this).Debug("Stall buffet amplitude suggested multiplier {MaxAmp}",
            //                        1 / _maxAmp);
        }

        if (nextAmp > 1)
        {
            Logging.At(this).Debug("Stall buffet amplitude greater than 1, capping - suggested multiplier {MaxAmp}",
                                   1 / _maxAmp);
            nextAmp = 1;
        }

        nextAmp *= Volume.Amplitude;

        _waveGenerator.SetTarget(nextFreq, nextAmp, TransitionTime);
    }
}