using System.ComponentModel.DataAnnotations;

namespace Fido2Authentication.Web.Models;

public class LoginViewModel
{
    [Required]
    public string Login { get; set; }

    [Required]
    public string Password { get; set; }

    public string? ReturnUrl { get; set; }
}
