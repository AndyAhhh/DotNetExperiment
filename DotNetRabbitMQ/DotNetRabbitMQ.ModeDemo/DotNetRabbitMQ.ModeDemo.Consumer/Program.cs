using DotNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DotNetRabbitMQ.ModeDemo.Consumer;

/// <summary>
/// 消费者
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        #region 简单模式
        var conn = await RabbitMQHelper.GetConnectionAsync();
        var channel = await conn.CreateChannelAsync();
        await channel.QueueDeclareAsync(
                    queue: RabbitMQHelper.Queue_1,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
            /*await channel.BasicAckAsync(e.DeliveryTag, false);*/
        };
        await channel.BasicConsumeAsync(RabbitMQHelper.Queue_1, autoAck: true/*false*/, consumer);
        #endregion

        #region 工作队列模式
        //var conn = await RabbitMQHelper.GetConnectionAsync();
        //var channel = await conn.CreateChannelAsync();
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.BasicQosAsync(0, 1, false);// 当worker处理并确认后才分派,prefetchSize:传输内容大小限制,0为不限制;prefetchCount:同时消费限制数量;global:true应用于信道,false应用于消费者
        //var consumer = new AsyncEventingBasicConsumer(channel);
        //consumer.ReceivedAsync += (sender, e) =>
        //{
        //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
        //    return Task.CompletedTask;
        //};
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_1, true, consumer);
        #endregion

        #region 发布订阅模式
        //var conn = await RabbitMQHelper.GetConnectionAsync();
        //var channel = await conn.CreateChannelAsync();
        //await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Fanout);
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, string.Empty);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, string.Empty);
        //var consumer = new AsyncEventingBasicConsumer(channel);
        //consumer.ReceivedAsync += (sender, e) =>
        //{
        //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
        //    return Task.CompletedTask;
        //};
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_1, true, consumer);
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_2, true, consumer);
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_3, true, consumer);
        #endregion

        #region 路由模式
        //var conn = await RabbitMQHelper.GetConnectionAsync();
        //var channel = await conn.CreateChannelAsync();
        //await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //var consumer = new AsyncEventingBasicConsumer(channel);
        //consumer.ReceivedAsync += async (sender, e) =>
        //{
        //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, routingKey:{e.RoutingKey}");
        //    await channel.BasicAckAsync(e.DeliveryTag, true);//手动签收,批量
        //};
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_1, false, consumer);
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_2, false, consumer);
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_3, false, consumer);
        #endregion

        #region 主题模式
        //var conn = await RabbitMQHelper.GetConnectionAsync();
        //var channel = await conn.CreateChannelAsync();
        //await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Topic);
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_1);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_2);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_Etc);
        //var consumer = new AsyncEventingBasicConsumer(channel);
        //consumer.ReceivedAsync += async (sender, e) =>
        //{
        //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, routingKey:{e.RoutingKey}");
        //    await channel.BasicAckAsync(e.DeliveryTag, true);//手动签收,批量
        //};
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_1, false, consumer);
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_2, false, consumer);
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_3, false, consumer);
        #endregion

        #region 重试 & 死信(延迟)
        //var conn = await RabbitMQHelper.GetConnectionAsync();
        //var channel = await conn.CreateChannelAsync();
        //await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
        //await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_Retry, ExchangeType.Direct);
        //await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchane_Dead, ExchangeType.Direct);
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?>
        //{
        //    ["x-queue-type"] = "quorum",
        //    ["x-dead-letter-exchange"] = RabbitMQHelper.Exchane_Dead, //指定死信交换机,用于将Queue_1队列中失败的消息投递到Exchane_Dead交换机
        //    ["x-dead-letter-routing-key"] = RabbitMQHelper.RoutingKey_1, //指定死信路由键,用于将Queue_1队列中失败的消息投递到Exchane_Dead交换机的RoutingKey_1路由键
        //    ["x-message-ttl"] = 30000 //定义消息默认的最大停留时间,超时后投递到死信队列
        //});
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?>
        //{
        //    ["x-queue-type"] = "quorum",
        //    ["x-dead-letter-exchange"] = RabbitMQHelper.Exchange_1, ////指定死信交换机,用于将Queue_2队列中超时的消息投递到Exchange_1交换机
        //    ["x-message-ttl"] = 30000 //定义消息默认的最大停留时间,超时后投递到死信队列
        //});
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_Retry, RabbitMQHelper.RoutingKey_1);
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchane_Dead, RabbitMQHelper.RoutingKey_1);

        //var consumer = new AsyncEventingBasicConsumer(channel);
        //consumer.ReceivedAsync += async (sender, e) =>
        //{
        //    var _sender = (AsyncEventingBasicConsumer)sender;                           //消息传送者
        //    var _channel = ((AsyncEventingBasicConsumer)sender).Channel;                //消息传送信道
        //    var _message = (BasicDeliverEventArgs)e;                                    //消息传送参数
        //    var _header = e.BasicProperties.Headers;                                    //消息头
        //    var _content = System.Text.Encoding.UTF8.GetString(e.Body.ToArray());       //消息内容

        //    var retryCount = 0;
        //    if (e.BasicProperties.Headers != null && e.BasicProperties.Headers.ContainsKey("x-retry-count"))
        //    {
        //        int.TryParse(e.BasicProperties.Headers["x-retry-count"]?.ToString(), out retryCount);
        //    }

        //    //模拟成功
        //    try
        //    {
        //        //throw new Exception("模拟消息处理失败效果");
        //        Console.WriteLine($"{DateTime.Now} Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, retryCount:{retryCount}");
        //        await channel.BasicAckAsync(e.DeliveryTag, false);
        //    }
        //    //模拟失败
        //    catch
        //    {
        //        Console.WriteLine($"{DateTime.Now} Consume failure:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, retryCount:{retryCount}");
        //        if (retryCount >= 2)//重试第3次(首次为0)还没成功,投递到死信交换机--也就是上边定义的{ "x-dead-letter-exchange", RabbitMQHelper.Exchane_Dead }
        //            await channel.BasicNackAsync(e.DeliveryTag, false, false);
        //        else//否则投递到Exchange_Retry
        //        {
        //            BasicProperties properties = new BasicProperties();
        //            properties.Headers ??= new Dictionary<string, object?>();
        //            properties.Headers["x-retry-count"] = retryCount + 1;
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_Retry, RabbitMQHelper.RoutingKey_1, false, properties, e.Body);
        //            await channel.BasicAckAsync(e.DeliveryTag, false);
        //        }
        //    }
        //};
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_1, false, consumer);
        #endregion

        #region 消息持久化 & 集群
        //var conn = await RabbitMQHelper.GetClusterConnectionAsync();
        //var channel = await conn.CreateChannelAsync();
        //await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Fanout, true, false, null);
        //await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty, null);
        //var consumer = new AsyncEventingBasicConsumer(channel);
        //consumer.ReceivedAsync += async (sender, e) =>
        //{
        //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
        //    await channel.BasicAckAsync(e.DeliveryTag, false);
        //};
        //await channel.BasicConsumeAsync(RabbitMQHelper.Queue_1, false, consumer);
        #endregion

        Console.ReadLine();
    }
}