# MixSystems.VneCashRecycler

By [MIX Systems](https://mixsystems.net). Previously published as `SYT.VneCashRecycler`; to switch, change the package reference and the `SYT.VneCashRecycler` namespace to `MixSystems.VneCashRecycler`.

`MixSystems.VneCashRecycler` is a production-oriented .NET SDK for VNE cash recycler
integrations.

It provides strongly typed clients, protocol contracts, value objects, and
resilient HTTP execution for:
- payment lifecycle commands
- cancellation and pending-payment flows
- withdrawals, refill, and emptying operations
- machine status and reporting queries
- configuration and maintenance commands

## Maintainer

<table>
  <tr>
    <td width="112">
      <img
        src="docs/images/anatoliy-yermakov.jpeg"
        alt="Anatoliy Yermakov"
        width="96" />
    </td>
    <td>
      <strong>Anatoliy Yermakov</strong><br />
      Founder of MIX Systems, maintainer and original author of <code>MixSystems.VneCashRecycler</code>.<br />
      The SDK is published as a standalone product and no longer depends on the
      Vectis repository layout.
    </td>
  </tr>
</table>

## Package

- Package ID: `MixSystems.VneCashRecycler`
- Current channel: `1.0.0-rc.1` (initial standalone release candidate)
- Target framework: `net10.0`
- License: `MIT`
- Repository: `https://github.com/mix-systems/MixSystems.VneCashRecycler`

## Installation

```bash
dotnet add package MixSystems.VneCashRecycler --prerelease
```

## Registration

Register the reusable SDK services once:

```csharp
using MixSystems.VneCashRecycler.Extensions;

builder.Services.AddVneCashRecycler();
```

Then create per-recycler clients through the factory:

```csharp
using MixSystems.VneCashRecycler.Clients;
using MixSystems.VneCashRecycler.Configuration;

IVneCashRecyclerClientFactory factory = app.Services.GetRequiredService<IVneCashRecyclerClientFactory>();

IVneCashRecyclerClient client = factory.Create(new VneCashRecyclerClientOptions
{
    Endpoint = new Uri("https://192.168.1.6/selfcashapi/")
});
```

## Quick Start

```csharp
using MixSystems.VneCashRecycler.Clients;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.ValueObjects;

PaymentRequestResult request = await client.SendPaymentAsync(
    MoneyCents.FromDecimal(12.50m),
    new OperatorName("cashier-01"),
    ct);

PaymentProgress progress = await client.GetPaymentStatusAsync(
    request.PaymentId,
    new OperatorName("cashier-01"),
    ct);

Console.WriteLine($"Payment {progress.PaymentId}: {progress.LifecycleStatus}");
```

## Core Features

### Payments and cancellations
- `SendPaymentAsync`
- `GetPaymentStatusAsync`
- `CancelPaymentAsync`
- `ListPendingPaymentsAsync`

### Withdrawals
- `StartWithdrawalAsync`
- `GetWithdrawalStatusAsync`
- `EndWithdrawalAsync`

### Refill and emptying
- `StartRefillAsync`
- `EndRefillAsync`
- `ManualRefillAsync`
- `PollRefillAsync`
- `StartHopperEmptyingAsync`
- `StartRecyclerEmptyingAsync`
- `ResetStackerAsync`
- `PollEmptyingAsync`
- `StartCoinDispenserEmptyingAsync`

### Machine control and reporting
- `GetMachineStatusAsync`
- `LoginOperatorAsync`
- `LogoutOperatorAsync`
- `OpenDoorAsync`
- `GetCashClosingAsync`
- `GetWithdrawalsAsync`
- `GetPaymentsAsync`
- `GetOpeningsAsync`
- `GetCashClosingHistoryAsync`
- `GetSoftwareVersionAsync`

### Configuration
- `AccessConfigurationMenuAsync`
- `RebootOrShutdownAsync`
- `ConfigurePeripheralsAsync`
- `ConfigureSoftwareAsync`
- `SetPettyCashAsync`
- `SignalEndPettyCashRestoreAsync`

## Configuration Model

Each created client is configured through `VneCashRecyclerClientOptions`:

```csharp
new VneCashRecyclerClientOptions
{
    Endpoint = new Uri("https://192.168.1.6/selfcashapi/"),
    DefaultOperatorName = new OperatorName("cashier-01"),
    Certificate = new VneCertificateOptions
    {
        SkipCertificateValidation = true
    },
    Resilience = new VneResilienceOptions
    {
        AttemptTimeout = TimeSpan.FromSeconds(10),
        RetryCount = 2
    }
};
```

## Error Handling

The SDK raises typed exceptions so application code can map them to domain-level
failures:
- `VneException`
- `VneTransportException`
- `VneApiException`

`VneApiException` includes Appendix-I error descriptions for known machine error
codes and preserves optional operation/user metadata when VNE reports an ongoing
operation conflict.

## Testing Strategy

- `UnitTests` validate value objects, serialization, DI registration, and error
  mapping without talking to a real recycler.
- `IntegrationTests` run against an embedded in-process simulator by default, so
  the suite is self-contained and does not require physical hardware.
- `IntegrationTests` can still target a real recycler or compatible external
  simulator by setting `UseEmbeddedSimulator=false` and providing `Endpoint` in
  `appsettings.test.json`.

## Product Boundaries

- `MixSystems.VneCashRecycler` focuses on transport, protocol contracts, typed value
  objects, and reusable client orchestration for VNE cash recyclers.
- Application-specific abstractions, POS workflows, and domain mapping should
  stay in the consuming application.
- Hardware-specific operational checks are intentionally outside the NuGet
  package and belong in integration or manual smoke environments.

## Release Notes

See [CHANGELOG.md](CHANGELOG.md) for the current release history.

## Additional Documentation

- [docs/README.md](docs/README.md)
- [docs/vendor-integration-notes.md](docs/vendor-integration-notes.md)
- [docs/sdk-conformance-report.md](docs/sdk-conformance-report.md)
