using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using cloudscribe.Web.Pagination;
using IdServerDemo.Models;
using IdServerDemo.ViewModels;
using IdServerDemo.ViewModels.Account;


namespace IdServerDemo.Controllers
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class UserController : Controller
    {
        private const int DefaultPageSize = 12;
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

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /<controller>/
        public IActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "userName_desc" : "";
            ViewBag.AddressSortParm = sortOrder == "State" ? "state_desc" : "State";
            ViewBag.EmailSortParm = sortOrder == "Email" ? "email_desc" : "Email";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            IEnumerable<ApplicationUser> users = from s in _userManager.Users.ToList()
                                                 select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                var _users = users.Where(s => s.UserName.ToUpper().Contains(searchString.ToUpper())
                || s.Email.ToUpper().Contains(searchString.ToUpper())
                );

                if (_users != null)
                {
                    return View(_users.ToPagedList(currentPageIndex, DefaultPageSize));
                }
            }

            switch (sortOrder)
            {
                case "userName_desc":
                    users = _userManager.Users.OrderByDescending(s => s.UserName);
                    break;
                case "Email":
                    users = _userManager.Users.OrderBy(s => s.Email);
                    break;
                case "email_desc":
                    users = _userManager.Users.OrderByDescending(s => s.Email);
                    break;
                case "State":
                    users = _userManager.Users.OrderBy(s => s.State);
                    break;
                case "state_desc":
                    users = _userManager.Users.OrderByDescending(s => s.State);
                    break;
                default:
                    users = _userManager.Users.OrderBy(s => s.UserName);
                    break;
            }

            //var users = _userManager.Users.ToList();
            return View(users.ToPagedList(currentPageIndex, DefaultPageSize));
        }

        //
        // GET: /Users/Edit/1
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            IList<string> userRoles = await _userManager.GetRolesAsync(user);

            return View(new EditUserViewModel()
            {
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                Address = user.Address,
                City = user.City,
                State = user.State,
                Country = user.Country,
                PostalCode = user.PostalCode,
                RolesList = _roleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel editUser, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.UserName = editUser.UserName;
                user.Email = editUser.Email;
                user.EmailConfirmed = editUser.EmailConfirmed;
                user.FirstName = editUser.FirstName;
                user.LastName = editUser.LastName;
                user.BirthDate = editUser.BirthDate;
                user.Address = editUser.Address;
                user.City = editUser.City;
                user.State = editUser.State;
                user.Country = editUser.Country;
                user.PostalCode = editUser.PostalCode;

                IList<string> userRoles = await _userManager.GetRolesAsync(user);

                selectedRole = selectedRole ?? new string[] { };

                IdentityResult result = _userManager.AddToRolesAsync(user, selectedRole.Except(userRoles).ToArray<string>()).Result;

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View();
                }
                result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View();
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "Something_failed_");
            return View();
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            ViewBag.RoleNames = _userManager.GetRolesAsync(user).Result;

            return View(user);
        }

        //
        // GET: /Users/Create
        public IActionResult Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
            return View();
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = userViewModel.UserName,
                    Email = userViewModel.Email,
                    EmailConfirmed = userViewModel.EmailConfirmed,
                    FirstName = userViewModel.FirstName,
                    LastName = userViewModel.LastName,
                    BirthDate = userViewModel.BirthDate,
                    Address = userViewModel.Address,
                    City = userViewModel.City,
                    State = userViewModel.State,
                    Country = userViewModel.Country,
                    PostalCode = userViewModel.PostalCode,
                    Joindate = DateTime.Now,
                    EmailLinkDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,

                };
                IdentityResult adminresult = await _userManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        IdentityResult result = _userManager.AddToRolesAsync(user, selectedRoles).Result;
                        if (!result.Succeeded)
                        {
                            AddErrors(result);

                            ViewBag.RoleId = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
                            return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, adminresult.Errors.First().ToString());
                    ViewBag.RoleId = new SelectList(_roleManager.Roles, "Name", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        //
        // GET: /Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

    }
}
