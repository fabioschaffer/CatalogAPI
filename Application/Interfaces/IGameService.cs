using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGameService
    {
        Task<IReadOnlyList<GameSearchResult>> SearchAsync(string query, int size = 20, CancellationToken cancellationToken = default);
        Task<Game> CriarGameAsync(GameRequest User);
        Task<IEnumerable<Game>> ObterTodosAsync();
        Task<Game> ObterPorIdAsync(int id);
        Task AtualizarAsync(GameUpdateRequest User);
        Task DeletarAsync(int id);
    }
}
