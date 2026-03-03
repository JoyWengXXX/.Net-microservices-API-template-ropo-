using CQRS.Core.DefaultConcreteObjects.Dispatchers;
using CQRS.Core.Infrastructure;
using DataAccess;
using ControllerService.Query.Domain.Mappers;
using ControllerService.Query.Domain.Queries;
using ControllerService.Query.Domain.Queries.Interfaces;
using ControllerService.Query.Infrastructure.Handlers;
using Service.Common.Filters;
using Service.Common.Helpers;
using Service.Common.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

string serviceName = "ControllerService";

//注冊DbContext
ServicesInjectionHelper.InjectDbContext(builder, null, serviceName, typeof(MainDBConnectionManager));

// 設定Serilog
ServicesInjectionHelper.InitialzeSerilogSettings(builder, serviceName);

//JWT設定
ServicesInjectionHelper.InitializeApiServicesAndSecurity(builder);

//注冊Dispatcher
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
builder.Services.AddScoped<IQueryDispatcher>(sp =>
{
    var handler = sp.GetRequiredService<IQueryHandler>();
    var dispatcher = new QueryDispatcher();
    dispatcher.RegisterHandler<GetControllersQuery>(handler.HandleAsync);
    dispatcher.RegisterHandler<GetControllerByIdQuery>(handler.HandleAsync);
    return dispatcher;
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});
builder.Services.AddAutoMapper(typeof(ControllerProfile));

//注冊Filter
builder.Services.AddTransient<ActionRoleFilter>();

var app = builder.Build();

//注冊middleware
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
