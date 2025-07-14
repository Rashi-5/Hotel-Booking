using System.ComponentModel.DataAnnotations;

public class BookingFormModel
{
    [Required]
    public string CustomerName { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }
}

public class RecurringBookingInput
{
    public string RoomType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RecurrenceFrequency { get; set; } // "Weekly"
    public int Interval { get; set; } // e.g., every 1 week
    public List<DayOfWeek> DaysOfWeek { get; set; }
    public string SpecialRequests { get; set; }
}
