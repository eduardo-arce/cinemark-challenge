using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.Core.Configuration;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Domain.Repositories;
using MovieCatalog.Infra.Caching;
using MovieCatalog.Infra.Messaging;
using MovieCatalog.Infra.Persistence;
using MovieCatalog.Infra.Settings;
using StackExchange.Redis;

namespace MovieCatalog.Infra
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)));
            services.Configure<RedisSettings>(configuration.GetSection(nameof(RedisSettings)));
            services.Configure<SqsSettings>(configuration.GetSection(nameof(SqsSettings)));

            services.AddSingleton<MongoContext>();
            services.AddSingleton<MongoIndexInitializer>();
            services.AddSingleton<MongoDataSeeder>();
            services.AddScoped<IFilmRepository, FilmRepository>();

            var redisConnectionString = configuration["RedisSettings:ConnectionString"] ?? "localhost:6379";
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "cinemark:";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnectionString));

            var sqsSettings = configuration.GetSection(nameof(SqsSettings)).Get<SqsSettings>()!;
            services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(
                new Amazon.Runtime.BasicAWSCredentials("test", "test"),
                new AmazonSQSConfig
                {
                    ServiceURL = sqsSettings.ServiceUrl,
                    AuthenticationRegion = sqsSettings.Region
                }));
            services.AddScoped<IEventPublisher, SqsEventPublisher>();

            return services;
        }
    }

}