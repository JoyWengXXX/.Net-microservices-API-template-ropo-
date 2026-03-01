using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using CQRS.Core.Infrastructure;
using DataAccess;
using Service.Common.Filters;
using Service.Common.Helpers;
using Service.Common.Middleware;
using SignUpService.Query.Domain.Queries;
using SignUpService.Query.Domain.Queries.Interfaces;
using SignUpService.Query.Infrastructure.Handlers;

var builder = WebApplication.CreateBuilder(args);
string serviceName = "SignUpService";

//���UDbContext
ServicesInjectionHelper.InjectDbContext(builder, null, "", typeof(MainDBConnectionManager));

// �]�wSerilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

//JWT�]�w
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

//���UDispatcher
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();
var dispatcher = new QueryDispatcher();
dispatcher.RegisterHandler<UserInfoQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<AllUserQuery>(queryHandler.HandleAsync);
builder.Services.AddScoped<IQueryDispatcher>(_ => dispatcher);

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
