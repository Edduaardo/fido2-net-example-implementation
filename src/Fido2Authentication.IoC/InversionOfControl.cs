using Fido2Authentication.Business.Interfaces;
using Fido2Authentication.Business.Interfaces.Repositories;
using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Business.Services;
using Fido2Authentication.Infrastructure;
using Fido2Authentication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fido2Authentication.IoC;

public static class InversionOfControl
{
    public static void RegisterServices(this IHostApplicationBuilder hostApplicationBuilder)
    {
        hostApplicationBuilder.Services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlite("Data Source=Database.db"));

        hostApplicationBuilder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        hostApplicationBuilder.Services.AddTransient<IUserRepository, UserRepository>();

        hostApplicationBuilder.Services.AddTransient<ITokenService, TokenService>();
        hostApplicationBuilder.Services.AddTransient<IPasswordHelper, PasswordHelper>();

        hostApplicationBuilder.Services.AddTransient<IUserService, UserService>();
        hostApplicationBuilder.Services.AddTransient<IPasskeyService, PasskeyService>();
    }
}
