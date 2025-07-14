using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System;

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
        if (Users.Any(u => u.Username == model.Username))
        {
            ModelState.AddModelError("Username", "Username already exists.");
            return View(model);
        }
        Users.Add(model);
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
        var user = Users.FirstOrDefault(u => u.Username == username && u.Password == password);
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
        var userBookings = HttpContext.Session.GetObjectFromJson<List<BookingFormModel>>("UserBookings_" + username) ?? new List<BookingFormModel>();
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
        var userBookings = HttpContext.Session.GetObjectFromJson<List<BookingFormModel>>("UserBookings_" + username) ?? new List<BookingFormModel>();
        var bookingToRemove = userBookings.FirstOrDefault(b => b.BookingId == bookingId);
        if (bookingToRemove != null)
        {
            userBookings.Remove(bookingToRemove);
            HttpContext.Session.SetObjectAsJson("UserBookings_" + username, userBookings);
            TempData["Success"] = "Booking deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Booking not found.";
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
        var userBookings = HttpContext.Session.GetObjectFromJson<List<BookingFormModel>>("UserBookings_" + username) ?? new List<BookingFormModel>();
        var booking = userBookings.FirstOrDefault(b => b.BookingId == bookingId);
        if (booking == null)
        {
            TempData["Error"] = "Booking not found.";
            return RedirectToAction("MyBookings");
        }
        return View(booking);
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
        var userBookings = HttpContext.Session.GetObjectFromJson<List<BookingFormModel>>("UserBookings_" + username) ?? new List<BookingFormModel>();
        var booking = userBookings.FirstOrDefault(b => b.BookingId == updatedBooking.BookingId);
        if (booking == null)
        {
            TempData["Error"] = "Booking not found.";
            return RedirectToAction("MyBookings");
        }
        // Update allowed fields
        booking.CheckInDate = updatedBooking.CheckInDate;
        booking.CheckOutDate = updatedBooking.CheckOutDate;
        booking.Note = updatedBooking.Note;
        booking.NumberOfRooms = updatedBooking.NumberOfRooms;
        booking.RoomType = updatedBooking.RoomType;
        booking.TotalPrice = updatedBooking.TotalPrice;
        HttpContext.Session.SetObjectAsJson("UserBookings_" + username, userBookings);
        TempData["Success"] = "Booking updated successfully.";
        return RedirectToAction("MyBookings");
    }
}
