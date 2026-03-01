using CommonLibrary.Helpers;
using CommonLibrary.Helpers.Interfaces;
using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using CQRS.Core.DefaultConcreteObjects.Repository;
using CQRS.Core.DefaultConcreteObjects.Stores;
using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using DataAccess;
using Service.Common.Filters;
using Service.Common.Helpers;
using Service.Common.Middleware;
using SignUpService.Cmd.API.Commands;
using SignUpService.Cmd.API.Commands.Interfaces;
using SignUpService.Cmd.Domain.Aggregates;
using SignUpService.Cmd.Domain.Handlers;
using SignUpService.Cmd.Infrastructure.Handlers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "SignUpService";

// 註冊 DbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// 設定 Serilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

// JWT 設定
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

// 註冊 HttpClientRequestHelper
builder.Services.AddHttpClientRequestHelper();

// 註冊服務
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventHandler, SignUpService.Cmd.Infrastructure.Handlers.EventHandler>();
builder.Services.AddScoped<IEventStore, EventStore<IEventHandler>>();
builder.Services.AddScoped<IEventSourcingHandler<SignUpAggregate>, EventSourcingHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();
builder.Services.AddScoped<IKeyGeneratorHelper, KeyGeneratorHelper>();
builder.Services.AddScoped<IValidateHelper, ValidateHelper>();

// 註冊 Dispatcher
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

// 註冊命令處理函式
builder.Services.AddScoped<Func<SignInCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<ValidateCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<ForgetPasswordCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<PasswordChangeCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<UserInfoChangeCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<AccountDisableCommand, Task<TResult>>>(sp =>
    async cmd => await sp.GetRequiredService<ICommandHandler>().HandleAsync(cmd));
builder.Services.AddScoped<Func<AccountDeleteCommand, Task<TResult>>>(sp =>
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