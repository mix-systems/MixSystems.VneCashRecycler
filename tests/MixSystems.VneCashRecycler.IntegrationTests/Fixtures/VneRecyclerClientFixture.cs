using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MixSystems.VneCashRecycler.Clients;
using MixSystems.VneCashRecycler.Configuration;
using MixSystems.VneCashRecycler.Exceptions;
using MixSystems.VneCashRecycler.Extensions;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.IntegrationTests.Fixtures;

public sealed class VneRecyclerClientFixture : IAsyncLifetime
{
    private ServiceProvider? _serviceProvider;
    private VneRecyclerTestConfiguration? _config;
    private EmbeddedVneSimulator? _simulator;

    public IVneCashRecyclerClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        _config = VneRecyclerTestConfiguration.Load("appsettings.test.json");

        string endpoint;
        if (_config.UseEmbeddedSimulator)
        {
            _simulator = await EmbeddedVneSimulator.StartAsync();
            endpoint = _simulator.Endpoint.ToString();
        }
        else
        {
            endpoint = _config.Endpoint ?? throw new InvalidOperationException("A VNE endpoint must be configured when the embedded simulator is disabled.");
        }

        ServiceCollection services = new();
        services.AddLogging(builder => builder.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "hh:mm:ss ";
        }).SetMinimumLevel(LogLevel.Information));
        services.AddVneCashRecycler();

        _serviceProvider = services.BuildServiceProvider();

        IVneCashRecyclerClientFactory factory = _serviceProvider.GetRequiredService<IVneCashRecyclerClientFactory>();

        OperatorName? defaultOperator = null;
        if (!string.IsNullOrWhiteSpace(_config.DefaultOperatorName))
        {
            defaultOperator = new OperatorName(_config.DefaultOperatorName!);
        }

        VneCashRecyclerClientOptions options = new()
        {
            Endpoint = new Uri(endpoint),
            DefaultOperatorName = defaultOperator,
            Certificate = new VneCertificateOptions
            {
                SkipCertificateValidation = _config.SkipCertificateValidation
            },
            Resilience = new VneResilienceOptions
            {
                AttemptTimeoutSeconds = _config.AttemptTimeoutSeconds,
                RetryCount = _config.RetryCount,
                RetryDelaySeconds = _config.RetryDelaySeconds,
                CircuitBreakerFailures = 0
            }
        };

        Client = factory.Create(options);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Client is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            Client.Dispose();
        }

        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        if (_simulator is not null)
        {
            await _simulator.DisposeAsync();
        }
    }

    public async Task EnsureNoPendingPaymentsAsync()
    {
        PendingPaymentList list = await Client.ListPendingPaymentsAsync();
        foreach (PaymentId id in list.PaymentIds)
        {
            try
            {
                await Client.CancelPaymentAsync(id, PaymentCancellationMode.ReturnInsertedAmount);
            }
            catch (VneApiException ex) when (ex.ErrorCode is 104 or 113)
            {
                // already gone
            }
        }

        await WaitUntilNoPendingAsync();
    }

    public async Task<PaymentProgress> WaitUntilFinalStateAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        TimeSpan pollInterval = TimeSpan.FromMilliseconds(250);
        TimeSpan timeout = TimeSpan.FromSeconds(20);
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PaymentProgress progress = await Client.GetPaymentStatusAsync(paymentId, cancellationToken: cancellationToken);
            if (progress.LifecycleStatus == PaymentLifecycleStatus.Completed)
            {
                return progress;
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException($"Payment {paymentId.Value} did not reach terminal state within {timeout.TotalSeconds:F0}s.");
            }

            TimeSpan delay = pollInterval;
            TimeSpan remaining = deadline - DateTimeOffset.UtcNow;
            if (remaining < delay)
            {
                delay = remaining;
            }

            await Task.Delay(delay, cancellationToken);
        }
    }

    public async Task<WithdrawalProgress> WaitUntilWithdrawalCompletesAsync(WithdrawalId withdrawalId, CancellationToken cancellationToken = default)
    {
        TimeSpan pollInterval = TimeSpan.FromMilliseconds(500);
        TimeSpan timeout = TimeSpan.FromSeconds(30);
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WithdrawalProgress progress = await Client.GetWithdrawalStatusAsync(withdrawalId, cancellationToken: cancellationToken);
            if (progress.Status == WithdrawalStatus.Completed)
            {
                return progress;
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException($"Withdrawal {withdrawalId.Value} did not complete within {timeout.TotalSeconds:F0}s.");
            }

            TimeSpan delay = pollInterval;
            TimeSpan remaining = deadline - DateTimeOffset.UtcNow;
            if (remaining < delay)
            {
                delay = remaining;
            }

            await Task.Delay(delay, cancellationToken);
        }
    }

    private async Task WaitUntilNoPendingAsync()
    {
        const int maxIterations = 20;
        for (int i = 0; i < maxIterations; i++)
        {
            PendingPaymentList list = await Client.ListPendingPaymentsAsync();
            if (list.PaymentIds.Count == 0)
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Recycler still has pending payments after cleanup.");
    }
}
