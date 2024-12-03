using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public interface IRabbitMQService
{
    void PublishMessage<T>(string queueName, T message);
}

public class RabbitMQService : IRabbitMQService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void PublishMessage<T>(string queueName, T message)
    {
        _channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }
} 