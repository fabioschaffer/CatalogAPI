using Application.DTOs;
using Domain.Entities;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ILibrarySevice
    {
        Task OrderPaymentProcessed(PaymentProcessedEvent message);
        Task CriarLibraryAsync(LibraryRequest Library);
        Task<IEnumerable<Library>> ObterTodosAsync();
        Task<Library> ObterPorIdAsync(int id);
        Task DeletarAsync(int id);
    }
}
