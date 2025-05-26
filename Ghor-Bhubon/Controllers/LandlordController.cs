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
using Microsoft.AspNetCore.SignalR;
using Ghor_Bhubon.Hubs;

namespace Ghor_Bhubon.Controllers
{
    public class LandlordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<PendingPostHub> _pendingPostHub;


        public LandlordController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IHubContext<PendingPostHub> pendingPostHub)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _pendingPostHub = pendingPostHub;
        }
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }



        

        public IActionResult Dashboard()
        {
            var landlordId = HttpContext.Session.GetInt32("UserID");

            if (landlordId.HasValue)
            {
                var flats = _context.Flats.Where(f => f.UserID == landlordId.Value).ToList();
                return View(flats);
            }

            return RedirectToAction("Login", "User");
        }

        public IActionResult AddProperty()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProperty(Flat flat, string uploadedImages, IFormFile PropertyDocument)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserID");

                if (userId.HasValue)
                {
                    // Retrieve User details from database
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId.Value);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "User not found.");
                        return View(flat);
                    }

                    // Handle PDF upload
                    string? pdfFilePath = null;
                    if (PropertyDocument != null && PropertyDocument.Length > 0)
                    {
                        string pdfFolder = Path.Combine(_webHostEnvironment.WebRootPath, "property_docs");
                        if (!Directory.Exists(pdfFolder))
                        {
                            Directory.CreateDirectory(pdfFolder);
                        }

                        string uniquePdfName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PropertyDocument.FileName);
                        string pdfPath = Path.Combine(pdfFolder, uniquePdfName);

                        using (var stream = new FileStream(pdfPath, FileMode.Create))
                        {
                            await PropertyDocument.CopyToAsync(stream); // Use async method
                        }

                        pdfFilePath = "/property_docs/" + uniquePdfName;
                    }

                    // Save to PropertyPending table
                    var propertyPending = new PropertyPending
                    {
                        UserID = userId.Value,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Rent = flat.Rent,
                        Location = flat.Location,
                        Latitude = flat.Latitude,
                        Longitude = flat.Longitude,
                        Description = flat.Description,
                        NumberOfRooms = flat.NumberOfRooms,
                        NumberOfBathrooms = flat.NumberOfBathrooms,
                        ImagePaths = uploadedImages,
                        PdfFilePath = pdfFilePath,
                        City = flat.City,
                        Area = flat.Area,
                        AvailableFrom = flat.AvailableFrom
                    };

                    _context.PropertyPending.Add(propertyPending);
                    await _context.SaveChangesAsync(); // Use async method

                    int totalPending = await _context.PropertyPending.CountAsync(); // Use async

                    // Send real-time update to SignalR clients
                    await _pendingPostHub.Clients.All.SendAsync("ReceivePendingPostUpdate", totalPending);

                    return RedirectToAction("Dashboard");
                }

                ModelState.AddModelError("", "User not found.");
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Log errors if needed
                }
            }

            return View(flat);
        }




        [HttpPost]
        public IActionResult AddSingleImage(IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                // Ensure the upload folder exists
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }


                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Image.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);


                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(fileStream);
                }


                return Json(new { success = true, imagePath = "/uploads/" + uniqueFileName });
            }

            return Json(new { success = false, error = "Invalid file upload" });
        }




        public IActionResult PropertyDetails(int id)
        {
            var flat = _context.Flats.FirstOrDefault(f => f.FlatID == id);
            if (flat == null)
            {
                return NotFound();
            }

            Console.WriteLine("Property Image Paths: " + flat.ImagePaths); // Debugging

            return View(flat);
        }

        [HttpPost]
        public IActionResult DeleteProperty(int flatId)
        {
            var flat = _context.Flats.FirstOrDefault(f => f.FlatID == flatId);

            if (flat != null)
            {
                if (!string.IsNullOrEmpty(flat.ImagePaths))
                {
                    var imagePaths = flat.ImagePaths.Split(',');
                    foreach (var imagePath in imagePaths)
                    {
                        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }

                _context.Flats.Remove(flat);
                _context.SaveChanges();

                return RedirectToAction("Dashboard");
            }

            return NotFound();
        }


        public IActionResult EditProperty(int id)
        {
            var flat = _context.Flats.FirstOrDefault(f => f.FlatID == id);

            if (flat == null)
            {
                return NotFound();
            }

            return View(flat);
        }


        [HttpPost]
        public IActionResult SaveEditedProperty(Flat flat, List<IFormFile> NewImages, List<string> deleteImages)
        {
            var existingFlat = _context.Flats.FirstOrDefault(f => f.FlatID == flat.FlatID);

            if (existingFlat != null)
            {
                existingFlat.Location = flat.Location;
                existingFlat.Latitude = flat.Latitude;  // Store Latitude
                existingFlat.Longitude = flat.Longitude; // Store Longitude
                existingFlat.Rent = flat.Rent;
                existingFlat.NumberOfRooms = flat.NumberOfRooms;
                existingFlat.NumberOfBathrooms = flat.NumberOfBathrooms;
                existingFlat.Description = flat.Description;

                // Process image deletions
                if (deleteImages != null && deleteImages.Count > 0)
                {
                    var imageList = existingFlat.ImagePaths.Split(",").ToList();
                    foreach (var img in deleteImages)
                    {
                        imageList.Remove(img);
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    existingFlat.ImagePaths = string.Join(",", imageList);
                }

                // Process new images
                if (NewImages != null && NewImages.Count > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    List<string> uploadedPaths = existingFlat.ImagePaths.Split(",").ToList();
                    foreach (var image in NewImages)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(fileStream);
                        }

                        uploadedPaths.Add("/uploads/" + uniqueFileName);
                    }

                    existingFlat.ImagePaths = string.Join(",", uploadedPaths);
                }

                _context.SaveChanges();
                return RedirectToAction("PropertyDetails", new { id = flat.FlatID }); // Redirect to details page
            }

            return View(flat);
        }


        [HttpGet]
        public IActionResult GetMatchingAreas(string city, string area)
        {
            if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(area))
            {
                return Json(new List<string>());
            }

            // Query Areas based on selected city and area
            var matchingAreas = _context.Areas
                .Where(a => a.City.Name == city && EF.Functions.Like(a.Name, area + "%")) // Case-insensitive match
                .Select(a => a.Name)
                .ToList();

            return Json(matchingAreas);
        }


    }
}
