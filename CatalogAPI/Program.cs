using Infrastructure.Search;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Application.Interfaces;
using Application.Services;
using Domain.Common.Settings;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Cache;
using Infrastructure.Context;
using Infrastructure.Messaging.Consumer;
using Infrastructure.Messaging.Producers;
using Infrastructure.Observability;
using Infrastructure.Nosql;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOptions<ElasticsearchSettings>()
    .Bind(builder.Configuration.GetSection(ElasticsearchSettings.SectionName))
    .Validate(x => Uri.TryCreate(x.Url, UriKind.Absolute, out var uri)
        && (uri.Scheme == "https" || uri.Scheme == "http"), "Elasticsearch URL must use HTTP or HTTPS.")
    .Validate(x => System.Text.RegularExpressions.Regex.IsMatch(x.IndexName, @"^[a-z0-9][a-z0-9_-]*$")
        && x.IndexName.Length <= 255, "Elasticsearch index name must be lowercase and valid.")
    .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey)
        || (string.IsNullOrWhiteSpace(x.Username) && string.IsNullOrEmpty(x.Password))
        || (!string.IsNullOrWhiteSpace(x.Username) && !string.IsNullOrEmpty(x.Password)),
        "Configure both Elasticsearch:Username and Elasticsearch:Password.")
    .ValidateOnStart();
builder.Services.AddHttpClient<IGameSearchIndex, ElasticsearchGameIndex>((services, client) =>
{
    var settings = services.GetRequiredService<IOptions<ElasticsearchSettings>>().Value;
    client.BaseAddress = new Uri(settings.Url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
    if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", settings.ApiKey);
    else if (!string.IsNullOrWhiteSpace(settings.Username))
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{settings.Username}:{settings.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FGC API Catalog",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Autenticação JWT usando Bearer Token.

            Para usar, copie o token recebido no login e cole no campo abaixo.
            O sistema adicionará automaticamente 'Bearer' no início do token.

            Exemplo: se seu token é 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...',
            cole apenas 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...' (sem aspas, sem 'Bearer').",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<FCGDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString));

    options.AddInterceptors(new DatabaseConnectionMetricsInterceptor());
});

DatabaseMetrics.ConfigureMaxPoolSize((int)new MySqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")!).MaximumPoolSize);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "CatalogAPI"))
    .WithMetrics(metrics => metrics
        .AddMeter(DatabaseMetrics.MeterName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        // Buckets em escala de segundos: o padrão do SDK assume durações em milissegundos
        // e deixaria toda query real (sub-segundo) no primeiro bucket, inutilizando o P95/P99.
        .AddView(
            instrumentName: "db_query_duration_seconds",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = [0.001, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
            })
        .AddPrometheusExporter());

// =========================================
// MongoDB (item 4, Fase 3 — persistência poliglota: avaliações de jogos)
// =========================================
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection(MongoSettings.SectionName));

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IGameReviewRepository, GameReviewRepository>();
builder.Services.AddScoped<IGameReviewService, GameReviewService>();

// =========================================
// Redis (item 4, Fase 3 — cache distribuído)
// =========================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "catalog-api:";
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentProcessedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"],
            ushort.Parse(builder.Configuration["RabbitMQ:Port"]!),
            "/",
            h =>
            {
                h.Username(
                    builder.Configuration["RabbitMQ:Username"]);

                h.Password(
                    builder.Configuration["RabbitMQ:Password"]);
            });

        cfg.ReceiveEndpoint(
                builder.Configuration["RabbitMQ:Queues:FCG_Catalog"],
                e =>
                {
                    e.ConfigureConsumer<PaymentProcessedConsumer>(
                        context);

                    e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10)));

                    e.UseInMemoryOutbox();
                });

    });
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ILibrarySevice, LibraryService>();
builder.Services.AddScoped<IOrderPlacedProducer, OrderPlacedProducer>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName);

var jwtConfig = jwtSettings.Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings não configurado.");

var key = Encoding.UTF8.GetBytes(jwtConfig.SecretKey);

builder.Services.Configure<JwtSettings>(jwtSettings);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = jwtConfig.ValidateIssuer,
        ValidIssuer = jwtConfig.Issuer,
        ValidateAudience = jwtConfig.ValidateAudience,
        ValidAudience = jwtConfig.Audience,
        ValidateLifetime = jwtConfig.ValidateLifetime,
        ClockSkew = TimeSpan.FromMinutes(jwtConfig.ClockSkew)
    };
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapPrometheusScrapingEndpoint("/metrics");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FCGDbContext>();

    //db.Database.Migrate();
}

app.Run();
