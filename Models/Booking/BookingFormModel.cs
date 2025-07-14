using System.ComponentModel.DataAnnotations;

public class BookingFormModel
{
    public Guid BookingId { get; set; }
    [Required]
    public string CustomerName { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    public string Username { get; set; }

    public string Note { get; set; }
    public int NumberOfRooms { get; set; }
    public decimal TotalPrice { get; set; }
    public string RoomType { get; set; }

}

public class RecurringBookingInput
{
    public string RoomType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RecurrenceFrequency { get; set; } 
    public int Interval { get; set; } 
    public List<DayOfWeek> DaysOfWeek { get; set; }
    public string SpecialRequests { get; set; }
}
