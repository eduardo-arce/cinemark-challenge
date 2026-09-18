using Amazon.SQS;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Notification.Worker.Settings;

namespace Notification.Worker.HealthChecks
{
    public sealed class SqsHealthCheck : IHealthCheck
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly SqsSettings _settings;

        public SqsHealthCheck(IAmazonSQS sqsClient, IOptions<SqsSettings> settings)
        {
            _sqsClient = sqsClient;
            _settings = settings.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _sqsClient.GetQueueUrlAsync(_settings.QueueName, cancellationToken);
                return HealthCheckResult.Healthy("SQS connection is healthy");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("SQS connection failed", ex);
            }
        }
    }
}

