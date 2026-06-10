# 🏋️ Gym Management System

A full-featured gym management web application built with **ASP.NET Core MVC** using a clean **3-Tier Architecture**. Designed to streamline gym operations including member registration, health tracking, and membership management.

---

## 📸 Screenshots

> _Coming soon_

---

## 🏗️ Architecture

This project follows the **3-Tier Architecture** pattern, separating concerns across three distinct layers:

```
GymmanagmentSystem/
├── GymManagment.DAL/        # Data Access Layer  — Models, DbContext, Repositories
├── GymMangment.BLL/         # Business Logic Layer — Services, ViewModels, Interfaces
└── GymmanagmentSystem/      # Presentation Layer  — Controllers, Views, wwwroot
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **DAL** | `GymManagment.DAL` | Database models, Entity Framework Core, data access |
| **BLL** | `GymMangment.BLL` | Business logic, service interfaces, ViewModels |
| **PL**  | `GymmanagmentSystem` | MVC Controllers, Razor Views, UI |

---

## ✨ Features

- ✅ Member registration with personal & address information
- ✅ Health record tracking (Height, Weight, Blood Type, Notes)
- ✅ Gender selection with enum support
- ✅ Multi-tab form UI for clean data entry
- ✅ Member listing and management
- 🔲 Member edit & delete _(in progress)_
- 🔲 Membership plans & subscriptions _(planned)_
- 🔲 Authentication & authorization _(planned)_

---

## 🛠️ Tech Stack

| Technology | Usage |
|------------|-------|
| ASP.NET Core MVC | Web framework |
| Entity Framework Core | ORM / Database access |
| SQL Server | Database |
| Bootstrap 5 | UI styling |
| Bootstrap Icons | Icon set |
| C# | Primary language |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- Visual Studio 2022+ or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Moha-sami/GymmanagmentSystem.git
   cd GymmanagmentSystem
   ```

2. **Set up the connection string**

   In `GymmanagmentSystem/appsettings.json`, update:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=GymDB;Trusted_Connection=True;"
   }
   ```

3. **Apply migrations**
   ```bash
   dotnet ef database update --project GymManagment.DAL --startup-project GymmanagmentSystem
   ```

4. **Run the application**
   ```bash
   dotnet run --project GymmanagmentSystem
   ```

5. Open your browser at `https://localhost:PORT`

---

## 📁 Project Structure

```
GymManagment.DAL/
├── Models/
│   ├── Member.cs
│   ├── HealthRecord.cs
│   └── Enum/
│       └── Gender.cs
└── Data/
    └── AppDbContext.cs

GymMangment.BLL/
├── Services/
│   └── Interfaces/
│       └── ImemberService.cs
└── ViewModels/
    └── MemberViewModels/
        └── CreateMemberViewModel.cs

GymmanagmentSystem/
├── Controllers/
│   └── MembersController.cs
├── Views/
│   └── Members/
│       ├── Index.cshtml
│       └── Create.cshtml
└── wwwroot/
```

---

## 🤝 Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 👨‍💻 Author

**Moha-sami** — [@Moha-sami](https://github.com/Moha-sami)

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
