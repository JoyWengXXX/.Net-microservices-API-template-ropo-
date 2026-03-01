using CQRS.Core.DefaultConcreteObjects.Repository;
using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.DefaultConcreteObjects.Stores;
using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using Service.Common.Filters;
using Service.Common.Middleware;
using RolePermissionService.Cmd.API.Commands;
using RolePermissionService.Cmd.API.Commands.Interfaces;
using RolePermissionService.Cmd.Domain.Aggregates;
using RolePermissionService.Cmd.Domain.Handlers;
using RolePermissionService.Cmd.Infrastructure.Handlers;
using DataAccess;
using Service.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "RolePermissionService";

// ���UDbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// �]�wSerilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

// JWT�]�w
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

// ���U�A��
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventHandler, RolePermissionService.Cmd.Infrastructure.Handlers.EventHandler>();
builder.Services.AddScoped<IEventStore, EventStore<IEventHandler>>();
builder.Services.AddScoped<IEventSourcingHandler<RolePermissionAggregate>, EventSourcingHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

// ���UDispatcher
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

// ���U�R�O�B�z��
builder.Services.AddScoped<Func<UpdateRolePermissionCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));

// CQRS �]�w
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

// ���UFilter
builder.Services.AddScoped<ActionRoleFilter>();

var app = builder.Build();

// ���Umiddleware
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
