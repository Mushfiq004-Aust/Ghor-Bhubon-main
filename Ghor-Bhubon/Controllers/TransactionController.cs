using Ghor_Bhubon.Data;
using Ghor_Bhubon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class TransactionController : Controller
{
    private readonly ApplicationDbContext _context;

    public TransactionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Handle transaction submission from the Property page
    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> SubmitTransaction(string TransactionID, decimal Amount, int FlatID, int LandlordID, string TransactionType, DateTime TransactionDate, string Status, string Notes)
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        // Create a new Transaction object
        var transaction = new Transaction
        {

            TransactionID = TransactionID,
            Amount = Amount,
            UserID = (int)userId,
            FlatID = FlatID,
            LandlordID = LandlordID,
            TransactionType = TransactionType,
            TransactionDate = TransactionDate,
            Status = Status,
            Notes = Notes
        };

        // Add the transaction to the database
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Transaction submitted successfully!";
        return RedirectToAction("TenantDashboard", "Tenant"); // Redirect after submission (or another page as needed)
    }

}
