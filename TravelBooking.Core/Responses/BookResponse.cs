using System;
namespace TravelBooking.Core.Responses
{
    public class BookResponse
    {
        public string BookingCode { get; set; } = string.Empty;
        public DateTime BookingTime { get; set; }
    }
}
