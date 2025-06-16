using System.ComponentModel.DataAnnotations;

namespace T3H.Poll.WebApi.Models.Users;

public class LoginModel
{
    [Required]
    public string userName { get; set; }
    [Required]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}