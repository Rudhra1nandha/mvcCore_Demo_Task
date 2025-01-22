using Microsoft.EntityFrameworkCore;
using mvccore_dotnet_app.Models;

namespace mvccore_dotnet_app.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
    { }
    public DbSet<UserRole> UserRole { get; set; }
  
}
