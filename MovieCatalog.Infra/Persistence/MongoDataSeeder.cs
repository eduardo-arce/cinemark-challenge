using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Infra.Persistence
{
    public sealed class MongoDataSeeder
    {
        private readonly MongoContext _context;

        public MongoDataSeeder(MongoContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            var collection = _context.Database.GetCollection<FilmEntity>("films");

            var count = await collection.CountDocumentsAsync(FilterDefinition<FilmEntity>.Empty);
            if (count > 0)
                return;

            var now = DateTime.UtcNow;

            var films = new List<FilmEntity>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "The Shawshank Redemption",
                Synopsis = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                Genre = Genre.Drama,
                ReleaseDate = new DateTime(1994, 9, 23, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 142,
                Rating = 9.3m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "The Dark Knight",
                Synopsis = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                Genre = Genre.Action,
                ReleaseDate = new DateTime(2008, 7, 18, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 152,
                Rating = 9.0m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "Interstellar",
                Synopsis = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.",
                Genre = Genre.SciFi,
                ReleaseDate = new DateTime(2014, 11, 7, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 169,
                Rating = 8.7m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "The Hangover",
                Synopsis = "Three buddies wake up from a bachelor party in Las Vegas with no memory of the previous night and the bachelor missing.",
                Genre = Genre.Comedy,
                ReleaseDate = new DateTime(2009, 6, 5, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 100,
                Rating = 7.7m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "The Conjuring",
                Synopsis = "Paranormal investigators Ed and Lorraine Warren work to help a family terrorized by a dark presence in their farmhouse.",
                Genre = Genre.Horror,
                ReleaseDate = new DateTime(2013, 7, 19, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 112,
                Rating = 7.5m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "Spider-Man: Into the Spider-Verse",
                Synopsis = "Teen Miles Morales becomes the Spider-Man of his reality, crossing paths with five counterparts from other dimensions to stop a threat for all realities.",
                Genre = Genre.Animation,
                ReleaseDate = new DateTime(2018, 12, 14, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 117,
                Rating = 8.4m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "Gladiator",
                Synopsis = "A former Roman General sets out to exact vengeance against the corrupt emperor who murdered his family and sent him into slavery.",
                Genre = Genre.Action,
                ReleaseDate = new DateTime(2000, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 155,
                Rating = 8.5m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "Blade Runner 2049",
                Synopsis = "Young Blade Runner K's discovery of a long-buried secret leads him to track down former Blade Runner Rick Deckard, who's been missing for thirty years.",
                Genre = Genre.SciFi,
                ReleaseDate = new DateTime(2017, 10, 6, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 164,
                Rating = 8.0m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "Forrest Gump",
                Synopsis = "The presidencies of Kennedy and Johnson, the Vietnam War, the Watergate scandal and other historical events unfold from the perspective of an Alabama man with an IQ of 75.",
                Genre = Genre.Drama,
                ReleaseDate = new DateTime(1994, 7, 6, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 142,
                Rating = 8.8m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "Toy Story",
                Synopsis = "A cowboy doll is profoundly threatened and jealous when a new spaceman action figure supplants him as top toy in a boy's bedroom.",
                Genre = Genre.Animation,
                ReleaseDate = new DateTime(1995, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 81,
                Rating = 8.3m,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

            await collection.InsertManyAsync(films);
        }
    }
}
