using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RabbitMQ.Client;
using SuperCom.Infrastructure.Data;

namespace SuperCom.ReminderService;

/// <summary>
/// Background service that periodically checks for overdue tasks
/// and publishes reminder messages to RabbitMQ queue.
/// </summary>
public class DueDateCheckerService : BackgroundService
{
    private readonly ILogger<DueDateCheckerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public DueDateCheckerService(
        ILogger<DueDateCheckerService> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DueDateCheckerService started.");

        var checkInterval = _configuration.GetValue<int>("ReminderService:CheckIntervalSeconds", 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOverdueTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overdue tasks.");
            }

            await Task.Delay(TimeSpan.FromSeconds(checkInterval), stoppingToken);
        }

        _logger.LogInformation("DueDateCheckerService stopped.");
    }

    private async Task CheckOverdueTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SuperComDbContext>();

        var overdueTasks = await dbContext.Tasks
            .Where(t => t.DueDate < DateTime.UtcNow && !t.IsCompleted && !t.ReminderSent)
            .ToListAsync(cancellationToken);

        if (!overdueTasks.Any())
        {
            _logger.LogDebug("No overdue tasks found.");
            return;
        }

        _logger.LogInformation("Found {Count} overdue task(s).", overdueTasks.Count);

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

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        foreach (var task in overdueTasks)
        {
            var message = new TaskReminderMessage
            {
                TaskId = task.Id,
                Title = task.Title,
                DueDate = task.DueDate,
                UserFullName = task.UserFullName,
                UserEmail = task.UserEmail,
                SentAt = DateTime.UtcNow
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Published reminder for Task {TaskId}: '{Title}'", task.Id, task.Title);

            // Mark as sent in database for persistent idempotent tracking
            task.ReminderSent = true;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Another instance already processed some of these tasks.
            // This is expected in multi-instance deployments — log and continue.
            _logger.LogWarning(ex,
                "Concurrency conflict while marking reminders as sent. " +
                "Another instance likely processed the same task(s). Skipping.");
        }
    }
}
