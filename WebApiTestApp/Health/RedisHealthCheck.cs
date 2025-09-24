using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace WebApiTestApp.Health;

public class RedisHealthCheck(IConnectionMultiplexer multiplexer) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(multiplexer.IsConnected ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
    }
}
