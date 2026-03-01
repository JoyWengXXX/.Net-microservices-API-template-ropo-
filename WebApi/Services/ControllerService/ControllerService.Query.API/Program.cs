using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using CQRS.Core.Infrastructure;
using DataAccess;
using ControllerService.Query.Domain.Queries;
using ControllerService.Query.Domain.Queries.Interfaces;
using ControllerService.Query.Infrastructure.Handlers;
using Service.Common.Filters;
using Service.Common.Helpers;
using Service.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

string serviceName = "ControllerService";

//���UDbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// �]�wSerilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

//JWT�]�w
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

//���UDispatcher
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();
var pageDispatcher = new QueryDispatcher();
pageDispatcher.RegisterHandler<GetControllersQuery>(queryHandler.HandleAsync);
builder.Services.AddScoped<IQueryDispatcher>(_ => pageDispatcher);


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

//���UFilter
builder.Services.AddTransient<ActionRoleFilter>();

var app = builder.Build();

//���Umiddleware
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
