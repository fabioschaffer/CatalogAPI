using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ILibraryRepository : IRepository<Library>
    {
        Task<Library> AdicionarAsync(Library Library);
        Task<Library> ObterPorIdAsync(long id);        
        Task DeletarAsync(Library Library);
    }
}
