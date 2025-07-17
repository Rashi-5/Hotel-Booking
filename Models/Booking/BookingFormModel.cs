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
    public int Adult {get; set;}
    public int Children {get; set;}
    public string BookingType { get; set; }
    // Recurrence details
    public string Frequency { get; set; } 
    public int? Interval { get; set; }
    public List<string> Days { get; set; } 
}