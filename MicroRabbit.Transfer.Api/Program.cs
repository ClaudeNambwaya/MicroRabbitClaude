using MicroRabbit.Banking.Domain.CommandHandlers;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Application.Services;
using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Data.Repository;
using MicroRabbit.Transfer.Domain.EventHandlers;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IServiceCollection, ServiceCollection>();
builder.Services.AddScoped<IEventHandler<TransferCreatedEvent>, TransferEventHandler>();
builder.Services.AddScoped<IEventBus, RabbitMQBus>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<TransferCommandHandler>());
builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddDbContext<TransferDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TransferDbConnection"));

});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transfer Microservice V1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

ConfigureEventBus(app);

void ConfigureEventBus(WebApplication app)
{
    var eventBus = ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IEventBus>();
    eventBus.Subscribe<TransferCreatedEvent, TransferEventHandler>();
}





