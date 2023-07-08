using IL2TelemetryRelay;
using IL2TelemetryRelay.Motion;
using IL2TelemetryRelay.State;

namespace IL2ShakerDriver;

internal class Listener
{
    public static event Action<MotionData>? MotionDataReceived;
    public static event Action<StateData>?  StateDataReceived;
    public static event Action<Event>?      EventDataReceived;

    public static async void ListenToStream(CancellationToken token)
    {
        var telemetryStream = Relay.Listen("127.0.0.1", 29373, token);

        await foreach (var telemetry in telemetryStream)
        {
            switch (telemetry)
            {
                case MotionData motionData:
                    MotionDataReceived?.Invoke(motionData);
                    break;
                case StateData stateData:
                    StateDataReceived?.Invoke(stateData);
                    break;
                default:
                    EventDataReceived?.Invoke(telemetry);
                    break;
            }
        }
    }
}