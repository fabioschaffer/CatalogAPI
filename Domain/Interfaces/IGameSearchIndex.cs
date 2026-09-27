using Domain.Entities;

namespace Domain.Interfaces;

public interface IGameSearchIndex
{
    Task<IReadOnlyList<GameSearchResult>> SearchAsync(string query, int size = 20, CancellationToken cancellationToken = default);

    Task IndexAsync(Game game, CancellationToken cancellationToken = default);
}
