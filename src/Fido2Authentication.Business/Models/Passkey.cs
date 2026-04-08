namespace Fido2Authentication.Business.Models;

public class Passkey
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreationDate { get; set; }
    public Guid ManufactorerGuid { get; set; }
    public uint SignCount { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] CredentialId { get; set; }
    
    public virtual User User { get; set; }

    public void UpdateSignCounter() => SignCount = ++SignCount;
}
