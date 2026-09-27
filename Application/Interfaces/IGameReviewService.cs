using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGameReviewService
    {
        Task<GameReview> CriarAvaliacaoAsync(int userId, GameReviewRequest request);
        Task<IEnumerable<GameReview>> ObterPorJogoAsync(int gameId);
    }
}
