# KTPham Restaurant Management System

A full-stack ASP.NET MVC web application for restaurant management, demonstrating frontend-backend integration through JavaScript event handling and C# controllers.

## Overview

KTPham Restaurant is a comprehensive restaurant management system built to simulate real-world interactions between frontend JavaScript event handling and backend C# controllers. The application manages core restaurant operations including menu items, orders, table reservations, kitchen workflow, and payment processing.

## Features

### Menu Management
- **CRUD Operations**: Create, read, update menu items with details (name, size, price, dish type)
- **Menu Categorization**: Organize dishes by type (Appetizers, Main Courses, Desserts, etc.)
- **Dynamic Menu Display**: View and filter menu items dynamically

### Order Management
- **Create Orders**: Place orders by selecting menu items and quantities
- **Order Details**: View comprehensive order information including items, quantities, and prices
- **Order Editing**: Modify pending orders before payment
- **Order Status Tracking**: Track orders through different states (Pending, In Progress, Completed, Cancelled)
- **Automatic OrderID Generation**: Sequential order ID generation (ORDR0001, ORDR0002, etc.)
- **Table Status Integration**: Automatic table status updates when orders are placed or cancelled

### Table Management
- **Table Status Monitoring**: Track table availability (Available, Occupied, Reserved)
- **Status Updates**: Change table status with validation
- **Active Order Validation**: Prevent table status changes when active orders exist
- **Reservation Integration**: Link tables with reservation system

### Kitchen Dashboard
- **Active Orders View**: Display all pending and in-progress orders
- **Order Status Updates**: Update order status via AJAX calls
- **Order Statistics**: View counts of pending, in-progress, and completed orders
- **Popular Items Analytics**: Track the top 5 most ordered menu items
- **Real-time Updates**: Kitchen staff can update order status without page reload

### Reservation System
- **Create Reservations**: Book tables for customers with date/time selection
- **Customer Information**: Store customer name, contact, guest count, and special requests
- **Reservation Status Management**: Track reservations (Confirmed, Cancelled, Completed)
- **Table Assignment**: Link reservations to specific tables

### Payment Tracking
- **Payment Records**: Track payment status for orders
- **Payment Status**:  Monitor payment states (Pending, Paid)
- **Order-Payment Linking**: Associate payments with specific orders

## Technology Stack

### Backend
- **ASP.NET MVC 5**:  Model-View-Controller architecture
- **C# . NET Framework**: Server-side programming
- **Entity Framework 6**: Code-First ORM for database operations
- **SQL Server**:  Relational database management
- **LINQ**: Query operations on data collections

### Frontend
- **JavaScript/jQuery**: Client-side event handling and DOM manipulation
- **HTML5**: Markup and structure
- **CSS3**: Styling and layout
- **Bootstrap**:  Responsive UI framework
- **AJAX**: Asynchronous server communication

### Key Libraries & Dependencies
- jQuery 3.4.1
- Bootstrap
- Popper.js
- Entity Framework 6.2. 0
- Modernizr

## Database Schema

### Core Models

**MenuItem**
- DishID (Primary Key)
- DishName, Size, Price, DishType
- StarRating, TotalSold

**Table**
- TableID (Primary Key)
- TableNumber, Status

**Order**
- OrderID (Primary Key, String)
- EmployeeID, TableID (Foreign Keys)
- OrderDate, Status

**OrderDetail**
- OrderDetailID (Primary Key)
- OrderID, DishID (Foreign Keys)
- Quantity

**Payment**
- PaymentID (Primary Key)
- OrderID (Foreign Key)
- PaymentStatus, PaymentDate

**Employee**
- EmployeeID (Primary Key)
- EmployeeName, Position

**Reservation**
- ReservationID (Primary Key)
- TableID (Foreign Key)
- CustomerName, ContactNumber
- ReservationDateTime, NumberOfGuests
- SpecialRequests, Status

## Key Frontend-Backend Interactions

This project demonstrates practical JavaScript event handling working with C# backend controllers:

### 1. **AJAX Status Updates** (Kitchen Dashboard)
- **Frontend**: jQuery AJAX POST requests triggered by button clicks
- **Backend**: `KitchenController. UpdateOrderStatus()` receives and processes status changes
- **Response**: JSON success/failure sent back to update UI dynamically

### 2. **Form Validation & Submission** (Order Creation)
- **Frontend**: JavaScript validates form inputs and handles FormCollection data
- **Backend**: `OrderController.Create()` processes form data, generates OrderID, creates transactions
- **Database**: Multiple related records created atomically (Order, OrderDetails, Table status update)

### 3. **Dynamic Table Status Management**
- **Frontend**: JavaScript event handlers for status change buttons
- **Backend**: `TableController.UpdateStatus()` validates business rules before updates
- **Validation**: Checks for active orders and reservations before allowing status changes

### 4. **Cascading Data Operations**
- **Order Cancellation**: Updates order status AND returns table to available
- **Order Creation**: Creates order, order details, AND changes table status to occupied
- **Transaction Management**: Entity Framework database transactions ensure data consistency

## Getting Started

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.7. 2 or later
- SQL Server 2016 or later

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/HTHao-git/KTPhamRestaurant.git
