using AltWirePoint.DataAccess.Identity;
using System.ComponentModel.DataAnnotations;

namespace AltWirePoint.BusinessLogic.Models.Identity;

public class RegisterRequest
{
    [Required(ErrorMessage = "Person Name can't be blank")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number can't be blank")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number should contain digits only")]
    public string PhoneNumber { get; set; } = string.Empty;


    [Required(ErrorMessage = "Password can't be blank")]
    public string Password { get; set; } = string.Empty;


    [Required(ErrorMessage = "Confirm Password can't be blank")]
    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public static class RegisterRequestExtensions
{
    public static ApplicationUser ToApplicationUser(this RegisterRequest request)
    {
        return new ApplicationUser
        {
            Name = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            UserName = request.Email,
            Role = "User"
        };
    }
}
