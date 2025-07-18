using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models.Booking;
using System.IO;
using System.Text.Json;

public class ChatbotService
{
    private readonly string _bookingFilePath = "bookings.json";

    public List<BookingFormModel> GetAllBookings()
    {
        var bookings = new List<BookingFormModel>();
        if (File.Exists(_bookingFilePath))
        {
            var lines = File.ReadAllLines(_bookingFilePath);
            foreach (var line in lines)
            {
                try
                {
                    var booking = JsonSerializer.Deserialize<BookingFormModel>(line);
                    if (booking != null)
                        bookings.Add(booking);
                }
                catch
                {
                    // Could log the bad line for debugging
                }
            }
        }
        return bookings;
    }

    public string GetBotResponse(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return "Please enter a message.";

        string msg = userMessage.Trim().ToLower();

        if (msg == "check room availability")
            return GetWeeklyAvailability();

        if (msg == "i want to book a room")
            return "Great! You can book a room as a one-time or recurring booking using the booking form.";

        if (msg == "show me a report")
            return GenerateBookingSummary();

        if (msg == "what's the price for rooms?" || msg == "what is the price for rooms?")
            return GetRoomPricing();

        if (msg == "show price predictions")
            return GetPricingTrend();

        if (msg == "i need help")
            return "Sure! You can check availability, book rooms, or view reports. Ask a question or select an option.";

        return $"You said: {userMessage}";
    }

    private string GetPricingTrend()
    {
        var bookings = GetAllBookings();

        if (!bookings.Any())
            return "No booking data available to calculate pricing trends.";

        // Group bookings by date and calculate average price per day
        var dailyPrices = bookings
            .GroupBy(b => b.CheckIn.Date)
            .Select(g => new
            {
                Date = g.Key,
                AveragePrice = g.Average(b => b.TotalPrice / b.NumberOfRooms)
            })
            .OrderBy(x => x.Date)
            .ToList();

        if (dailyPrices.Count < 2)
            return "Not enough data to determine a pricing trend.";

        var first = dailyPrices.First().AveragePrice;
        var last = dailyPrices.Last().AveragePrice;

        string trend;
        if (last > first)
            trend = $"increasing (from {first:C} to {last:C})";
        else if (last < first)
            trend = $"decreasing (from {first:C} to {last:C})";
        else
            trend = "stable";

        return $"Based on past bookings, the room pricing trend is {trend}.";
    }

    private string GetWeeklyAvailability()
    {
        var bookings = GetAllBookings();
        var today = DateTime.Today;
        var nextWeek = today.AddDays(7);

        var dates = Enumerable.Range(0, 7).Select(i => today.AddDays(i)).ToList();

        var availability = new Dictionary<DateTime, int>();

        foreach (var date in dates)
        {
            // Assuming each room type has 10 rooms
            int total = 10;
            int booked = bookings.Count(b => b.CheckIn.Date == date.Date);
            availability[date] = total - booked;
        }

        var lines = availability.Select(kvp =>
            $"{kvp.Key:ddd, MMM d}: {(kvp.Value > 0 ? $"{kvp.Value} room(s) available" : "Fully booked")}" 
        );

        return "🗓️ Availability for the next 7 days:\n" + string.Join("\n", lines);
    }

    private string GetRoomPricing()
    {
        return "🏷️ Room Prices:\n- Standard Room: $149.99/night\n- Deluxe Room: $199.99/night\n- Suite: $249.99/night";
    }

    private string GenerateBookingSummary()
    {
        var bookings = GetAllBookings();
        if (!bookings.Any())
            return "There are no bookings available right now.";

        int totalBookings = bookings.Count;
        var groupedByType = bookings.GroupBy(b => b.RoomType)
                                    .Select(g => $"{g.Key}: {g.Count()} booking(s)");

        return $"📋 Booking Report:\nTotal Bookings: {totalBookings}\n" +
               string.Join("\n", groupedByType);
    }
}