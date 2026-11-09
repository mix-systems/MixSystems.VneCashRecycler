using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.IntegrationTests.Fixtures;

internal sealed class EmbeddedVneSimulator : IAsyncDisposable
{
    private readonly object _gate = new();
    private readonly Dictionary<string, SimulatedPayment> _payments = new(StringComparer.Ordinal);
    private WebApplication? _app;

    public Uri Endpoint { get; private set; } = default!;

    public static async Task<EmbeddedVneSimulator> StartAsync(CancellationToken cancellationToken = default)
    {
        EmbeddedVneSimulator simulator = new();
        await simulator.StartCoreAsync(cancellationToken);
        return simulator;
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
        _app = null;
    }

    private async Task StartCoreAsync(CancellationToken cancellationToken)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseKestrel();
        builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, "http://127.0.0.1:0");

        WebApplication app = builder.Build();
        app.MapPost("/selfcashapi/{**rest}", (JsonElement root) => Results.Json(Handle(root)));

        await app.StartAsync(cancellationToken);

        IServer server = app.Services.GetRequiredService<IServer>();
        IServerAddressesFeature? addressesFeature = server.Features.Get<IServerAddressesFeature>();
        string address = addressesFeature?.Addresses.SingleOrDefault()
            ?? throw new InvalidOperationException("Embedded VNE simulator did not expose a listening address.");

        _app = app;
        Endpoint = new Uri($"{address.TrimEnd('/')}/selfcashapi/");
    }

    private object Handle(JsonElement root)
    {
        VneRequestType requestType = (VneRequestType)root.GetProperty("tipo").GetInt32();

        return requestType switch
        {
            VneRequestType.PaymentRequest => HandlePaymentRequest(root),
            VneRequestType.PaymentStatusPolling => HandlePaymentStatus(root),
            VneRequestType.DeletePendingPayment => HandleCancellation(root),
            VneRequestType.PendingPaymentsList => HandlePendingPayments(),
            VneRequestType.MachineStatus => HandleMachineStatus(),
            _ => BuildErrorResponse(requestType, 999)
        };
    }

    private object HandlePaymentRequest(JsonElement root)
    {
        int amount = root.GetProperty("importo").GetInt32();

        lock (_gate)
        {
            if (_payments.Values.Any(payment => payment.IsPending))
            {
                return BuildErrorResponse(VneRequestType.PaymentRequest, 105);
            }

            string paymentId = Guid.NewGuid().ToString("N");
            _payments[paymentId] = new SimulatedPayment(paymentId, amount);

            return new
            {
                req_status = 1,
                tipo = (int)VneRequestType.PaymentRequest,
                importo = amount,
                id = paymentId
            };
        }
    }

    private object HandlePaymentStatus(JsonElement root)
    {
        string paymentId = root.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Payment status polling request did not include an id.");

        lock (_gate)
        {
            if (!_payments.TryGetValue(paymentId, out SimulatedPayment? payment))
            {
                return BuildErrorResponse(VneRequestType.PaymentStatusPolling, 104);
            }

            return new
            {
                req_status = 1,
                tipo = (int)VneRequestType.PaymentStatusPolling,
                id = payment.Id,
                payment_status = payment.IsPending ? 0 : 1,
                payment_details = new
                {
                    amount = payment.Amount,
                    inserted = payment.Inserted,
                    rest = payment.IsPending ? payment.Amount : 0,
                    status = payment.IsPending ? "pending" : payment.FinalDetailStatus,
                    not_returned = 0,
                    cancelled_by = payment.CancelledBy
                }
            };
        }
    }

    private object HandleCancellation(JsonElement root)
    {
        string paymentId = root.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Cancellation request did not include an id.");

        lock (_gate)
        {
            if (!_payments.TryGetValue(paymentId, out SimulatedPayment? payment))
            {
                return BuildErrorResponse(VneRequestType.DeletePendingPayment, 104);
            }

            if (!payment.IsPending)
            {
                return new
                {
                    req_status = 1,
                    tipo = (int)VneRequestType.DeletePendingPayment,
                    payment_status = 3,
                    id = payment.Id
                };
            }

            payment.IsPending = false;
            payment.FinalDetailStatus = "deleted";
            payment.CancelledBy = "embedded-simulator";

            return new
            {
                req_status = 1,
                tipo = (int)VneRequestType.DeletePendingPayment,
                payment_status = 1,
                id = payment.Id
            };
        }
    }

    private object HandlePendingPayments()
    {
        lock (_gate)
        {
            Dictionary<string, string> pending = _payments.Values
                .Where(payment => payment.IsPending)
                .Select((payment, index) => new KeyValuePair<string, string>($"pending_{index}", payment.Id))
                .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);

            return new
            {
                req_status = 1,
                tipo = (int)VneRequestType.PendingPaymentsList,
                list_length = pending.Count,
                pending_list = pending.Count == 0 ? null : pending
            };
        }
    }

    private static object HandleMachineStatus()
    {
        return new
        {
            req_status = 1,
            tipo = (int)VneRequestType.MachineStatus,
            recycler = new
            {
                error = 0,
                mess = "ok",
                banconota_1 = new
                {
                    valore = 500,
                    quantita = 10,
                    alert = 0,
                    min_level = 0,
                    max_level = 100
                },
                banconota_2 = new
                {
                    valore = 1000,
                    quantita = 8,
                    alert = 0,
                    min_level = 0,
                    max_level = 100
                },
                banconota_3 = new
                {
                    valore = 2000,
                    quantita = 6,
                    alert = 1,
                    min_level = 0,
                    max_level = 100
                },
                banconota_4 = new
                {
                    valore = 5000,
                    quantita = 4,
                    alert = 0,
                    min_level = 0,
                    max_level = 100
                },
                stacker = 0,
                totalInRecycle = 39000,
                detailNotesStacker = new Dictionary<string, int>
                {
                    ["500"] = 0,
                    ["1000"] = 1,
                    ["2000"] = 2,
                    ["5000"] = 0
                }
            },
            hopper = new
            {
                error = 0,
                mess = "ok",
                moneta_1 = 120,
                moneta_1_alert = 0,
                moneta_1_min_level = 10,
                moneta_1_max_level = 500,
                moneta_1_accept = 1,
                moneta_1_recycle = 1,
                moneta_1_cashbox = 5,
                moneta_2 = 80,
                moneta_2_alert = 0,
                moneta_2_min_level = 10,
                moneta_2_max_level = 500,
                moneta_2_accept = 1,
                moneta_2_recycle = 1,
                moneta_2_cashbox = 3,
                moneta_5 = 50,
                moneta_5_alert = 0,
                moneta_5_min_level = 10,
                moneta_5_max_level = 500,
                moneta_5_accept = 1,
                moneta_5_recycle = 1,
                moneta_5_cashbox = 2,
                moneta_10 = 40,
                moneta_10_alert = 1,
                moneta_10_min_level = 10,
                moneta_10_max_level = 500,
                moneta_10_accept = 1,
                moneta_10_recycle = 1,
                moneta_10_cashbox = 1,
                moneta_50 = 20,
                moneta_50_alert = 0,
                moneta_50_min_level = 0,
                moneta_50_max_level = 100,
                moneta_50_accept = 1,
                moneta_50_recycle = 1,
                moneta_50_cashbox = 0,
                stacker = 0,
                totalInRecycle = 1980
            },
            dispenser = new
            {
                error = 0,
                mess = "ok",
                banconota_1 = new
                {
                    valore = 500,
                    quantita = 10,
                    alert = 0,
                    error = 0
                },
                banconota_2 = new
                {
                    valore = 1000,
                    quantita = 8,
                    alert = 0,
                    error = 0
                },
                totalInRecycle = 13000,
                rejectBin = 0
            },
            coinDisp = new
            {
                hopper_1 = new
                {
                    valore = 50,
                    quantita = 20,
                    alert = 0,
                    error = 0
                },
                hopper_2 = new
                {
                    valore = 100,
                    quantita = 10,
                    alert = 1,
                    error = 0
                }
            }
        };
    }

    private static object BuildErrorResponse(VneRequestType requestType, int errorCode)
    {
        return new
        {
            req_status = 0,
            tipo = (int)requestType,
            mess = errorCode
        };
    }

    private sealed class SimulatedPayment(string id, int amount)
    {
        public string Id { get; } = id;

        public int Amount { get; } = amount;

        public int Inserted { get; set; }

        public bool IsPending { get; set; } = true;

        public string FinalDetailStatus { get; set; } = "deleted";

        public string? CancelledBy { get; set; }
    }
}
