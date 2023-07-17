using NAudio.Wave;

namespace IL2ShakerDriver.Samplers;

public class ZeroingSampler : ISampleProvider
{
    public WaveFormat WaveFormat { get; }

    private readonly ISampleProvider _source;

    public ZeroingSampler(ISampleProvider source)
    {
        WaveFormat = source.WaveFormat;
        _source    = source;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        Array.Clear(buffer);

        return _source.Read(buffer, offset, count);
    }
}