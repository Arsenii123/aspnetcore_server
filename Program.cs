using mvc.Repositories.Interfaces;
using mvc.Repository;
using Scalar.AspNetCore;

// dotnet add package Swashbuckle.AspNetCore
// dotnet add package Scalar.AspNetCore

namespace mvc
{
    public class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddControllers();

            builder.Services.AddScoped<IEntityRepository<Student>, StudentRepository>();

            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapGet("/", () => "API is running!");

            app.Run();
        }
    }
}
