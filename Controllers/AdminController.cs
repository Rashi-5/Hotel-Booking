using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Controllers.Helper;

public class AdminController : Controller
{
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin123";

    // GET: /Admin/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    // POST: /Admin/Admin
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (username == AdminUsername && password == AdminPassword)
        {
            HttpContext.Session.SetString("IsAdminLoggedIn", "true");
            return RedirectToAction("Admin");
        }

        TempData["Error"] = "Invalid credentials";
        return RedirectToAction("Login");
    }

    // GET: /Admin/Admin (Dashboard)
    public IActionResult Admin()
    {
        if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
        {
            return RedirectToAction("Privacy");
        }

        // Get rooms from session or use default list
        var rooms = HttpContext.Session.GetObjectFromJson<List<RoomCardViewModel>>("Rooms") ?? RoomDataHelper.GetDefaultRooms();

        return View(rooms);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Privacy");
    }

    [HttpPost]
    public IActionResult AddRoomType(string roomName)
    {
        // Get current rooms from session or create new list
        var rooms = HttpContext.Session.GetObjectFromJson<List<RoomCardViewModel>>("Rooms") ?? new List<RoomCardViewModel>();
        
        // Add new room with default values
        rooms.Add(new RoomCardViewModel
        {
            RoomName = roomName,
            ImageUrl = "/images/rooms/default.jpg", // Default image
            Description = $"Comfortable {roomName} room.",
            Amenities = new List<string> { "Wi-Fi", "TV" } // Default amenities
        });

        // Save back to session
        HttpContext.Session.SetObjectAsJson("Rooms", rooms);

        return RedirectToAction("Admin");
    }

}

