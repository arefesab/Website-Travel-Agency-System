# ✈️ Ofogh Air Agency — Travel Agency Management System

<p align="center">
  <strong>A full-featured travel agency web application for browsing and managing flights, hotels, reservations, and payments.</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 6">
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core MVC">
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-6.0-512BD4?style=for-the-badge" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/SQL%20Server-Supported-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server">
</p>

<p align="center">
  <a href="https://github.com/arefesab/Website-Travel-Agency-System">Repository</a>
  ·
  <a href="#-features">Features</a>
  ·
  <a href="#-getting-started">Getting Started</a>
  ·
  <a href="#-project-structure">Project Structure</a>
</p>

---

## 🌍 Overview

**Ofogh Air Agency** is an ASP.NET Core MVC travel-agency system designed around a realistic booking workflow.

The application brings together:

- ✈️ Flight discovery and management
- 🏨 Hotel discovery and management
- 🎫 Flight reservations
- 🛏️ Hotel reservations
- 💳 Payment workflow with a simulated gateway and ZarinPal integration
- 🔐 Cookie-based authentication for the management area
- 🧑‍💼 Administrative booking management
- 🌐 Persian / English interface support
- 🎨 Multiple visual themes, responsive layouts and UI animations
- 🗄️ SQL Server persistence through Entity Framework Core
- 🌱 Database seeding for first-run data

The codebase follows the familiar **MVC architecture** with controllers, views, models, services, helpers, EF Core migrations and static assets separated into their respective areas.

---

## ✨ Features

### ✈️ Flight Management

The system supports a complete flight lifecycle:

- Create, edit and delete flights
- Origin and destination management
- Flight date and entry-time information
- Ticket pricing
- Seat capacity
- Flight class
- Availability calculation
- Prevention of deleting flights with active bookings
- Upcoming / last-minute flight presentation

### 🎫 Flight Booking

Customers can reserve seats on available flights with:

- Passenger name
- Mobile number validation
- Number of seats
- Optional notes
- Automatic total-price calculation
- Seat availability checks
- Booking status tracking
- Temporary booking locks
- Payment and verification flow

### 🏨 Hotel Management

Hotels contain detailed accommodation information including:

- Hotel name and city
- Availability date range
- Nightly price
- Star rating
- Address
- Meal plans
- Hotel description
- Amenities
- Hotel photos
- Room photos
- Room name
- Room capacity
- Optional discount percentage

### 🛏️ Hotel Booking

Hotel reservations support:

- Check-in / check-out dates
- Automatic number-of-nights calculation
- Guest information
- Mobile number validation
- Special requests / notes
- Total-price calculation
- Booking status
- Payment authority / reference tracking
- Booking conflict protection

### 💳 Payment System

Payment is implemented behind an abstraction:

`IPaymentGateway`

Two gateway modes are available:

| Provider | Purpose |
|---|---|
| **Fake** | Local development and end-to-end testing without a real bank transaction |
| **ZarinPal** | Real payment request and payment verification flow |

The current repository configuration intentionally uses the **Fake** provider by default, so the application can be tested without real banking credentials.

### 🔐 Authentication & Admin Area

The application uses ASP.NET Core cookie authentication for the management area.

Administrative functionality includes:

- Admin login
- Flight management
- Hotel management
- Booking management
- Viewing booking details
- Managing reservation states
- Protection of management endpoints with authorization

### 🌐 Localization

The UI supports Persian and English presentation.

The application includes:

- RTL layout for Persian
- LTR layout for English
- Localized validation messages
- Language switching
- Google Translate integration for the English experience
- Separate RTL/LTR Bootstrap handling

### 🎨 UI / UX

The frontend is more than a default MVC template.

It includes:

- Responsive design
- Multiple visual themes
- Light / dark mode behavior
- Scroll-reveal animations
- Custom styling
- Responsive navigation
- Floating actions
- WhatsApp contact shortcut
- Back-to-top control
- Travel destination pages
- Hotel and flight imagery

