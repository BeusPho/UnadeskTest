using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using UnadeskTestCommon.Entities;

namespace UnadeskTestApiGateway.Producers;

public class MqGetContentRequestResponse(IConnection connection)
{
    public async Task<MqGetContentResponse?> GetPdfContentAsync(int id, CancellationToken cancellationToken = default)
    {
        string? consumerTag = null;
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        try
        {
            var correlationId = Guid.NewGuid().ToString();

            var replyQueue = await channel.QueueDeclareAsync(
                queue: string.Empty,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = replyQueue.QueueName,
            };

            var request = new MqGetContentRequest { Id = id };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

            Console.WriteLine($"Идентификаторы отправляемого сообщения. Id:{request.Id}, replyQueue.QueueName:{replyQueue.QueueName}, props.CorrelationId:{correlationId}");
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "pdf_getcontent_request_queue",
                mandatory: true,
                basicProperties: props,
                body: body,
                cancellationToken);

            var tcs = new TaskCompletionSource<MqGetContentResponse?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += (model, ea) =>
            {
                Console.WriteLine($"Получен ответ. ea.BasicProperties.CorrelationId:{ea.BasicProperties.CorrelationId}, ожидаемый: {correlationId}");
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    Console.WriteLine("CorrelationId совпали");

                    var response = JsonSerializer.Deserialize<MqGetContentResponse>(Encoding.UTF8.GetString(ea.Body.Span));

                    tcs.TrySetResult(response);

                    if (!string.IsNullOrEmpty(consumerTag))
                    {
                        _ = channel.BasicCancelAsync(consumerTag, noWait: true, cancellationToken);
                    }
                }
                return Task.CompletedTask;
            };

            consumerTag = await channel.BasicConsumeAsync(replyQueue.QueueName, autoAck: true, consumer: consumer, cancellationToken);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            using (cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                return await tcs.Task;
            }
        }
        finally
        {
            await channel.CloseAsync(cancellationToken);
            await channel.DisposeAsync();
        }
    }
}
