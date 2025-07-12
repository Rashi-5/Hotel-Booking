using HotelBookingSystem.Models.Booking;

namespace HotelBookingSystem.Controllers.Helper
{
    public static class RoomDataHelper
    {
        public static List<RoomCardViewModel> GetDefaultRooms()
        {
            return new List<RoomCardViewModel>
            {
                new RoomCardViewModel
                {
                    RoomName = "Deluxe Suite",
                    ImageUrl = "/images/rooms/deluxe.jpg",
                    Description = "Luxurious room with sea view, private balcony, and jacuzzi.",
                    Amenities = new List<string> { "King Bed", "Wi-Fi", "Jacuzzi", "Mini Bar" }
                },
                new RoomCardViewModel
                {
                    RoomName = "Standard Room",
                    ImageUrl = "/images/rooms/standard.jpg",
                    Description = "Affordable comfort for business or short stays.",
                    Amenities = new List<string> { "Queen Bed", "Wi-Fi", "TV" }
                },
                new RoomCardViewModel
                {
                    RoomName = "Family Room",
                    ImageUrl = "/images/rooms/family.jpg",
                    Description = "Spacious room ideal for families with kids.",
                    Amenities = new List<string> { "Two Double Beds", "Wi-Fi", "Play Area", "Extra Sofa" }
                }
            };
        }
    }
}
