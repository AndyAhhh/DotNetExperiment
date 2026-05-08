using DotNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;

namespace DotNetRabbitMQ.ModeDemo.Producer;

/// <summary>
/// 生产者
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        #region 简单模式 & 工作队列模式
        // 简单模式：点对点基础通信，包含三个核心角色：生产者、消费者和消息队列。生产者将消息发送到指定队列，消费者从同一队列中拉取消息，每条消息仅能被消费一次。适用于单个消费者顺序处理消息。（如果消费者处理速度较慢，消息会在队列中堆积，建议根据业务量合理设置队列的消息TTL和最大长度）
        // 工作队列模式：在简单模式基础上引入多个消费者，队列中的消息会根据轮询策略自动分发给不同的消费者，每个消费者接收到的消息互不相同。适用于多个消费者并行处理消息。
        using (var conn = await RabbitMQHelper.GetConnectionAsync())
        {
            using (var channel = await conn.CreateChannelAsync())
            {
                await channel.QueueDeclareAsync(
                    queue: RabbitMQHelper.Queue_1,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });// 官方推荐使用的队列类型

                string[] arrMsg = ["test1", "test2", "test3", "test4", "test5"];
                /* 在BasicPublishAsync中传参
                var properties = new BasicProperties
                {
                    Persistent = true,// 设置为持久消息, 保存到磁盘
                    Expiration = "6000"// 消息过期时间, 单位毫秒
                };
                 */
                foreach (string msg in arrMsg)
                    await channel.BasicPublishAsync(string.Empty, RabbitMQHelper.Queue_1, false, System.Text.Encoding.UTF8.GetBytes(msg));
                Console.WriteLine("Publish succeed.");
            }
        }
        #endregion

        #region 发布订阅模式
        // 即广播模式，生产者将消息发送到交换机，交换机将消息广播给所有绑定的队列。适用于需要将同一消息广播给多个消费者的场景。（交换机不存储消息，如果没有任何队列绑定，消息会丢失）
        //using (var conn = await RabbitMQHelper.GetConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Fanout);
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, string.Empty);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, string.Empty);
        //        string[] arrMsg = ["test1", "test2", "test3", "test4", "test5"];
        //        foreach (string msg in arrMsg)
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, string.Empty, false, System.Text.Encoding.UTF8.GetBytes(msg));
        //        Console.WriteLine("Publish succeed.");
        //    }
        //}
        #endregion

        #region 路由模式
        // 在发布订阅模式基础上增加了RoutingKey（路由键），生产者发送消息时指定RoutingKey，交换机根据RoutingKey将消息路由到完全匹配的队列。这样，不同的消费者可以只接收自己感兴趣的消息。
        //using (var conn = await RabbitMQHelper.GetConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //        string[] arrMsg = ["test1", "test2", "test3", "test4", "test5"];
        //        foreach (string msg in arrMsg)
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, false, System.Text.Encoding.UTF8.GetBytes(msg));
        //        Console.WriteLine("Publish succeed.");
        //    }
        //}
        #endregion

        #region 主题模式
        // 路由模式的增强版，它使用通配符来匹配RoutingKey，而不是完全相等。两个核心通配符：*：匹配一个单词（以点分隔）；#：匹配零个或多个单词。（通配符模式虽然灵活，但过多的绑定规则会降低交换机路由性能。建议控制绑定数量在100个以内）
        //using (var conn = await RabbitMQHelper.GetConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Topic);
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_1);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_2);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_Etc);
        //        string[] arrMsg = ["test1", "test2", "test3", "test4", "test5"];
        //        foreach (string msg in arrMsg)
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_Publish, false, System.Text.Encoding.UTF8.GetBytes(msg));
        //        Console.WriteLine("Publish succeed.");
        //    }
        //}
        #endregion

        #region 重试 & 死信(延迟)
        //using (var conn = await RabbitMQHelper.GetConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_Retry, ExchangeType.Direct);
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchane_Dead, ExchangeType.Direct);
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?>
        //        {
        //            ["x-queue-type"] = "quorum",
        //            ["x-dead-letter-exchange"] = RabbitMQHelper.Exchane_Dead, // 指定死信交换机,用于将Queue_1队列中失败的消息投递到Exchane_Dead交换机
        //            ["x-dead-letter-routing-key"] = RabbitMQHelper.RoutingKey_1, // 指定死信路由键,用于将Queue_1队列中失败的消息投递到Exchane_Dead交换机的RoutingKey_1路由键
        //            ["x-message-ttl"] = 30000 // 定义消息默认的最大停留时间,超时后投递到死信队列
        //        });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?>
        //        {
        //            ["x-queue-type"] = "quorum",
        //            ["x-dead-letter-exchange"] = RabbitMQHelper.Exchange_1, // 指定死信交换机,用于将Queue_2队列中超时的消息投递到Exchange_1交换机
        //            ["x-message-ttl"] = 30000 // 定义消息默认的最大停留时间,超时后投递到死信队列
        //        });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_3, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_Retry, RabbitMQHelper.RoutingKey_1);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchane_Dead, RabbitMQHelper.RoutingKey_1);
        //        string[] arrMsg = ["test1", "test2", "test3", "test4", "test5"];
        //        foreach (string msg in arrMsg)
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, false, System.Text.Encoding.UTF8.GetBytes(msg));
        //        Console.WriteLine("Publish succeed.");
        //    }
        //}
        #endregion

        #region 消息持久化 & 集群
        //using (var conn = await RabbitMQHelper.GetClusterConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Fanout, true, false, null);
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty, null);
        //        var properties = new BasicProperties
        //        {
        //            Persistent = true
        //        };
        //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
        //        foreach (string msg in arrMsg)
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, string.Empty, false, properties, System.Text.Encoding.UTF8.GetBytes(msg));
        //        Console.WriteLine("Publish succeed.");
        //    }
        //}
        #endregion

        #region 事务
        //using (var conn = await RabbitMQHelper.GetConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        try
        //        {
        //            await channel.TxSelectAsync();//开启事务
        //            await channel.BasicPublishAsync(string.Empty, RabbitMQHelper.Queue_1, false, System.Text.Encoding.UTF8.GetBytes("事务消息测试"));
        //            int i = 1;
        //            int j = i / 0;//模拟异常
        //            await channel.TxCommitAsync();
        //            Console.WriteLine("Publish succeed.");
        //        }
        //        catch (Exception)
        //        {
        //            if (channel.IsOpen)
        //            {
        //                await channel.TxRollbackAsync();
        //                Console.WriteLine("Publish failure and execute rollback");
        //            }
        //        }
        //    }
        //}
        #endregion

        #region 推送队列失败回调
        //using (var conn = await RabbitMQHelper.GetConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Direct, false, false, null);
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, null);
        //        channel.BasicReturnAsync += (sender, e) =>
        //        {
        //            var code = e.ReplyCode;//失败code
        //            var text = e.ReplyText;//失败原因
        //            var content = System.Text.Encoding.UTF8.GetString(e.Body.Span);//消息内容
        //            Console.WriteLine($"Publish failure,code:{code},text:{text},content:{content}");//对消息不可达做处理
        //            return Task.CompletedTask;
        //        };
        //        var properties = new BasicProperties
        //        {
        //            MessageId = "MsgId"
        //        };
        //        await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, "Push to error routing key", true, properties, System.Text.Encoding.UTF8.GetBytes("消息推送失败测试"));//mandatory必须为true
        //        Console.WriteLine("Publish succeed.");
        //    }
        //}
        #endregion

        #region 备份交换机
        //using (var conn = await RabbitMQHelper.GetConnectionAsync())
        //{
        //    using (var channel = await conn.CreateChannelAsync())
        //    {
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_1, ExchangeType.Direct, false, false, new Dictionary<string, object?>
        //        {
        //            ["alternate-exchange"] = RabbitMQHelper.Exchange_Backup// 指定备份交换机
        //        });
        //        await channel.ExchangeDeclareAsync(RabbitMQHelper.Exchange_Backup, ExchangeType.Fanout, false, false, null);// 声明备份交换机,模式Fanout(直接推到交换机而无需推到路由键)
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueDeclareAsync(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object?> { ["x-queue-type"] = "quorum" });
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, null);
        //        await channel.QueueBindAsync(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_Backup, string.Empty, null);
        //        string[] arrMsg = ["test1", "test2", "test3", "test4", "test5"];
        //        foreach (string msg in arrMsg)
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, false, System.Text.Encoding.UTF8.GetBytes(msg));
        //        foreach (string msg in arrMsg)
        //            await channel.BasicPublishAsync(RabbitMQHelper.Exchange_1, "Push to error routing key", false, System.Text.Encoding.UTF8.GetBytes(msg));
        //        Console.WriteLine("Publish succeed.");
        //    }
        //}
        #endregion

        Console.ReadLine();
    }
}