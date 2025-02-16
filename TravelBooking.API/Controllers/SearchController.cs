using System;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.API.Filters;
using TravelBooking.Core.Interfaces;
using TravelBooking.Core.Requests;

namespace TravelBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiKeyAuthorize]
    public class SearchController : ControllerBase
    {
        private readonly IManager _bookingManager;

        public SearchController(IManager bookingManager)
        {
            _bookingManager = bookingManager;
        }

        [HttpPost]
        public IActionResult Search([FromBody] SearchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = _bookingManager.Search(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later."); 
            }
        }
    }
}
