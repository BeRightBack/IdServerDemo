﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdSvrMvcClientDemo.Controllers
{
    public class TestController : Controller
    {
        string api1 = Startup.Configuration["Api1"];

        [HttpGet("~/Test")]
        public ActionResult Index()
        {
            return View("Index");
        }

        [Authorize, HttpPost("~/Test")]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, api1);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return View("Index", model: await response.Content.ReadAsStringAsync());
            }
        }

        protected string AccessToken
        {
            get
            {
                var claim = HttpContext.User?.FindFirst("access_token");
                if (claim == null)
                {
                    throw new InvalidOperationException();
                }

                return claim.Value;
            }
        }
    }
}
