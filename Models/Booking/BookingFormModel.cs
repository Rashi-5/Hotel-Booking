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
