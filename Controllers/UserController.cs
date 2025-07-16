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
        return View(booking);
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

    [HttpPost]
    public IActionResult EditBooking(BookingFormModel updatedBooking)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "You must be logged in to update a booking.";
            return RedirectToAction("Login");
        }
        var booking = BookingService.Instance.GetBookingById(updatedBooking.BookingId);
        if (booking == null || booking.Username != username)
        {
            TempData["Error"] = "Booking not found or you do not have permission to update it.";
            return RedirectToAction("MyBookings");
        }
        // Update allowed fields
        booking.CheckInDate = updatedBooking.CheckInDate;
        booking.CheckOutDate = updatedBooking.CheckOutDate;
        booking.Note = updatedBooking.Note;
        booking.NumberOfRooms = updatedBooking.NumberOfRooms;
        booking.RoomType = updatedBooking.RoomType;
        BookingService.Instance.UpdateBooking(booking);
        TempData["Success"] = "Booking updated successfully.";
        return RedirectToAction("MyBookings");
    }
}
