using Fido2Authentication.Business.Interfaces.Repositories;
using Fido2Authentication.Business.Models;
using Microsoft.EntityFrameworkCore;

namespace Fido2Authentication.Infrastructure.Repositories;

public class UserRepository(DatabaseContext databaseContext) : IUserRepository
{
    private readonly DatabaseContext _databaseContext = databaseContext;

    public async Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Users
            .Include(x => x.Passkeys)
            .FirstOrDefaultAsync(
                x => x.Email == email,
                cancellationToken
            );
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetUserByPasskeyIdAsync(
        Guid passkeyId,
        CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Users
            .Include(x => x.Passkeys)
            .Where(x => x.Passkeys.Any(x => x.Id == passkeyId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetUserByPasskeyCredentialIdAsync(
        byte[] passkeyCredentialId,
        CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Users
            .Include(x => x.Passkeys)
            .Where(x => x.Passkeys.Any(y => y.CredentialId.SequenceEqual(passkeyCredentialId)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _databaseContext.Users.AddAsync(user);
        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        _databaseContext.Attach(user);
        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByCredentialIdAsync(
        byte[] credentialId,
        CancellationToken cancellationToken = default)
    {
        return (await _databaseContext.Passkeys
            .Include(x => x.User)
            .Where(x => x.CredentialId.SequenceEqual(credentialId))
            .ToListAsync(cancellationToken)
        )
        .Select(x => x.User);
    }
}
