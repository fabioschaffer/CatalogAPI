using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IGameRepository Games { get; }
        ILibraryRepository Library { get; }
        Task<int> SaveChangesAsync();
    }
}
