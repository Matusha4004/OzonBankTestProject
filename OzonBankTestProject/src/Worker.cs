using Confluent.Kafka;
using Google.Protobuf;
using Grpc.Net.Client;
using ReportService.Api;

namespace OzonBankTestProject;

public class Worker : BackgroundService
{
    private readonly string _kafkaBootstrapServers = "kafka:9092"; 
    private readonly string _requestTopic = "report-requests";
    private readonly string _responseTopic = "report-responses";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configConsumer = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = "report-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };

        var configProducer = new ProducerConfig
        {
            BootstrapServers = _kafkaBootstrapServers
        };

        using var consumer = new ConsumerBuilder<string, byte[]>(configConsumer).Build();
        using var producer = new ProducerBuilder<string, byte[]>(configProducer).Build();

        consumer.Subscribe(_requestTopic);

        var channel = GrpcChannel.ForAddress("http://app:5000");
        var grpcClient = new Report.ReportClient(channel);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult == null || consumeResult.Message?.Value == null)
                        continue;

                    CreateReportRequest request;
                    try
                    {
                        request = CreateReportRequest.Parser.ParseFrom(consumeResult.Message.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка парсинга сообщения: {ex.Message}");
                        continue;
                    }

                    var reply = await grpcClient.CreateReportAsync(request, cancellationToken: stoppingToken);

                    using var ms = new MemoryStream();
                    reply.WriteTo(ms);
                    var responseBytes = ms.ToArray();

                    await producer.ProduceAsync(_responseTopic, new Message<string, byte[]>
                    {
                        Key = consumeResult.Message.Key,
                        Value = responseBytes
                    }, stoppingToken);

                    Console.WriteLine($"Обработано сообщение с ключом {consumeResult.Message.Key}");
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Ошибка Consumer: {ex.Error.Reason}");
                }

                await Task.Yield();
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Worker остановлен.");
        }
        finally
        {
            consumer.Close();
        }
    }
}
