using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models.Booking;
using System.IO;
using System.Text.Json;
using HotelBookingSystem.Helper;

namespace HotelBookingSystem.Services
{
    public class BookingService
    {
        private static readonly Lazy<BookingService> _instance = new Lazy<BookingService>(() => new BookingService());
        public static BookingService Instance => _instance.Value;

        private readonly List<BookingFormModel> _bookings = new List<BookingFormModel>();
        private readonly object _lock = new object();
        private readonly string _bookingFilePath = "bookings.json";

        private BookingService()
        {
            // _bookings = BookingFileHelper.LoadBookingsFromFile(_bookingFilePath); // Only ChatbotService should read from file
        }

        public void AddBooking(BookingFormModel booking)
        {
            lock (_lock)
            {
                _bookings.Add(booking);
                AppendBookingToFile(booking);
            }
        }

        private void AppendBookingToFile(BookingFormModel booking)
        {
            var json = JsonSerializer.Serialize(booking);
            File.AppendAllText(_bookingFilePath, json + Environment.NewLine);
        }

        public List<BookingFormModel> GetAllBookings()
        {
            lock (_lock)
            {
                return _bookings.ToList();
            }
        }

        public List<BookingFormModel> GetBookingsByUser(string username)
        {
            lock (_lock)
            {
                return _bookings.Where(b => b.Username == username).ToList();
            }
        }

        public BookingFormModel GetBookingById(Guid bookingId)
        {
            lock (_lock)
            {
                return _bookings.FirstOrDefault(b => b.BookingId == bookingId);
            }
        }

        public void UpdateBooking(BookingFormModel updatedBooking)
        {
            lock (_lock)
            {
                var index = _bookings.FindIndex(b => b.BookingId == updatedBooking.BookingId);
                if (index >= 0)
                {
                    _bookings[index] = updatedBooking;
                }
            }
        }

        public void DeleteBooking(Guid bookingId)
        {
            lock (_lock)
            {
                var booking = _bookings.FirstOrDefault(b => b.BookingId == bookingId);
                if (booking != null)
                {
                    _bookings.Remove(booking);
                }
            }
        }
    }
} 