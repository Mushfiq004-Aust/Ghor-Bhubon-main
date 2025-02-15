using Ghor_Bhubon.Data;
using Ghor_Bhubon.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

                
                user.Role = (UserRole)Enum.Parse(typeof(UserRole), user.Role.ToString());

               
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

        
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password && u.Role == selectedRole);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserID", user.UserID);

                switch (user.Role)
                {
                    case UserRole.Tenant:
                        return RedirectToAction("TenantDashboard", "Home");
                    case UserRole.Landlord:
                        return RedirectToAction("Dashboard", "LandLord");
                    case UserRole.Admin:
                        return RedirectToAction("AdminDashboard", "Admin");
                }
            }

            ModelState.AddModelError("", "Invalid login credentials.");
            return View();
        }



    }
}
