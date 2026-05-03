# PitStop 🚗💨

PitStop is a comprehensive Romanian automotive directory web application. It serves as a one-stop platform for users to find and review various vehicle-related services across Romania, including auto repair shops, parts stores, car washes, tire shops, and more.

## 🌟 Features

### 🔍 Search & Discovery
- Search by keyword, city, or county.
- Filter by category, minimum rating, and current availability ("Open Now").
- Sort by recommendation, rating, or review count.
- Detailed shop cards showing photos, ratings, and status.

### 🏪 Shop Profiles
- Full profile pages with photo galleries and lightboxes.
- Detailed service lists with price ranges.
- Opening hours and contact information (Phone, Email, WhatsApp).
- Interactive Google Maps integration for directions.
- Customer reviews and rating breakdowns.

### 👤 User Accounts
- Secure registration and login (ASP.NET Core Identity).
- Google OAuth support for easy access.
- Personalized "Favorites" list to save preferred shops.
- Review management (write, edit, or delete your own reviews).

### 🏗️ Shop Onboarding & Management
- Public request form for shop owners to list their services.
- Admin approval workflow with automated notifications.
- Dedicated **Shop Owner Dashboard** for profile management, photo uploads, and service updates.
- **Admin Dashboard** for overseeing all requests, users, and listings.

## 🛠️ Tech Stack

- **Framework:** .NET 10
- **UI:** Blazor Server (Interactive Server mode)
- **Styling:** Tailwind CSS v4
- **Database:** PostgreSQL (Production), SQLite (Development fallback)
- **ORM:** Entity Framework Core 10
- **Auth:** ASP.NET Core Identity
- **Icons:** Google Material Symbols
- **Fonts:** Manrope (Headings), Inter (Body)

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js & npm](https://nodejs.org/) (for Tailwind CSS)
- PostgreSQL (optional, defaults to SQLite for dev)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-repo/pitstop.git
   cd pitstop
   ```

2. **Install dependencies:**
   ```bash
   cd PitStop.Web
   npm install
   cd ..
   ```

3. **Database Setup:**
   By default, the project uses SQLite in development. If you wish to use PostgreSQL, update `appsettings.Development.json` with your connection string and ensure `DatabaseProvider` is not set to `"Sqlite"`.

4. **Run the application:**
   ```bash
   dotnet run --project PitStop.Web
   ```

## 📂 Project Structure

The project follows Clean Architecture principles:

- **`PitStop.Domain`**: Core entities, enums, and domain logic.
- **`PitStop.Application`**: Business logic, interfaces, and DTOs.
- **`PitStop.Infrastructure`**: Data access (EF Core), Identity implementation, and external services.
- **`PitStop.Web`**: The Blazor Server UI, components, and static assets.

## 💻 Common Commands

| Task | Command |
|---|---|
| Run Project | `dotnet run --project PitStop.Web` |
| Watch Mode (hot reload) | `dotnet watch --project PitStop.Web` |
| Tailwind CSS watcher | `cd PitStop.Web && npm run build:css` |
| Build Solution | `dotnet build PitStop.sln` |
| Add Migration | `dotnet ef migrations add <Name> --project PitStop.Infrastructure --startup-project PitStop.Web` |
| Update Database | `dotnet ef database update --project PitStop.Infrastructure --startup-project PitStop.Web` |

> **Hot reload in Rider:** Run the project normally and use the flame icon in the toolbar. For CSS changes, also run `npm run build:css` in a terminal — Tailwind is compiled separately and is not covered by Rider's hot reload.

## 🎨 UI & Design

The UI is built with Tailwind CSS v4 using a custom design system.
- **Primary Color:** Brand Red (`#C0392B`)
- **Typography:** Manrope for headings, Inter for body text.

---
*PitStop - Găsește serviciul auto potrivit, rapid și ușor.*
