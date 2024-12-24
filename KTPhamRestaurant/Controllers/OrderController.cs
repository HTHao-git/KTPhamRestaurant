using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using KTPhamRestaurant;
using KTPhamRestaurant.Models;
using System.Collections.Generic;
namespace KTPhamRestaurant.Controllers
{
    public class OrderController : Controller
    {
        private readonly RestaurantContext db = new RestaurantContext();

        // GET: Order
        public ActionResult Index()
        {
            var orders = db.Orders
                .Include(o => o.Employee)
                .Include(o => o.Table)
                .ToList();
            return View(orders);
        }

        // GET: Order/Create
        public ActionResult Create()
        {
            // Only show available tables
            ViewBag.TableID = new SelectList(
                db.Tables.Where(t => t.Status == "Available"),
                "TableID",
                "TableNumber"
            );

            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "EmployeeName");
            ViewBag.MenuItems = db.MenuItems
                .OrderBy(m => m.DishType)
                .ThenBy(m => m.DishName)
                .ToList();

            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TableID,EmployeeID")] Order order, FormCollection form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            // Generate OrderID
                            var lastOrder = db.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault();
                            int nextNumber = 1;
                            if (lastOrder != null)
                            {
                                nextNumber = int.Parse(lastOrder.OrderID.Substring(4)) + 1;
                            }
                            order.OrderID = $"ORDR{nextNumber:D4}";
                            order.OrderDate = DateTime.Now;
                            order.Status = "Pending";

                            // Add order
                            db.Orders.Add(order);

                            // Process order details from form
                            var menuItems = db.MenuItems.ToList();
                            foreach (var menuItem in menuItems)
                            {
                                string quantityKey = $"orderDetails[{menuItem.DishID}].Quantity";
                                if (form[quantityKey] != null)
                                {
                                    int quantity;
                                    if (int.TryParse(form[quantityKey], out quantity) && quantity > 0)
                                    {
                                        var orderDetail = new OrderDetail
                                        {
                                            OrderID = order.OrderID,
                                            DishID = menuItem.DishID,
                                            Quantity = quantity
                                        };
                                        db.OrderDetails.Add(orderDetail);
                                    }
                                }
                            }

                            // Update table status
                            var table = db.Tables.Find(order.TableID);
                            if (table != null)
                            {
                                table.Status = "Occupied";
                                db.Entry(table).State = EntityState.Modified;
                            }

                            db.SaveChanges();
                            transaction.Commit();
                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError("", "Error creating order: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
            }

            // If we got this far, something failed
            ViewBag.TableID = new SelectList(
                db.Tables.Where(t => t.Status == "Available"),
                "TableID",
                "TableNumber",
                order.TableID
            );
            ViewBag.EmployeeID = new SelectList(
                db.Employees,
                "EmployeeID",
                "EmployeeName",
                order.EmployeeID
            );
            ViewBag.MenuItems = db.MenuItems.OrderBy(m => m.DishType).ThenBy(m => m.DishName).ToList();
            return View(order);
        }

        // GET: Order/Details/ORDR0001
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.Employee)
                .Include(o => o.Table)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Load menu items for each order detail
            foreach (var detail in order.OrderDetails)
            {
                db.Entry(detail).Reference(d => d.MenuItem).Load();
            }

            return View(order);
        }

        // GET: Order/Edit/ORDR0001
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Check if order can be edited
            if (order.Payments.Any(p => p.PaymentStatus == "Paid"))
            {
                TempData["ErrorMessage"] = "Cannot edit a paid order.";
                return RedirectToAction("Details", new { id = id });
            }

            ViewBag.TableID = new SelectList(db.Tables, "TableID", "TableNumber", order.TableID);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "EmployeeName", order.EmployeeID);
            ViewBag.MenuItems = db.MenuItems.OrderBy(m => m.DishType).ThenBy(m => m.DishName).ToList();

            return View(order);
        }

        // POST: Order/Edit/ORDR0001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, FormCollection form)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Update order status
                    order.Status = form["Status"];

                    // Update table and employee IDs
                    order.TableID = int.Parse(form["TableID"]);
                    order.EmployeeID = int.Parse(form["EmployeeID"]);

                    db.Entry(order).State = EntityState.Modified;

                    // Handle order details
                    var menuItems = db.MenuItems.ToList();

                    // Remove existing order details
                    var existingDetails = db.OrderDetails.Where(od => od.OrderID == id);
                    db.OrderDetails.RemoveRange(existingDetails);

                    // Add new order details
                    foreach (var menuItem in menuItems)
                    {
                        string quantityKey = $"orderDetails[{menuItem.DishID}].Quantity";
                        if (form[quantityKey] != null)
                        {
                            int quantity;
                            if (int.TryParse(form[quantityKey], out quantity) && quantity > 0)
                            {
                                var orderDetail = new OrderDetail
                                {
                                    OrderID = id,
                                    DishID = menuItem.DishID,
                                    Quantity = quantity
                                };
                                db.OrderDetails.Add(orderDetail);
                            }
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("Details", new { id = order.OrderID });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Error updating order: " + ex.Message);
                }
            }

            // If we got this far, something failed
            ViewBag.TableID = new SelectList(db.Tables, "TableID", "TableNumber", order.TableID);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "EmployeeName", order.EmployeeID);
            ViewBag.MenuItems = db.MenuItems.OrderBy(m => m.DishType).ThenBy(m => m.DishName).ToList();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(string id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = db.Orders
                        .Include(o => o.Table)
                        .FirstOrDefault(o => o.OrderID == id);

                    if (order != null)
                    {
                        // Update order status
                        order.Status = "Cancelled";

                        // Return table to Available status
                        if (order.Table != null)
                        {
                            order.Table.Status = "Available";
                            db.Entry(order.Table).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        return RedirectToAction("Index");
                    }
                    return HttpNotFound();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Error cancelling order: " + ex.Message);
                    return RedirectToAction("Index");
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
}