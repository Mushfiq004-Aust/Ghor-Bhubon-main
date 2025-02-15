using Ghor_Bhubon.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Ghor_Bhubon.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult AdminDashBoard()
        {
            return View();
        }


        public IActionResult Expenses()
        {
            return View();
        }


        public IActionResult ManageUsers()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult PendingPosts()
        {
            return View();
        }
    }
}



