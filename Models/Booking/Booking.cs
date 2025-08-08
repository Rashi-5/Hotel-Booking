namespace HotelBookingSystem.Models.Booking
  {
    public class Booking
      {
          public string RoomType { get; set; }
          public DateTime CheckIn { get; set; }
          public DateTime CheckOut { get; set; }
          public int NumberOfRooms { get; set; }
          public decimal Price { get; set; }
      }
  }