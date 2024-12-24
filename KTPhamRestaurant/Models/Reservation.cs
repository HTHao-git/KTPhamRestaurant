using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTPhamRestaurant.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationID { get; set; }

        public int TableID { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(20)]
        public string ContactNumber { get; set; }

        [Required]
        public DateTime ReservationDateTime { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        [StringLength(255)]
        public string SpecialRequests { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        // Navigation property
        public virtual Table Table { get; set; }
    }
}