using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;
using Infrastructure.EntityFramework.Entities;
using Infrastructure.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Context;

public class ContactsDbContext : IdentityDbContext<CrmUser, CrmRole, string>
{
    public DbSet<Person> People { get; set; } = default!;
    public DbSet<Company> Companies { get; set; } = default!;
    public DbSet<Organization> Organizations { get; set; } = default!;
    public DbSet<RefreshToken> RefresTokens { get; set; } = default!;

    public ContactsDbContext()
    {
    }

    public ContactsDbContext(DbContextOptions<ContactsDbContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=contacts.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CrmUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.Property(u => u.FullName).HasMaxLength(200);
            entity.Property(u => u.Department).HasMaxLength(100);
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        builder.Entity<CrmRole>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(20);
            entity.Property(r => r.Description).HasMaxLength(200);
        });

        builder.Entity<Contact>(entity =>
        {
            entity.HasDiscriminator<string>("ContactType")
                .HasValue<Person>("Person")
                .HasValue<Company>("Company")
                .HasValue<Organization>("Organization");

            entity.Property(p => p.Email).HasMaxLength(200);
            entity.Property(p => p.Phone).HasMaxLength(20);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.CreatedByUserId).HasMaxLength(450);
            entity.Property(p => p.Status).HasConversion<string>();
            entity.Property<string>("ContactType").HasMaxLength(50);

            entity.Ignore(c => c.Tags);
            entity.Ignore(c => c.Notes);
        });

        builder.Entity<Person>(entity =>
        {
            entity.Property(p => p.FirstName).HasMaxLength(100);
            entity.Property(p => p.LastName).HasMaxLength(100);
            entity.Property(p => p.Position).HasMaxLength(100);
            entity.Property(p => p.BirthDate).HasColumnType("date");
            entity.Property(p => p.Gender).HasConversion<string>();

            entity.HasIndex(p => p.EmployerId);
            entity.HasIndex(p => p.OrganizationId);
        });

        builder.Entity<Company>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200);
            entity.Property(c => c.Nip).HasMaxLength(20);
            entity.Property(c => c.Regon).HasMaxLength(20);

            entity.HasData(
                new Company
                {
                    Id = Guid.Parse("516A34D7-CCFB-4F20-85F3-62BD0F3AF271"),
                    Name = "WSEI",
                    Nip = "1234567890",
                    Regon = "000000000",
                    Phone = "123567123",
                    Email = "biuro@wsei.edu.pl",
                    Status = ContactStatus.Active,
                    CreatedAt = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        });

        builder.Entity<Organization>(entity =>
        {
            entity.Property(o => o.Name).HasMaxLength(200);
            entity.Property(o => o.Type).HasConversion<string>();
        });

        builder.Entity<Person>().HasData(
            new
            {
                Id = Guid.Parse("3D54091D-ABC8-49EC-9590-93AD3ED5458F"),
                FirstName = "Adam",
                LastName = "Nowak",
                Gender = Gender.Male,
                Status = ContactStatus.Active,
                Email = "adam@wsei.edu.pl",
                Phone = "123456789",
                BirthDate = DateTime.Parse("2001-01-11"),
                Position = "Programista",
                CreatedAt = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("B4DCB17C-F875-43F8-9D66-36597895A466"),
                FirstName = "Ewa",
                LastName = "Kowalska",
                Gender = Gender.Female,
                Status = ContactStatus.Blocked,
                Email = "ewa@wsei.edu.pl",
                Phone = "123123123",
                BirthDate = DateTime.Parse("2001-01-11"),
                Position = "Tester",
                CreatedAt = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        builder.Entity<Contact>()
            .OwnsOne(c => c.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.City).HasMaxLength(100);
                address.Property(a => a.PostalCode).HasMaxLength(20);
                address.Property(a => a.Country).HasMaxLength(100);
                address.Property(a => a.Type).HasConversion<string>();

                address.HasData(new
                {
                    ContactId = Guid.Parse("3D54091D-ABC8-49EC-9590-93AD3ED5458F"),
                    Street = "ul. Św. Filipa 17",
                    City = "Kraków",
                    PostalCode = "25-009",
                    Country = "Poland",
                    Type = AddressType.Correspondence
                });
            });

        builder.Entity<CrmRole>().HasData(
            new CrmRole(UserRole.Administrator.ToString())
            {
                Id = "11111111-1111-1111-1111-111111111111",
                Description = "System administrator"
            },
            new CrmRole(UserRole.SalesManager.ToString())
            {
                Id = "22222222-2222-2222-2222-222222222222",
                Description = "Sales manager"
            },
            new CrmRole(UserRole.Salesperson.ToString())
            {
                Id = "33333333-3333-3333-3333-333333333333",
                Description = "Salesperson"
            },
            new CrmRole(UserRole.SupportAgent.ToString())
            {
                Id = "44444444-4444-4444-4444-444444444444",
                Description = "Support agent"
            },
            new CrmRole(UserRole.ReadOnly.ToString())
            {
                Id = "55555555-5555-5555-5555-555555555555",
                Description = "Read only access"
            }
        );

        builder.Entity<CrmUser>().HasData(
            new CrmUser
            {
                Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                UserName = "admin@wsei.edu.pl",
                NormalizedUserName = "ADMIN@WSEI.EDU.PL",
                Email = "admin@wsei.edu.pl",
                NormalizedEmail = "ADMIN@WSEI.EDU.PL",
                EmailConfirmed = true,
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                LockoutEnd = null,
                AccessFailedCount = 0,
                FirstName = "Jan",
                LastName = "Administrator",
                FullName = "Jan Administrator",
                Department = "IT",
                Status = SystemUserStatus.Active,
                CreatedAt = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                ConcurrencyStamp = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                PasswordHash = "AQAAAAIAAYagAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=="
            },
            new CrmUser
            {
                Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                UserName = "sales@wsei.edu.pl",
                NormalizedUserName = "SALES@WSEI.EDU.PL",
                Email = "sales@wsei.edu.pl",
                NormalizedEmail = "SALES@WSEI.EDU.PL",
                EmailConfirmed = true,
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                LockoutEnd = null,
                AccessFailedCount = 0,
                FirstName = "Anna",
                LastName = "Sprzedawca",
                FullName = "Anna Sprzedawca",
                Department = "Sales",
                Status = SystemUserStatus.PendingActivation,
                CreatedAt = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                ConcurrencyStamp = "cccccccc-cccc-cccc-cccc-cccccccccccc",
                PasswordHash = "AQAAAAIAAYagAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=="
            }
        );
    }
}