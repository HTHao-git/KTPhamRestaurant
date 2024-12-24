using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KTPhamRestaurant.Models
{
    public class MenuItem
    {
        [Key]
        public int DishID { get; set; }

        [Required]
        [StringLength(100)]
        public string DishName { get; set; }

        [Required]
        [StringLength(50)]
        public string Size { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string DishType { get; set; }

        public int? StarRating { get; set; }

        [DefaultValue(0)]
        public int TotalSold { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}