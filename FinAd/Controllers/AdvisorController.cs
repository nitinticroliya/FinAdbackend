/*using FinAd.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorController : ControllerBase
    {

        [HttpGet("Admins")]
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminsEndpoint()
        {
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser.Address}, you are an {currentUser.Password}");
        }


        [HttpGet("Sellers")]
        [Authorize(Roles = "Seller")]
        public IActionResult SellersEndpoint()
        {
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser.Address}, you are a {currentUser.Password}");
        }

        [HttpGet("AdminsAndSellers")]
        [Authorize(Roles = "Administrator,Seller")]
        public IActionResult AdminsAndSellersEndpoint()
        {
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser.Address}, you are an {currentUser.Password}");
        }

        [HttpGet("Public")]
        public IActionResult Public()
        {
            return Ok("Hi, you're on public property");
        }

        private AdvisorData GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new AdvisorData
                {
                    Name = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    Address = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.StreetAddress)?.Value,
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Password = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Thumbprint)?.Value
                };
            }
            return null;
        }
    }
}*/