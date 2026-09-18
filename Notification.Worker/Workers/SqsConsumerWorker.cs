using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using Notification.Worker.Settings;

namespace Notification.Worker.Workers
{
    public sealed class SqsConsumerWorker : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly SqsSettings _settings;
        private readonly ILogger<SqsConsumerWorker> _logger;
        private string? _queueUrl;

        public SqsConsumerWorker(IAmazonSQS sqsClient, IOptions<SqsSettings> settings, ILogger<SqsConsumerWorker> logger)
        {
            _sqsClient = sqsClient;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SQS Consumer Worker starting...");

            var queueUrl = await GetOrCreateQueueUrlAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20,
                        MessageAttributeNames = ["All"]
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                    if (response.Messages != null)
                    {
                        foreach (var message in response.Messages)
                        {
                            await ProcessMessageAsync(message, queueUrl, stoppingToken);
                        }
                    }                    
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving messages from SQS");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("SQS Consumer Worker stopped");
        }

        private async Task ProcessMessageAsync(Message message, string queueUrl, CancellationToken cancellationToken)
        {
            try
            {
                var eventType = message.MessageAttributes
                    .GetValueOrDefault("EventType")?.StringValue ?? "Unknown";

                using var doc = JsonDocument.Parse(message.Body);
                var root = doc.RootElement;

                var title = root.TryGetProperty("title", out var t) ? t.GetString() : "N/A";
                var eventId = root.TryGetProperty("eventId", out var e) ? e.GetString() : "N/A";
                var occurredAt = root.TryGetProperty("occurredAt", out var o) ? o.GetString() : "N/A";

                _logger.LogInformation(
                    "Event received | Type: {EventType} | Title: {FilmTitle} | EventId: {EventId} | OccurredAt: {OccurredAt}",
                    eventType, title, eventId, occurredAt);

                await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
            }
        }

        private async Task<string> GetOrCreateQueueUrlAsync(CancellationToken cancellationToken)
        {
            if (_queueUrl is not null) return _queueUrl;

            try
            {
                var response = await _sqsClient.GetQueueUrlAsync(_settings.QueueName, cancellationToken);
                _queueUrl = response.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                var response = await _sqsClient.CreateQueueAsync(_settings.QueueName, cancellationToken);
                _queueUrl = response.QueueUrl;
                _logger.LogInformation("Created SQS queue: {QueueName}", _settings.QueueName);
            }

            return _queueUrl;
        }
    }
}
