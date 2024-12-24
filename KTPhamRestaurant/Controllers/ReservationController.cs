using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using KTPhamRestaurant;
using KTPhamRestaurant.Models;

public class ReservationController : Controller
{
    private readonly RestaurantContext db = new RestaurantContext();

    // GET: Reservation
    public ActionResult Index()
    {
        var reservations = db.Reservations
            .Include(r => r.Table)
            .OrderByDescending(r => r.ReservationDateTime)
            .ToList();
        return View(reservations);
    }

    // GET: Reservation/Create
    public ActionResult Create()
    {
        ViewBag.TableID = new SelectList(
            db.Tables.Where(t => t.Status == "Available")
                .OrderBy(t => t.TableNumber),
            "TableID",
            "TableNumber"
        );

        var reservation = new Reservation
        {
            ReservationDateTime = DateTime.Now.AddHours(1),
            NumberOfGuests = 1,
            Status = "Confirmed"
        };

        return View(reservation);
    }

    // POST: Reservation/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "TableID,CustomerName,ContactNumber,ReservationDateTime,NumberOfGuests,SpecialRequests,Status")] Reservation reservation)
    {
        System.Diagnostics.Debug.WriteLine("Create action started");
        System.Diagnostics.Debug.WriteLine($"Received data: Customer={reservation.CustomerName}, DateTime={reservation.ReservationDateTime}, Table={reservation.TableID}");

        try
        {
            // Set status first
            reservation.Status = "Confirmed";
            System.Diagnostics.Debug.WriteLine($"Status set to: {reservation.Status}");

            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is valid, proceeding with save");
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Reservations.Add(reservation);
                        System.Diagnostics.Debug.WriteLine("Reservation added to context");

                        // Update table status if needed
                        if (reservation.ReservationDateTime.Date == DateTime.Today)
                        {
                            var table = db.Tables.Find(reservation.TableID);
                            if (table != null)
                            {
                                table.Status = "Reserved";
                                db.Entry(table).State = EntityState.Modified;
                                System.Diagnostics.Debug.WriteLine($"Updated table {table.TableNumber} to Reserved");
                            }
                        }

                        db.SaveChanges();
                        System.Diagnostics.Debug.WriteLine("Changes saved successfully");
                        transaction.Commit();
                        System.Diagnostics.Debug.WriteLine("Transaction committed");

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error during save: {ex.Message}");
                        transaction.Rollback();
                        ModelState.AddModelError("", $"Error creating reservation: {ex.Message}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ModelState is invalid");
                foreach (var modelStateEntry in ModelState)
                {
                    foreach (var error in modelStateEntry.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error for {modelStateEntry.Key}: {error.ErrorMessage}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"General error: {ex.Message}");
            ModelState.AddModelError("", $"Error: {ex.Message}");
        }

        // If we got this far, something failed
        ViewBag.TableID = new SelectList(
            db.Tables.Where(t => t.Status == "Available"),
            "TableID",
            "TableNumber",
            reservation.TableID
        );
        return View(reservation);
    }

    // GET: Reservation/Edit/5
    public ActionResult Edit(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        var reservation = db.Reservations.Find(id);
        if (reservation == null)
        {
            return HttpNotFound();
        }

        ViewBag.TableID = new SelectList(db.Tables, "TableID", "TableNumber", reservation.TableID);
        ViewBag.Statuses = new SelectList(
            new[] { "Confirmed", "Cancelled", "Completed" },
            reservation.Status
        );

        return View(reservation);
    }

    // POST: Reservation/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "ReservationID,TableID,CustomerName,ContactNumber,ReservationDateTime,NumberOfGuests,SpecialRequests,Status")] Reservation reservation)
    {
        if (ModelState.IsValid)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Get the original reservation to check for changes
                    var originalReservation = db.Reservations
                        .Include(r => r.Table)
                        .FirstOrDefault(r => r.ReservationID == reservation.ReservationID);

                    if (originalReservation != null)
                    {
                        // Update table status based on reservation status
                        if (reservation.Status != originalReservation.Status)
                        {
                            var table = db.Tables.Find(reservation.TableID);
                            if (table != null)
                            {
                                if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
                                {
                                    table.Status = "Available";
                                }
                                else if (reservation.Status == "Confirmed" &&
                                       reservation.ReservationDateTime.Date == DateTime.Today)
                                {
                                    table.Status = "Reserved";
                                }
                                db.Entry(table).State = EntityState.Modified;
                            }
                        }

                        // Update reservation
                        db.Entry(originalReservation).CurrentValues.SetValues(reservation);
                        db.SaveChanges();
                        transaction.Commit();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        ViewBag.TableID = new SelectList(db.Tables, "TableID", "TableNumber", reservation.TableID);
        ViewBag.Statuses = new SelectList(
            new[] { "Confirmed", "Cancelled", "Completed" },
            reservation.Status
        );
        return View(reservation);
    }

    // POST: Reservation/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Cancel(int id)
    {
        using (var transaction = db.Database.BeginTransaction())
        {
            try
            {
                var reservation = db.Reservations
                    .Include(r => r.Table)
                    .FirstOrDefault(r => r.ReservationID == id);

                if (reservation != null)
                {
                    reservation.Status = "Cancelled";

                    if (reservation.Table.Status == "Reserved")
                    {
                        reservation.Table.Status = "Available";
                    }

                    db.SaveChanges();
                    transaction.Commit();
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
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