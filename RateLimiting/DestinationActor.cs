using Akka.Actor;

namespace RateLimiting;

public class DestinationActor : ReceiveActor, IWithTimers
{
    private readonly TimeSpan _reportingInterval;

    public DestinationActor(TimeSpan reportingInterval)
    {
        _reportingInterval = reportingInterval;

        var counter = 0;
        double delayMs = 0;
        var lastRateMessage = DateTime.Now;

        Receive<SampleMessage>(message =>
        {
            counter++;
            delayMs += (DateTime.Now - message.Sent).TotalMilliseconds;
        });

        Receive<RateMessage>(message =>
        {
            var period = (DateTime.Now - lastRateMessage).TotalSeconds;
            var rate = counter / period;
            var averageDelay = delayMs / counter;

            Console.WriteLine($"{counter} messages in {period:00}s @ {rate:00} m/s (avg delay {averageDelay:00}ms)");

            counter = 0;
            delayMs = 0;
            lastRateMessage = DateTime.Now;
        });
    }

    public ITimerScheduler? Timers { get; set; }

    protected override void PreStart()
    {
        Timers?.StartPeriodicTimer("rate", new RateMessage(), _reportingInterval, _reportingInterval);
    }

    public class SampleMessage
    {
        public DateTime Sent { get; init; }
    }

    public class RateMessage
    {
    }
}