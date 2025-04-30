using backend.Data;
using backend.Repositories.Interfaces;
using backend.Repositories.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using middlewares.ExceptionMiddleware;
using dotenv.net;
using Microsoft.Extensions.Logging;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // Método para configurar serviços
    public void ConfigureServices(IServiceCollection services)
    {
        DotEnv.Load();

        var server = Environment.GetEnvironmentVariable("PG_HOST")?.Trim();
        var database = Environment.GetEnvironmentVariable("PG_DB_NAME")?.Trim();
        var user = Environment.GetEnvironmentVariable("PG_USER")?.Trim();
        var password = Environment.GetEnvironmentVariable("PG_PASSWORD")?.Trim();
        var port = Environment.GetEnvironmentVariable("PG_PORT")?.Trim();

        var connectionString = $"Server={server};Database={database};User Id={user};Password={password};Port={port}";

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
        );

        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // 🔥 Configuração de CORS (liberando tudo)
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddControllers();
        services.AddSwaggerGen();
    }

    // Método para configurar o pipeline HTTP
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document API v1");
            });
        }

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseRouting();

        // 🔥 Ativa o CORS
        app.UseCors("AllowAll");

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
