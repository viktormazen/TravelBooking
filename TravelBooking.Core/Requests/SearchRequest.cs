using System;
using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Core.Requests
{
    public class SearchRequest
    {
        [Required(ErrorMessage = "Destination is required")]
        public string Destination { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        [Required(ErrorMessage = "From Date is required")]
        public DateTime FromDate { get; set; }
        [Required(ErrorMessage = "To Date is required")]
        public DateTime ToDate { get; set; }
    }
}
