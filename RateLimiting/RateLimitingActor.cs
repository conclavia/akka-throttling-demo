using Akka.Actor;

namespace RateLimiting;

public class RateLimitingActor : ReceiveActor
{
    public RateLimitingActor(IActorRef destination, int maximumMessagesPerPeriod, TimeSpan period)
    {
        var bucketCapacity = period.TotalMilliseconds;
        var costPerMessage = bucketCapacity / maximumMessagesPerPeriod;

        var lastMessageReceived = DateTime.Now;
        double bucket = 0;

        ReceiveAny(m =>
        {
            // Capture current values so we can close over them
            var message = m;
            var sender = Sender;

            // Adjust the bucket for the time that has passed since the last message
            bucket = Math.Max(0, bucket - (DateTime.Now - lastMessageReceived).TotalMilliseconds);
            lastMessageReceived = DateTime.Now;

            // Adjust the bucket for the message just received
            bucket += costPerMessage;

            // Send the message
            var delay = bucket > bucketCapacity ? Convert.ToInt32(bucket) : 0;
            Task.Delay(delay).ContinueWith(_ => destination.Tell(message, sender));
        });
    }
}