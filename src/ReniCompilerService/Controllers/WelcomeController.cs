using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;

namespace ReniCompilerService.Controllers
{
    [Route("welcome")]
    public sealed class WelcomeController : Controller
    {
        [HttpGet]
        public string Get() => "Welcome to reni compiler service.";
    }
}