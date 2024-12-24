using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KTPhamRestaurant.Models
{
    public class KitchenDashboardViewModel
    {
        public int PendingOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int CompletedOrders { get; set; }
        public List<PopularItemViewModel> PopularItems { get; set; }
    }
}