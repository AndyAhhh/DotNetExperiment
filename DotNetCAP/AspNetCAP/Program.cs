using DotNetCore.CAP;
using DotNetCore.CAP.Filter;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Serialization;
using Microsoft.AspNetCore.Mvc;
using Savorboard.CAP.InMemoryMessageQueue;
using System.Text.Json;

namespace AspNetCAP;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //DI容器中注册CAP服务，至少要配置一个存储和一个传输器，示例使用内存存储和内存消息队列，适用于测试和开发环境，不建议在生产环境使用
        builder.Services.AddCap(config =>
        {
            config.UseInMemoryStorage();
            config.UseInMemoryMessageQueue();

            // 各配置参数：
            // DefaultGroupName，默认值：cap.queue.{程序集名称}，默认的消费者组的名字，在不同的Transports中对应不同的名字，可以通过自定义此值来自定义不同Transports中的名字，以便于查看。
            // 在RabbitMQ中映射到Queue Names
            // 在Apache Kafka中映射到Consumer Group Id
            // 在Azure Service Bus中映射到Subscription Name
            // 在NATS中映射到Queue Group Name
            // 在Redis Streams中映射到Consumer Group

            // GroupNamePrefix，默认值：Null，为订阅Group统一添加前缀。

            // TopicNamePrefix，默认值： Null，为Topic统一添加前缀。

            // Version：默认值：v1用于给消息指定版本来隔离不同版本服务的消息，常用于A/B测试或者多服务版本的场景。

            // FailedRetryInterval：默认值：60 秒，在消息发送的时候，如果发送失败，CAP将会对消息进行重试，此配置项用来配置每次重试的间隔时间。
            // 在消息消费的过程中，如果消费失败，CAP将会对消息进行重试消费，此配置项用来配置每次重试的间隔时间。
            // 发送和消费消息的过程中失败会立即重试3次，在3次以后将进入重试轮询。
            // 初次轮询时间为240秒（FallbackWindowLookbackSeconds默认值）后开始，后续轮询重试间隔为FailedRetryInterval
            // 基于数据库的分布式锁以应对在多个实例下对数据库重试的并发数据获取问题，需要显式配置UseStorageLock为true

            // UseStorageLock：默认值：false，如果设置为true，将使用基于数据库的分布式锁以应对重试进程在多个实例下对数据库数据的并发获取问题。这将会在数据库生成cap.lock表

            // ConsumerThreadCount：默认值：1，消费者线程并行处理消息的线程数，当这个值大于1时，将不能保证消息执行的顺序

            // CollectorCleaningInterval：默认值：300秒，收集器删除已经过期消息的时间间隔

            // SchedulerBatchSize：默认值：1000，调度器每次循环获取的延迟或排队消息的最大数量

            // FailedRetryCount：默认值：50，重试的最大次数，当达到此设置值时，将不会再继续重试，通过改变此参数来设置重试的最大次数

            // FallbackWindowLookbackSeconds：默认值：240秒，配置重试处理器拾取Scheduled或Failed状态消息的回退时间窗

            // FailedThresholdCallback：默认值：NULL，类型：Action<FailedInfo>，重试阈值的失败回调。当重试达到 FailedRetryCount 设置的值的时候，将调用此 Action 回调，你可以通过指定此回调来接收失败达到最大的通知，以做出人工介入。例如发送邮件或者短信

            // SucceedMessageExpiredAfter：默认值：24*3600 秒（1天后），成功消息的过期时间（秒）。当消息发送或者消费成功时候，在时间达到SucceedMessageExpiredAfter秒时候将会从Persistent中删除，你可以通过指定此值来设置过期的时间

            // FailedMessageExpiredAfter：默认值：15*24*3600 秒（15天后），失败消息的过期时间（秒）。当消息发送或者消费失败时候，在时间达到FailedMessageExpiredAfter秒时候将会从Persistent中删除，你可以通过指定此值来设置过期的时间

            // EnableSubscriberParallelExecute：默认值：false，如果设置为 true，CAP将提前从Broker拉取一批消息置于内存缓冲区，然后执行订阅方法；当订阅方法执行完成后，拉取下一批消息至于缓冲区然后执行
            // 设置为true可能会产生一些问题，当订阅方法执行过慢耗时太久时，会导致重试线程拾取到还未执行的的消息。重试线程默认拾取4分钟前（FallbackWindowLookbackSeconds 配置项）的消息，也就是说如果消费端积压了超过4分钟（FallbackWindowLookbackSeconds 配置项）的消息就会被重新拾取到再次执行

            // SubscriberParallelExecuteThreadCount：默认值：Environment.ProcessorCount，当启用EnableSubscriberParallelExecute时, 可通过此参数执行并行处理的线程数，默认值为处理器个数

            // SubscriberParallelExecuteBufferFactor：默认值：1，当启用EnableSubscriberParallelExecute时, 通过此参数设置缓冲区和线程数的因子系数，也就是缓冲区大小等于SubscriberParallelExecuteThreadCount乘SubscriberParallelExecuteBufferFactor

            // EnablePublishParallelSend：默认值：false，默认情况下，发送的消息都先放置到内存同一个Channel中，然后线性处理。 如果设置为 true，则发送消息的任务将由.NET线程池并行处理，这会大大提高发送的速度
        }).AddSubscribeFilter<MyCapFilter>();// 配置过滤器（目前不支持添加多个过滤器），过滤器的生命周期为Scoped

        //注册自定义序列化
        builder.Services.AddSingleton<ISerializer, MySerializer>();

        var app = builder.Build();

        // 消息
        // 使用ICapPublisher接口发送出去的数据称之为Message(消息)
        // 消费者抛出OperationCanceledException（包括TaskCanceledException），异常会被忽略。若HTTPClient配置了Timeout，需要单独对异常进行处理
        {
            // 发送消息
            app.MapGet("/send", ([FromServices] ICapPublisher capBus) => capBus.PublishAsync("test.show.time", DateTimeOffset.Now));

            // 发送延迟消息
            app.MapGet("/send/delay", ([FromServices] ICapPublisher capBus) => capBus.PublishDelayAsync(TimeSpan.FromSeconds(3), "test.show.time", DateTimeOffset.Now));

            // 发送包含头信息的消息
            app.MapGet("/send/header", ([FromServices] ICapPublisher capBus) => capBus.PublishAsync("test.show.time", DateTimeOffset.Now, new Dictionary<string, string?>
            {
                ["my.header.first"] = "first",
                ["my.header.second"] = "second"
            }));

            // 补偿事务
            // 发送的时候指定callbackName，将消费者的执行结果转发到指定的callbackName的消息中
            app.MapGet("/send/compensatingTransaction", ([FromServices] ICapPublisher capBus) => capBus.PublishAsync("place.order.qty.deducted",
                contentObj: new { OrderId = 1234, ProductId = 23255, Qty = 1 },
                callbackName: "place.order.mark.status"));

            // 异构系统集成
            // 在异构系统中，需要在发消息的时候向消息的Header中写入的内容：
            // cap-msg-id	   long	    消息Id，由雪花算法生成
            // cap-msg-name    string   消息名称，即Topic/RoutingKey名字
            // cap-msg-type    string   消息的类型，即typeof(T).FullName(非必须)
            // cap-senttime    string   发送的时间(非必须)

            // 消息调度
            // CAP接收到消息之后会将消息发送到Transport，由Transport进行运输（ICapPublisher目前不支持批量发送消息）

            // 消息存储
            // CAP接收到消息之后会将消息进行Persistent（持久化）

            // 消息重试异常
            // 无论发送失败或者消费失败，异常消息存储到消息header中的cap-exception字段中，可以在数据库表的Content字段的json中找到

            // 消息数据清理
            // CAP默认情况下会每隔5分钟将消息表的数据进行清理删除，避免数据量过多导致性能的降低。清理规则为ExpiresAt不为空并且小于当前时间的数据
        }

        // 传输器：
        // CAP支持多种消息队列作为传输器，默认情况下没有配置传输器，消息发送后将会被存储到数据库中，但是不会被发送到消息队列中。需要至少配置一个传输器来将消息发送到消息队列中，目前CAP支持的传输器有：RabbitMQ、Kafka、Azure Service Bus、NATS、Redis Streams等
        // CAP支持以下几种运输方式：
        // RabbitMQ
        // Kafka
        // Azure Service Bus
        // Amazon SQS
        // NATS
        // In - Memory Queue
        // Redis Streams
        // Apache Pulsar
        // 以下是社区支持：
        // ActiveMQ
        // RedisMQ
        // ZeroMQ
        // MQTT

        // 存储：
        // CAP需要使用具有持久化功能的存储介质来存储事件消息
        // 持久化：
        // 发送前：在消息进入到消息队列之前，CAP使用本地数据库表对消息进行持久化
        // 发送后：消息进入到消息队列之后，CAP会启动消息队列的持久化功能
        // CAP支持以下几种具有事务支持的数据库做为存储：
        // SQL Server
        // MySQL
        // PostgreSql
        // MongoDB
        // In-Memory Storage
        // 在CAP启动后，会向持久化介质中生成两个表，默认情况下名称为：Cap.Published和Cap.Received
        // 包装器对象：
        // CAP在进行消息发送到时候，会对原始消息对象进行一个二次包装存储到Content字段中
        // 以下是社区支持：
        // SQLite
        // LiteDB
        // SQLite & Oracle
        // SmartSql
        // DM

        // 监控：
        // Consul
        // Dashboard
        // Kubernetes
        // Diagnostics
        // OpenTelemetry

        app.Run();
    }

    /// <summary>
    /// 处理消息
    /// 使用[CapSubscribe]特性标记订阅方法，参数：
    /// 
    /// Name（string, 必须项）：通过指定Name参数来订阅消息，对应发布消息时通过 Publish("Name") 指定的名称，该名称在不同的 Broker 有不同的对应项：
    /// ·在RabbitMQ中对应Routing Key
    /// ·在Kafka中对应Topic
    /// ·在AzureServiceBus中对应Subject
    /// ·在NATS 中对应Subject
    /// ·在RedisStrems中对应Stream
    /// 
    /// Group（string, 可选项）：通过指定 Group 参数来使订阅者位于单独的消费者组中，消费者组的概念类似于 Kafka 中的消费者组。如果不指定此参数将使用当前程序集名称(DefaultGroupName)作为默认值
    /// 相同Name的订阅者设置为不同的组时，他们都会收到消息。相反如果相同Name的订阅者设置相同的组时，只有一个会收到消息
    /// 不同Name的订阅者设置为不同的组时，也是有意义的，他们可以拥有独立的线程来执行。相反如果不同 Name 的订阅者设置相同的组时，他们将共享消费线程。
    /// Group 在不同的 Broker 有不同的对应项：
    /// ·在RabbitMQ中对应Queue
    /// ·在Kafka中对应Consumer Group
    /// ·在AzureServiceBus中对应Subscription Name
    /// ·在NATS中对应Queue Group
    /// ·在RedisStrems中对应Consuemr Group
    /// 
    /// GroupConcurrent（GroupConcurrent）：通过指定GroupConcurrent参数的值来设置订阅者并行执行的并行度。并行执行意味着其需要位于独立线程中，因此如果你没有指定Group参数，则CAP将会以Name的值自动创建一个Group。
    /// 如果你有多个订阅者都设置为了相同的Group，并且也给订阅者都设置了GroupConcurrent的值，则并行度为组内值的和。
    /// 本设置只对新消息生效，重试的消息不受并行度限制。
    /// </summary>
    [CapSubscribe("test.show.time")]
    public void ReceiveMessage(DateTime time)
    {
        Console.WriteLine("message time is:" + time);
    }

    /// <summary>
    /// 处理包含头信息的消息
    /// </summary>
    [CapSubscribe("test.show.time")]
    public void ReceiveMessage(DateTime time, [FromCap] CapHeader header)
    {
        Console.WriteLine("message time is:" + time);
        Console.WriteLine("message firset header :" + header["my.header.first"]);
        Console.WriteLine("message second header :" + header["my.header.second"]);
    }

    [CapSubscribe("place.order.mark.status")]
    public void MarkOrderStatus(JsonElement param)
    {
        var orderId = param.GetProperty("OrderId").GetInt32();
        var isSuccess = param.GetProperty("IsSuccess").GetBoolean();

        if (isSuccess)
        {
            // mark order status to succeeded
        }
        else
        {
            // mark order status to failed
        }
    }

    [CapSubscribe("place.order.qty.deducted")]
    public object DeductProductQty(JsonElement param, [FromCap] CapHeader header)
    {
        var orderId = param.GetProperty("OrderId").GetInt32();
        var productId = param.GetProperty("ProductId").GetInt32();
        var qty = param.GetProperty("Qty").GetInt32();

        // 控制回调响应
        // 通过[FromCap]标记在订阅方法中注入CapHeader参数，并利用其提供的方法来向回调上下文中添加额外的头信息或者终止回调
        {
            // 添加额外的头信息到响应消息中
            header.AddResponseHeader("some-message-info", "this is the test");

            // 或再次添加回调的回调
            header.AddResponseHeader(DotNetCore.CAP.Messages.Headers.CallbackName, "place.order.qty.deducted-callback");

            // 如果你不再遵从发送着指定的回调，想修改回调，可通过 RewriteCallback 方法修改。
            header.RewriteCallback("new-callback-name");

            // 如果你想终止/停止，或不再给发送方响应，调用 RemoveCallback 来移除回调。
            header.RemoveCallback();
        }

        return new { OrderId = orderId, IsSuccess = true };
    }
}

