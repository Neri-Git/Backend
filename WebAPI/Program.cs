using AppCore.Interfaces;
using AppCore.Module;
using Infrastructure;
using Infrastructure.Security;

namespace WebAPI;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddContactsEfModule(builder.Configuration, builder.Environment);
        builder.Services.AddContactsCoreModule(builder.Configuration);

        builder.Services.AddSingleton(new JwtSettings(builder.Configuration));
        builder.Services.AddJwt(new JwtSettings(builder.Configuration));

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            using var scope = app.Services.CreateScope();

            var seeders = scope.ServiceProvider
                .GetServices<IDataSeeder>()
                .OrderBy(s => s.Order);

            foreach (var seeder in seeders)
                await seeder.SeedAsync();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseExceptionHandler();
        app.MapControllers();

        app.Run();
    }
}