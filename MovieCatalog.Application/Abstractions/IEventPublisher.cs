using MovieCatalog.Domain.Events;

namespace MovieCatalog.Application.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync(IntegrationEvent @event);
    }
}