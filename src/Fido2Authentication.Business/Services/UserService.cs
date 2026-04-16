using System.Text;
using Fido2Authentication.Business.Interfaces;
using Fido2Authentication.Business.Interfaces.Repositories;
using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Business.Models;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http;

namespace Fido2Authentication.Business.Services;

public class UserService(
    //IRepository<User> userRepository,
    IUserRepository userRepository,
    IPasswordHelper passwordHelper,
    IFido2 fido2,
    IHttpContextAccessor httpContextAccessor,
    IPasskeyService passkeyService) : IUserService
{
    //private readonly IRepository<User> _userRepository = userRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHelper _passwordHelper = passwordHelper;
    private readonly IFido2 _fido2 = fido2;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IPasskeyService _passkeyService = passkeyService;

    public async Task UpdateAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        //return await _userRepository.FindOneAsync(x => x.Email == email);
        return await _userRepository.GetUserByEmailAsync(email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _userRepository.AddAsync(user, cancellationToken);
    }
    
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
       return await _userRepository.GetAllAsync(cancellationToken);
    }

    public bool VerifyLogin(User user, string password)
    {
        return _passwordHelper.VerifyPassword(user, password);
    }

    public async Task ChangePasswordAsync(
        User user,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        _passwordHelper.HashPassword(user, newPassword);

        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<CredentialCreateOptions> PasskeyGetAttestationOptionsAsync(
        string userEmail,
        bool residentKey = false,
        CancellationToken cancellationToken = default)
    {
        var user = await GetByEmailAsync(userEmail, cancellationToken);

        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = new Fido2User
            {
                Id = Encoding.UTF8.GetBytes(user.Email),
                Name = user.Name,
                DisplayName = $"Display {user.Name}"
            },
            ExcludeCredentials = [.. user.Passkeys.Select(x => new PublicKeyCredentialDescriptor(x.CredentialId))],
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = residentKey ? ResidentKeyRequirement.Preferred : ResidentKeyRequirement.Discouraged,
                UserVerification = UserVerificationRequirement.Preferred
            },
            AttestationPreference = AttestationConveyancePreference.None,
            Extensions = new AuthenticationExtensionsClientInputs
            {
                CredProps = true
            }
        });

        _httpContextAccessor.HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());

        return options;
    }

    public async Task<RegisteredPublicKeyCredential> SavePasskeyAsync(
        AuthenticatorAttestationRawResponse authenticatorAttestationRawResponse,
        string passkeyName,
        CancellationToken cancellationToken = default)
    {
        var jsonOptions = _httpContextAccessor.HttpContext.Session.GetString("fido2.attestationOptions");
        var options = CredentialCreateOptions.FromJson(jsonOptions);

        IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
        {
            var users = await _userRepository.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
            
            if (users.Any()) return false;

            return true;
        };

        var credential = await _fido2.MakeNewCredentialAsync(
            new MakeNewCredentialParams
            {
                AttestationResponse = authenticatorAttestationRawResponse,
                OriginalOptions = options,
                IsCredentialIdUniqueToUserCallback = callback
            },
            cancellationToken
        );

        await _passkeyService.AddPasskeyAsync(
            new Passkey
            {
                Id = Guid.NewGuid(),
                Name = passkeyName,
                CredentialId = credential.Id,
                CreationDate = DateTime.Now,
                ManufacturerGuid = credential.AaGuid.Equals(Guid.Empty) ? null : credential.AaGuid,
                SignCount = credential.SignCount,
                PublicKey = credential.PublicKey,
                User = await GetByEmailAsync(_httpContextAccessor.HttpContext.User.Identity.Name)
            }
        );

        return credential;
    }

    public async Task<AssertionOptions> GetPasskeyAssertionOptionsAsync(
        string? userName = null,
        string? userVerification = null,
        CancellationToken cancellationToken = default)
    {
        User? user = null;
        
        if (!string.IsNullOrEmpty(userName))
            user = await GetByEmailAsync(userName, cancellationToken);

        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams()
        {
            AllowedCredentials = [.. user?.Passkeys.Select(x => new PublicKeyCredentialDescriptor(x.CredentialId)) ?? []],
            UserVerification = userVerification?.ToEnum<UserVerificationRequirement>() ?? UserVerificationRequirement.Discouraged,
            Extensions = new AuthenticationExtensionsClientInputs
            {
                Extensions = true,
                UserVerificationMethod = true
            }
        });

        _httpContextAccessor.HttpContext.Session.SetString("fido2.assertionOptions", options.ToJson());

        return options;
    }

    public async Task<VerifyAssertionResult> MakePasskeyAssertionAsync(
        AuthenticatorAssertionRawResponse authenticatorAssertionRawResponse,
        CancellationToken cancellationToken = default)
    {
        var assertionOptions = _httpContextAccessor.HttpContext.Session.GetString("fido2.assertionOptions");
        //_httpContextAccessor.HttpContext.Session.SetString("fido2.assertionOptions", null);
        
        var user = await _userRepository.GetUserByPasskeyCredentialIdAsync(
            authenticatorAssertionRawResponse.RawId,
            cancellationToken);

        var passkey = user.Passkeys.First(x => x.CredentialId.SequenceEqual(authenticatorAssertionRawResponse.RawId));

        IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
        {
            //var storedCreds = await GetByEmailAsync(Encoding.UTF8.GetString(args.UserHandle), cancellationToken);
            return user.Passkeys.Any(c => c.CredentialId.SequenceEqual(args.CredentialId));
        };

        var assertionResult = await _fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = authenticatorAssertionRawResponse,
                OriginalOptions = AssertionOptions.FromJson(assertionOptions),
                StoredPublicKey = passkey.PublicKey,
                StoredSignatureCounter = passkey.SignCount,
                IsUserHandleOwnerOfCredentialIdCallback = callback
            },
            cancellationToken
        );

        passkey.UpdateSignCounter();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return assertionResult;
    }

    public async Task<User?> GetUserByPasskeyIdAsync(Guid passkeyId, CancellationToken cancellationToken = default)
        => await _userRepository.GetUserByPasskeyIdAsync(passkeyId, cancellationToken);

    public async Task DeleteUserPasskeyAsync(Guid passkeyId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByPasskeyIdAsync(passkeyId, cancellationToken);

        user!.Passkeys.Remove(user.Passkeys.First(x => x.Id == passkeyId));

        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<User?> GetUserByPasskeyCredentialIdAsync(
        byte[] credentialId,
        CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserByPasskeyCredentialIdAsync(
            credentialId,
            cancellationToken
        );
    }
}
