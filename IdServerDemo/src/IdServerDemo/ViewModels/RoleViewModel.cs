using Microsoft.AspNet.Identity.EntityFramework;
namespace IdServerDemo.ViewModels
{
    public class RoleViewModel : IdentityRole<int>
    {
        public string Description { get; set; }
    }
}
