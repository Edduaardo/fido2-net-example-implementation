using Fido2Authentication.Business.Models;
using Microsoft.EntityFrameworkCore;

namespace Fido2Authentication.Infrastructure;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Passkey> Passkeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var defaultUser = new User
        {
            Id = Guid.Parse("45f667ad-a2bc-4ada-9b8f-323641b4fb6c"),
            Name = "Admin",
            Email = "admin@system.com",
            DateRegistered = DateTime.Parse("2026-04-06T10:00:00"),
            PasswordHash = "$2a$11$hVQvIo3SS4pXq2P9OZN9FupoOPgsoQ6Q9fU9leAX0O6TIn/ruKF8i",
            PasswordSalt = "$2a$11$hVQvIo3SS4pXq2P9OZN9Fu"
        };

        //PasswordHelper.HashPassword(defaultUser, "123Abc");

        modelBuilder.Entity<User>().HasData(defaultUser);
    }
}
