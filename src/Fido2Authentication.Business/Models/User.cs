namespace Fido2Authentication.Business.Models;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime DateRegistered { get; set; }
    public required string PasswordHash { get; set; }

    public ICollection<Passkey> Passkeys { get; set; } = [];

    public static User Default()
        => new()
        {
            Name = string.Empty,
            Email = string.Empty,
            DateRegistered = DateTime.Now,
            PasswordHash = string.Empty
        };
}
