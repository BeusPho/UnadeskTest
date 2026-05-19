using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using UnadeskTestCommon.Entities;

namespace UnadeskTestApiGateway.Producers;

public class MqLoadProducer(IConnection connection)
{
    public async Task LoadPdfAsync(int id, string name, byte[] fileContent)
    {
        var record = new MqLoadPdfDto
        {
            Id = id,
            Name = name,
            FileContent = fileContent,
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(record));

        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync( // можно вынести в отдельный IHostedService
            queue: "pdf_processing_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "pdf_processing_queue",
            body: body);
    }
}

