using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Observability
{
    public class DatabaseConnectionMetricsInterceptor : DbConnectionInterceptor
    {
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            DatabaseMetrics.ConnectionOpened();
            base.ConnectionOpened(connection, eventData);
        }

        public override Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            DatabaseMetrics.ConnectionOpened();
            return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

        public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
        {
            DatabaseMetrics.ConnectionClosed();
            base.ConnectionClosed(connection, eventData);
        }

        public override Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
        {
            DatabaseMetrics.ConnectionClosed();
            return base.ConnectionClosedAsync(connection, eventData);
        }
    }
}
