using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DotNetOpenTelemetry.Web;

/// <summary>
/// Opentelemetry demo on github
/// ASP.NET Core part
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Custom metrics
        var customMeter = new Meter("Custom.Example", "1.0.0");
        var counter = customMeter.CreateCounter<int>("custom.count");

        // Custom ActivitySource
        var customActivitySource = new ActivitySource("Custom.Example");

        var builder = WebApplication.CreateBuilder(args);

        var tracingOtlpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];
        var otel = builder.Services.AddOpenTelemetry();

        // Configure OpenTelemetry Resources with the application name
        otel.ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName));

        // Add metrics for ASP.NET Core and custom metrics and export to console
        otel.WithMetrics(metrics => metrics
            // Metrics provider from OpenTelemetry
            .AddAspNetCoreInstrumentation()
            // Metrics provides by ASP.NET Core in .NET 10
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddMeter(customMeter.Name)
            .AddMeter(nameof(OrderService))
            .AddConsoleExporter((exporterOptions, metricReaderOptions) =>
            {
                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
            })
        /* Add custom exporter
        .AddReader(new PeriodicExportingMetricReader(new CustomMetricExporter(), 3000)
        {
            TemporalityPreference = MetricReaderTemporalityPreference.Delta
        })
        */
        );

        // Add Tracing for ASP.NET Core and custom ActivitySource and export to console
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddSource(customActivitySource.Name);
            tracing.AddSource(nameof(OrderService));
            tracing.SetSampler(new ParentBasedSampler(new AlwaysOnSampler()));
            if (tracingOtlpEndpoint != null)
            {
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                });
            }
            else tracing.AddConsoleExporter();
        });

        // Configure OpenTelemetry Logging
        builder.Logging.ClearProviders();
        otel.WithLogging(logging =>
        {
            logging.AddConsoleExporter();
        });

        // Services or Modules use OpenTelemetry
        builder.Services.AddSingleton<OrderTelemetry>();
        builder.Services.AddSingleton<OrderService>();

        var app = builder.Build();

        app.MapGet("/", () =>
        {
            counter.Add(1);

            using var activity = customActivitySource.StartActivity("CustomActivity");
            activity?.SetTag("greeting", "Hello World!");

            return "Hello World!";
        });

        app.MapGet("/order", (OrderService orderService) => orderService.CreateOrderAsync("user123", 99.99m));

        app.Run();
    }
}

/// <summary>
/// Custom meteric exporter
/// </summary>
public class CustomMetricExporter : BaseExporter<Metric>
{
    public override ExportResult Export(in Batch<Metric> batch)
    {
        Console.WriteLine($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff} Custom metric exporter");

        try
        {
            foreach (var metric in batch)
            {
                Console.WriteLine($"Metric: {metric.Name}, Type: {metric.MetricType}, Description: {metric.Description}");

                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                {
                    var msg = metric.MetricType switch
                    {
                        MetricType.LongSum => $"Value: {metricPoint.GetSumLong()}",
                        MetricType.LongGauge => $"Value: {metricPoint.GetGaugeLastValueLong()}",
                        MetricType.DoubleSum => $"Value: {metricPoint.GetSumDouble()}",
                        MetricType.DoubleGauge => $"Value: {metricPoint.GetGaugeLastValueDouble()}",
                        MetricType.Histogram => $"Count: {metricPoint.GetHistogramCount()}, Sum: {metricPoint.GetHistogramSum()}",
                        _ => "None"
                    };
                    Console.WriteLine(msg);

                    if (metricPoint.Tags.Count > 0)
                    {
                        Console.WriteLine("Tags: ");
                        foreach (var tag in metricPoint.Tags)
                        {
                            Console.WriteLine($"{tag.Key} = {tag.Value}");
                        }
                    }
                }
            }

            return ExportResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return ExportResult.Failure;
        }
    }
}