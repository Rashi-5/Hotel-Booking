using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HotelBookingSystem.Models.Booking;

namespace HotelBookingSystem.Helper
{
    public static class BookingFileHelper
    {
        public static List<BookingFormModel> LoadBookingsFromFile(string filePath)
        {
            var bookings = new List<BookingFormModel>();
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    try
                    {
                        var booking = JsonSerializer.Deserialize<BookingFormModel>(line);
                        if (booking != null)
                            bookings.Add(booking);
                    }
                    catch { }
                }
            }
            return bookings;
        }
    }
}
