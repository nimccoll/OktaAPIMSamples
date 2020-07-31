using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OktaAPIMCore.Models;

namespace OktaAPIMCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _client = new HttpClient();

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        public IActionResult Index()
        {
            string accessToken = User.Claims.FirstOrDefault(c => c.Type == "AccessToken").Value;
            ViewBag.User = User.Identity.Name;
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration.GetValue<string>("APISubscriptionKey"));
            HttpResponseMessage echoResult = _client.GetAsync(_configuration.GetValue<string>("APIUrl")).Result;
            if (echoResult.IsSuccessStatusCode)
            {
                ViewBag.Message = $"Successfully called the ECHO API at {_configuration.GetValue<string>("APIURL")}";
                dynamic echoData = JsonConvert.DeserializeObject(echoResult.Content.ReadAsStringAsync().Result);
            }
            else
            {
                ViewBag.Message = $"Call to the ECHO API failed with a status code of {echoResult.StatusCode}";
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