---

## 🖼️ Screenshots

> **Screenshots will be added here to showcase the real UI.**
>
> Recommended screenshots:
>
> 1. 🏠 **Homepage** — hero section + flight/hotel search + special offers
> 2. ✈️ **Flight listing** — available flights and booking actions
> 3. 🎫 **Flight reservation** — passenger and seat reservation form
> 4. 🏨 **Hotel listing** — hotel cards, prices and discounts
> 5. 🛏️ **Hotel details / booking** — room information and booking form
> 6. 💳 **Payment flow** — payment / confirmation screen
> 7. 🧑‍💼 **Admin dashboard** — management area
> 8. 📋 **Admin bookings** — reservation management table
>
> These screenshots can be placed under:
>
> `docs/screenshots/`
>
> and displayed in this section as a visual walkthrough.

---

## 🧩 Architecture

The project follows an **ASP.NET Core MVC** structure:

```text
                    ┌──────────────────────┐
                    │      Browser / UI    │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │      MVC Views       │
                    │  Razor + Bootstrap   │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │     Controllers      │
                    │ Flights / Hotels /   │
                    │ Booking / Admin ...  │
                    └──────────┬───────────┘
                               │
                    ┌──────────┴───────────┐
                    ▼                      ▼
          ┌──────────────────┐   ┌──────────────────┐
          │ Application       │   │    Services      │
          │ DbContext         │   │ Payment / Seed   │
          └────────┬─────────┘   └────────┬─────────┘
                   │                      │
                   └──────────┬───────────┘
                              ▼
                    ┌──────────────────────┐
                    │     SQL Server       │
                    │   EF Core / Migrations│
                    └──────────────────────┘
```

---

## 🛠️ Tech Stack

### Backend

- **C#**
- **.NET 6**
- **ASP.NET Core MVC**
- **Entity Framework Core 6**
- **SQL Server / LocalDB**
- **Cookie Authentication**

### Frontend

- **Razor Views**
- **HTML5**
- **CSS3**
- **JavaScript**
- **Bootstrap 5**
- **jQuery**
- Responsive RTL / LTR layouts

### Integrations

- **ZarinPal Payment Gateway**
- **WhatsApp**
- **Google Translate**

---

## 📁 Project Structure

```text
Website-Travel-Agency-System/
│
├── Controllers/
│   ├── AdminController.cs
│   ├── AdminBookingsController.cs
│   ├── BookingController.cs
│   ├── FlightBookingController.cs
│   ├── FlightsController.cs
│   ├── FlightsshowController.cs
│   ├── HotelsController.cs
│   ├── HotelsshowController.cs
│   └── HomeController.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Helpers/
│   ├── FlightRoutes.cs
│   ├── HotelPricing.cs
│   ├── Lang.cs
│   ├── Lang.Messages.cs
│   └── ...
│
├── Models/
│   ├── DB/
│   │   ├── Booking.cs
│   │   ├── Flight.cs
│   │   ├── FlightBooking.cs
│   │   ├── Hotel.cs
│   │   └── UserLogin.cs
│   └── ...
│
├── Services/
│   ├── DbSeeder.cs
│   ├── PasswordHasher.cs
│   └── Payment/
│       ├── IPaymentGateway.cs
│       ├── FakePaymentGateway.cs
│       └── ZarinpalGateway.cs
│
├── Migrations/
│   └── Entity Framework Core migrations
│
├── SeedData/
│   └── Initial / seed data
│
├── Views/
│   ├── Admin/
│   ├── AdminBookings/
│   ├── Booking/
│   ├── FlightBooking/
│   ├── Flights/
│   ├── Flightsshow/
│   ├── Home/
│   ├── Hotels/
│   ├── Hotelsshow/
│   └── Shared/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── lib/
│   └── uploads/
│
├── Program.cs
├── appsettings.json
├── agancywebProject.csproj
└── agancywebProject.sln
```

