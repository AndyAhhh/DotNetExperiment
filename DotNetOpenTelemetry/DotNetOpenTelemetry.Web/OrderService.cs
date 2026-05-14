namespace DotNetOpenTelemetry.Web;

/// <summary>
/// 模拟在DI中订单服务模块的Telemetry记录
/// </summary>
public class OrderService(OrderTelemetry orderTelemetry)
{
    private readonly OrderTelemetry _orderTelemetry = orderTelemetry;

    public async Task CreateOrderAsync(string UserId, decimal Amount)
    {
        using var activity = _orderTelemetry.StartCreateOrderActivity(UserId, Amount);

        // mock order creation logic
        await Task.Delay(TimeSpan.FromSeconds(2));
        // mock end

        _orderTelemetry.OrderCount.Add(1);
    }
}