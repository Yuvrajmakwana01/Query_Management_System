using System.Text;
using RabbitMQ.Client;

namespace Repository.Services
{
    public class RabbitService
    {
        public async Task SendMessage(string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "query_queue",
                durable: false,
                exclusive: false,
                autoDelete: false
            );

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "query_queue",
                body: body
            );
        }

        public async Task ClearQueue()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("query_queue", false, false, false);

            // This removes ALL messages from queue
            await channel.QueuePurgeAsync("query_queue");
        }
    }
}