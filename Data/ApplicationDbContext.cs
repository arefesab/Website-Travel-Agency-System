using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using agancywebProject.Models.DB;

namespace agancywebProject.Data
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<agancywebProject.Models.DB.Flight>? Flight { get; set; }
        public DbSet<agancywebProject.Models.DB.Hotel>? Hotel { get; set; }
        public DbSet<agancywebProject.Models.DB.UserLogin>? UserLogin { get; set; }
        public DbSet<agancywebProject.Models.DB.Booking>? Booking { get; set; }
        public DbSet<agancywebProject.Models.DB.FlightBooking>? FlightBooking { get; set; }

    }
}
