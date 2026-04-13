using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Web.ResponseViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fido2Authentication.Web.Controllers;

[Authorize]
public class UserController(IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;

    public IActionResult Index() => View();

    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsersAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return Json(
                new
                {
                    data = (await _userService.GetAllAsync(cancellationToken)).Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Email,
                        x.DateRegistered
                    })
                }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
        }
    }
}