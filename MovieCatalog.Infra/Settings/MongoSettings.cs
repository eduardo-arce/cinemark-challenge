namespace MovieCatalog.Infra.Settings;

public sealed class MongoSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}
