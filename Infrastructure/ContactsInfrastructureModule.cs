using AppCore.Authorization;
using AppCore.Interfaces;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Entities;
using Infrastructure.EntityFramework.Repositories;
using Infrastructure.EntityFramework.UnitOfWork;
using Infrastructure.Memory;
using Infrastructure.Seeders;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class ContactsInfrastructureModule
{
    public static IServiceCollection AddContactsEfModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<ICompanyRepository, EfCompanyRepository>();
        services.AddScoped<IPersonRepository, EfPersonRepository>();
        services.AddScoped<IOrganizationRepository, EfOrganizationRepository>();
        services.AddScoped<IContactUnitOfWork, EfContactsUnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDataSeeder, IdentityDbSeeder>();
        services.AddScoped<IDataSeeder, PeopleDbSeeder>();

        services.AddDbContext<ContactsDbContext>(options =>
        {
            if (env.IsEnvironment("Testing"))
            {
                options.UseInMemoryDatabase("ContactsTestDb");
            }
            else
            {
                options.UseSqlite(configuration.GetConnectionString("CrmDb"));
            }
        });

        services.AddIdentity<CrmUser, CrmRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ContactsDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
    
    public static IServiceCollection AddContactsMemoryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPersonRepository, MemoryPersonRepository>();
        services.AddScoped<ICompanyRepository, MemoryCompanyRepository>();
        services.AddScoped<IOrganizationRepository, MemoryOrganizationRepository>();
        services.AddScoped<IContactUnitOfWork, MemoryContactUnitOfWork>();

        return services;
    }

    public static IServiceCollection AddJwt(this IServiceCollection services, JwtSettings jwtOptions)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = jwtOptions.GetSymmetricKey(),
                        ClockSkew = TimeSpan.Zero
                    };
                }
            );

        services.AddAuthorization(options =>
        {
            // Polityki oparte o role
            // metoda RequireRole akceptuje dowolną liczbę parametrów typu string
            options.AddPolicy(CrmPolicies.AdminOnly.ToString(), policy =>
                policy.RequireRole(UserRole.Administrator.ToString()));

            // dodaj politykę dla CrmPolicies.SalesAccess
            // która wymaga użytkownika z jedną z ról: Administrator, SalesManager i Salesperson
            options.AddPolicy(CrmPolicies.SalesAccess.ToString(), policy =>
                policy.RequireRole(
                    UserRole.Administrator.ToString(),
                    UserRole.SalesManager.ToString(),
                    UserRole.Salesperson.ToString()));

            // dodaj politykę dla SalesManagerAccess
            // która wymaga róli Administratora lub SalesManagera
            options.AddPolicy(CrmPolicies.SalesManagerAccess.ToString(), policy =>
                policy.RequireRole(
                    UserRole.Administrator.ToString(),
                    UserRole.SalesManager.ToString()));

            // dodaj politykę dla SuppportAccess
            // która wymaga roli administratora lub SupportAgent'a
            options.AddPolicy(CrmPolicies.SupportAccess.ToString(), policy =>
                policy.RequireRole(
                    UserRole.Administrator.ToString(),
                    UserRole.SupportAgent.ToString()));

            // dodaj politykę ReadOnlyAccess
            // która wymaga dowolnej roli
            options.AddPolicy(CrmPolicies.ReadOnlyAccess.ToString(), policy =>
                policy.RequireAuthenticatedUser());

            // Polityka złożona — wymaga roli i aktywnego konta
            options.AddPolicy(CrmPolicies.ActiveUser.ToString(), policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim("status", SystemUserStatus.Active.ToString()));

            // Polityka oparta o dział
            options.AddPolicy(CrmPolicies.SalesDepartment.ToString(), policy =>
                policy.RequireClaim("department", "Sales"));

            // Domyślna polityka — każdy zalogowany użytkownik
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Polityka fallback — stosowana gdy brak atrybutu [Authorize]
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}