using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTPhamRestaurant.Models
{
    [Table("OrderLog")]
    public class Order
    {
        [Key]
        [StringLength(8)]
        public string OrderID { get; set; }

        public int EmployeeID { get; set; }

        public DateTime? OrderDate { get; set; }

        public int? TableID { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        // Navigation properties
        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("TableID")]
        public virtual Table Table { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }  

        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Payments = new HashSet<Payment>();  
        }
    }
}