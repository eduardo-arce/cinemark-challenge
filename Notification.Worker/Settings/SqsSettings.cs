namespace Notification.Worker.Settings
{
    public sealed class SqsSettings
    {
        public string ServiceUrl { get; set; } = null!;
        public string QueueName { get; set; } = null!;
        public string Region { get; set; } = "us-east-1";
    }
}
