using System.Text;
using DotNetTask.Api.Middleware;
using DotNetTask.Application.Common.Extensions;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Constants.Settings;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.PostgreSQL;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString(CommonPolicy.DataBaseConnectionString)
    ?? throw new InvalidOperationException(CommonPolicy.MissingConnectionStringMessage);

Dictionary<string, ColumnWriterBase> columnWriters = new()
{
    { "Timestamp", new TimestampColumnWriter() },
    { "Level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
    { "Message", new RenderedMessageColumnWriter() },
    { "Exception", new ExceptionColumnWriter() },
    { "Context", new PropertiesColumnWriter(NpgsqlTypes.NpgsqlDbType.Text, null) },
};

// Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: connectionString,
        tableName: "Logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true)
    .CreateLogger();

builder.Logging.ClearProviders();

builder.Host.UseSerilog();

// DI
builder.Services.AddProblemDetails();

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

builder.Services.AddSingleton(resolver =>
   resolver.GetRequiredService<IOptions<AuthSettings>>().Value);

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddSingleton(resolver =>
   resolver.GetRequiredService<IOptions<TokenSettings>>().Value);

builder.Services.Configure<UserSettings>(builder.Configuration.GetSection("Users"));

builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<UserSettings>>().Value);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<UserSecurityMiddleware>();

app.MapControllers();

app.Run();
