using System.ComponentModel.DataAnnotations;
using Fido2Authentication.Web.Attributes.ValidationAttributes;

namespace Fido2Authentication.Web.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "This field is not optional.")]
    [MaxLength(16)]
    public string CurrentPassword { get; set; }
    
    [Required(ErrorMessage = "This field is not optional.")]
    [MaxLength(16)]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "This field is not optional.")]
    [MaxLength(16)]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; }
}
