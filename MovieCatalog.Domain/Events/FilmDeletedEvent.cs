namespace MovieCatalog.Domain.Events
{
    public sealed record FilmDeletedEvent(
    string FilmId,
    string Title) : IntegrationEvent
    {
        public override string EventType => "FilmDeleted";
    }
}