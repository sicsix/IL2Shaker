using MathNet.Filtering;
using NAudio.Wave;

namespace IL2ShakerDriver.Samplers;

internal class HighPassFilter : ISampleProvider
{
    public WaveFormat WaveFormat { get; }

    private          bool         _enabled;
    private          float        _cuttOffFreq;
    private readonly int          _order;
    private          OnlineFilter _filter;

    private readonly ISampleProvider _source;

    public HighPassFilter(ISampleProvider source, int cutOffFreq, int order)
    {
        WaveFormat = source.WaveFormat;
        _cuttOffFreq = cutOffFreq;
        _order = order;
        _source = source;
        _filter = OnlineFilter.CreateHighpass(ImpulseResponse.Finite, source.WaveFormat.SampleRate, cutOffFreq, order);
    }

    public void UpdateSettings(IEffectSettings settings)
    {
        bool switchedState = settings.Enabled != _enabled;
        _enabled = settings.Enabled;

        if (_cuttOffFreq == settings.Value && !switchedState)
            return;

        Logging.At(this).Verbose("{State}: {Hz}Hz", _enabled ? "Enabled" : "Disabled", settings.Value);
        _cuttOffFreq = settings.Value;
        _filter      = OnlineFilter.CreateHighpass(ImpulseResponse.Finite, WaveFormat.SampleRate, _cuttOffFreq, _order);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samples = _source.Read(buffer, offset, count);

        if (!_enabled)
            return samples;

        for (int i = 0; i < samples; i++)
        {
            buffer[offset + i] = (float)_filter.ProcessSample(buffer[offset + i]);
        }

        return samples;
    }
}