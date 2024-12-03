using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class TestConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare("order_processing", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<Order>(message);

            Console.WriteLine($"Processing order {order.Id}:");
            Console.WriteLine($"- Simulating payment processing for ${order.TotalPrice}");
            Thread.Sleep(1000);
            Console.WriteLine($"- Payment processed successfully");

            Console.WriteLine($"- Sending confirmation email to {order.UserEmail}");
            Thread.Sleep(500);
            Console.WriteLine($"- Email sent successfully");
            
            Console.WriteLine("Order processing completed\n");
        };

        channel.BasicConsume(queue: "order_processing",
                           autoAck: true,
                           consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
} 