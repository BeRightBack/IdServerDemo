using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdSvrMvcClientDemo.Controllers
{
    public class AccountController : Controller
    {
        string returnUrl = Startup.Configuration["Account"];
        string BaseUrl = Startup.Configuration["Authority"];

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        // GET: /<controller>/
        public IActionResult Register()
        {
            var _Url = BaseUrl.ToString() + "Authentication/Register?returnurl=" + returnUrl.ToString();
            return RedirectPermanent(_Url);
        }

        public IActionResult ForgotPassword()
        {
            var _Url = BaseUrl.ToString() + "Authentication/ForgotPassword?returnurl=" + returnUrl.ToString();
            return RedirectPermanent(_Url);
        }

        public IActionResult Manage()
        {
            var _Url = BaseUrl.ToString() + "Authentication/Index?returnurl=" + returnUrl.ToString();
            return RedirectPermanent(_Url);
        }

        public IActionResult Info()
        {
            ViewBag.Message = "Check your email and confirm your account, you must be confirmed "
                         + "before you can log in.";
            return View();
        }

        public IActionResult ConfirmEmail()
        {
            return View();
        }

        public IActionResult ChangePasswordSuccess()
        {
            ViewBag.Message = "You successfully changed your password ";
            return View("Info");
        }
        public IActionResult SetPasswordSuccess()
        {
            ViewBag.Message = "You successfully set your password ";
            return View("Info");
        }

        public IActionResult ExternalLoginFailure()
        {
            ViewBag.Message = "Unsuccessful login with service.";
            return View("Info");
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            ViewBag.Message = "Please check your email to reset your password.";
            return View("Info");
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public IActionResult Lockout()
        {
            return View();
        }
    }
}
