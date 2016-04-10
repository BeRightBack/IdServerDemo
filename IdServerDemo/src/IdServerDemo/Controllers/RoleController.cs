using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using IdServerDemo.Models;
using IdServerDemo.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdServerDemo.Controllers
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class RoleController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private ApplicationDbContext _context;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public RoleController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        public IActionResult Index()
        {
            var roles = _context.Roles.ToList();

            return View(roles);
        }
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {


            if (ModelState.IsValid)
            {
                var role = new ApplicationRole();
                role.Name = model.Name;
                role.Description = model.Description;

                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }
            return View(model);
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id.Value);
            if (role == null)
            {
                return HttpNotFound();
            }
            RoleViewModel roleModel = new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name
            };
            roleModel.Description = role.Description;
            return View(roleModel);
        }
        
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == roleModel.Id);
                role.Description = roleModel.Description;
                role.Name = roleModel.Name;
                await _roleManager.UpdateAsync(role);
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            ApplicationRole role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id);

            List<ApplicationUser> users = new List<ApplicationUser>();
            foreach (ApplicationUser user in _userManager.Users.ToList())
            {
                var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                if (isInRole)
                {
                    users.Add(user);
                }
            }

            ViewBag.Users = users;
            ViewBag.UserCount = users.Count();
            return View(role);
        }
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            ApplicationRole role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
                }
                ApplicationRole role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    return HttpNotFound();
                }

                await _roleManager.DeleteAsync(role);

                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult ManageUserRoles()
        {
            var list = _context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleAddToUser(string UserName, string RoleName)
        {
            var user = _context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            ApplicationRole role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == RoleName);


            await _userManager.AddToRoleAsync(user, role.ToString());
            ViewBag.ResultMessage = "Role created successfully !";
            
            var list = _context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            return View("ManageUserRoles");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetRoles(string UserName)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                ApplicationUser user = _context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                var list1 = _userManager.GetRolesAsync(user).Result;
                ViewBag.RolesForThisUser = list1;
                
                var list2 = _context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
                ViewBag.Roles = list2;
            }

            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleForUser(string UserName, string RoleName)
        {

            ApplicationUser user = _context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var isInRole = _userManager.IsInRoleAsync(user, RoleName);
            if (await isInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, RoleName);
                ViewBag.ResultMessage = "Role removed from this user successfully !";
            }
            else
            {
                ViewBag.ResultMessage = "This user doesn't belong to selected role.";
            }
            var list = _context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            return View("ManageUserRoles");
        }

    }
}
