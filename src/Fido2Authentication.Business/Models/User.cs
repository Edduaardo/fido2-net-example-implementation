namespace Fido2Authentication.Business.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime DateRegistered { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }

    public ICollection<Passkey> Passkeys { get; set; }
}
