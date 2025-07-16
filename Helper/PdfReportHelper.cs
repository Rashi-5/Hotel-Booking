using System.Collections.Generic;
using System.IO;
using HotelBookingSystem.Models.Booking;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class PdfReportHelper
{
    public static byte[] GenerateBookingsReportPdf(List<BookingFormModel> bookings, string username)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text($"Booking Report for {username}").FontSize(20).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Check-In");
                        header.Cell().Text("Check-Out");
                        header.Cell().Text("Room Type");
                        header.Cell().Text("Rooms");
                        header.Cell().Text("Total Price");
                    });

                    foreach (var b in bookings)
                    {
                        table.Cell().Text(b.CheckInDate.ToShortDateString());
                        table.Cell().Text(b.CheckOutDate.ToShortDateString());
                        table.Cell().Text(b.RoomType);
                        table.Cell().Text(b.NumberOfRooms.ToString());
                        table.Cell().Text($"${b.TotalPrice:F2}");
                    }
                });
                page.Footer().Text($"Total Bookings: {bookings.Count} | Generated: {System.DateTime.Now}");
            });
        });

        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return ms.ToArray();
    }
}
