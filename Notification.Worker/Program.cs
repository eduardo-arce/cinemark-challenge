using Amazon.SQS;
using Notification.Worker.HealthChecks;
using Notification.Worker.Settings;
using Notification.Worker.Workers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Notification.Worker")
        .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter()));

    builder.Services.Configure<SqsSettings>(builder.Configuration.GetSection(nameof(SqsSettings)));

    var sqsSettings = builder.Configuration.GetSection(nameof(SqsSettings)).Get<SqsSettings>()!;
    builder.Services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(
        new Amazon.Runtime.BasicAWSCredentials("test", "test"),
        new AmazonSQSConfig
        {
            ServiceURL = sqsSettings.ServiceUrl,
            AuthenticationRegion = sqsSettings.Region
        }));

    builder.Services.AddHostedService<SqsConsumerWorker>();

    builder.Services.AddHealthChecks()
        .AddCheck<SqsHealthCheck>("sqs", tags: ["ready"]);

    var app = builder.Build();

    app.MapHealthChecks("/health");

    Log.Information("Notification Worker starting...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
