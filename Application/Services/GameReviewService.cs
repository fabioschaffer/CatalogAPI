using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GameReviewService(IGameReviewRepository gameReviewRepository, ICacheService cacheService) : IGameReviewService
    {
        private static string ChaveCache(int gameId) => $"catalog:game:{gameId}:avaliacoes";

        public async Task<GameReview> CriarAvaliacaoAsync(int userId, GameReviewRequest request)
        {
            if (request.Nota < 1 || request.Nota > 5)
                throw new ArgumentOutOfRangeException(nameof(request.Nota), "A nota deve estar entre 1 e 5.");

            var review = new GameReview
            {
                GameId = request.GameId,
                UserId = userId,
                Nota = request.Nota,
                Comentario = request.Comentario,
                CriadoEm = DateTime.UtcNow
            };

            var criada = await gameReviewRepository.AdicionarAsync(review);

            // invalida o cache da lista de avaliações desse jogo
            await cacheService.RemoverAsync(ChaveCache(request.GameId));

            return criada;
        }

        public async Task<IEnumerable<GameReview>> ObterPorJogoAsync(int gameId)
        {
            var chave = ChaveCache(gameId);

            var emCache = await cacheService.ObterAsync<List<GameReview>>(chave);
            if (emCache != null)
                return emCache;

            var avaliacoes = await gameReviewRepository.ObterPorJogoAsync(gameId);
            var lista = new List<GameReview>(avaliacoes);

            await cacheService.DefinirAsync(chave, lista, TimeSpan.FromMinutes(5));

            return lista;
        }
    }
}
