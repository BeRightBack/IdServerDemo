using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using IdServerDemo.Models;
using System.Net;
using System.Linq;
using System;
using Microsoft.AspNet.Identity;
using IdServerDemo.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdServerDemo.Controllers
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class ResourceController : Controller
    {
        private ApplicationDbContext _context;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public ResourceController(ApplicationDbContext context)
        {
            _context = context;

        }
        //
        // GET: /Roles/
        public IActionResult Index()
        {

            var resource = _context.ApplicationResources.ToList();

            return View(resource);

        }

        public ActionResult Create()
        {
            return View();
        }

        // POST: /Clients/Create
        [HttpPost]
        public IActionResult Create(ApplicationResourceViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationResource app = new ApplicationResource
                {
                    ResourceUri = model.ResourceUri
                    
                };

                _context.ApplicationResources.Add(app);

                _context.SaveChanges();
                ViewBag.ResultMessage = "Uri created successfully !";
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Clients/Edit/5
        public IActionResult Edit(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            var model = _context.ApplicationResources.Where(r => r.Id.Equals(id)).FirstOrDefault();
            var obj = new ApplicationResourceViewModel
            {
                ResourceUri = model.ResourceUri
            };
            return View(obj);
        }

        //
        // POST: /Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationResourceViewModel model)
        {

            if (ModelState.IsValid)
            {
                var app = _context.ApplicationResources.FirstOrDefault(r => r.Id == model.Id);

                app.ResourceUri = model.ResourceUri;
                

                _context.ApplicationResources.Update(app);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: /Clients/Delete/5
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            ApplicationResource app = _context.ApplicationResources.FirstOrDefault(r => r.Id == id);
            if (app == null)
            {
                return HttpNotFound();
            }
            _context.ApplicationResources.Remove(app);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize(Policy = "API"), HttpGet, Route("api/message")]        
        public IActionResult GetMessage()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return HttpBadRequest();
            }

            // Note: identity is the ClaimsIdentity representing the resource owner
            // and identity.Actor is the identity corresponding to the client
            // application the access token has been issued to (delegation).
            return Content(string.Format(
                CultureInfo.InvariantCulture,
                "{0} has been successfully authenticated via {1}",
                identity.Name, identity.Actor.Name));
        }
    }
}
