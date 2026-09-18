namespace MovieCatalog.Domain.Events
{
    public sealed record FilmCreatedEvent(
        string FilmId,
        string Title,
        string Genre,
        DateTime ReleaseDate) : IntegrationEvent
    {
        public override string EventType => "FilmCreated";
    }
}