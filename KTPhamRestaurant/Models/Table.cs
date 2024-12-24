using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTPhamRestaurant.Models
{
    [Table("RestaurantTable")]  
    public class Table
    {
        [Key]
        public int TableID { get; set; }

        [Required]
        [StringLength(10)]
        public string TableNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }

        public Table()
        {
            Orders = new HashSet<Order>();
            Reservations = new HashSet<Reservation>();
        }
    }
}