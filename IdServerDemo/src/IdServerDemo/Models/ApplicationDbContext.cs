using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;

namespace IdServerDemo.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public DbSet<ApplicationClient> ApplicationClients { get; set; }
        public DbSet<ApplicationResource> ApplicationResources { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);            
        }
    }
}
