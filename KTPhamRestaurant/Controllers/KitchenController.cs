using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTPhamRestaurant.Models;

namespace KTPhamRestaurant.Controllers
{
    public class KitchenController : Controller
    {
        private readonly RestaurantContext db = new RestaurantContext();

        // GET: Kitchen
        public ActionResult Index()
        {
            var activeOrders = db.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Table)
                .Include(o => o.Employee)
                .Include(o => o.Payments)  
                .Where(o => !o.Payments.Any() || o.Payments.Any(p => p.PaymentStatus == "Pending")) 
                .OrderBy(o => o.OrderDate)
                .ToList();

            foreach (var order in activeOrders)
            {
                foreach (var detail in order.OrderDetails)
                {
                    db.Entry(detail).Reference(d => d.MenuItem).Load();
                }
            }

            return View(activeOrders);
        }

        // GET: Kitchen/Dashboard
        public ActionResult Dashboard()
        {
            var viewModel = new KitchenDashboardViewModel
            {
                PendingOrders = db.Orders
                    .Count(o => o.Status == "Pending" || o.Status == null),

                InProgressOrders = db.Orders
                    .Count(o => o.Status == "In Progress"),

                CompletedOrders = db.Orders
                    .Count(o => o.Status == "Completed"),

                PopularItems = db.OrderDetails
                    .GroupBy(od => od.MenuItem)
                    .Select(g => new PopularItemViewModel
                    {
                        ItemName = g.Key.DishName,
                        OrderCount = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(5)
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Kitchen/OrderDetails/ORDR0001
        public ActionResult OrderDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Table)
                .Include(o => o.Employee)
                .Include(o => o.Payments)  
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            foreach (var detail in order.OrderDetails)
            {
                db.Entry(detail).Reference(d => d.MenuItem).Load();
            }

            return View(order);
        }

        // POST: Kitchen/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(string orderId, string status)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}