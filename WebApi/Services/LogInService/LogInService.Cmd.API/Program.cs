using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using CQRS.Core.DefaultConcreteObjects.Repository;
using CQRS.Core.DefaultConcreteObjects.Stores;
using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Service.Common.Filters;
using Service.Common.Middleware;
using LogInService.API.Commands;
using LogInService.Cmd.Commands;
using LogInService.Cmd.Commands.Interfaces;
using LogInService.Cmd.Domain.Aggregates;
using LogInService.Cmd.Domain.Handlers;
using LogInService.Cmd.Infrastructure.Handlers;
using DataAccess;
using Service.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "LogInService";

// 註冊 DbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// JWT 設定
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

// 設定 Serilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

// 註冊 HttpClientRequestHelper
builder.Services.AddHttpClientRequestHelper();

// 註冊服務
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventHandler, LogInService.Cmd.Infrastructure.Handlers.EventHandler>();
builder.Services.AddScoped<IEventStore, EventStore<IEventHandler>>();
builder.Services.AddScoped<IEventSourcingHandler<LogInAggregate>, EventSourcingHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

// 註冊 Dispatcher
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

// 註冊命令處理函數
builder.Services.AddScoped<Func<LogInCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<AdminLogInCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<GoogleLogInCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<LogOutCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<EnableNotificationCommand, Task<TResult>>>(sp =>
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

// 註冊 Filter
builder.Services.AddTransient<ActionRoleFilter>();

var app = builder.Build();

// 註冊 middleware
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