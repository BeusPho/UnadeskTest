using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UnadeskTestCommon.Entities;

namespace UnadeskTestApp.Services;

public class LoadPdfBackgroundService(IServiceScopeFactory serviceScopeFactory, IConnection connection) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "pdf_processing_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                Console.WriteLine("Load Pdf Mq получило сообщение");

                var record = JsonSerializer.Deserialize<MqLoadPdfDto>(Encoding.UTF8.GetString(ea.Body.Span));

                if (record is not null)
                {
                    Console.WriteLine($"Идентификаторы полученного сообщения. ProcessGuid:{record.Id}, Name:{record.Name}");
                    using var scope = serviceScopeFactory.CreateScope();
                    var processingService = scope.ServiceProvider.GetRequiredService<PdfProcessingService>();
                    await processingService.ProcessAsync(record);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), ea.DeliveryTag);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync("pdf_processing_queue", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        stoppingToken.Register(tcs.SetResult);
        await tcs.Task;

        // или использовать await Task.Delay(Timeout.Infinite, stoppingToken); ?
    }
}
