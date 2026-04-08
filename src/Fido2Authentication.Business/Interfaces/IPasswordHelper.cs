using Fido2Authentication.Business.Models;

namespace Fido2Authentication.Business.Interfaces;

public interface IPasswordHelper
{
    void HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}