---

## 🗃️ Data Model

The main persistence layer is built around **Entity Framework Core**.

Core entities include:

- `Flight`
- `Hotel`
- `Booking`
- `FlightBooking`
- `UserLogin`

Bookings keep track of important transaction information such as:

- Status
- Creation time
- Payment authority
- Payment reference
- Payment time
- Temporary lock time

This allows the system to distinguish between states such as:

`Pending → Locked → Paid`

as well as failed or cancelled reservations.

---

## ⚙️ Getting Started

### 1. Prerequisites

Install:

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 / VS Code (optional)

### 2. Clone the repository

```bash
git clone https://github.com/arefesab/Website-Travel-Agency-System.git
cd Website-Travel-Agency-System
```

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Configure the database

The default configuration uses SQL Server LocalDB:

```text
(localdb)\\MSSQLLocalDB
```

Database configuration is located in:

```text
appsettings.json
```

You can change `ConnectionStrings:DefaultConnectionString` if you want to use another SQL Server instance.

### 5. Apply migrations

```bash
dotnet ef database update
```

### 6. Run the application

```bash
dotnet run
```

Then open the local URL shown by ASP.NET Core.

---

## 💳 Payment Configuration

### Development mode

The project is configured to use a simulated payment gateway:

```json
"Payment": {
  "Provider": "Fake"
}
```

This is recommended while developing or testing the booking workflow.

### ZarinPal

To use ZarinPal, configure:

```json
"Payment": {
  "Provider": "Zarinpal",
  "Zarinpal": {
    "MerchantId": "YOUR_MERCHANT_ID",
    "Sandbox": true
  }
}
```

For production, use the appropriate production configuration and **do not commit real credentials to source control**.

---

## 🔄 Booking Flow

### Flight

```text
Browse flights
     │
     ▼
Select flight
     │
     ▼
Enter passenger information
     │
     ▼
Check seat availability
     │
     ▼
Create booking
     │
     ▼
Payment
     │
     ▼
Verify payment
     │
     ▼
Booking confirmed
```

### Hotel

```text
Browse hotels
     │
     ▼
Select hotel / room
     │
     ▼
Choose check-in & check-out
     │
     ▼
Calculate nights + price
     │
     ▼
Create booking
     │
     ▼
Payment
     │
     ▼
Verify payment
     │
     ▼
Booking confirmed
```

---

## 🛡️ Validation & Business Rules

The project includes server-side validation for important business constraints, including:

- Flight dates cannot be in the past when creating a flight
- Flight capacity must be positive
- Flight price must be positive
- Hotel availability dates must be valid
- Hotel price must be positive
- Hotel rating must be between 1 and 5
- Room capacity must be positive
- Hotel check-out must be after check-in
- Passenger phone numbers are validated
- Seat counts are validated
- Active bookings prevent unsafe deletion of flights
- Booking locks expire according to the application's booking rules

---

## 🎯 Project Highlights

This project is particularly useful as a demonstration of how a real-world MVC application can combine:

**UI + MVC + Database + Business Rules + Authentication + Booking Logic + Payment Integration**

rather than being only a static travel website.

---

## 🚀 Future Improvements

Potential next steps include:

- [ ] Add automated unit and integration tests
- [ ] Add CI/CD with GitHub Actions
- [ ] Add richer admin analytics and dashboards
- [ ] Add email / SMS booking notifications
- [ ] Add customer booking history
- [ ] Add stronger role-based authorization
- [ ] Add centralized application logging
- [ ] Add API endpoints for mobile clients
- [ ] Improve automated deployment configuration

---

## 👤 Author

**Aref Sab**

GitHub: [@arefesab](https://github.com/arefesab)

Project: [Website-Travel-Agency-System](https://github.com/arefesab/Website-Travel-Agency-System)

---

## 📄 License

No license file is currently defined in the repository.

If this project is intended to be reused publicly, adding an explicit open-source license is recommended.
