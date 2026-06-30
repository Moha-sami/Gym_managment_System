# 🏋️ Gym Management System

A full-featured gym management web application built with **ASP.NET Core MVC** using a clean **3-Tier Architecture**. Supports member registration, health tracking, trainer management, session scheduling, bookings, and role-based authentication.

---

## 📸 Screenshots

> _Coming soon_

---

## 🏗️ Architecture

This project follows the **3-Tier Architecture** pattern, separating concerns across three distinct layers:

```
GymmanagmentSystem/
├── GymManagment.DAL/        # Data Access Layer  — Models, DbContext, Repositories, UnitOfWork, Identity
├── GymMangment.BLL/         # Business Logic Layer — Services, ViewModels, Mapping, Result Pattern
└── GymmanagmentSystem.PL/   # Presentation Layer  — Controllers, Views, wwwroot
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **DAL** | `GymManagment.DAL` | Database models, EF Core, Identity, Generic Repository, Unit of Work |
| **BLL** | `GymMangment.BLL` | Business logic, service interfaces, ViewModels, AutoMapper profiles |
| **PL**  | `GymmanagmentSystem.PL` | MVC Controllers, Razor Views, UI, File uploads |

---

## ✨ Features

### Members
- ✅ List, create, view details, edit, delete (with confirmation)
- ✅ Health record tracking (Height, Weight, Blood Type, Notes) integrated into Create flow
- ✅ Profile photo upload (required on Create)

### Trainers
- ✅ Full CRUD with specialty tracking (Cardio, Strength, Boxing, CrossFit)
- ✅ Locked fields on edit (Name, DOB, Gender)

### Plans
- ✅ List, view details, edit
- ✅ Activate / Deactivate (soft delete via `IsActive` toggle)

### Sessions
- ✅ Full CRUD with trainer/category specialty matching validation
- ✅ Status-aware UI (Upcoming / Ongoing / Completed)

### Memberships
- ✅ Assign member to a plan, auto-calculated end date based on plan duration
- ✅ Prevents duplicate active memberships per member

### Sessions Schedule & Bookings
- ✅ Browse available sessions and book a spot (requires active membership)
- ✅ Cancel bookings, attendance tracking
- ✅ Capacity and slot availability enforcement

### Home Dashboard
- ✅ Live KPIs: Total Members, Active Members, Trainers, Upcoming/Ongoing/Completed Sessions
- ✅ Public landing page with Register/Login CTAs for guests

### Authentication & Authorization
- ✅ ASP.NET Core Identity (custom `AppUser`)
- ✅ Roles: **Admin**, **Manager**, **Member**, **Trainer**
- ✅ Public registration → automatically assigned **Member** role
- ✅ Admin can assign/change roles via User Management page
- ✅ Manager can create Members/Trainers/Sessions but cannot delete directly — submits a **Delete Request** for Admin approval/rejection

### Data Seeding
- ✅ Plans, Categories, Trainers, Members, and 7 days of upcoming Sessions seeded automatically on first run
- ✅ Roles (Admin, Manager, Member, Trainer) and a default Admin account seeded on startup
- ✅ Idempotent — skips seeding if data already exists

---

## 🛠️ Tech Stack

| Technology | Usage |
|------------|-------|
| ASP.NET Core MVC (.NET 9) | Web framework |
| Entity Framework Core 9 | ORM / Database access |
| ASP.NET Core Identity | Authentication & Authorization |
| SQL Server | Database |
| AutoMapper | Object mapping (ViewModel ↔ Entity) |
| Bootstrap 5 | UI styling |
| Bootstrap Icons | Icon set |
| C# | Primary language |

---

## 🧱 Design Patterns

| Pattern | Where Used |
|---------|-----------|
| **3-Tier Architecture** | Full project structure |
| **Generic Repository** | `IGenericRepository<T>` in DAL, with `Include` overloads for eager loading |
| **Unit of Work** | `IUnitOfWork` wrapping all repositories |
| **Result Pattern** | `Result<T>` returned from all service methods |
| **AutoMapper** | `MappingProfile` in BLL |
| **TempData Alert System** | Global success/warning/error banners in `_Layout.cshtml`, auto-dismiss after 3s |
| **Approval Workflow** | Manager-submitted Delete Requests reviewed by Admin before destructive actions execute |

---

## 🔐 Default Seeded Accounts

| Role | Email | Password |
|---|---|---|
| Admin | `admin@gymmanagement.com` | `Admin@1234` |
| Manager | `manager1@gymmanagement.com` | `Manager@1234` |
| Manager | `manager2@gymmanagement.com` | `Manager@1234` |

> ⚠️ Change these credentials before deploying to production.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- Visual Studio 2022+ or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Moha-sami/GymmanagmentSystem.git
   cd GymmanagmentSystem
   ```

