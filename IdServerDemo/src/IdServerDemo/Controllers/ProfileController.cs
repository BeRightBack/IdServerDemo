﻿using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdServerDemo.Controllers
{
    public class ProfileController : Controller
    {
        // Note: make sure to always specify ActiveAuthenticationSchemes = "oidc-server"
        // or use AutomaticAuthentication = true in the OpenID Connect server middleware options.
        [Authorize(ActiveAuthenticationSchemes = OpenIdConnectServerDefaults.AuthenticationScheme)]
        [HttpGet("/connect/userinfo")]
        public IActionResult Get()
        {
            return Json(new
            {
                sub = User.GetClaim(ClaimTypes.NameIdentifier),
                name = User.GetClaim(ClaimTypes.Name)
            });
        }
    }
}
