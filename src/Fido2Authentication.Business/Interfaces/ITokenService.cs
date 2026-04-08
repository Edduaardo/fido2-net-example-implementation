using Fido2Authentication.Business.Models;

namespace Fido2Authentication.Business.Interfaces;

public interface ITokenService
{
    Task LoginUserAsync(User user);
}
