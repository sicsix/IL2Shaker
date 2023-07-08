using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using IL2TelemetryRelay.Events;
using IL2TelemetryRelay.Motion;
using IL2TelemetryRelay.State;

namespace IL2TelemetryRelay;

public static class Relay
{
    public static async IAsyncEnumerable<Event> Listen(string                                     ipAddress,
                                                       int                                        port,
                                                       [EnumeratorCancellation] CancellationToken token)
    {
        UdpClient?  client     = null;
        IPEndPoint? ipEndPoint = null;
        try
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            client     = new UdpClient(ipEndPoint);
        }
        catch (SocketException ex)
        {
            Logging.At(typeof(Relay)).Error(ex, "Socket exception");
            yield break;
        }
        catch (ArgumentNullException ex)
        {
            Logging.At(typeof(Relay)).Error(ex, "Null argument");
            yield break;
        }
        catch (Exception ex)
        {
            Logging.At(typeof(Relay)).Error(ex, "Exception encountered");
            yield break;
        }

        Logging.At(typeof(Relay)).Information("Listening at {IP}:{Port}", ipEndPoint.Address, ipEndPoint.Port);
        
        while (!token.IsCancellationRequested)
        {
            UdpReceiveResult result;
            try
            {
                result = await client.ReceiveAsync(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var packetID = (PacketID)BitConverter.ToUInt32(result.Buffer, 0);

            switch (packetID)
            {
                case PacketID.Motion:
                    yield return MotionDecoder.Decode(result.Buffer);
                    break;
                case PacketID.StateAndEvents:
                    yield return StateDecoder.Decode(result.Buffer, out int offset);
                    foreach (var eventData in new EventsDecoder().Decode(result.Buffer, offset))
                    {
                        yield return eventData;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(packetID.ToString());
            }
        }
    }
}