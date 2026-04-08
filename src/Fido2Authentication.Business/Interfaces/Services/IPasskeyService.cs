using Fido2Authentication.Business.Models;

namespace Fido2Authentication.Business.Interfaces.Services;

public interface IPasskeyService
{
    Task AddPasskeyAsync(Passkey passkey);
}
