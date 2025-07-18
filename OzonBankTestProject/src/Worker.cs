using Confluent.Kafka;
using Google.Protobuf;
using Grpc.Net.Client;
using ReportService.Api;
namespace OzonBankTestProject;

public class Worker : BackgroundService
{
    
    private readonly string _kafkaBootstrapServers = "localhost:9092";
    private readonly string _requestTopic = "report-requests";
    private readonly string _responseTopic = "report-responses";


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configConsumer = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = "report-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var configProducer = new ProducerConfig
        {
            BootstrapServers = _kafkaBootstrapServers
        };

        using var consumer = new ConsumerBuilder<string, byte[]>(configConsumer).Build();
        using var producer = new ProducerBuilder<string, byte[]>(configProducer).Build();

        consumer.Subscribe(_requestTopic);

        // Создаём gRPC клиент для обращения к своему сервису
        var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var grpcClient = new Report.ReportClient(channel);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);

                // Десериализация protobuf запроса из Kafka
                var request = CreateReportRequest.Parser.ParseFrom(consumeResult.Message.Value);
                
                // Отправляем запрос в gRPC сервис
                var reply = await grpcClient.CreateReportAsync(request, cancellationToken: stoppingToken);

                // Сериализуем ответ в protobuf байты
                using var ms = new MemoryStream();
                reply.WriteTo(ms);
                var responseBytes = ms.ToArray();

                // Отправляем ответ в Kafka
                await producer.ProduceAsync(_responseTopic, new Message<string, byte[]>
                {
                    Key = consumeResult.Message.Key,
                    Value = responseBytes
                }, stoppingToken);

            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
        finally
        {
            consumer.Close();
        }
    }
}
