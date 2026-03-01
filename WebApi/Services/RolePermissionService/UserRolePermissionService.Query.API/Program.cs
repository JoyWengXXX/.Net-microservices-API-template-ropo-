using CQRS.Core.Infrastructure;
using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using Service.Common.Middleware;
using Service.Common.Filters;
using RolePermissionService.Query.Infrastructure.Handlers;
using RolePermissionService.Query.Domain.Queries.Interfaces;
using RolePermissionService.Query.Domain.Queries;
using DataAccess;
using Service.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "RolePermissionService";

// è¨»å? DbContext
ServicesInjectionHelper.InjectDbContext(builder, dbContextTypes: typeof(MainDBConnectionManager));

// è¨­å? Serilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

// è¨­å? JWT
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

// è¨»å? Dispatcher
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();
var dispatcher = new QueryDispatcher();
builder.Services.AddScoped<IQueryDispatcher>(_ => dispatcher);
dispatcher.RegisterHandler<GetPermissionsQuery>(queryHandler.HandleAsync);

// Add services to the container.

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

// è¨»å? Filter
builder.Services.AddTransient<ActionRoleFilter>();

var app = builder.Build();

// è¨»å? middleware
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

