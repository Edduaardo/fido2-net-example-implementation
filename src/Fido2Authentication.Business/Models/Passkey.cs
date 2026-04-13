namespace Fido2Authentication.Business.Models;

public class Passkey
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreationDate { get; set; }
    public Guid? ManufacturerGuid { get; set; }
    public uint SignCount { get; set; }
    public required byte[] PublicKey { get; set; }
    public required byte[] CredentialId { get; set; }
    
    public virtual User User { get; set; } = User.Default();

    public void UpdateSignCounter() => SignCount = ++SignCount;
}
