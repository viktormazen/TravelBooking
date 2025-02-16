using TravelBooking.Core.Requests;
using TravelBooking.Core.Responses;

namespace TravelBooking.Core.Interfaces
{
    public interface IManager
    {
        SearchResponse Search(SearchRequest request);
        BookResponse Book(BookRequest request);
        CheckStatusResponse CheckStatus(CheckStatusRequest request);
    }
}
