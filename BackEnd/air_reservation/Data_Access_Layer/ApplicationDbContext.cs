using air_reservation.Models.Flight_Model_;
using air_reservation.Models.Passenger_Model_;
using air_reservation.Models.Payment_Model_;
using air_reservation.Models.Registration_Model_;
using air_reservation.Models.Reservation_Model_;
using air_reservation.Models.Users_Model_;
using Microsoft.EntityFrameworkCore;

namespace air_reservation.Data_Access_Layer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LoginModel> LoginLogs { get; set; }
        public DbSet<RegisterModel> RegistrationLogs { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Role).HasDefaultValue(UserRole.User);
            });

            // LoginModel Configuration
            modelBuilder.Entity<LoginModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
            });

            // RegisterModel Configuration
            modelBuilder.Entity<RegisterModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.VerificationToken).HasMaxLength(100);
                entity.Property(e => e.Role).HasDefaultValue(UserRole.User);
            });

            // Flight Configuration
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Airline).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Origin).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Destination).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EconomyPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.BusinessPrice).HasColumnType("decimal(10,2)");
            });

            // Reservation Configuration
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BookingReference).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.Reservations)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Passenger Configuration
            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Reservation)
                    .WithMany(r => r.Passengers)
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TransactionId).HasMaxLength(100);

                entity.HasOne(e => e.Reservation)
                    .WithOne(r => r.Payment)
                    .HasForeignKey<Payment>(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "aster@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("aster07"),
                    FirstName = "Aster",
                    LastName = "Admin",
                    PhoneNumber = "+1234567890",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Flights
            modelBuilder.Entity<Flight>().HasData(
                new Flight
                {
                    Id = 1,
                    FlightNumber = "AA101",
                    Airline = "American Airlines",
                    Origin = "New York",
                    Destination = "Los Angeles",
                    DepartureDateTime = DateTime.UtcNow.AddDays(7),
                    ArrivalDateTime = DateTime.UtcNow.AddDays(7).AddHours(6),
                    TotalEconomySeats = 150,
                    TotalBusinessSeats = 30,
                    AvailableEconomySeats = 150,
                    AvailableBusinessSeats = 30,
                    EconomyPrice = 299.99m,
                    BusinessPrice = 899.99m,
                    Aircraft = "Boeing 737",
                    Status = FlightStatus.Scheduled
                },
                new Flight
                {
                    Id = 2,
                    FlightNumber = "DL202",
                    Airline = "Delta Airlines",
                    Origin = "Chicago",
                    Destination = "Miami",
                    DepartureDateTime = DateTime.UtcNow.AddDays(5),
                    ArrivalDateTime = DateTime.UtcNow.AddDays(5).AddHours(3),
                    TotalEconomySeats = 120,
                    TotalBusinessSeats = 24,
                    AvailableEconomySeats = 120,
                    AvailableBusinessSeats = 24,
                    EconomyPrice = 249.99m,
                    BusinessPrice = 699.99m,
                    Aircraft = "Airbus A320",
                    Status = FlightStatus.Scheduled
                },
                new Flight
                {
                    Id = 3,
                    FlightNumber = "UA303",
                    Airline = "United Airlines",
                    Origin = "San Francisco",
                    Destination = "Seattle",
                    DepartureDateTime = DateTime.UtcNow.AddDays(4),
                    ArrivalDateTime = DateTime.UtcNow.AddDays(4).AddHours(2),
                    TotalEconomySeats = 140,
                    TotalBusinessSeats = 28,
                    AvailableEconomySeats = 140,
                    AvailableBusinessSeats = 28,
                    EconomyPrice = 199.99m,
                    BusinessPrice = 599.99m,
                    Aircraft = "Boeing 757",
                    Status = FlightStatus.Scheduled
                },
new Flight
{
    Id = 4,
    FlightNumber = "BA404",
    Airline = "British Airways",
    Origin = "London",
    Destination = "Paris",
    DepartureDateTime = DateTime.UtcNow.AddDays(6),
    ArrivalDateTime = DateTime.UtcNow.AddDays(6).AddHours(1),
    TotalEconomySeats = 180,
    TotalBusinessSeats = 40,
    AvailableEconomySeats = 180,
    AvailableBusinessSeats = 40,
    EconomyPrice = 149.99m,
    BusinessPrice = 499.99m,
    Aircraft = "Airbus A321",
    Status = FlightStatus.Scheduled
},
new Flight
{
    Id = 5,
    FlightNumber = "AF505",
    Airline = "Air France",
    Origin = "Paris",
    Destination = "Berlin",
    DepartureDateTime = DateTime.UtcNow.AddDays(8),
    ArrivalDateTime = DateTime.UtcNow.AddDays(8).AddHours(2),
    TotalEconomySeats = 160,
    TotalBusinessSeats = 35,
    AvailableEconomySeats = 160,
    AvailableBusinessSeats = 35,
    EconomyPrice = 179.99m,
    BusinessPrice = 579.99m,
    Aircraft = "Boeing 737",
    Status = FlightStatus.Scheduled
},
new Flight
{
    Id = 6,
    FlightNumber = "QF606",
    Airline = "Qantas",
    Origin = "Sydney",
    Destination = "Melbourne",
    DepartureDateTime = DateTime.UtcNow.AddDays(3),
    ArrivalDateTime = DateTime.UtcNow.AddDays(3).AddHours(1),
    TotalEconomySeats = 190,
    TotalBusinessSeats = 42,
    AvailableEconomySeats = 190,
    AvailableBusinessSeats = 42,
    EconomyPrice = 139.99m,
    BusinessPrice = 459.99m,
    Aircraft = "Airbus A320",
    Status = FlightStatus.Scheduled
},
new Flight
{
    Id = 7,
    FlightNumber = "EK707",
    Airline = "Emirates",
    Origin = "Dubai",
    Destination = "Singapore",
    DepartureDateTime = DateTime.UtcNow.AddDays(10),
    ArrivalDateTime = DateTime.UtcNow.AddDays(10).AddHours(7),
    TotalEconomySeats = 200,
    TotalBusinessSeats = 50,
    AvailableEconomySeats = 200,
    AvailableBusinessSeats = 50,
    EconomyPrice = 399.99m,
    BusinessPrice = 999.99m,
    Aircraft = "Boeing 777",
    Status = FlightStatus.Scheduled
}
            );
        }
    }
}
