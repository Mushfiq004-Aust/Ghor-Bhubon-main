using Ghor_Bhubon.Data;
using Ghor_Bhubon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Ghor_Bhubon.Controllers
{
    public class TenantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private object db;

        public TenantController(ApplicationDbContext context) => _context = context;


        public IActionResult OwnedProperty()
        {
            var flats = _context.Flats?.ToList() ?? new List<Flat>(); // Ensure it's never null
            return View(flats);
        }



        public IActionResult ViewProperty(int id)
        {
            var flat = _context.Flats.FirstOrDefault(f => f.FlatID == id);
            if (flat == null)
            {
                return NotFound();
            }

            Console.WriteLine("Property Image Paths: " + flat.ImagePaths); // Debugging

            return View(flat);
        }

        public IActionResult TenantDashboard()
        {
            {
                int tenantId = 1; // Currently logged-in tenant ID (replace with actual logged-in user ID)

                var rentedProperties = db.Transactions
                    .Where(t => t.UserID == tenantId && t.Status == "Approved")
                    .Select(t => t.Flat)
                    .ToList();

                return View(rentedProperties);
            }
        }


    }
}
