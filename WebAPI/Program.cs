using AppCore.Interfaces;
using Infrastructure.Memory;

namespace WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();

        // Rejestracja repozytorium generycznego
        builder.Services.AddSingleton(typeof(IGenericRepositoryAsync<>), 
            typeof(MemoryGenericRepository<>));

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.Run();
    }
}