using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTPhamRestaurant.Models
{
    [Table("PaymentLog")]
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        [StringLength(8)]
        public string OrderID { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; }

        public DateTime? PaymentDate { get; set; }

        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }
    }
}