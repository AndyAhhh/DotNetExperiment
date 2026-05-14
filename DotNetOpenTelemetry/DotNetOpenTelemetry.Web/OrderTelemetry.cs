using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DotNetOpenTelemetry.Web;

public class OrderTelemetry
{
    private static readonly ActivitySource _activitySource = new(nameof(OrderService));

    public Counter<long> OrderCount { get; }

    public OrderTelemetry(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(nameof(OrderService));
        OrderCount = meter.CreateCounter<long>("orders_created_total");
    }

    public Activity? StartCreateOrderActivity(string userId, decimal amount)
    {
        var activity = _activitySource.StartActivity("CreateOrder");
        activity?.SetTag("user.id", userId);
        activity?.SetTag("order.amount", amount);

        return activity;
    }
}