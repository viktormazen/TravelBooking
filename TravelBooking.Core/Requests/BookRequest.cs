using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Core.Requests
{
    public class BookRequest
    {
        [Required(ErrorMessage = "Option Code is required")]
        public string OptionCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Search Request is required")]
        public SearchRequest SearchRequest { get; set; } 
    }
}
