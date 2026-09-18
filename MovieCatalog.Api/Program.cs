using MovieCatalog.Api.Middlewares;
using MovieCatalog.Application;
using MovieCatalog.Infra.Persistence;
using MovieCatalog.Infra;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;

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
        .Enrich.WithProperty("Application", "MovieCatalog.Api")
        .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter()));

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Cinemark Movie Catalog API",
            Version = "v1",
            Description = "API for managing the Cinemark movie catalog"
        });
        options.UseInlineDefinitionsForEnums();
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cinemark Movie Catalog API v1");
        options.RoutePrefix = "swagger";
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
       .ExcludeFromDescription();
    app.MapControllers();
    app.MapHealthChecks("/health");

    await InitializeMongoAsync(app);

    Log.Information("MovieCatalog API starting on {Urls}", string.Join(", ", app.Urls));
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

static async Task InitializeMongoAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var indexInitializer = scope.ServiceProvider.GetRequiredService<MongoIndexInitializer>();
    await indexInitializer.InitializeAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<MongoDataSeeder>();
    await seeder.SeedAsync();
}