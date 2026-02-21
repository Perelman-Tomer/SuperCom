using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SuperCom.ReminderService;

/// <summary>
/// Background service that subscribes to the RabbitMQ queue
/// and logs each reminder that comes through.
/// Uses manual acknowledgment for reliable message processing.
/// </summary>
public class ReminderConsumerService : BackgroundService
{
    private readonly ILogger<ReminderConsumerService> _logger;
    private readonly IConfiguration _configuration;

    public ReminderConsumerService(
        ILogger<ReminderConsumerService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderConsumerService started. Waiting for messages...");

        var hostName = _configuration.GetValue<string>("RabbitMQ:HostName", "localhost")!;
        var port = _configuration.GetValue<int>("RabbitMQ:Port", 5672);
        var userName = _configuration.GetValue<string>("RabbitMQ:UserName", "guest")!;
        var password = _configuration.GetValue<string>("RabbitMQ:Password", "guest")!;
        var queueName = _configuration.GetValue<string>("RabbitMQ:QueueName", "task-reminders")!;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };

        // Retry connection logic
        IConnection? connection = null;
        IChannel? channel = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                // Set prefetch count to 1 for fair dispatch and concurrency control
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Connected to RabbitMQ. Consuming from queue: {QueueName}", queueName);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (connection == null || channel == null)
        {
            _logger.LogError("Could not establish RabbitMQ connection. Service stopping.");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<TaskReminderMessage>(messageJson);

                if (message != null)
                {
                    // Log the reminder as required by the assignment
                    _logger.LogWarning("Hi your Task is due {{Task {Title}}} - ID: {TaskId}, Due: {DueDate}, Assigned to: {UserFullName} ({UserEmail})",
                        message.Title,
                        message.TaskId,
                        message.DueDate,
                        message.UserFullName,
                        message.UserEmail);
                }

                // Manual acknowledgment - message processed successfully
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminder message.");

                // Negative acknowledgment - requeue the message
                await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        // Start consuming with manual ack (autoAck: false)
        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        // Keep the service running
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ReminderConsumerService stopping...");
        }
        finally
        {
            if (channel.IsOpen)
                await channel.CloseAsync();
            if (connection.IsOpen)
                await connection.CloseAsync();
        }
    }
}
