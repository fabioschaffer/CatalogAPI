using System;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    // Abstração de cache distribuído (Redis) usada pelo Application. Item 4 da Fase 3.
    public interface ICacheService
    {
        Task<T?> ObterAsync<T>(string chave);
        Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao);
        Task RemoverAsync(string chave);
    }
}
