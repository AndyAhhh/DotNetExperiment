using RabbitMQ.Client;

namespace DotNetRabbitMQ.ModeDemo.Utils;

public class RabbitMQHelper
{
    public static Task<IConnection> GetConnectionAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin",
            VirtualHost = "/"
        };

        return connectionFactory.CreateConnectionAsync();
    }
    public static Task<IConnection> GetClusterConnectionAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            UserName = "admin",
            Password = "admin",
            VirtualHost = "/"
        };
        IList<AmqpTcpEndpoint> lstEndpoint = new List<AmqpTcpEndpoint>
        {
            new AmqpTcpEndpoint{ HostName = "localhost", Port = 5672 }
        };

        return connectionFactory.CreateConnectionAsync(lstEndpoint);
    }
    public static string Exchange_1 => "Exchange_1";
    public static string Exchane_Dead => "Exchane_Dead";
    public static string Exchange_Retry => "Exchange_Retry";
    public static string Exchange_Backup => "Exchange_Backup";
    public static string Queue_1 => "Queue_1";
    public static string Queue_2 => "Queue_2";
    public static string Queue_3 => "Queue_3";
    public static string RoutingKey_1 => "RoutingKey_1";
    public static string RoutingKey_Topic_1 => "RoutingKey.Topic.1";
    public static string RoutingKey_Topic_2 => "RoutingKey.Topic.2";
    public static string RoutingKey_Topic_Etc => "RoutingKey.Topic.#";
    public static string RoutingKey_Topic_Publish => "RoutingKey.Topic.Publish";
}