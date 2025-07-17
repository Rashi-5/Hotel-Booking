using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Helper;
using HotelBookingSystem.Services;

public class AdminController : Controller
{
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin123";

    // GET: /Admin/Privacy
    public IActionResult Privacy()
    {
        var isLoggedIn = HttpContext.Session.GetString("IsAdminLoggedIn");
        var rememberMe = HttpContext.Session.GetString("RememberMe");
        var loginTime = HttpContext.Session.GetString("LoginTime");

        // If already logged in, redirect to dashboard
        if (isLoggedIn == "true")
        {
            // If Remember Me is active, check if login is still valid (7 days)
            if (rememberMe == "true" && !string.IsNullOrEmpty(loginTime))
            {
                var loginDateTime = DateTime.Parse(loginTime);
                var daysSinceLogin = (DateTime.UtcNow - loginDateTime).TotalDays;
                if (daysSinceLogin <= 7)
                {
                    return RedirectToAction("Admin");
                }
            }
            // If not Remember Me, or still within 7 days, redirect
            else if (rememberMe != "true")
            {
                return RedirectToAction("Admin");
            }
        }
        return View();
    }

    // POST: /Admin/Privacy
    [HttpPost]
    public IActionResult Authenticate(string username, string password, bool rememberMe = false)
    {
        try
        {
            var authService = new AuthService();
            if (authService.ValidateAdmin(username, password))
        {
            HttpContext.Session.SetString("IsAdminLoggedIn", "true");
                // If "Remember Me" is checked, set a longer session timeout
                if (rememberMe)
                {
                    HttpContext.Session.SetString("RememberMe", "true");
                    // Set session to last for 7 days
                    HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString());
                }
            return RedirectToAction("Admin");
        }
        TempData["Error"] = "Invalid credentials";
            return RedirectToAction("Privacy");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An unexpected error occurred. Please try again.";
            // Optionally log ex.Message
            return RedirectToAction("Privacy");
        }
    }

    // GET: /Admin/Admin (Dashboard)
    public IActionResult Admin()
    {
        var isLoggedIn = HttpContext.Session.GetString("IsAdminLoggedIn");
        var rememberMe = HttpContext.Session.GetString("RememberMe");
        var loginTime = HttpContext.Session.GetString("LoginTime");
        
        // Check if user is logged in
        if (isLoggedIn != "true")
        {
            return RedirectToAction("Privacy");
        }

        // If "Remember Me" is active, check if login is still valid (7 days)
        if (rememberMe == "true" && !string.IsNullOrEmpty(loginTime))
        {
            var loginDateTime = DateTime.Parse(loginTime);
            var daysSinceLogin = (DateTime.UtcNow - loginDateTime).TotalDays;
            
            // If more than 7 days have passed, log out
            if (daysSinceLogin > 7)
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Your session has expired. Please login again.";
                return RedirectToAction("Privacy");
            }
        }

        // Get rooms from RoomService
        var rooms = RoomService.Instance.GetAllRooms();
        return View(rooms);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Privacy");
    }

    [HttpPost]
    public IActionResult CreateRoomType(string roomType, string roomDescription, string[] amenities, string price, bool isUpdate = false, string originalRoomName = "", int NumberOfRooms = 3)
    {
        // Validate price
        if (!decimal.TryParse(price, out decimal priceValue) || priceValue < 1 || priceValue > 10000)
        {
            TempData["Error"] = "Please enter a valid price between $1 and $10,000.";
            return RedirectToAction("Admin");
        }

        var rooms = RoomService.Instance.GetAllRooms();
        if (isUpdate)
        {
            // Update existing room
            var roomToUpdate = rooms.FirstOrDefault(r => r.RoomName == originalRoomName);
            if (roomToUpdate == null)
            {
                TempData["Error"] = "Room type not found!";
                return RedirectToAction("Admin");
            }
            // Check if new name conflicts with existing rooms (excluding current room)
            var conflictingRoom = rooms.FirstOrDefault(r => 
                r.RoomName != originalRoomName && 
                r.RoomName.Equals(roomType, StringComparison.OrdinalIgnoreCase));
            if (conflictingRoom != null)
            {
                TempData["Error"] = $"Room type '{roomType}' already exists!";
                return RedirectToAction("Admin");
            }
            // Update the room
            roomToUpdate.RoomName = roomType;
            roomToUpdate.Description = roomDescription ?? roomToUpdate.Description;
            roomToUpdate.Amenities = amenities?.ToList() ?? roomToUpdate.Amenities;
            roomToUpdate.Price = priceValue.ToString("F2");
            roomToUpdate.NumberOfRooms = NumberOfRooms;
            RoomService.Instance.UpdateRoom(roomToUpdate);
            TempData["Success"] = $"Room type '{originalRoomName}' updated successfully!";
        }
        else
        {
            // Create new room
            var existingRoom = rooms.FirstOrDefault(r => 
                r.RoomName.Equals(roomType, StringComparison.OrdinalIgnoreCase));
            if (existingRoom != null)
            {
                TempData["Error"] = $"Room type '{roomType}' already exists! Please choose a different name.";
                return RedirectToAction("Admin");
            }
            var newRoom = new RoomCardViewModel
        {
                RoomName = roomType,
                ImageUrl = "/images/default.jpg",
                Description = roomDescription ?? $"Comfortable {roomType} room.",
                Amenities = amenities?.ToList() ?? new List<string> { "Wi-Fi", "TV" },
                isDefault = false,
                Price = priceValue.ToString("F2"),
                NumberOfRooms = NumberOfRooms
            };
            RoomService.Instance.AddRoom(newRoom);
            TempData["Success"] = $"Room type '{roomType}' added successfully!";
        }
        return RedirectToAction("Admin");
    }

    [HttpPost]
    public IActionResult DeleteRoomType(string roomName)
    {
        var rooms = RoomService.Instance.GetAllRooms();
        var roomToDelete = rooms.FirstOrDefault(r => r.RoomName == roomName);
        if (roomToDelete == null)
        {
            TempData["Error"] = "Room type not found!";
            return RedirectToAction("Admin");
        }
        if (roomToDelete.isDefault)
        {
            TempData["Error"] = "Cannot delete default room types!";
            return RedirectToAction("Admin");
        }
        RoomService.Instance.DeleteRoom(roomName);
        TempData["Success"] = $"Room type '{roomName}' deleted successfully!";
        return RedirectToAction("Admin");
    }

    [HttpPost]
    public IActionResult DownloadGlobalReport([FromBody] WeekReportRequest request)
    {
        DateTime weekStart;
        if (!DateTime.TryParse(request.WeekStart, out weekStart))
            return BadRequest("Invalid week start date.");

        var weekEnd = weekStart.AddDays(7);
        var bookings = BookingService.Instance.GetAllBookings()
            .Where(b => b.CheckInDate.Date >= weekStart.Date && b.CheckInDate.Date < weekEnd.Date)
            .ToList();

        var pdfBytes = PdfReportHelper.GenerateBookingsReportPdf(bookings, $"All Users (Week of {weekStart:yyyy-MM-dd})");
        return File(pdfBytes, "application/pdf", "WeeklyBookingsReport.pdf");
    }

    public class WeekReportRequest
    {
        public string WeekStart { get; set; }
    }
}

