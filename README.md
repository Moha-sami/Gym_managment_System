# 🏋️ Gym Management System
A full-featured gym management web application built with ASP.NET Core MVC using a clean 3-Tier Architecture. Supports member self-service, health tracking, trainer management, session scheduling, bookings, and role-based authentication with an approval workflow for sensitive actions.

### 📸 Screenshots

| Home Page | Login Page |
| :---: | :---: |
| ![Home Page](screenshots/Home_page.png) | ![Login Page](screenshots/login-page.png) |

| Members | Trainers / Member Plans |
| :---: | :---: |
| ![Members](screenshots/Member_page.png) | ![Trainers](screenshots/Member_palns.png) |

| Plans | Memberships |
| :---: | :---: |
| ![Plans](screenshots/Plans_page.png) | ![Memberships](screenshots/Membership_page.png) |

| Sessions | Sessions Schedule |
| :---: | :---: |
| ![Sessions](screenshots/Session_Page.png) | ![Schedule](screenshots/SessionsSchedule_page.png) |

| Admin — User Management | Admin — Delete Requests |
| :---: | :---: |
| ![User Management](screenshots/User_Management_page.png) | ![Delete Requests](screenshots/DeleteRequest_page.png) |

🏗️ Architecture
This project follows the 3-Tier Architecture pattern, separating concerns across three distinct layers:

GymmanagmentSystem/
├── GymManagment.DAL/        # Data Access Layer  — Models, DbContext, Repositories, UnitOfWork, Identity
├── GymMangment.BLL/         # Business Logic Layer — Services, ViewModels, Mapping, Result Pattern
└── GymmanagmentSystem.PL/   # Presentation Layer  — Controllers, Views, wwwroot
LayerProjectResponsibilityDALGymManagment.DALDatabase models, EF Core, Identity, Generic Repository, Unit of WorkBLLGymMangment.BLLBusiness logic, service interfaces, ViewModels, AutoMapper profilesPLGymmanagmentSystem.PLMVC Controllers, Razor Views, UI, File uploads✨ Features
Public / Guest
✅ Landing page with live gym stats (Total Members, Active Members, Trainers, Sessions by status)
✅ Self-registration — automatically creates a linked Member profile, assigns the Member role, and subscribes to the Basic Plan
Members (Admin/Manager-managed)
✅ Full CRUD, health record tracking, required profile photo upload on Create
✅ Locked fields on edit (Name, DOB, Gender) to preserve identity integrity
Trainers
✅ Full CRUD with specialty tracking (Cardio, Strength, Boxing, CrossFit)
Plans
✅ List, view details, edit, Activate/Deactivate (soft delete)
✅ Members can browse active plans and switch their subscription directly from the Plans page
Sessions
✅ Full CRUD with trainer/category specialty matching validation
✅ Status-aware UI (Upcoming / Ongoing / Completed)
✅ Auto-seeded 7 days of upcoming sessions on first run
Memberships
✅ Assign member to a plan, auto-calculated end date based on plan duration
✅ Prevents duplicate active memberships per member
✅ Plan switching/upgrade flow for logged-in Members
Sessions Schedule & Bookings
✅ Browse available sessions and book a spot (requires active membership)
✅ Bookings are now tied to the logged-in user's own Member profile — no booking on behalf of others
✅ Cancel bookings, attendance tracking, capacity/slot enforcement
Member Self-Service Area
✅ My Profile — view and edit own contact/address details (Name, DOB, Gender locked)
✅ My Membership — current plan, price, start/end date, days remaining, quick link to switch plans
✅ My Bookings — view and cancel only their own session bookings
Authentication & Authorization
✅ ASP.NET Core Identity (custom AppUser, linked to a Member or Trainer record via MemberId/TrainerId)
✅ Roles: Admin, Manager, Member, Trainer
✅ Public registration → automatically linked Member profile + Member role + Basic Plan membership
✅ Admin can assign/change roles and delete accounts via User Management page
✅ Manager can create Members/Trainers/Sessions but cannot delete directly — submits a Delete Request for Admin approval/rejection
Data Seeding
✅ Plans, Categories, 4 Trainers, 10 Members (with avatar photos), and 7 days of upcoming Sessions seeded automatically on first run
✅ Roles (Admin, Manager, Member, Trainer) and default Admin + Manager accounts seeded on startup
✅ Idempotent — skips seeding if data already exists
🛠️ Tech Stack
TechnologyUsageASP.NET Core MVC (.NET 9)Web frameworkEntity Framework Core 9ORM / Database accessASP.NET Core IdentityAuthentication & AuthorizationSQL ServerDatabaseAutoMapperObject mapping (ViewModel ↔ Entity)Bootstrap 5UI stylingBootstrap IconsIcon setC#Primary language🧱 Design Patterns
PatternWhere Used3-Tier ArchitectureFull project structureGeneric RepositoryIGenericRepository<T> in DAL, with Include overloads for eager loadingUnit of WorkIUnitOfWork wrapping all repositoriesResult PatternResult<T> returned from all service methodsAutoMapperMappingProfile in BLLTempData Alert SystemGlobal success/warning/error banners in _Layout.cshtml, auto-dismiss after 3sApproval WorkflowManager-submitted Delete Requests reviewed by Admin before destructive actions executeIdentity-to-Domain LinkingAppUser.MemberId / AppUser.TrainerId connect login accounts to domain profiles for self-service scoping🔐 Default Seeded Accounts
RoleEmailPasswordAdminadmin@gymmanagement.comAdmin@1234Managermanager1@gymmanagement.comManager@1234Managermanager2@gymmanagement.comManager@1234⚠️ Change these credentials before deploying to production.
🚀 Getting Started
Prerequisites
.NET 9 SDK
SQL Server
Visual Studio 2022+ or VS Code
Installation
Clone the repository

git clone https://github.com/Moha-sami/GymmanagmentSystem.git
cd GymmanagmentSystem
Set up the connection string
In GymmanagmentSystem.PL/appsettings.json, update:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=GymDB;Trusted_Connection=True;"
}
Apply migrations

dotnet ef database update --project GymManagment.DAL --startup-project GymmanagmentSystem.PL
Run the application

dotnet run --project GymmanagmentSystem.PL
On first run, the database will be automatically seeded with sample Plans, Categories, Trainers, Members, Sessions, Roles, and Admin/Manager accounts.
Open your browser, register a new account (gets a Member profile + Basic Plan automatically), or log in with the Admin account above to manage the gym.
📁 Project Structure
GymManagment.DAL/
├── Models/
│   ├── BaseEntity.cs
│   ├── GymUser.cs (abstract)
│   ├── AppUser.cs (Identity, linked to Member/Trainer)
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
    ├── MembershipViewModels/ (incl. MyMembershipViewModel)
    ├── BookingViewModels/
    └── AccountViewModels/ (Login, Register, User)

GymmanagmentSystem.PL/
├── Controllers/
│   ├── HomeController.cs
│   ├── MembersController.cs (incl. MyProfile)
│   ├── PlansController.cs
│   ├── TrainersController.cs
│   ├── SessionsController.cs
│   ├── MembershipsController.cs (incl. MyMembership, UpgradePlan)
│   ├── SessionsScheduleController.cs
│   ├── BookingsController.cs (incl. MyBookings)
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
    └── images/
        ├── avatars/ (seeded member default photos)
        └── uploads/ (member profile photos uploaded via Create)
👨‍💻 Author
Moha-sami — @Moha-sami
📄 License
This project is open source and available under the MIT License.
