using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdServerDemo.Models
{
    public class SeedData
    {
        private static ApplicationDbContext _context;
        private static RoleManager<ApplicationRole> _roleManager;
        private static UserManager<ApplicationUser> _userManager;

        public SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task EnsureSeedDataAsync()
        {         

            

            if (_context.Database == null)
            {
                throw new Exception("DB is null");
            }

            if (_context.Roles.Any())
            {
                return;    // DB has been seeded
            }
            _context.Roles.Add(
                 new ApplicationRole()
                 {
                     Name = "Administrators",
                     Description = "most powerfull"
                 });
            _context.SaveChanges();
            _context.Roles.Add(
                 new ApplicationRole()
                 {
                     Name = "SuperUsers",
                     Description = "Also powerfull"
                 });
            _context.SaveChanges();
            _context.Roles.Add(
                 new ApplicationRole()
                 {
                     Name = "Moderators",
                     Description = "Can grant or removes previleges to some users"
                 });
            _context.SaveChanges();
            _context.Roles.Add(
                 new ApplicationRole()
                 {
                     Name = "AdvancedUsers",
                     Description = "VIP"
                 });
            _context.SaveChanges();
            _context.Roles.Add(
                 new ApplicationRole()
                 {
                     Name = "Users",
                     Description = "Able to navigate to some protected area when authenticated"
                 });
            _context.SaveChanges();

            var pass = "?DemoPassword123";
            var user = new ApplicationUser { UserName = "demoadmin", Email = "sgpconcept@example.com" };
            user.UserName = "DemoAdmin";
            user.Email = "sgpconcept.com@example.com";
            user.FirstName = "Steven";
            user.LastName = "Pinel";
            user.BirthDate = DateTime.Parse("12/09/1965");
            user.Country = "Canada";
            user.EmailConfirmed = true;
            
            var result = _userManager.CreateAsync(user, pass);

            ApplicationUser _user = _context.Users.Where(u => u.UserName.Equals("DemoAdmin", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            
                await _userManager.AddToRoleAsync(_user, "Administrators");
            
            //newUser = new ApplicationUser
            //{
            //    UserName = "DemoUser",
            //    Email = "sgpconcept.org@example.com",
            //    FirstName = "Steven",
            //    LastName = "Pinel",
            //    BirthDate = DateTime.Parse("12/09/1965"),
            //    Country = "Canada",
            //    EmailConfirmed = true
            //};

            // result = await userManager.CreateAsync(newUser, "DemoPassword");
            //if (result.Succeeded)
            //{
            //    await userManager.AddToRoleAsync(newUser, "Users");
            //}
        
        }
    }
}
