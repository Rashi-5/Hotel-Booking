using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Models.Prediction;
using System.Text.Json;

namespace HotelBookingSystem.Services
{
    public class PredictionService
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
                        // Optionally log malformed lines
                    }
                }
            }
            return bookings;
        }

        public Dictionary<string, int> GetRoomBookingFrequencyByType()
        {
            var allBookings = GetAllBookings();
            var frequency = new Dictionary<string, int>();

            foreach (var booking in allBookings)
            {
                if (!frequency.ContainsKey(booking.RoomType))
                    frequency[booking.RoomType] = 0;

                frequency[booking.RoomType]++;
            }

            return frequency;
        }

        public Dictionary<DateTime, int> GetBookingDemandPerDate()
        {
            var allBookings = GetAllBookings();
            var demand = new Dictionary<DateTime, int>();

            foreach (var booking in allBookings)
            {
                var date = booking.CheckIn.Date;
                if (!demand.ContainsKey(date))
                    demand[date] = 0;

                demand[date] += booking.NumberOfRooms;
            }

            return demand;
        }

        public List<DateTime> GetMostBookedDates(int top = 5)
        {
            var demand = GetBookingDemandPerDate();
            return demand
                .OrderByDescending(x => x.Value)
                .Take(top)
                .Select(x => x.Key)
                .ToList();
        }

        public Dictionary<string, decimal> GetAveragePricePerRoom()
        {
            var allBookings = GetAllBookings();
            var priceMap = new Dictionary<string, List<decimal>>();

            foreach (var booking in allBookings)
            {
                var days = (booking.CheckOut - booking.CheckIn).Days;
                days = days == 0 ? 1 : days;

                decimal pricePerRoom = booking.TotalPrice / (booking.NumberOfRooms * days);

                if (!priceMap.ContainsKey(booking.RoomType))
                    priceMap[booking.RoomType] = new List<decimal>();

                priceMap[booking.RoomType].Add(pricePerRoom);
            }

            return priceMap.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.Average(), 2)
            );
        }

        public PredictionSummary GeneratePredictionReport()
        {
            return new PredictionSummary
            {
                MostBookedRoomTypes = GetRoomBookingFrequencyByType(),
                MostDemandedDates = GetMostBookedDates(),
                AverageRoomPricePerType = GetAveragePricePerRoom()
            };
        }
    }
}
