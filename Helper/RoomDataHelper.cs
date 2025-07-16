using HotelBookingSystem.Models.Booking;

namespace HotelBookingSystem.Helper
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
                    ImageUrl = "/images/delux.jpg",
                    Description = "Luxurious room with sea view, private balcony, and jacuzzi.",
                    Amenities = new List<string> { "King Bed", "Wi-Fi", "Jacuzzi", "Mini Bar" },
                    isDefault = true,
                    Price = "299.99", 
                    NumberOfRooms = 5
                },
                new RoomCardViewModel
                {
                    RoomName = "Standard Room",
                    ImageUrl = "/images/standard.jpg",
                    Description = "Affordable comfort for business or short stays.",
                    Amenities = new List<string> { "Queen Bed", "Wi-Fi", "TV" },
                    isDefault = true,
                    Price = "149.99",
                    NumberOfRooms = 5
                },
                new RoomCardViewModel
                {
                    RoomName = "Family Room",
                    ImageUrl = "/images/family.jpg",
                    Description = "Spacious room ideal for families with kids.",
                    Amenities = new List<string> { "Two Double Beds", "Wi-Fi", "Play Area", "Extra Sofa" },
                    isDefault = true,
                    Price = "199.99",
                    NumberOfRooms = 5
                }
            };
        }
    }
}
