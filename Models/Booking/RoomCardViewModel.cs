namespace HotelBookingSystem.Models.Booking
{
    public class RoomCardViewModel
    {
        public string RoomName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<string> Amenities { get; set; }
        public bool isDefault { get; set; } 
        public string Price { get; set; }
        public int NumberOfRooms { get; set; }
    }
}
