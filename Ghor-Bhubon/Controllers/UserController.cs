using Ghor_Bhubon.Data;
using Ghor_Bhubon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ghor_Bhubon.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                if (user.PasswordHash != confirmPassword)
                {
                    ModelState.AddModelError("PasswordHash", "Password and Confirm Password must match.");
                    return View(user);
                }

                user.PasswordHash = HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.Now;

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(user);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            if (!Enum.TryParse(role, out UserRole selectedRole))
            {
                ModelState.AddModelError("", "Invalid role selection.");
                return View();
            }

            if (email == "root") email = "root@gmail.com";

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Role == selectedRole);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Incorrect password.");
                return View();
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);

            switch (user.Role)
            {
                case UserRole.Tenant:
                    return RedirectToAction("TenantDashboard", "Tenant");
                case UserRole.Landlord:
                    return RedirectToAction("Dashboard", "LandLord");
                case UserRole.Admin:
                    return RedirectToAction("AdminDashboard", "Admin");
            }
 
            ModelState.AddModelError("", "Invalid login credentials.");
            return View();  

      
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }

        public void SeedAdminUser()
        {
            if (!_context.Users.Any(u => u.Email == "root@gmail.com"))
            {
                var adminUser = new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "root@gmail.com",
                    Phone = "1234567890",
                    PasswordHash = HashPassword("root"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.Now
                };


                _context.Users.Add(adminUser);
                _context.SaveChanges();
            }
        }

        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Login", "User"); 
            }

            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";


            return RedirectToAction("Login", "User");
        }
    }
}
