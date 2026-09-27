using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GameService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, IGameSearchIndex searchIndex) : IGameService
    {
        private const string ChaveListaTodos = "catalog:games:todos";
        private static string ChaveJogo(int id) => $"catalog:game:{id}";

        public Task<IReadOnlyList<GameSearchResult>> SearchAsync(string query, int size = 20,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            if (query.Trim().Length > 200)
                throw new ArgumentException("A busca deve ter até 200 caracteres.", nameof(query));
            if (size is < 1 or > 100)
                throw new ArgumentOutOfRangeException(nameof(size), "O limite deve estar entre 1 e 100.");
            return searchIndex.SearchAsync(query.Trim(), size, cancellationToken);
        }

        public async Task<Game> CriarGameAsync(GameRequest User)
        {
            var Game = mapper.Map<Game>(User);

            var createdGame = await unitOfWork.Games.AdicionarAsync(Game);

            await unitOfWork.SaveChangesAsync();

            await cacheService.RemoverAsync(ChaveListaTodos);

            await searchIndex.IndexAsync(createdGame);

            return createdGame;
        }

        public async Task AtualizarAsync(GameUpdateRequest GameUpdateRequest)
        {
            var Game = await unitOfWork.Games.ObterPorIdAsync(GameUpdateRequest.Id) ?? throw new DirectoryNotFoundException(GameUpdateRequest.Id.ToString());

            Game.Nome = GameUpdateRequest.Nome;
            Game.Price = GameUpdateRequest.Price;

            await unitOfWork.Games.Atualizar(Game);
            await unitOfWork.SaveChangesAsync();

            await cacheService.RemoverAsync(ChaveListaTodos);
            await cacheService.RemoverAsync(ChaveJogo(GameUpdateRequest.Id));

            await searchIndex.IndexAsync(Game);
        }

        public async Task DeletarAsync(int id)
        {
            var User = await unitOfWork.Games.ObterPorIdAsync(id) ?? throw new DirectoryNotFoundException(id.ToString());

            await unitOfWork.Games.DeletarAsync(User);

            await unitOfWork.SaveChangesAsync();

            await cacheService.RemoverAsync(ChaveListaTodos);
            await cacheService.RemoverAsync(ChaveJogo(id));
        }

        public async Task<Game> ObterPorIdAsync(int id)
        {
            var chave = ChaveJogo(id);

            var emCache = await cacheService.ObterAsync<Game>(chave);
            if (emCache != null)
                return emCache;

            var game = await unitOfWork.Games.ObterPorIdAsync(id);

            if (game != null)
                await cacheService.DefinirAsync(chave, game, TimeSpan.FromSeconds(60));

            return game;
        }

        public async Task<IEnumerable<Game>> ObterTodosAsync()
        {
            var emCache = await cacheService.ObterAsync<List<Game>>(ChaveListaTodos);
            if (emCache != null)
                return emCache;

            var jogos = await unitOfWork.Games.ObterTodosAsync();
            var lista = jogos.ToList();

            await cacheService.DefinirAsync(ChaveListaTodos, lista, TimeSpan.FromSeconds(60));

            return lista;
        }
    }
}
