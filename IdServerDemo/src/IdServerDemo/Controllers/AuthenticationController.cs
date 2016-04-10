using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using IdServerDemo.Models;
using IdServerDemo.Services;
using IdServerDemo.ViewModels.Account;
using IdServerDemo.ViewModels.Manage;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdServerDemo.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;

        }

        //
        // GET: /Authentication/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.UserUpdateSuccess ? "User has been updated."
                : "";

            var user = await GetCurrentUserAsync();
            var model = new ManageUserViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                Address = user.Address,
                City = user.City,
                State = user.State,
                Country = user.Country,
                PostalCode = user.PostalCode
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ManageUserViewModel editUser, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(editUser.Id.ToString());
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.UserName = editUser.UserName;
                user.Email = editUser.Email;
                user.FirstName = editUser.FirstName;
                user.LastName = editUser.LastName;
                user.BirthDate = editUser.BirthDate;
                user.Address = editUser.Address;
                user.City = editUser.City;
                user.State = editUser.State;
                user.Country = editUser.Country;
                user.PostalCode = editUser.PostalCode;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var model = new ManageUserViewModel
                    {
                        HasPassword = await _userManager.HasPasswordAsync(user),
                        Logins = await _userManager.GetLoginsAsync(user),
                        BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        BirthDate = user.BirthDate,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        Country = user.Country,
                        PostalCode = user.PostalCode
                    };
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.UserUpdateSuccess, returnUrl = ViewData["ReturnUrl"] });
                }
            }
            ModelState.AddModelError(string.Empty, "Something_failed_");
            return View();
        }

        //
        // GET: /Authentication/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Authentication/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Require the user to have a confirmed email before they can log on.
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Authentication", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        await _emailSender.SendEmailAsync(user.Email, Startup.Configuration["SmtpUser"], "Confirm your account",
                            "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                        ModelState.AddModelError(string.Empty, "You must have a confirmed email to log in.");
                        return View(model);
                    }
                }
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Authentication/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Authentication/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                // Add the Address properties:
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.BirthDate = model.BirthDate;
                user.Address = model.Address;
                user.City = model.City;
                user.State = model.State;
                user.Country = model.Country;
                user.PostalCode = model.PostalCode;
                user.EmailConfirmed = false;
                user.Joindate = DateTime.Now;
                user.EmailLinkDate = DateTime.Now;
                user.LastLoginDate = DateTime.Now;

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    ApplicationRole role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Users");
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.ToString());
                    }
                    //await _userManager.AddToRoleAsync(user, "Users");
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Authentication", new { userId = user.Id, code = code, returnUrl }, protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email, Startup.Configuration["SmtpUser"], "Confirm your account",
                        "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                    //await _signInManager.SignInAsync(user, isPersistent: false);                   


                    var _Url = returnUrl.ToString() + "Info";
                    return RedirectPermanent(_Url);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Authentication/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Authentication", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        //
        // GET: /Authentication/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        //
        // POST: /Authentication/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (User.IsSignedIn())
            {
                return RedirectToAction(nameof(ManageController.Index), "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.BirthDate = model.BirthDate;
                user.Address = model.Address;
                user.City = model.City;
                user.State = model.State;
                user.Country = model.Country;
                user.PostalCode = model.PostalCode;
                user.EmailConfirmed = true;
                user.Joindate = DateTime.Now;
                user.EmailLinkDate = DateTime.Now;
                user.LastLoginDate = DateTime.Now;

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    ApplicationRole role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Users");
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.ToString());
                    }
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {


                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        //
        // GET: /Authentication/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                var _Url = returnUrl.ToString() + "ConfirmEmail";
                return RedirectPermanent(_Url);
            }
            return View("Error");
        }

        //
        // GET: /Authentication/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Authentication/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Authentication", new { userId = user.Id, code = code, returnUrl }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, Startup.Configuration["SmtpUser"], "Reset Password",
                   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");

                var _Url = returnUrl.ToString() + "ForgotPasswordConfirmation";
                return RedirectPermanent(_Url);
                //return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Authentication/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // GET: /Authentication/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Authentication/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                var _Url = returnUrl.ToString() + "ResetPasswordConfirmation";
                return RedirectPermanent(_Url);
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                var _Url = returnUrl.ToString() + "ResetPasswordConfirmation";
                return RedirectPermanent(_Url);
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Authentication/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Authentication/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        //
        // GET: /Authentication/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Authentication/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Authentication/SetPassword
        [HttpGet]
        public IActionResult SetPassword(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Authentication/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //GET: /Authentication/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await _userManager.GetLoginsAsync(user);
            var otherLogins = _signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Authentication/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action("LinkLoginCallback", "Authentication");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, User.GetUserId());
            return new ChallengeResult(provider, properties);
        }

        //
        // GET: /Authentication/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(User.GetUserId());
            if (info == null)
            {
                return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }
            var result = await _userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            UserUpdateSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.FindByIdAsync(HttpContext.User.GetUserId());
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion


    }
}
