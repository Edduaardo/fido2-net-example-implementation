using Fido2Authentication.Business.Interfaces.Repositories;
using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Business.Models;

namespace Fido2Authentication.Business.Services;

public class PasskeyService(
    IRepository<Passkey> passkeyRepository
    ) : IPasskeyService
{
    private readonly IRepository<Passkey> _passkeyRepository = passkeyRepository;

    public async Task AddPasskeyAsync(Passkey passkey)
    {
        await _passkeyRepository.AddAsync(passkey);
    }
}
