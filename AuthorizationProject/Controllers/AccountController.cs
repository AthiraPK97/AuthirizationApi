using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly string csvPath = "Users/users.csv";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var lines = System.IO.File.ReadAllLines(csvPath).Skip(1); // skip header

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length == 3)
                {
                    var fileUsername = parts[0].Trim();
                    var filePassword = parts[1].Trim();
                    var role = parts[2].Trim();

                    if (username == fileUsername && password == filePassword)
                    {
                        if (role=="admin")
                        {
                            var Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "admin")
                };
                            var identity = new ClaimsIdentity(Claims, "MyCookieAuth");
                            var principal = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync("MyCookieAuth", principal);

                            return RedirectToAction("AdminPanel", "Account");
                        }

                        else 
                        {
                            var Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "User")
                };
                            var identity = new ClaimsIdentity(Claims, "MyCookieAuth");
                            var principal = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync("MyCookieAuth", principal);

                            return RedirectToAction("Dashboard", "Account");
                        }
                    }
                }
            }
            ViewBag.Message = "Invalid username or password.";
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles ="User")]
        public IActionResult Dashboard()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel()
        {
            return View();
        }

        private UserModel? GetUserFromCsv(string username, string password)
        {
            if (!System.IO.File.Exists(csvPath))
                return null;

            var lines = System.IO.File.ReadAllLines(csvPath);
            foreach (var line in lines.Skip(1)) // Skip header
            {
                var parts = line.Split(',');
                if (parts.Length == 3 &&
                    parts[0] == username &&
                    parts[1] == password)
                {
                    return new UserModel { Username = parts[0], Password = parts[1], Role = parts[2] };
                }
            }
            return null;
        }

        private class UserModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }
    }
}

