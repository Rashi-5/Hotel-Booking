using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using HotelBookingSystem.Services;
using System.IO;
using System.Text.Json;
using HotelBookingSystem.Models.Booking;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class UserController : Controller
{
    // In-memory user list for demo (replace with DB in production)
    private static List<UserModel> Users = new List<UserModel>();

    // GET: /User/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: /User/Register
    [HttpPost]
    public IActionResult Register(UserModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var authService = new AuthService();
        if (!authService.RegisterUser(model, out string error))
        {
            ModelState.AddModelError("Username", error);
            return View(model);
        }
        TempData["Success"] = "Registration successful! Please log in.";
        return RedirectToAction("Login");
    }

    // GET: /User/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /User/Login
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var authService = new AuthService();
        var user = authService.ValidateUser(username, password);
        if (user != null)
        {
            HttpContext.Session.SetString("IsUserLoggedIn", "true");
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Booking", "Home");
        }
        TempData["Error"] = "Invalid username or password.";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult MyBookings()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "You must be logged in to view your bookings.";
            return RedirectToAction("Login");
        }
        var userBookings = BookingService.Instance.GetBookingsByUser(username);
        return View(userBookings);
    }

    [HttpPost]
    public IActionResult DeleteBooking(Guid bookingId)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "You must be logged in to delete a booking.";
            return RedirectToAction("Login");
        }
        var bookingToRemove = BookingService.Instance.GetBookingById(bookingId);
        if (bookingToRemove != null && bookingToRemove.Username == username)
        {
            BookingService.Instance.DeleteBooking(bookingId);
            TempData["Success"] = "Booking deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Booking not found or you do not have permission to delete it.";
        }
        return RedirectToAction("MyBookings");
    }

    [HttpGet]
    public IActionResult EditBooking(Guid bookingId)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "You must be logged in to update a booking.";
            return RedirectToAction("Login");
        }
        
        var booking = BookingService.Instance.GetBookingById(bookingId);
        if (booking == null || booking.Username != username)
        {
            TempData["Error"] = "Booking not found or you do not have permission to edit it.";
            return RedirectToAction("MyBookings");
        }
        
        // Get rooms data same way as Booking() action
        var rooms = RoomService.Instance.GetAllRooms();
        ViewBag.Rooms = rooms;
        
        return View(booking);
    }

    [HttpGet]
    public IActionResult EditBookingPartial(Guid bookingId)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "You must be logged in to update a booking.";
            return RedirectToAction("Login");
        }
        
        var booking = BookingService.Instance.GetBookingById(bookingId);

        if (booking == null || booking.Username != username)
        {
            TempData["Error"] = "Booking not found or you do not have permission to edit it.";
            return RedirectToAction("MyBookings");
        }
        
        // Get rooms data same way as Booking() action
        var rooms = RoomService.Instance.GetAllRooms();
        ViewBag.Rooms = rooms;
        Console.WriteLine($"[DEBUG] EditBookingPartial loaded with {rooms.Count} rooms");
        
        if (rooms != null && rooms.Any())
        {
            foreach (var room in rooms)
            {
                Console.WriteLine($"[DEBUG] Available room: {room.RoomName}");
            }
            Console.WriteLine($"[DEBUG] Looking for room: '{booking.RoomType}'");
        }
        
        return PartialView("_BookingForm", booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitBooking(BookingFormModel model, string frequency, int? interval, string[] days)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "You must be logged in to submit a booking.";
            return RedirectToAction("Login");
        }

        // Validate model, update or create booking as needed
        if (model.BookingId != Guid.Empty)
        {
            // Edit existing booking
            var booking = BookingService.Instance.GetBookingById(model.BookingId);
            if (booking == null || booking.Username != username)
            {
                TempData["Error"] = "Booking not found or you do not have permission to edit it.";
                return RedirectToAction("MyBookings");
            }
            // Update booking fields here...
            booking.CheckIn = model.CheckIn;
            booking.CheckOut = model.CheckOut;
            booking.RoomType = model.RoomType;
            booking.NumberOfRooms = model.NumberOfRooms;
            booking.Note = model.Note;
            booking.Adult = model.Adult;
            booking.Children = model.Children;
            booking.BookingType = model.BookingType;
            booking.Frequency = frequency;
            booking.Interval = interval;
            booking.Days = days?.ToList();
            TempData["Success"] = "Booking updated successfully!";
        }

        return RedirectToAction("MyBookings");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DownloadReport()
    {
        string username = HttpContext.Session.GetString("Username") ?? "Unknown User";
        var bookings = HttpContext.Session.GetObjectFromJson<List<BookingFormModel>>("UserBookings_" + username) ?? new List<BookingFormModel>();

        var pdfBytes = PdfReportHelper.GenerateBookingsReportPdf(bookings, username);

        return File(pdfBytes, "application/pdf", "MyBookingsReport.pdf");
    }

}
