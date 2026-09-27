using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GameRepository(FCGDbContext dbContext) : IGameRepository
    {
        private const string Entity = "Game";

        public Task<Game> AdicionarAsync(Game entidade) =>
            DatabaseMetrics.TrackAsync(nameof(GameRepository) + "." + nameof(AdicionarAsync), Entity, async () =>
            {
                var retorno = await dbContext.Games.AddAsync(entidade);

                return retorno.Entity;
            });

        public Task Atualizar(Game User) =>
            DatabaseMetrics.TrackAsync(nameof(GameRepository) + "." + nameof(Atualizar), Entity, () =>
            {
                dbContext.Games.Update(User);
                return Task.CompletedTask;
            });

        public Task DeletarAsync(Game User) =>
            DatabaseMetrics.TrackAsync(nameof(GameRepository) + "." + nameof(DeletarAsync), Entity, () =>
            {
                dbContext.Games.Remove(User);
                return Task.CompletedTask;
            });

        public Task<Game> ObterPorIdAsync(long id) =>
            DatabaseMetrics.TrackAsync(nameof(GameRepository) + "." + nameof(ObterPorIdAsync), Entity, () =>
                dbContext.Games.FirstOrDefaultAsync(u => u.Id == id));

        public Task<IEnumerable<Game>> ObterTodosAsync() =>
            DatabaseMetrics.TrackAsync(nameof(GameRepository) + "." + nameof(ObterTodosAsync), Entity, async () =>
                (IEnumerable<Game>)await dbContext.Games.ToListAsync());
    }
}