/// <summary>
/// 自定义过滤器
/// 如果想终止订阅者方法执行，可以在OnSubscribeExecutingAsync中抛出异常，并且在OnSubscribeExceptionAsync中选择忽略该异常，通过在ExceptionContext中设置context.ExceptionHandled = true来忽略异常
/// </summary>
public class MyCapFilter : SubscribeFilter
{
    public override Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        // 订阅方法执行前
        return Task.CompletedTask;
    }

    public override Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        // 订阅方法执行后
        return Task.CompletedTask;
    }

    public override Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        // 订阅方法执行异常
        return Task.CompletedTask;
    }
}

/// <summary>
/// 自定义序列化
/// 默认情况使用json来对消息进行序列化处理并存储到数据库中
/// </summary>
public class MySerializer : ISerializer
{
    public Message? Deserialize(string json)
    {
        throw new NotImplementedException();
    }

    public object? Deserialize(object value, Type valueType)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Message> DeserializeAsync(TransportMessage transportMessage, Type? valueType)
    {
        throw new NotImplementedException();
    }

    public bool IsJsonType(object jsonObject)
    {
        throw new NotImplementedException();
    }

    public string Serialize(Message message)
    {
        throw new NotImplementedException();
    }

    public ValueTask<TransportMessage> SerializeAsync(Message message)
    {
        throw new NotImplementedException();
    }
}