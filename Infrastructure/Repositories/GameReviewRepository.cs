using Domain.Common.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Nosql;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GameReviewRepository : IGameReviewRepository
    {
        private readonly IMongoCollection<GameReview> _reviews;

        public GameReviewRepository(MongoDbContext mongoDbContext, IOptions<MongoSettings> settings)
        {
            _reviews = mongoDbContext.GetCollection<GameReview>(settings.Value.ReviewsCollection);
        }

        public async Task<GameReview> AdicionarAsync(GameReview review)
        {
            if (string.IsNullOrWhiteSpace(review.Id))
                review.Id = ObjectId.GenerateNewId().ToString();

            await _reviews.InsertOneAsync(review);

            return review;
        }

        public async Task<IEnumerable<GameReview>> ObterPorJogoAsync(int gameId)
        {
            var filtro = Builders<GameReview>.Filter.Eq(r => r.GameId, gameId);

            return await _reviews.Find(filtro)
                .SortByDescending(r => r.CriadoEm)
                .ToListAsync();
        }
    }
}
