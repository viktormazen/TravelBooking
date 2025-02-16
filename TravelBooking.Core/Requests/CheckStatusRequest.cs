using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Core.Requests
{
    public class CheckStatusRequest
    {
        [Required(ErrorMessage = "Booking Code is required")]
        public string BookingCode { get; set; } = string.Empty;
    }
}
