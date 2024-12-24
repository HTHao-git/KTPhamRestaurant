using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using KTPhamRestaurant;
using KTPhamRestaurant.Models;

public class TableController : Controller
{
    private readonly RestaurantContext db = new RestaurantContext();

    // GET: Table
    public ActionResult Index()
    {
        var tables = db.Tables
            .Include(t => t.Orders)
            .Include(t => t.Reservations)
            .OrderBy(t => t.TableNumber)
            .ToList();
        return View(tables);
    }

    // GET: Table/Edit/5
    public ActionResult Edit(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        var table = db.Tables.Find(id);
        if (table == null)
        {
            return HttpNotFound();
        }

        ViewBag.Statuses = new SelectList(
            new[] { "Available", "Occupied", "Reserved" },
            table.Status);

        return View(table);
    }

    // POST: Table/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "TableID,TableNumber,Status")] Table table)
    {
        if (ModelState.IsValid)
        {
            db.Entry(table).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.Statuses = new SelectList(
            new[] { "Available", "Occupied", "Reserved" },
            table.Status);

        return View(table);
    }

    // POST: Table/UpdateStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UpdateStatus(int id, string status) 
    {
        try
        {
            var table = db.Tables.Find(id);
            if (table != null)
            {
                // Check if table has active orders 
                var hasActiveOrders = db.Orders
                    .Any(o => o.TableID == id &&
                             (!o.Payments.Any() || o.Payments.All(p => p.PaymentStatus != "Paid")));

                if (status == "Available" && hasActiveOrders)
                {
                    return Json(new { success = false, message = "Cannot make table available while there are active orders." });
                }

                // Check if table has active reservations
                if (status == "Available" && db.Reservations.Any(r => r.TableID == id && r.Status == "Confirmed"))
                {
                    return Json(new { success = false, message = "Cannot make table available while there are active reservations." });
                }

                table.Status = status;
                db.SaveChanges();
                return Json(new { success = true, message = "Status updated successfully" });
            }
            return Json(new { success = false, message = "Table not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
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