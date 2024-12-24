using System.Data.Entity;
using KTPhamRestaurant.Models;

namespace KTPhamRestaurant
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext() : base("name=DefaultConnection")
        {
            this.Configuration.LazyLoadingEnabled = true;
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Basic table mappings
            modelBuilder.Entity<MenuItem>().ToTable("MenuItem");
            modelBuilder.Entity<Table>().ToTable("RestaurantTable");
            modelBuilder.Entity<Order>().ToTable("OrderLog");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail");
            modelBuilder.Entity<Payment>().ToTable("PaymentLog");
            modelBuilder.Entity<Employee>().ToTable("Employee");
            modelBuilder.Entity<Reservation>().ToTable("Reservation");

            // Configure Order as principal in Order-Payment relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Payments)  
                .WithRequired(p => p.Order)
                .HasForeignKey(p => p.OrderID);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.MenuItem)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(od => od.DishID);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Employee)
                .WithMany(e => e.Orders)
                .HasForeignKey(o => o.EmployeeID);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Table)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.TableID);
        }
    }
}