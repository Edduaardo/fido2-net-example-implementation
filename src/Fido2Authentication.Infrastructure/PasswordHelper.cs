using Fido2Authentication.Business.Interfaces;
using Fido2Authentication.Business.Models;
namespace Fido2Authentication.Infrastructure;

public class PasswordHelper : IPasswordHelper
{
    public void HashPassword(User user, string password)
    {
        user.PasswordSalt = BCrypt.Net.BCrypt.GenerateSalt();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, user.PasswordSalt);
    }

    public bool VerifyPassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        //return BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash);
    }
}
