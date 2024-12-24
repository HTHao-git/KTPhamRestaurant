namespace KTPhamRestaurant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Employee",
                c => new
                    {
                        EmployeeID = c.Int(nullable: false, identity: true),
                        EmployeeName = c.String(nullable: false, maxLength: 100),
                        Position = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.EmployeeID);
            
            CreateTable(
                "dbo.OrderLog",
                c => new
                    {
                        OrderID = c.String(nullable: false, maxLength: 8),
                        EmployeeID = c.Int(nullable: false),
                        OrderDate = c.DateTime(),
                        TableID = c.Int(nullable: false),
                        Status = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.OrderID)
                .ForeignKey("dbo.Employee", t => t.EmployeeID)
                .ForeignKey("dbo.RestaurantTable", t => t.TableID)
                .Index(t => t.EmployeeID)
                .Index(t => t.TableID);
            
            CreateTable(
                "dbo.OrderDetail",
                c => new
                    {
                        OrderDetailID = c.Int(nullable: false, identity: true),
                        OrderID = c.String(nullable: false, maxLength: 8),
                        DishID = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OrderDetailID)
                .ForeignKey("dbo.MenuItem", t => t.DishID)
                .ForeignKey("dbo.OrderLog", t => t.OrderID)
                .Index(t => t.OrderID)
                .Index(t => t.DishID);
            
            CreateTable(
                "dbo.MenuItem",
                c => new
                    {
                        DishID = c.Int(nullable: false, identity: true),
                        DishName = c.String(nullable: false, maxLength: 100),
                        Size = c.String(nullable: false, maxLength: 50),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DishType = c.String(nullable: false, maxLength: 50),
                        StarRating = c.Int(),
                        TotalSold = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DishID);
            
            CreateTable(
                "dbo.PaymentLog",
                c => new
                    {
                        PaymentID = c.Int(nullable: false, identity: true),
                        OrderID = c.String(nullable: false, maxLength: 8),
                        PaymentStatus = c.String(nullable: false, maxLength: 50),
                        PaymentDate = c.DateTime(),
                        Order_OrderID = c.String(nullable: false, maxLength: 8),
                    })
                .PrimaryKey(t => t.PaymentID)
                .ForeignKey("dbo.OrderLog", t => t.Order_OrderID)
                .Index(t => t.Order_OrderID);
            
            CreateTable(
                "dbo.RestaurantTable",
                c => new
                    {
                        TableID = c.Int(nullable: false, identity: true),
                        TableNumber = c.String(nullable: false, maxLength: 10),
                        Status = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.TableID);
            
            CreateTable(
                "dbo.Reservation",
                c => new
                    {
                        ReservationID = c.Int(nullable: false, identity: true),
                        TableID = c.Int(nullable: false),
                        CustomerName = c.String(nullable: false, maxLength: 100),
                        ContactNumber = c.String(nullable: false, maxLength: 20),
                        ReservationDateTime = c.DateTime(nullable: false),
                        NumberOfGuests = c.Int(nullable: false),
                        SpecialRequests = c.String(maxLength: 255),
                        Status = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ReservationID)
                .ForeignKey("dbo.RestaurantTable", t => t.TableID, cascadeDelete: true)
                .Index(t => t.TableID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderLog", "TableID", "dbo.RestaurantTable");
            DropForeignKey("dbo.Reservation", "TableID", "dbo.RestaurantTable");
            DropForeignKey("dbo.PaymentLog", "Order_OrderID", "dbo.OrderLog");
            DropForeignKey("dbo.OrderDetail", "OrderID", "dbo.OrderLog");
            DropForeignKey("dbo.OrderDetail", "DishID", "dbo.MenuItem");
            DropForeignKey("dbo.OrderLog", "EmployeeID", "dbo.Employee");
            DropIndex("dbo.Reservation", new[] { "TableID" });
            DropIndex("dbo.PaymentLog", new[] { "Order_OrderID" });
            DropIndex("dbo.OrderDetail", new[] { "DishID" });
            DropIndex("dbo.OrderDetail", new[] { "OrderID" });
            DropIndex("dbo.OrderLog", new[] { "TableID" });
            DropIndex("dbo.OrderLog", new[] { "EmployeeID" });
            DropTable("dbo.Reservation");
            DropTable("dbo.RestaurantTable");
            DropTable("dbo.PaymentLog");
            DropTable("dbo.MenuItem");
            DropTable("dbo.OrderDetail");
            DropTable("dbo.OrderLog");
            DropTable("dbo.Employee");
        }
    }
}
