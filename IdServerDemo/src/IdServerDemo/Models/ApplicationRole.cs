using Microsoft.AspNet.Identity.EntityFramework;

namespace IdServerDemo.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }

        public string Description { get; set; }
    }
}
