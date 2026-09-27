using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> ObterAsync<T>(string chave)
        {
            var valor = await _cache.GetStringAsync(chave);

            if (string.IsNullOrEmpty(valor))
                return default;

            return JsonSerializer.Deserialize<T>(valor);
        }

        public async Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao)
        {
            var json = JsonSerializer.Serialize(valor);

            await _cache.SetStringAsync(chave, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiracao
            });
        }

        public async Task RemoverAsync(string chave)
        {
            await _cache.RemoveAsync(chave);
        }
    }
}
