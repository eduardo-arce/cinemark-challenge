namespace MovieCatalog.Domain.Events
{
    public sealed record FilmUpdatedEvent(
    string FilmId,
    string Title,
    string Genre) : IntegrationEvent
    {
        public override string EventType => "FilmUpdated";
    }
}