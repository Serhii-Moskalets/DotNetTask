using System.Text;
using System.Threading.RateLimiting;
using DotNetTask.Api.Middleware;
using DotNetTask.Application.Common.Extensions;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.PostgreSQL;

Serilog.Debugging.SelfLog.Enable(Console.Error);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString(CommonPolicy.DataBaseConnectionString)
    ?? throw new InvalidOperationException(CommonPolicy.MissingConnectionStringMessage);

string loggingConnectionString = builder.Configuration.GetConnectionString(CommonPolicy.LoggingDatabaseConnectionString)
    ?? throw new InvalidOperationException(CommonPolicy.MissingConnectionStringMessage);

Dictionary<string, ColumnWriterBase> columnWriters = new()
{
    { "Timestamp", new TimestampColumnWriter() },
    { "Level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
    { "Message", new RenderedMessageColumnWriter() },
    { "Exception", new ExceptionColumnWriter() },
    { "Context", new PropertiesColumnWriter(NpgsqlTypes.NpgsqlDbType.Text, null) },
};

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Async(w => w.PostgreSQL(
        connectionString: loggingConnectionString,
        tableName: "Logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true))
    .CreateLogger();

builder.Logging.ClearProviders();

builder.Host.UseSerilog();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        string partitionKey = httpContext.User.Identity?.Name
                             ?? httpContext.Connection.RemoteIpAddress?.ToString()
                             ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
              partitionKey: partitionKey,
              factory: _ => new FixedWindowRateLimiterOptions
              {
                  AutoReplenishment = true,
                  PermitLimit = 150,
                  Window = TimeSpan.FromMinutes(1),
                  QueueLimit = 3,
              });
    });
});

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplicationServices();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    IConfigurationSection jwtSettings = builder.Configuration.GetSection("JwtSettings");
    string? secret = jwtSettings.GetValue<string>("Secret");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
    };
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token in format: Bearer {your_token}",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

WebApplication app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler();

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (_, _, ex) =>
    {
        if (ex is DomainException)
        {
            return Serilog.Events.LogEventLevel.Information;
        }

        return ex != null ? Serilog.Events.LogEventLevel.Error : Serilog.Events.LogEventLevel.Information;
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.UseMiddleware<UserSecurityMiddleware>();

app.MapControllers();

app.Run();
