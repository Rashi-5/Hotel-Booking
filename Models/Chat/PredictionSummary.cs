namespace HotelBookingSystem.Models.Prediction
{
    public class PredictionSummary
    {
        public Dictionary<string, int> MostBookedRoomTypes { get; set; }
        public List<DateTime> MostDemandedDates { get; set; }
        public Dictionary<string, decimal> AverageRoomPricePerType { get; set; }
    }
}
