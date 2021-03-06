﻿using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using IdServerDemo.Services;
using IdServerDemo.ViewModels;

namespace IdServerDemo.Controllers
{
    public class HomeController : Controller
    {
        IEmailSender _emailSender;

        public HomeController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        [Authorize]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = Startup.Configuration["SmtpUser"];

                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError("", "Could not send email, configuration problem.");
                }

                _emailSender.SendEmailAsync(email, email, $"Contact Page from {model.Name} ({model.Email})", model.Message);
                ModelState.Clear();

                ViewData["Success"] = "Mail Sent. Thanks!";


            }

            return View();

        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
