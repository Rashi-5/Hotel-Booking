using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Controllers.Helper;

namespace HotelBookingSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Booking()
    {
        // Use the same room data source as AdminController
        var rooms = HttpContext.Session.GetObjectFromJson<List<HotelBookingSystem.Models.Booking.RoomCardViewModel>>("Rooms") ?? RoomDataHelper.GetDefaultRooms();
        ViewBag.Rooms = rooms;
        
        // Debug: Log the count of rooms
        _logger.LogInformation($"Booking page loaded with {rooms.Count} rooms");

        return View();
    }

    public IActionResult Admin()
    {
        
        return View();
    }

    [HttpPost]
    public IActionResult SubmitBooking(string checkIn, string checkOut, int? adults, int? children, int? rooms, 
        string roomType, string bookingType, string frequency, int? interval, string[] days, string note)
    {
        try
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(checkIn) || string.IsNullOrEmpty(checkOut))
            {
                TempData["Error"] = "Check-in and check-out dates are required.";
                return RedirectToAction("Booking");
            }

            var checkInDate = DateTime.Parse(checkIn);
            var checkOutDate = DateTime.Parse(checkOut);
            
            // Set default values for nullable parameters
            adults ??= 1;
            children ??= 0;
            rooms ??= 1;
            interval ??= 1;

            // Load rooms from session or default
            var allRooms = HttpContext.Session.GetObjectFromJson<List<RoomCardViewModel>>("Rooms") ?? RoomDataHelper.GetDefaultRooms();
            var selectedRoom = allRooms.FirstOrDefault(r => r.RoomName == roomType);
            if (selectedRoom == null)
            {
                TempData["Error"] = $"Room type '{roomType}' not found.";
                return RedirectToAction("Booking");
            }

            // Check if enough rooms are available
            if (selectedRoom.NumberOfRooms < rooms.Value)
            {
                TempData["Error"] = $"Not enough rooms available. Only {selectedRoom.NumberOfRooms} left for '{roomType}'.";
                return RedirectToAction("Booking");
            }

            // 1. Load or initialize per-date booking dictionary
            var roomBookings = HttpContext.Session.GetObjectFromJson<Dictionary<string, Dictionary<DateTime, int>>>("RoomBookings")
                ?? new Dictionary<string, Dictionary<DateTime, int>>();

            // 2. Check availability for each date
            List<DateTime> bookingDates = new List<DateTime>();
            
            if (bookingType == "Recurring")
            {
                if (frequency == "Daily")
                {
                    for (var date = checkInDate; date <= checkOutDate; date = date.AddDays(interval.Value))
                    {
                        bookingDates.Add(date);
                    }
                }
                else if (frequency == "Weekly")
                {
                    var selectedDays = new List<DayOfWeek>();
                    if (days != null)
                    {
                        foreach (var day in days)
                        {
                            if (Enum.TryParse<DayOfWeek>(day, out DayOfWeek dayOfWeek))
                            {
                                selectedDays.Add(dayOfWeek);
                            }
                        }
                    }
                    bookingDates = GetRecurringBookingDates(checkInDate, checkOutDate, selectedDays, interval.Value);
                }
                else if (frequency == "Monthly")
                {
                    var date = checkInDate;
                    while (date <= checkOutDate)
                    {
                        bookingDates.Add(date);
                        date = date.AddMonths(interval.Value);
                    }
                }
            }
            else
            {
                bookingDates.Add(checkInDate);
            }
            
            foreach (var date in bookingDates)
            {
                if (!roomBookings.ContainsKey(roomType))
                    roomBookings[roomType] = new Dictionary<DateTime, int>();

                int booked = roomBookings[roomType].ContainsKey(date) ? roomBookings[roomType][date] : 0;
                if (booked + rooms.Value > selectedRoom.NumberOfRooms)
                {
                    TempData["Error"] = $"Not enough rooms available for '{roomType}' on {date:yyyy-MM-dd}.";
                    return RedirectToAction("Booking");
                }
            }

            // 3. Deduct rooms for each date
            foreach (var date in bookingDates)
            {
                if (!roomBookings[roomType].ContainsKey(date))
                    roomBookings[roomType][date] = 0;
                roomBookings[roomType][date] += rooms.Value;
            }

            // 4. Save back to session
            HttpContext.Session.SetObjectAsJson("RoomBookings", roomBookings);
            
            // Log the booking details
            _logger.LogInformation($"Booking submitted: {bookingDates.Count} dates, Room: {roomType}, Guests: {adults} adults, {children} children");
            
            TempData["Success"] = $"Booking submitted successfully! {bookingDates.Count} booking(s) created. {rooms.Value} room(s) deducted from '{roomType}'.";
            return RedirectToAction("Booking");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Booking submission error: {ex.Message}");
            TempData["Error"] = "Error submitting booking. Please try again.";
            return RedirectToAction("Booking");
        }
    }
    
    private static List<DateTime> GetRecurringBookingDates(
        DateTime startDate,
        DateTime endDate,
        List<DayOfWeek> selectedDays,
        int intervalWeeks = 1)
    {
        var result = new List<DateTime>();

        // Find the first Monday (or the earliest selected day) on or after startDate
        DateTime firstWeekStart = startDate.Date;
        // Align to the start of the week (Monday)
        firstWeekStart = firstWeekStart.AddDays(-(int)firstWeekStart.DayOfWeek + (int)DayOfWeek.Monday);
        if (firstWeekStart > startDate) firstWeekStart = startDate;

        for (DateTime weekStart = firstWeekStart; weekStart <= endDate; weekStart = weekStart.AddDays(7 * intervalWeeks))
        {
            foreach (var day in selectedDays)
            {
                var candidate = weekStart.AddDays((int)day - (int)weekStart.DayOfWeek);
                if (candidate < startDate)
                    candidate = candidate.AddDays(7);
                if (candidate >= startDate && candidate <= endDate && !result.Contains(candidate))
                {
                    result.Add(candidate);
                }
            }
        }

        result.Sort();
        return result;
    }

    // Helper: Get next specific day (e.g., next Monday from a date)
    private static DateTime GetNextWeekday(DateTime from, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)from.DayOfWeek + 7) % 7;
        return from.AddDays(daysToAdd);
    }

    [HttpGet]
    public JsonResult CheckRoomAvailability(string roomType, string checkIn, string checkOut, int? rooms)
    {
        var checkInDate = DateTime.Parse(checkIn);
        var checkOutDate = DateTime.Parse(checkOut);
        rooms ??= 1;

        // Get all rooms and per-date bookings
        var allRooms = HttpContext.Session.GetObjectFromJson<List<RoomCardViewModel>>("Rooms") ?? RoomDataHelper.GetDefaultRooms();
        var selectedRoom = allRooms.FirstOrDefault(r => r.RoomName == roomType);
        if (selectedRoom == null)
            return Json(new { available = false, message = "Room type not found." });

        var roomBookings = HttpContext.Session.GetObjectFromJson<Dictionary<string, Dictionary<DateTime, int>>>("RoomBookings")
            ?? new Dictionary<string, Dictionary<DateTime, int>>();

        // Check each date in the range
        for (var date = checkInDate; date <= checkOutDate; date = date.AddDays(1))
        {
            int booked = 0;
            if (roomBookings.ContainsKey(roomType) && roomBookings[roomType].ContainsKey(date))
                booked = roomBookings[roomType][date];

            if (booked + rooms.Value > selectedRoom.NumberOfRooms)
            {
                return Json(new
                {
                    available = false,
                    message = $"Not enough rooms available for '{roomType}' on {date:yyyy-MM-dd}. Only {selectedRoom.NumberOfRooms - booked} left."
                });
            }
        }

        return Json(new { available = true, message = "Rooms are available for the selected dates." });
    }

    [HttpGet]
    public JsonResult GetAllRoomAvailabilities(string checkIn, string checkOut, int? rooms)
    {
        var checkInDate = DateTime.Parse(checkIn);
        var checkOutDate = DateTime.Parse(checkOut);
        rooms ??= 1;

        var allRooms = HttpContext.Session.GetObjectFromJson<List<RoomCardViewModel>>("Rooms") ?? RoomDataHelper.GetDefaultRooms();
        var roomBookings = HttpContext.Session.GetObjectFromJson<Dictionary<string, Dictionary<DateTime, int>>>("RoomBookings")
            ?? new Dictionary<string, Dictionary<DateTime, int>>();

        var result = new List<object>();

        foreach (var room in allRooms)
        {
            int minAvailable = room.NumberOfRooms;
            bool available = true;
            for (var date = checkInDate; date <= checkOutDate; date = date.AddDays(1))
            {
                int booked = 0;
                if (roomBookings.ContainsKey(room.RoomName) && roomBookings[room.RoomName].ContainsKey(date))
                    booked = roomBookings[room.RoomName][date];

                int availableRooms = room.NumberOfRooms - booked;
                if (availableRooms < rooms.Value)
                {
                    available = false;
                    minAvailable = availableRooms;
                }
                if (availableRooms < minAvailable)
                    minAvailable = availableRooms;
            }
            result.Add(new { roomName = room.RoomName, available = minAvailable, isAvailable = available });
        }

        return Json(result);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
