using KTPhamRestaurant;
using KTPhamRestaurant.Models;
using System.Data.Entity;
using System.Net;
using System.Web.Mvc;
using System;
using System.Linq;

public class PaymentController : Controller
{
    private readonly RestaurantContext db = new RestaurantContext();

    // GET: Payment
    public ActionResult Index()
    {
        var query = from p in db.Payments
                    join o in db.Orders on p.OrderID equals o.OrderID
                    orderby p.PaymentDate descending
                    select p;

        return View(query.ToList());
    }

    public ActionResult Create(string orderId)
    {
        if (string.IsNullOrEmpty(orderId))
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        var order = db.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Employee)
            .Include(o => o.Table)
            .FirstOrDefault(o => o.OrderID == orderId);

        if (order == null)
        {
            return HttpNotFound();
        }

        // Check if payment already exists
        var existingPayment = db.Payments.FirstOrDefault(p => p.OrderID == orderId);
        if (existingPayment != null && existingPayment.PaymentStatus == "Paid")
        {
            TempData["ErrorMessage"] = "Payment has already been processed for this order.";
            return RedirectToAction("Details", "Order", new { id = orderId });
        }

        // Load menu items for calculation
        foreach (var detail in order.OrderDetails)
        {
            db.Entry(detail).Reference(d => d.MenuItem).Load();
        }

        // Calculate total
        decimal total = order.OrderDetails.Sum(od => od.MenuItem.Price * od.Quantity);
        ViewBag.Total = total;

        return View(order);
    }

    // GET: Payment/Details/5
    public ActionResult Details(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        var payment = db.Payments
            .Include(p => p.Order)
            .Include(p => p.Order.Employee)
            .Include(p => p.Order.Table)
            .FirstOrDefault(p => p.PaymentID == id);

        if (payment == null)
        {
            return HttpNotFound();
        }

        // Load order details
        var orderDetails = db.OrderDetails
            .Include(od => od.MenuItem)
            .Where(od => od.OrderID == payment.OrderID)
            .ToList();

        ViewBag.OrderDetails = orderDetails;

        return View(payment);
    }

    // POST: Payment/ProcessPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ProcessPayment(string orderId, string paymentMethod)
    {
        using (var transaction = db.Database.BeginTransaction())
        {
            try
            {
                var order = db.Orders
                    .Include(o => o.Table)
                    .FirstOrDefault(o => o.OrderID == orderId);

                if (order == null)
                {
                    return HttpNotFound();
                }

                // Process payment
                var payment = db.Payments.FirstOrDefault(p => p.OrderID == orderId) ?? new Payment { OrderID = orderId };
                payment.PaymentStatus = "Paid";
                payment.PaymentDate = DateTime.Now;

                if (payment.PaymentID == 0)
                {
                    db.Payments.Add(payment);
                }

                // Update order status
                order.Status = "Completed";

                if (order.Table != null)
                {
                    order.Table.Status = "Available";
                    db.Entry(order.Table).State = EntityState.Modified;
                }

                db.SaveChanges();
                transaction.Commit();

                return RedirectToAction("Details", new { id = payment.PaymentID });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", "Error processing payment: " + ex.Message);
                return RedirectToAction("Create", new { orderId = orderId });
            }
        }
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