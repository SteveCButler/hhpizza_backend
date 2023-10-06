using Microsoft.EntityFrameworkCore;
namespace hhpizza_backend.Models;

public class HHPizzaDbContext : DbContext
{

    public DbSet<Order> Orders { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<User> Users { get; set; }
    

    public HHPizzaDbContext(DbContextOptions<HHPizzaDbContext> context) : base(context)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Example to create seed data
        //modelBuilder.Entity<CampsiteType>().HasData(new CampsiteType[]
        //{
        //new CampsiteType {Id = 1, CampsiteTypeName = "Tent", FeePerNight = 15.99M, MaxReservationDays = 7},
        //new CampsiteType {Id = 2, CampsiteTypeName = "RV", FeePerNight = 26.50M, MaxReservationDays = 14},
        //new CampsiteType {Id = 3, CampsiteTypeName = "Primitive", FeePerNight = 10.00M, MaxReservationDays = 3},
        //new CampsiteType {Id = 4, CampsiteTypeName = "Hammock", FeePerNight = 12M, MaxReservationDays = 7}
        //});
    }
}