using Microsoft.AspNetCore.HttpOverrides;
using mvc.Repositories.Interfaces;
using mvc.Repository;
using Scalar.AspNetCore;

namespace mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Налаштування CORS (дозволяє запити з будь-яких сайтів/доменів)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 2. Налаштування Forwarded Headers для обробки SSL на обраній платформі (Render)
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddControllers();
            builder.Services.AddScoped<IEntityRepository<Student>, StudentRepository>();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Активація Forwarded Headers на самому початку
            app.UseForwardedHeaders();

            // Активація CORS політики (має бути до UseAuthorization та MapControllers)
            app.UseCors("AllowAll");

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.MapGet("/", () => "API is running!");

            app.Run();
        }
    }
}
