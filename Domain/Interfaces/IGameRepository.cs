using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGameRepository : IRepository<Game>
    {
        Task<Game> AdicionarAsync(Game entidade);
        Task<Game> ObterPorIdAsync(long id);
        Task Atualizar(Game User);
        Task DeletarAsync(Game User);

    }
}
