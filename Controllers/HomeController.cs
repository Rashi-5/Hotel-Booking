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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