2. **Set up the connection string**

   In `GymmanagmentSystem.PL/appsettings.json`, update:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=GymDB;Trusted_Connection=True;"
   }
   ```

3. **Apply migrations**
   ```bash
   dotnet ef database update --project GymManagment.DAL --startup-project GymmanagmentSystem.PL
   ```

4. **Run the application**
   ```bash
   dotnet run --project GymmanagmentSystem.PL
   ```

   On first run, the database will be automatically seeded with sample Plans, Categories, Trainers, Members, Sessions, Roles, and an Admin account.

5. Open your browser at `https://localhost:PORT` and log in with the Admin account above.

---

## 📁 Project Structure

```
GymManagment.DAL/
├── Models/
│   ├── BaseEntity.cs
│   ├── GymUser.cs (abstract)
│   ├── AppUser.cs (Identity)
│   ├── Member.cs
│   ├── HealthRecord.cs
│   ├── Trainer.cs
│   ├── Plans.cs
│   ├── Membership.cs
│   ├── Session.cs
│   ├── Booking.cs
│   ├── DeleteRequest.cs
│   └── Enum/ (Gender, Specialty, Categories, DeleteTargetType, DeleteRequestStatus)
├── DbContext/
│   └── GymDbcontext.cs (IdentityDbContext)
└── Repositories/
    ├── Interfaces/ (IGenericRepository, IUnitOfWork)
    └── Class/ (GenericRepository, UnitOfWork)

GymMangment.BLL/
├── Common/
│   └── Result.cs
├── Mapping/
│   └── MappingProfile.cs
├── Services/
│   ├── Interfaces/
│   └── Class/ (MemberService, PlanService, TrainerService, SessionService,
│                MembershipService, ScheduleService, AnalyticsService, FileService)
└── ViewModels/
    ├── MemberViewModels/
    ├── HealthRecordsViewModels/
    ├── PlansViewModels/
    ├── TrainerViewModels/
    ├── SessionsViewModels/
    ├── MembershipViewModels/
    ├── BookingViewModels/
    └── AccountViewModels/ (Login, Register, User)

GymmanagmentSystem.PL/
├── Controllers/
│   ├── HomeController.cs
│   ├── MembersController.cs
│   ├── PlansController.cs
│   ├── TrainersController.cs
│   ├── SessionsController.cs
│   ├── MembershipsController.cs
│   ├── SessionsScheduleController.cs
│   ├── BookingsController.cs
│   ├── AccountController.cs
│   ├── AdminController.cs
│   └── DeleteRequestsController.cs
├── Services/
│   └── FileService.cs (implements IFileService)
├── DataSeeder.cs
├── Views/
│   └── (one folder per controller, plus Shared/_Layout.cshtml)
└── wwwroot/
    ├── data/ (plans.json, members.json, trainers.json — seed sources)
    └── images/uploads/ (member profile photos)
```

---

## 👨‍💻 Author

**Moha-sami** — [@Moha-sami](https://github.com/Moha-sami)

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
