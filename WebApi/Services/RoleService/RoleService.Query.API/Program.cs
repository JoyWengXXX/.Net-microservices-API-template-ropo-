using CQRS.Core.Infrastructure;
using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using Service.Common.Middleware;
using Service.Common.Filters;
using RoleService.Query.Domain.Queries.Interfaces;
using RoleService.Query.Domain.Queries;
using RoleService.Query.Infrastructure.Handlers;
using DataAccess;
using Service.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "RoleService";

//���UDbContext
ServicesInjectionHelper.InjectDbContext(builder, dbContextTypes: typeof(MainDBConnectionManager));

// �]�wSerilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

//JWT�]�w
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);


//���UDispatcher
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();
var dispatcher = new QueryDispatcher();
builder.Services.AddScoped<IQueryDispatcher>(_ => dispatcher);
dispatcher.RegisterHandler<GetRolesQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<GetRoleInStoreQuery>(queryHandler.HandleAsync);

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
