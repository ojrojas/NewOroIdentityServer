using Microsoft.Extensions.Options;

namespace OroIdentityServer.Server.Services;

public class KeyRotationOptions
{
    public int RotationIntervalMinutes { get; set; } = 60; // default hourly
}

public class KeyRotationService : IHostedService, IDisposable
{
    private readonly ILogger<KeyRotationService> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _cts = new();
    private Task? _executingTask;
    private readonly int _intervalMinutes;

    public KeyRotationService(ILogger<KeyRotationService> logger, IHostApplicationLifetime lifetime, IServiceProvider serviceProvider, IOptions<KeyRotationOptions> opts)
    {
        _logger = logger;
        _lifetime = lifetime;
        _serviceProvider = serviceProvider;
        _intervalMinutes = opts.Value.RotationIntervalMinutes;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("KeyRotationService starting, interval {minutes} minutes", _intervalMinutes);
        _executingTask = Task.Run(() => ExecuteAsync(_cts.Token));
        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Resolve scoped services inside a scope to avoid injecting scoped into singleton
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        try
                        {
                            var tokenService = scope.ServiceProvider.GetService<ITokenService>();
                            if (tokenService != null)
                            {
                                tokenService.RotateSigningKey();
                                _logger.LogInformation("Rotated signing key via KeyRotationService");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to resolve or rotate signing key");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Key rotation failed");
                }

                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), token);
            }
        }
        catch (OperationCanceledException) { }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("KeyRotationService stopping");
        _cts.Cancel();
        if (_executingTask != null)
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
