using Microsoft.AspNetCore.Identity;

namespace D3API.Data.Models;

public class AppUser:IdentityUser
{
    public string Department { get; set; }=string.Empty;
}
