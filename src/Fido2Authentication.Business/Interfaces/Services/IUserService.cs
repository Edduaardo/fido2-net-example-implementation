using Fido2Authentication.Business.Models;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Fido2Authentication.Business.Interfaces.Services;

public interface IUserService
{
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByPasskeyIdAsync(Guid passkeyId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByPasskeyCredentialIdAsync(
        byte[] passkeyCredentialId,
        CancellationToken cancellationToken = default);
    bool VerifyLogin(User user, string password);
    Task ChangePasswordAsync(User user, string newPassword, CancellationToken cancellationToken = default);
    Task<CredentialCreateOptions> PasskeyGetAttestationOptionsAsync(
        string userEmail,
        bool residentKey = false,
        CancellationToken cancellationToken = default);
    Task<RegisteredPublicKeyCredential> SavePasskeyAsync(
        AuthenticatorAttestationRawResponse authenticatorAttestationRawResponse,
        string passkeyName,
        CancellationToken cancellationToken = default);
    Task<AssertionOptions> GetPasskeyAssertionOptionsAsync(
        string userName,
        string? userVerification = null,
        CancellationToken cancellationToken = default);
    Task<VerifyAssertionResult> MakePasskeyAssertionAsync(
        AuthenticatorAssertionRawResponse authenticatorAssertionRawResponse,
        CancellationToken cancellationToken = default);
    Task DeleteUserPasskeyAsync(Guid passkeyId, CancellationToken cancellationToken = default);
}