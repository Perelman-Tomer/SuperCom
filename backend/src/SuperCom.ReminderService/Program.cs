using Microsoft.EntityFrameworkCore;
using SuperCom.Infrastructure.Data;
using SuperCom.ReminderService;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddDbContext<SuperComDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hosted Services
builder.Services.AddHostedService<DueDateCheckerService>();
builder.Services.AddHostedService<ReminderConsumerService>();

// Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SuperCom Task Reminder Service";
});

var host = builder.Build();
host.Run();
