using NAudio.Utils;

namespace IL2ShakerDriver;

internal struct Volume
{
    public readonly float dB;
    public readonly float Amplitude;

    public Volume(float dB)
    {
        this.dB   = dB;
        Amplitude = (float)Decibels.DecibelsToLinear(dB);
    }

    public Volume(Volume reference, float dB)
    {
        this.dB   = reference.dB + dB;
        Amplitude = (float)Decibels.DecibelsToLinear(this.dB);
    }
}