using System.Numerics;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class Engine : Effect
{
    private readonly List<HarmonicsGenerator> _engines;
    private          Vector4                  _distance;

    private readonly Vector4 _harmonics   = new(1, 2, 3, 4);
    private          Vector4 _harmonicsDB = new(-6, 0, -6, -3);

    private Vector4 _amplitudes;

    private const float TransitionTime = 0.05f;

    public Engine(ISampleProvider source, Audio audio) : base(source, audio)
    {
        _engines = new List<HarmonicsGenerator>(4);
        for (int i = 0; i < 4; i++)
        {
            _engines.Add(new HarmonicsGenerator((int)_harmonics[1], (int)_harmonics[2], (int)_harmonics[3]));
        }
    }

    protected override void OnSettingsUpdated()
    {
        _amplitudes = new Vector4(GetAmplitude(_harmonicsDB[0]), GetAmplitude(_harmonicsDB[1]),
                                  GetAmplitude(_harmonicsDB[2]), GetAmplitude(_harmonicsDB[3]));
    }


    protected override void Write(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < _engines.Count; i++)
        {
            if (Audio.SimSpeed != SimSpeed.x1)
                _engines[i].SetTarget(0, Vector4.Zero, 0.1f);
            _engines[i].Write(buffer, offset, count, Audio.SimClock.Time);
        }
    }

    protected override void OnEventDataReceived(Event eventData)
    {
        if (eventData is AircraftNameEvent aircraftNameEvent)
        {
            _harmonicsDB = Audio.Database.GetEngineHarmonics(aircraftNameEvent.Name);
            OnSettingsUpdated();
        }
        else if (eventData is EngineDataEvent engineDataEvent)
            _distance[engineDataEvent.Index] = engineDataEvent.Offset.Length();
    }

    protected override void OnStateDataReceived(StateData stateData)
    {
        for (int i = 0; i < _engines.Count; i++)
        {
            // TODO Include engine power? Need to figure out how to calculate it
            float fundamentalFreq = stateData.RPM[i] / 120;
            var   frequencies     = _harmonics       * fundamentalFreq;
            var   amplitudes      = _amplitudes;
            amplitudes[0] = Attenuate(frequencies[0], amplitudes[0], _distance[i]);
            amplitudes[1] = Attenuate(frequencies[1], amplitudes[1], _distance[i]);
            amplitudes[2] = Attenuate(frequencies[2], amplitudes[2], _distance[i]);
            amplitudes[3] = Attenuate(frequencies[3], amplitudes[3], _distance[i]);
            _engines[i].SetTarget(fundamentalFreq, amplitudes, TransitionTime);
        }
    }
}