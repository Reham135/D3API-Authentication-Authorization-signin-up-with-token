using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using D3API.Data.Models;

namespace D3API.Data.Context;

public class CompanyContext:IdentityDbContext<AppUser>
{
    public CompanyContext(DbContextOptions options):base(options)
    {
        
    }
}
