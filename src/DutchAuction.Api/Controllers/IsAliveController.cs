using System;
using DutchAuction.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace DutchAuction.Api.Controllers
{
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        [HttpGet]
        public IsAliveResponse Get()
        {
            return new IsAliveResponse
            {
                Version = PlatformServices.Default.Application.ApplicationVersion,
                Env = Environment.GetEnvironmentVariable("Env")
            };
        }
    }
}