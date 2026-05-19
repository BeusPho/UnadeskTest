using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using UnadeskTestApiGateway.Producers;
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

builder.Services.AddScoped<MqLoadProducer>();
builder.Services.AddScoped<MqGetContentRequestResponse>();
builder.Services.AddScoped<PdfRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
