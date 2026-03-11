using AppCore.Interfaces;
using Infrastructure.Memory;

namespace WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Controllers
        builder.Services.AddControllers();

        // Repozytoria
        builder.Services.AddSingleton<IPersonRepository, MemoryPersonRepository>();
        builder.Services.AddSingleton<ICompanyRepository, MemoryCompanyRepository>();
        builder.Services.AddSingleton<IOrganizationRepository, MemoryOrganizationRepository>();

        // UnitOfWork
        builder.Services.AddSingleton<IContactUnitOfWork, MemoryContactUnitOfWork>();

        // Service
        builder.Services.AddSingleton<IPersonService, MemoryPersonService>();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // REST Controllers
        app.MapControllers();

        app.Run();
    }
}