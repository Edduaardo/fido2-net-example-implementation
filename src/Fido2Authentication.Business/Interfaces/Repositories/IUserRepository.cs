using Fido2Authentication.Business.Models;

namespace Fido2Authentication.Business.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByPasskeyIdAsync(Guid passkeyId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByPasskeyCredentialIdAsync(
        byte[] passkeyCredentialId,
        CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByCredentialIdAsync(
        byte[] credentialId,
        CancellationToken cancellationToken = default);
}
