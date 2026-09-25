# 🌸 FlowerShop

FlowerShop is a full-stack **flower ordering and management application** developed primarily in **C#** using **.NET MAUI** for the client application and **ASP.NET Core Minimal API** for the backend.

The system provides separate experiences for customers and administrators. Customers can browse flowers, search and filter products, place orders, choose delivery details and track their orders, while administrators can manage products, categories, users, and customer orders.

## 🛠️ Languages & Technologies

### Programming Languages

* **C#** — Main programming language
* **XAML** — User interface design

### Frameworks & Tools

* **.NET MAUI**
* **ASP.NET Core Minimal API**
* **Entity Framework Core**
* **Microsoft SQL Server**
* **CommunityToolkit.Maui**
* **Swagger / OpenAPI**
* **Visual Studio 2022**

## ✨ Features

### 👤 Customer

* Create a new account
* Login and logout
* Reset and change password
* Browse available flowers
* Search products by name
* Filter products by category
* View product details
* Create orders
* Select product quantity
* Enter a delivery address
* Choose delivery date and time
* Surprise delivery option
* Select a payment method
* View previous and active orders
* Track order status

### 👑 Administrator

* Admin dashboard
* View order, product, user, and revenue statistics
* Add, edit, and delete products
* Add, edit, and delete categories
* View registered users
* View all customer orders
* Update order status
* Track record creation and update information

## 🛒 Ordering System

Customers can select a flower and create an order by providing:

* Quantity
* Delivery address
* Delivery date
* Delivery time
* Surprise delivery preference
* Payment method

The application includes **online payment** and **payment on delivery** options. Payment on delivery can be selected as cash or card.

## 🔐 Authentication

FlowerShop includes an authentication system for customers and administrators.

The application supports:

* Registration
* Login
* Logout
* Password reset
* Password change
* Token-based authenticated requests
* Admin and standard user functionality
* Password hashing

## 🌐 REST API

The backend is built with **ASP.NET Core Minimal API**.

Endpoints are implemented using:

* `MapGet`
* `MapPost`
* `MapPut`
* `MapDelete`

The API handles operations for:

* Authentication
* Products
* Categories
* Orders
* Users
* Admin functionality

The API can also be explored and tested through **Swagger UI**.

## 🗄️ Database

FlowerShop uses **Microsoft SQL Server** with **Entity Framework Core Code First**.

Main database entities include:

* `Product`
* `Category`
* `User`
* `Order`
* `OrderItem`
* `UserSession`

Entity Framework migrations are included to manage database schema changes.

The project also tracks information such as when records are created or updated and which user performed the operation.

## 📂 Project Structure

```text
FlowerShop/
│
├── FlowerShop.Api/       # ASP.NET Core Minimal API backend
├── FlowerShop.Maui/      # .NET MAUI client application
└── FlowerShop.sln        # Visual Studio solution
```

## 🏗️ Architecture

```text
┌──────────────────────┐
│   .NET MAUI Client   │
│     C# + XAML        │
└──────────┬───────────┘
           │
           │ HTTP Requests
           ▼
┌──────────────────────┐
│ ASP.NET Core         │
│ Minimal API          │
└──────────┬───────────┘
           │
           │ Entity Framework Core
           ▼
┌──────────────────────┐
│ Microsoft SQL Server │
└──────────────────────┘
```

## 🖥️ User Interface

The client application is developed using **.NET MAUI and XAML**.

It uses MAUI controls including:

* `CollectionView`
* `Picker`
* `DatePicker`
* `TimePicker`
* `Editor`
* `RadioButton`
* `Grid`

Products are displayed using responsive collection-based layouts, with support for searching and category filtering.

## 🚀 Getting Started

### Requirements

To run the project, you will need:

* Visual Studio 2022
* .NET SDK
* .NET MAUI workload
* Microsoft SQL Server
* Entity Framework Core tools

### Setup

1. Clone the repository.

2. Open `FlowerShop.sln` in Visual Studio.

3. Configure your SQL Server connection string in the API project's `appsettings.json`.

4. Apply the Entity Framework Core migrations.

5. Start the `FlowerShop.Api` project.

6. Start the `FlowerShop.Maui` application.

The MAUI client will communicate with the backend API to retrieve and manage application data.

## 🎓 About the Project

FlowerShop was developed as a **Computer Programming II course project**.

The project demonstrates the development of a full-stack application using **C#, .NET MAUI, ASP.NET Core Minimal API, Entity Framework Core, and SQL Server**, including authentication, CRUD operations, database management, REST API communication, and role-based application functionality.
