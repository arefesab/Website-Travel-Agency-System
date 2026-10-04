using agancywebProject.Models.DB;

namespace agancywebProject.Models
{
    public class AdminBookingsViewModel
    {
        public List<Booking> HotelBookings { get; set; } = new();
        public List<FlightBooking> FlightBookings { get; set; } = new();
        public string? Q { get; set; }
        public string? Status { get; set; }
        public string Type { get; set; } = "all";
    }
}
