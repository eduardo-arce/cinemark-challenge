using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Domain.Events;
using MovieCatalog.Infra.Settings;

namespace MovieCatalog.Infra.Messaging
{
    public sealed class SqsEventPublisher : IEventPublisher
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly SqsSettings _settings;
        private readonly ILogger<SqsEventPublisher> _logger;
        private string? _queueUrl;

        public SqsEventPublisher(IAmazonSQS sqsClient, IOptions<SqsSettings> settings, ILogger<SqsEventPublisher> logger)
        {
            _sqsClient = sqsClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task PublishAsync(IntegrationEvent @event)
        {
            var queueUrl = await GetOrCreateQueueUrlAsync();

            var message = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["EventType"] = new()
                    {
                        DataType = "String",
                        StringValue = @event.EventType
                    }
                }
            };

            await _sqsClient.SendMessageAsync(request);

            _logger.LogInformation("Published {EventType} event {EventId} to SQS",
                @event.EventType, @event.EventId);
        }

        private async Task<string> GetOrCreateQueueUrlAsync()
        {
            if (_queueUrl is not null) return _queueUrl;

            try
            {
                var response = await _sqsClient.GetQueueUrlAsync(_settings.QueueName);
                _queueUrl = response.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                var response = await _sqsClient.CreateQueueAsync(_settings.QueueName);
                _queueUrl = response.QueueUrl;
                _logger.LogInformation("Created SQS queue: {QueueName}", _settings.QueueName);
            }

            return _queueUrl;
        }
    }

}
