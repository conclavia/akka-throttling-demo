using Akka.Actor;
using RateLimiting;

var maximumMessagesPerPeriod = 10;
var period = TimeSpan.FromSeconds(1);

var totalMessages = 1000;

var reportingInterval = TimeSpan.FromSeconds(10);

var system = ActorSystem.Create("RateLimiting");
var destination = system.ActorOf(Props.Create(() => new DestinationActor(reportingInterval)), "Destination");
var rateLimiter =
    system.ActorOf(Props.Create(() => new RateLimitingActor(destination, maximumMessagesPerPeriod, period)),
        "RateLimiter");

for (var i = 0; i < totalMessages; i++) rateLimiter.Tell(new DestinationActor.SampleMessage { Sent = DateTime.Now });

Console.ReadLine();