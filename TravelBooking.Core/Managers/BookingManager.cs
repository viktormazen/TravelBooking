using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TravelBooking.Core.Interfaces;
using TravelBooking.Core.Models;
using TravelBooking.Core.Requests;
using TravelBooking.Core.Responses;

namespace TravelBooking.Core.Managers
{
    public class BookingManager : IManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, BookResponse> _bookings = new Dictionary<string, BookResponse>(); 
        private static readonly Dictionary<string, SearchResponse> _searchResponses = new Dictionary<string, SearchResponse>();

        public SearchResponse Search(SearchRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Destination) || request.FromDate == DateTime.MinValue || request.ToDate == DateTime.MinValue)
            {
                throw new ArgumentException("Invalid search request.");
            }

            SearchResponse searchResult = null;

            if (string.IsNullOrEmpty(request.DepartureAirport))
            {
                // HotelOnly or LastMinuteHotels
                if ((request.FromDate - DateTime.Now).TotalDays <= 45)
                {
                    // LastMinuteHotels
                    searchResult = SearchLastMinuteHotels(request.Destination);
                }
                else
                {
                    // HotelOnly
                    searchResult = SearchHotelOnly(request.Destination);
                }
            }
            else
            {
                // HotelAndFlight
                searchResult = SearchHotelAndFlight(request.Destination, request.DepartureAirport);
            }

            if (searchResult != null && searchResult.Options != null)
            {
                foreach (var option in searchResult.Options)
                {
                    _searchResponses[option.OptionCode] = searchResult;
                }
            }

            return searchResult;
        }

        public BookResponse Book(BookRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.OptionCode) || request.SearchRequest == null)
            {
                throw new ArgumentException("Invalid book request.");
            }

            if (!_searchResponses.ContainsKey(request.OptionCode))
            {
                throw new ArgumentException("Invalid OptionCode. Please search again to get a valid OptionCode.");
            }

            var bookingCode = GenerateBookingCode();
            var bookingTime = DateTime.Now;

            var bookResponse = new BookResponse
            {
                BookingCode = bookingCode,
                BookingTime = bookingTime
            };

            _bookings.Add(bookingCode, bookResponse);

            return bookResponse;
        }

        public CheckStatusResponse CheckStatus(CheckStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.BookingCode))
            {
                throw new ArgumentException("Invalid check status request.");
            }

            if (!_bookings.ContainsKey(request.BookingCode))
            {
                return new CheckStatusResponse { Status = BookingStatusEnum.Failed }; 
            }

            var booking = _bookings[request.BookingCode];
            var bookingCompletionTime = booking.BookingTime.AddSeconds(new Random().Next(30, 61)); 

            if (DateTime.Now < bookingCompletionTime)
            {
                return new CheckStatusResponse { Status = BookingStatusEnum.Pending };
            }
            else
            {
                string optionCodeUsedForBooking = _searchResponses.Keys.FirstOrDefault(key => _searchResponses[key].Options.Any(option => option.OptionCode == request.BookingCode)); 
                if (optionCodeUsedForBooking != null && _searchResponses[optionCodeUsedForBooking] != null)
                {
                    var originalSearchRequest = _searchResponses[optionCodeUsedForBooking]; 

                    if (string.IsNullOrEmpty(originalSearchRequest.Options[0].FlightCode)) 
                    {
                        if ((originalSearchRequest.Options[0].FlightCode == null) && (originalSearchRequest.Options[0].HotelCode != null))
                        {
                            return new CheckStatusResponse { Status = BookingStatusEnum.Success }; 
                        }
                        else if ((originalSearchRequest.Options[0].HotelCode != null)) 
                        {
                            return new CheckStatusResponse { Status = BookingStatusEnum.Failed }; 
                        }
                        else
                        {
                            return new CheckStatusResponse { Status = BookingStatusEnum.Failed }; 
                        }
                    }
                    else 
                    {
                        return new CheckStatusResponse { Status = BookingStatusEnum.Success }; 
                    }
                }
                else
                {
                    return new CheckStatusResponse { Status = BookingStatusEnum.Failed }; 
                }
            }
        }

        #region Helper Methods - External API Calls (Mocked) and Code Generation

        private SearchResponse SearchHotelOnly(string destinationCode)
        {
            // Mock API call to get hotels only
            var url = $"https://tripx-test-functions.azurewebsites.net/api/SearchHotels?destinationCode={destinationCode}";
            return MockExternalApiCall<SearchResponse>(url);
        }

        private SearchResponse SearchLastMinuteHotels(string destinationCode)
        {
            // Mock API call for last minute hotels
            var url = $"https://tripx-test-functions.azurewebsites.net/api/SearchHotels?destinationCode={destinationCode}"; 
            return MockExternalApiCall<SearchResponse>(url);
        }

        private SearchResponse SearchHotelAndFlight(string destinationCode, string departureAirport)
        {
            // Mock API call to get hotels and flights
            var hotelUrl = $"https://tripx-test-functions.azurewebsites.net/api/SearchHotels?destinationCode={destinationCode}";
            var flightUrl = $"https://tripx-test-functions.azurewebsites.net.net/api/SearchFlights?departureAirport={departureAirport}&arrivalAirport={destinationCode}";
            return string.IsNullOrEmpty(departureAirport) ? MockExternalApiCall<SearchResponse>(hotelUrl) : MockExternalApiCall<SearchResponse>(flightUrl);
        }

        private T MockExternalApiCall<T>(string url) where T : class
        {
            // Simulate delay
            Task.Delay(new Random().Next(500, 1500)).Wait(); 

            if (url.Contains("SearchHotels"))
            {
                return GenerateMockHotelData(url) as T;
            }
            else if (url.Contains("SearchFlights"))
            {
                return GenerateMockFlightData(url) as T;
            }
            return null; 
        }

        private SearchResponse GenerateMockHotelData(string url)
        {
            var destinationCode = url.Split('=')[1];
            var random = new Random();
            var options = new List<Option>();
            for (int i = 1; i <= 3; i++) // Generate 3 mock hotel options
            {
                options.Add(new Option
                {
                    OptionCode = GenerateOptionCode($"HOTEL-{destinationCode}-{i}"), //Unique OptionCode
                    HotelCode = $"HOTEL-{destinationCode}-{i}",
                    FlightCode = null, // For HotelOnly searches
                    ArrivalAirport = destinationCode,
                    Price = random.Next(50, 300) * 1.0 // Mock price
                });
            }
            return new SearchResponse { Options = options.ToArray() };
        }

        private SearchResponse GenerateMockFlightData(string url)
        {
            var departureAirport = url.Split('&')[0].Split('=')[1];
            var arrivalAirport = url.Split('&')[1].Split('=')[1];
            var random = new Random();
            var options = new List<Option>();
            for (int i = 1; i <= 2; i++) // Generate 2 mock flight options
            {
                options.Add(new Option
                {
                    OptionCode = GenerateOptionCode($"FLIGHT-{departureAirport}-{arrivalAirport}-{i}"), //Unique OptionCode
                    HotelCode = null, // No Hotel code for Flight only search in this example.
                    FlightCode = $"FLIGHT-{departureAirport}-{arrivalAirport}-{i}",
                    ArrivalAirport = arrivalAirport,
                    Price = random.Next(100, 500) * 1.0 // Mock price
                });
            }
            return new SearchResponse { Options = options.ToArray() };
        }

        private string GenerateBookingCode()
        {
            var pattern = "^[a-zA-Z0-9]{6}$";

            var random = new Random();
            string code;
            do
            {
                code = new string(Enumerable.Range(0, 6).Select(_ =>
                    (char)(random.Next(3) == 0 ?
                        random.Next('0', '9' + 1) :
                        random.Next(random.Next(0, 2) == 0 ? 'A' : 'a',
                                    random.Next(0, 2) == 0 ? 'Z' + 1 : 'z' + 1)))
                    .ToArray());
            } while (!System.Text.RegularExpressions.Regex.IsMatch(code, pattern));

            return code;
        }

        private string GenerateOptionCode(string prefix)
        {
            return $"{prefix}-{Guid.NewGuid().ToString().Substring(0, 8)}"; // Prefix with GUID to ensure uniqueness
        }

        #endregion
    }
}
