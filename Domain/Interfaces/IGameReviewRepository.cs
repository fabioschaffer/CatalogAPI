using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGameReviewRepository
    {
        Task<GameReview> AdicionarAsync(GameReview review);
        Task<IEnumerable<GameReview>> ObterPorJogoAsync(int gameId);
    }
}
