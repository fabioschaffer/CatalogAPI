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
    public class LibraryRepository (FCGDbContext dbContext) : ILibraryRepository
    {
        private const string Entity = "Library";

        public Task<Library> AdicionarAsync(Library Library) =>
            DatabaseMetrics.TrackAsync(nameof(LibraryRepository) + "." + nameof(AdicionarAsync), Entity, async () =>
            {
                var retorno = await dbContext.Library.AddAsync(Library);

                return retorno.Entity;
            });

        public Task DeletarAsync(Library Library) =>
            DatabaseMetrics.TrackAsync(nameof(LibraryRepository) + "." + nameof(DeletarAsync), Entity, () =>
            {
                dbContext.Library.Remove(Library);
                return Task.CompletedTask;
            });

        public Task<Library> ObterPorIdAsync(long id) =>
            DatabaseMetrics.TrackAsync(nameof(LibraryRepository) + "." + nameof(ObterPorIdAsync), Entity, () =>
                dbContext.Library.FirstOrDefaultAsync(u => u.Id == id));

        public Task<IEnumerable<Library>> ObterTodosAsync() =>
            DatabaseMetrics.TrackAsync(nameof(LibraryRepository) + "." + nameof(ObterTodosAsync), Entity, async () =>
                (IEnumerable<Library>)await dbContext.Library.ToListAsync());
    }
}
