namespace IL2TelemetryRelay.Events;

internal class EventsDecoder
{
    internal IEnumerable<Event> Decode(byte[] packet, int offset)
    {
        uint tick = BitConverter.ToUInt32(packet, 6);

        byte eventLength = packet[offset++];
        for (int i = 0; i < eventLength; i++)
        {
            var  eventType  = (EventType)BitConverter.ToUInt16(packet, offset);
            byte eventBytes = packet[offset + 2];
            offset += 3;

            switch (eventType)
            {
                case EventType.VehicleName:
                    yield return new AircraftNameEvent(tick, packet, offset);
                    break;
                case EventType.EngineData:
                    yield return new EngineDataEvent(tick, packet, offset);
                    break;
                case EventType.GunData:
                    yield return new GunDataEvent(tick, packet, offset);
                    break;
                case EventType.WheelData:
                    yield return new WheelDataEvent(tick, packet, offset);
                    break;
                case EventType.BombRelease:
                    var payloadReleaseEvent = new BombReleaseEvent(tick, packet, offset);
                    Logging.At(this).Debug("{evt}", payloadReleaseEvent);
                    yield return payloadReleaseEvent;
                    break;
                case EventType.RocketLaunch:
                    var rocketLaunchEvent = new RocketLaunchEvent(tick, packet, offset);
                    Logging.At(this).Debug("{evt}", rocketLaunchEvent);
                    yield return rocketLaunchEvent;
                    break;
                case EventType.Event6:
                    var event6 = new Event6(tick, packet, offset);
                    Logging.At(this).Debug("{evt}", event6);
                    yield return event6;
                    break;
                case EventType.Hit:
                    var hitEvent = new HitEvent(tick, packet, offset);
                    // LogLib.At(this).Debug("{evt}", hitEvent);
                    yield return hitEvent;
                    break;
                case EventType.Damage:
                    var damagedEvent = new DamageEvent(tick, packet, offset);
                    // LogLib.At(this).Debug("{evt}", damagedEvent);
                    yield return damagedEvent;
                    break;
                case EventType.GunFired:
                    yield return new GunFiredEvent(tick, packet, offset);
                    break;
                case EventType.Event13:
                    // Contains player name and bunch of other stuff? Happens only on mission start
                    Logging.At(this).Debug("Event 13");
                    break;
                case EventType.CurrentSeat:
                    var currentSeat = new CurrentSeat(tick, packet, offset);
                    // LogLib.At(this).Debug("{evt}", currentSeat);
                    yield return currentSeat;
                    break;
                default:
                    Logging.At(this).Debug("Unknown event type: {num}", eventType);
                    break;
            }

            // Here the state length refers to how many bytes
            offset += eventBytes;
        }
    }
}