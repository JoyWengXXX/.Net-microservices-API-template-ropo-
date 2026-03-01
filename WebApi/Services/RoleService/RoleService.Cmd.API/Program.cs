using CQRS.Core.Domain;
using CQRS.Core.Infrastructure;
using CQRS.Core.Handlers;
using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using CQRS.Core.DefaultConcreteObjects.Repository;
using CQRS.Core.DefaultConcreteObjects.Stores;
using Service.Common.Middleware;
using Service.Common.Filters;
using RoleService.Cmd.API.Commands.Interfaces;
using RoleService.Cmd.API.Commands;
using RoleService.Cmd.Domain.Aggregates;
using RoleService.Cmd.Domain.Handlers;
using RoleService.Cmd.Infrastructure.Handlers;
using DataAccess;
using Service.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "RoleService";

// Ë®ªÂ? DbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// Ë®≠Â? Serilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

// JWT Ë®≠Â?
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

// Ë®ªÂ??çÂ?
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventHandler, RoleService.Cmd.Infrastructure.Handlers.EventHandler>();
builder.Services.AddScoped<IEventStore, EventStore<IEventHandler>>();
builder.Services.AddScoped<IEventSourcingHandler<RoleAggregate>, EventSourcingHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

// Ë®ªÂ? Dispatcher
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

// Ë®ªÂ??Ω‰ª§?ïÁ??ΩÊï∏
builder.Services.AddScoped<Func<AddRoleCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<DisableRoleCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<UpdateRoleCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));

// CQRS Ë®≠Â?
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

// Ë®ªÂ? Filter
builder.Services.AddScoped<ActionRoleFilter>();

var app = builder.Build();

// Ë®ªÂ? middleware
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
