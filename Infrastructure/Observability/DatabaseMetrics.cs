using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Infrastructure.Observability
{
    public static class DatabaseMetrics
    {
        public const string MeterName = "CatalogAPI.Database";

        private static readonly Meter Meter = new(MeterName, "1.0.0");

        // Os limites de bucket (em segundos) são definidos via OpenTelemetry View no Program.cs:
        // a API de "advice" de histograma (bucket boundary por instrumento) só existe a partir do .NET 9,
        // e este projeto está no .NET 8.
        private static readonly Histogram<double> QueryDuration = Meter.CreateHistogram<double>(
            "db_query_duration_seconds",
            unit: "s",
            description: "Duração das operações de banco de dados, por operação.");

        private static readonly Counter<long> QueryErrors = Meter.CreateCounter<long>(
            "db_query_errors_total",
            description: "Quantidade de erros de banco de dados, por operação.");

        private static readonly UpDownCounter<long> ConnectionsActive = Meter.CreateUpDownCounter<long>(
            "db_connections_active",
            description: "Conexões abertas no momento com o banco de dados.");

        private static int _maxPoolSize = 100;

        // ObservableGauge lê o valor atual de _maxPoolSize a cada coleta do Prometheus.
        private static readonly ObservableGauge<int> ConnectionsMax = Meter.CreateObservableGauge(
            "db_connections_max",
            () => _maxPoolSize,
            description: "Tamanho máximo configurado do pool de conexões.");

        public static void ConfigureMaxPoolSize(int maxPoolSize) => _maxPoolSize = maxPoolSize;

        public static void ConnectionOpened() => ConnectionsActive.Add(1);

        public static void ConnectionClosed() => ConnectionsActive.Add(-1);

        public static async Task<T> TrackAsync<T>(string operation, string entity, Func<Task<T>> action)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                QueryErrors.Add(
                    1,
                    new KeyValuePair<string, object?>("operation", operation),
                    new KeyValuePair<string, object?>("entity", entity),
                    new KeyValuePair<string, object?>("error_type", ex.GetType().Name));

                throw;
            }
            finally
            {
                stopwatch.Stop();

                QueryDuration.Record(
                    stopwatch.Elapsed.TotalSeconds,
                    new KeyValuePair<string, object?>("operation", operation),
                    new KeyValuePair<string, object?>("entity", entity));
            }
        }

        public static Task TrackAsync(string operation, string entity, Func<Task> action) =>
            TrackAsync(operation, entity, async () =>
            {
                await action();
                return true;
            });
    }
}
