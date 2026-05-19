using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using UnadeskTestCommon.Entities;
using UnadeskTestCommon.Repository;

namespace UnadeskTestApp.Services;

public class GetContentBackgroundService(IServiceScopeFactory serviceScopeFactory, IConnection connection) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(queue: "pdf_getcontent_request_queue", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            Console.WriteLine("GetContent Mq получило сообщение");

            var request = JsonSerializer.Deserialize<MqGetContentRequest>(Encoding.UTF8.GetString(ea.Body.Span));

            if (request is not null)
            {
                var props = ea.BasicProperties;
                var replyTo = props.ReplyTo; // ? null check
                var correlationId = props.CorrelationId;
                Console.WriteLine($"Идентификаторы полученного сообщения. Id:{request.Id}, props.ReplyTo:{replyTo}, props.CorrelationId:{correlationId}");
                
                using var scope = serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<PdfRepository>();
                var pdfContent = await repository.GetPdfTextContentByIdAsync(request.Id);

                
                var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new MqGetContentResponse { TextContent = pdfContent }));

                var replyProps = new BasicProperties { CorrelationId = correlationId };
                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: replyTo!,
                    mandatory: true,
                    basicProperties: replyProps,
                    body: responseBody);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync("pdf_getcontent_request_queue", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        var tcs = new TaskCompletionSource();
        stoppingToken.Register(tcs.SetResult);
        await tcs.Task;
    }
}
