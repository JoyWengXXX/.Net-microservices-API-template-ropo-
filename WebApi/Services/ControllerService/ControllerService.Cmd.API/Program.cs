using Service.Common.Middleware;
using Service.Common.Filters;
using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.DefaultConcreteObjects.Repository;
using CQRS.Core.DefaultConcreteObjects.Stores;
using ControllerService.Cmd.API.Commands;
using ControllerService.Cmd.API.Commands.Interfaces;
using ControllerService.Cmd.Domain.Aggregates;
using ControllerService.Cmd.Domain.Handlers;
using ControllerService.Cmd.Infrastructure.Handlers;
using DataAccess;
using Service.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "ControllerService";

// 注入DbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// 設定Serilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

// JWT設定
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

// 注入服務
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventHandler, ControllerService.Cmd.Infrastructure.Handlers.EventHandler>();
builder.Services.AddScoped<IEventStore, EventStore<IEventHandler>>();
builder.Services.AddScoped<IEventSourcingHandler<ControllerAggregate>, EventSourcingHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

// 注入Dispatcher
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

// 注入命令處理器
builder.Services.AddScoped<Func<AddControllerCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<DisableControllerCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<UpdateControllerCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));

// CQRS 設定
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 注入Filter
builder.Services.AddTransient<ActionRoleFilter>();

var app = builder.Build();

// 注入middleware
app.UseMiddleware<ErrorHandler>();
app.UseMiddleware<AuthorizationHandler>();

// Configure the HTTP request pipeline.
if (CommonLibrary.Helpers.ConfigurationHelper.IsDevelopmentEnvironment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
