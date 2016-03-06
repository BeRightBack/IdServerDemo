using System;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using IdServerDemo.Models;
using IdServerDemo.ViewModels;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdServerDemo.Controllers
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class ClientsController : Controller
    {


        private ApplicationDbContext _context;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;

        }
        //
        // GET: /Roles/
        public IActionResult Index()
        {

            var clients = _context.Applications.ToList();

            return View(clients);

        }

        public ActionResult Create()
        {
            return View();
        }

        // POST: /Clients/Create
        [HttpPost]
        public IActionResult Create(ClientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var app = new Application
                {
                    ApplicationID = model.ApplicationID,
                    DisplayName = model.DisplayName,
                    RedirectUri = model.RedirectUri,
                    LogoutRedirectUri = model.LogoutRedirectUri,
                    Secret = model.Secret
                };

                _context.Applications.Add(app);

                _context.SaveChanges();
                ViewBag.ResultMessage = "Clients created successfully !";
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Clients/Edit/5
        public IActionResult Edit(string applicationID)
        {
            if (applicationID == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            var model = _context.Applications.Where(r => r.ApplicationID.Equals(applicationID, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var obj = new ClientViewModel
            {
                ApplicationID = model.ApplicationID,
                DisplayName = model.DisplayName,
                RedirectUri = model.RedirectUri,
                LogoutRedirectUri = model.LogoutRedirectUri,
                Secret = model.Secret
            };
            return View(obj);
        }

        //
        // POST: /Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ClientViewModel client)
        {

            if (ModelState.IsValid)
            {
                var app = _context.Applications.FirstOrDefault(r => r.ApplicationID == client.ApplicationID);

                app.ApplicationID = client.ApplicationID;
                app.DisplayName = client.DisplayName;
                app.RedirectUri = client.RedirectUri;
                app.LogoutRedirectUri = client.LogoutRedirectUri;
                app.Secret = client.Secret;

                _context.Applications.Update(app);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: /Clients/Delete/5
        public IActionResult Delete(string applicationID)
        {
            if (applicationID == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            Application client = _context.Applications.Where(r => r.ApplicationID.Equals(applicationID, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (client == null)
            {
                return HttpNotFound();
            }
            _context.Applications.Remove(client);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
