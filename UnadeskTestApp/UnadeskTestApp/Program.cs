using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using UnadeskTestApp.Services;
using UnadeskTestCommon.Infrastructure;
using UnadeskTestCommon.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("UnadeskTestApp")
    ));

builder.Services.AddSingleton<IConnection>(serviceProvider =>
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    return factory.CreateConnectionAsync().Result;
});

builder.Services.AddScoped<PdfProcessingService>();
builder.Services.AddScoped<PdfRepository>();

builder.Services.AddHostedService<LoadPdfBackgroundService>();
builder.Services.AddHostedService<GetContentBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
