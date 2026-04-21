# CLAUDE.md

This file provides guidance to Claude Code when working with this repository.

## What is PitStop?

PitStop is a Romanian automotive directory web app. Users can find and review
auto repair shops, parts stores, car washes, paint shops, tire shops, and any
vehicle-related service for PKW, LKW, and motorcycles — all in one place.
The app is in Romanian and targets the Romanian market only.

## Features

### 🔍 Search & Discovery
- Search shops by keyword, city, or county
- Filter by category, minimum rating, and open now
- Sort results by recommended, rating, or review count
- Paginated results grid with shop cards
- Each card shows: photo, category badge, name, city, rating, review count,
  open/closed status, short description

### 🏪 Shop Profiles
- Full profile page with photo gallery
- Services list with name, description, and price range per service
- Opening hours per day of week (open/closed + time range)
- Contact info: phone, email, website, address
- Embedded Google Maps for directions
- Customer reviews section with star rating, text, author, and date
- Social share links (WhatsApp, Facebook, copy link)
- Category badge and verified status indicator

### 👤 User Accounts
- Register with email and password
- Login / logout
- Google OAuth login (optional, mobile-friendly)
- Save shops to favorites
- View and manage saved favorites
- Write, edit, and delete reviews
- View own review history
- Profile settings (name, email, password change)

### 🏗️ Shop Onboarding
- Public request form: shop name, category, city, contact person,
  phone, email, short description
- Admin receives and reviews the request
- Admin can approve or reject with an optional note
- On approval: shop owner receives an email with account setup link
- Owner sets password and fills in full profile
- Profile goes live after owner completes setup

### 🛠️ Shop Owner Dashboard
- Edit all profile information
- Upload, reorder, and delete photos
- Manage services list (add, edit, remove)
- Set opening hours per day
- View and respond to customer reviews
- See profile view statistics
- Manage contact details

### 🔐 Admin Dashboard
- View all pending shop requests in a table
- Approve or reject requests with optional note
- View and manage all listed shops
- Activate or deactivate any listing
- View and manage all registered users
- Overview stats: total shops, pending requests,
  active users, total reviews

### 📱 Mobile & Responsive
- Fully responsive on mobile, tablet, and desktop
- Collapsible mobile navigation menu
- Touch-friendly UI components
- Social deep links on shop profiles
  (tap to call, tap to WhatsApp, tap to navigate via Google Maps)

### 🗺️ Maps & Location
- Embedded Google Maps on every shop profile
- "Get directions" button linking to Google Maps navigation
- City/county displayed on all shop cards and profiles

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| UI | Blazor Server, Interactive Server render mode |
| CSS | Tailwind CSS v4 |
| ORM | EF Core 10 |
| Database | PostgreSQL (local dev) / Supabase (prod) |
| Auth | ASP.NET Core Identity, cookie-based |
| Icons | Google Material Symbols |
| Fonts | Manrope (headings), Inter (body) |

## Dependency Policy

Always use the latest stable versions of all packages unless noted otherwise.
Before adding any NuGet or npm package, verify the current stable version at:
- NuGet: https://www.nuget.org/packages/<package>
- npm: https://www.npmjs.com/package/<package>

You're free to add any libraries — DO NOT REINVENT THE WHEEL.

Prefer well-maintained, widely adopted libraries over obscure ones.
If a library hasn't been updated in over a year, find an alternative.


## Architecture

Clean architecture — dependency flow: Web → Infrastructure → Application → Domain

| Project | Role |
|---|---|
| `PitStop.Domain` | Entities, enums, domain interfaces — no external deps |
| `PitStop.Application` | Use cases, service interfaces, DTOs |
| `PitStop.Infrastructure` | EF Core DbContext, Identity, repositories, file storage |
| `PitStop.Web` | Blazor Server pages and components |

## Commands

```bash
# Run
dotnet run --project PitStop.Web

# Watch mode (hot reload)
dotnet watch --project PitStop.Web

# Build solution
dotnet build PitStop.sln

# Tailwind (runs automatically on build via MSBuild hook)
cd PitStop.Web
npm install           # first time only
npm run build:css     # manual watch mode

# EF Core migrations (run from solution root)
dotnet ef migrations add <Name> --project PitStop.Infrastructure --startup-project PitStop.Web
dotnet ef database update --project PitStop.Infrastructure --startup-project PitStop.Web
```

## UI

- All pages in `PitStop.Web/Components/Pages/`
- Layout in `PitStop.Web/Components/Layout/` — NavBar, Footer, MainLayout
- Shared components in `PitStop.Web/Components/Shared/`
- Tailwind theme in `wwwroot/css/app.css` → compiled to `wwwroot/css/app.min.css`

Brand color tokens (defined in `wwwroot/css/app.css` via Tailwind v4 `@theme`):

| Token | Hex | Usage |
|---|---|---|
| `brand-red` / `brand-red-dark` / `brand-red-faint` | `#C0392B` / `#9e2016` / `#fde8e6` | Primary accent, buttons, badges |
| `brand-navy` | `#1A1A2E` | Footer, dark CTA backgrounds |
| `brand-blue` | `#258CE4` | Info highlights |
| `hero-dark` | `#0d1117` | Hero section backgrounds |
| `surface` / `surface-bright` / `surface-container` / `surface-low` | — | Page & section backgrounds |
| `text-main` / `text-muted` / `text-warm` | `#161c22` / `#5a6476` / `#59413d` | Typography |
| `border-light` | `#e5e8ec` | Card and input borders |
| `secondary` / `secondary-light` | `#5d5c74` / `#e8e8f8` | Secondary icon circles |

**Never use raw hex values in components** — always use the token class (e.g. `text-brand-red`, `bg-surface-container`).

### Pages

| Route | Status | Component |
|---|---|---|
| `/` | ✅ Done | `Home.razor` |
| `/servicii` | ✅ Done | `Search.razor` |
| `/despre-noi` | ✅ Done | `AboutUs.razor` |
| `/contact` | ✅ Done | `Contact.razor` |
| `/serviciu/{id}` | ✅ Done | `ShopProfile.razor` |
| `/auth/login` | ✅ Done | `Auth/Login.razor` |
| `/auth/register` | ✅ Done | `Auth/Register.razor` (redirects to `/auth/login?tab=register`) |
| `/not-found` | ✅ Done | `NotFound.razor` |
| `/Error` | ✅ Done | `Error.razor` |
| `/dashboard` | ✅ Done | `User/Dashboard.razor` |
| `/shop/dashboard` | ✅ Done | `Shop/Dashboard.razor` |
| `/admin` | ✅ Done | `Admin/Dashboard.razor` |

### Page notes

- **`Home.razor`** — loads featured shops via `IShopRepository.GetFeaturedAsync(6)`. Hero, category grid, how-it-works section, CTA all wired.
- **`Search.razor`** — fully wired to `IShopRepository.SearchAsync`. Filters: keyword, category, county, min rating, open now. Pagination (6/page). Sort by recommended/rating/review count.
- **`ShopProfile.razor`** — loads shop by `Id` via `IShopRepository.GetByIdAsync`. Shows hero banner, about, services, gallery, paginated reviews with rating breakdown, sticky contact card, hours, Google Maps link, and top 3 similar shops (same category).
- **`Auth/Login.razor`** — single page with tab switching (login/register). `[SupplyParameterFromQuery] Tab` activates the register tab when navigating from `/auth/register`. Login calls `SignInManager.PasswordSignInAsync`; register calls `UserManager.CreateAsync` + `AddToRoleAsync("User")` + `SignInAsync`. Both navigate with `forceLoad: true` on success. Google OAuth button is UI-only.
- **`Contact.razor`** — Blazor form with `@onsubmit` + success state. FAQ accordion driven by `OpenFaq` (int?) — one item open at a time. Form data is saved to DB via `IShopRequestRepository.CreateAsync` when the subject is the listing request option.
- **`AboutUs.razor`** — static page: mission/vision, process steps, hardcoded team member cards (Alexandru Popescu/CEO, Mihai Ionescu/Lead Dev, Elena Radu/Community Manager), CTA.
- **`User/Dashboard.razor`** — favorites list, review history, profile settings (name/email), password change. Auth guard in `OnInitializedAsync` via `AuthenticationStateProvider`.
- **`Shop/Dashboard.razor`** — profile editing, services CRUD, hours management (per day), reviews viewing. Uses internal `HourRow` class with `string OpenTime`/`CloseTime` (format `"HH:mm"`). `<input type="time">` uses `value`/`@onchange` instead of `@bind` — Blazor infers `DateTime` for time inputs when using `@bind` on string properties.
- **`Admin/Dashboard.razor`** — pending shop requests table (approve/reject with note), all shops list (activate/deactivate), all users list, overview stats. Reject panel toggled via `_rejectingId`.

### Shared Components

| Component | File | Used In |
|---|---|---|
| `ShopCard` | `Shared/ShopCard.razor` | Home, Search — `Variant`: `Grid` (vertical) or `Row` (horizontal) |
| `StatsSection` | `Shared/StatsSection.razor` | Home, AboutUs — `Large`: compact inline (Home) or full section with bg (AboutUs) |
| `CTASection` | `Shared/CTASection.razor` | Home, AboutUs, Contact — `Variant`: `DarkHorizontal` or `RedCentered` |
| `TeamSection` | `Shared/TeamSection.razor` | AboutUs |

`ShopCardModel` and `ShopCardVariant` are defined in `Shared/ShopCard.cs` — use this record for all shop data, not page-local records.

## Blazor conventions

- Every page or component with `@onclick`, `@bind`, or any other interactivity **must** have `@rendermode InteractiveServer` — either declared at the top of the component or inherited from `Routes.razor`.
- Never use raw hex color values (`bg-[#C0392B]`) — always use token classes (`bg-brand-red`). The token table above is the single source of truth.
- `ShopCardModel` is the unified data model for shop data across all pages — do not define page-local shop records.
- **`<input type="time">` with a `string` property** — do NOT use `@bind` (Blazor infers `DateTime`). Use `value="@field" @onchange="e => field = e.Value?.ToString() ?? string.Empty"` instead.
- **Inline `@onclick` lambdas with double quotes** — if the lambda body contains `""` (empty string), wrap the attribute in single quotes: `@onclick='() => { x = y ?? ""; }'`.

### Auth conventions

- **Do NOT use `AuthorizeRouteView` or `CascadingAuthenticationState`** — these cause cascading parameter errors and redirect loops in this setup. Use plain `RouteView` in `Routes.razor`.
- **Do NOT use `<AuthorizeView>` component** — it requires a cascading `Task<AuthenticationState>`. Instead, inject `AuthenticationStateProvider` directly and use `_isAuthenticated` / `_userName` fields set in `OnInitializedAsync`.
- **Do NOT use `@attribute [Authorize]` on Blazor pages** — without `AuthorizeRouteView`, the attribute is not enforced and causes errors. Guard pages manually in `OnInitializedAsync` via `AuthenticationStateProvider`.
- **NavBar** reads auth state via injected `AuthenticationStateProvider` in `OnInitializedAsync`. After login/register/logout, always use `NavigateTo(..., forceLoad: true)` so the NavBar re-initializes with the new auth state.
- **`_Imports.razor`** already has `@using Microsoft.AspNetCore.Authorization`, `@using Microsoft.AspNetCore.Identity`, `@using PitStop.Infrastructure.Identity` — no need to repeat these in individual components.

## Data

All public pages are wired to real repository calls. Hardcoded data remains only in `AboutUs.razor` (team members) and `StatsSection.razor` (stats numbers).

### Connection string

`appsettings.json`:
```
"ConnectionStrings": { "DefaultConnection": "Host=localhost;Database=pitstop_dev;Username=postgres;Password=postgres" }
```
`appsettings.Development.json` overrides the password (`Password=juliandev`) and enables `Microsoft.EntityFrameworkCore.Database.Command: Information` to log SQL.

### Program.cs registration order

1. `AddRazorComponents` + `AddInteractiveServerComponents`
2. `AddDbContext<AppDbContext>` with Npgsql
3. `AddIdentity<ApplicationUser, IdentityRole>` → `AddEntityFrameworkStores<AppDbContext>` → `AddDefaultTokenProviders`
   - Password: `RequireDigit=true`, `MinimumLength=8`, `RequireUppercase=false`
   - User: `RequireUniqueEmail=true`
4. `ConfigureApplicationCookie` — `LoginPath=/auth/login`, `AccessDeniedPath=/auth/access-denied`, 30-day sliding expiration
5. `AddAuthorization` with policies: `"ShopOwner"` (requires role ShopOwner), `"Admin"` (requires role Admin)
6. All repositories as `AddScoped`; `IFileStorage → LocalFileStorage` as `AddScoped`

### Middleware order

`UseStaticFiles` → `UseAuthentication` → `UseAuthorization` → `UseAntiforgery` → `MapStaticAssets` → `MapRazorComponents`

`UseStaticFiles` is before auth so that `wwwroot/uploads/` is public without login.

### Role seeding

`SeedRolesAsync` runs after `app.Build()`, creates roles `Admin`, `ShopOwner`, `User` if they don't exist. Uses `CreateAsyncScope` to avoid DI lifetime issues.

`SeedAdminAsync` runs immediately after, creates a default admin user (`admin@pitstop.ro` / `Admin1234!`) if one doesn't exist and assigns the `Admin` role. Safe to leave in production — no-op if user already exists.

## Domain

Entities in `PitStop.Domain/Entities/`:

| Entity | Inherits | Key fields |
|---|---|---|
| `BaseEntity` | — | `Id` (int), `CreatedAt`, `UpdatedAt` |
| `Shop` | `BaseEntity` | `Name`, `Description`, `Address`, `City`, `County`, `Phone`, `Email`, `Website?`, `CoverImage?`, `Category`, `Status`, `OwnerId?`, `AverageRating`, `ReviewCount` |
| `Review` | `BaseEntity` | `ShopId`, `UserId`, `UserName`, `UserInitials`, `Rating` (1–5), `Text`, `UsefulCount` |
| `ShopPhoto` | `BaseEntity` | `ShopId`, `Url`, `DisplayOrder` |
| `ShopService` | `BaseEntity` | `ShopId`, `Name`, `Description`, `PriceMin`, `PriceMax` (decimal) |
| `ShopHour` | `BaseEntity` | `ShopId`, `DayOfWeek` (int, 0=Monday 6=Sunday), `OpenTime?`, `CloseTime?`, `IsClosed` |
| `ShopBrand` | `BaseEntity` | `ShopId`, `Name` |
| `FavoriteShop` | `BaseEntity` | `UserId` (string), `ShopId` |
| `ShopRequest` | — (own `Id` + `CreatedAt`) | `ShopName`, `Category`, `City`, `County`, `ContactPerson`, `Phone`, `Email`, `Description?`, `Status`, `AdminNote?` |

Enums in `PitStop.Domain/Enums/`:
- `ShopCategory` — `ServiceAuto`, `PieseAuto`, `Spalatorie`, `Vopsitorie`, `Vulcanizare`, `Tuning`, `Tractari`, `Detailing`, `ITP`, `ServiceMoto`, `PieseMoto`, `Altele`
- `ShopStatus` — `Pending`, `Active`, `Inactive`, `Rejected`
- `ShopRequestStatus` — `Pending`, `Approved`, `Rejected`

Notes:
- `IsOpen` is **not** a field on `Shop` — compute it at runtime from `ShopHour` records
- `OwnerId` on `Shop` is a string FK to `ApplicationUser` (defined in Infrastructure)
- `UserId` on `Review` and `FavoriteShop` are string FKs — no navigation to `ApplicationUser` in Domain
- `ShopRequest` does not inherit `BaseEntity` because it has no `UpdatedAt`

## Infrastructure

### Identity
`PitStop.Infrastructure/Identity/ApplicationUser.cs` — extends `IdentityUser`, adds `FullName`, `CreatedAt`, `ProfilePhoto?`.

### DbContext
`PitStop.Infrastructure/Data/AppDbContext.cs` — extends `IdentityDbContext<ApplicationUser>`.

DbSets: `Shops`, `Reviews`, `ShopPhotos`, `ShopServices`, `ShopHours`, `ShopRequests`, `FavoriteShops`, `ShopBrands`.

`SaveChangesAsync` automatically sets `CreatedAt` + `UpdatedAt` on `BaseEntity` entries (Added sets both; Modified sets only `UpdatedAt`).

`OnModelCreating` calls `base` first, then `ApplyConfigurationsFromAssembly` — all configurations are picked up automatically.

### EF Core Configurations (`Data/Configurations/`)

| File | Entity | Key rules |
|---|---|---|
| `ShopConfiguration` | `Shop` | Required + max lengths; index on `City`, index on `Category`; `OwnerId` max 450 |
| `ReviewConfiguration` | `Review` | Check constraint `Rating BETWEEN 1 AND 5`; unique index on `(ShopId, UserId)` |
| `ShopServiceConfiguration` | `ShopService` | `PriceMin` + `PriceMax` precision `(10, 2)` |
| `ShopRequestConfiguration` | `ShopRequest` | Required + max lengths for all string fields |
| `FavoriteShopConfiguration` | `FavoriteShop` | Unique index on `(UserId, ShopId)` |

`ShopBrand`, `ShopPhoto`, and `ShopHour` use EF Core conventions (no explicit configuration files needed yet).

### Repositories (`Repositories/`)

Interfaces live in `PitStop.Application/Interfaces/`. Implementations take `AppDbContext` via primary constructor.

| Interface | Implementation | Notes |
|---|---|---|
| `IShopRepository` | `ShopRepository` | `SearchAsync` builds `IQueryable` pipeline; `openNow` maps .NET `DayOfWeek` (Sunday=0) → project convention (Monday=0) via `((int)dow + 6) % 7` |
| `IReviewRepository` | `ReviewRepository` | `GetAverageRatingAsync` queries live from DB; `DeleteAsync` uses `ExecuteDeleteAsync` |
| `IShopRequestRepository` | `ShopRequestRepository` | `GetPendingAsync` ordered by `CreatedAt ASC` (oldest first for admin queue) |
| `IFavoriteShopRepository` | `FavoriteShopRepository` | `AddAsync` is idempotent (no-op if already exists); `RemoveAsync` uses `ExecuteDeleteAsync` |
| `IFileStorage` | `LocalFileStorage` | Saves to `wwwroot/uploads/<folder>/`, returns `/uploads/<folder>/<file>` URL |

`SearchAsync` returns `(List<Shop> Items, int TotalCount)` — callers are responsible for computing `TotalPages`.

#### IShopRepository methods
- `GetByIdAsync(id)` — full shop with photos, hours, services, brands, reviews
- `GetAllAsync()` — all shops ordered by rating
- `SearchAsync(query, city, category?, minRating?, openNow?, page, pageSize)` — returns `(Items, TotalCount)`
- `GetFeaturedAsync(count)` — top shops by rating + review count
- `GetByOwnerIdAsync(ownerId)` — shop for logged-in owner
- `CreateAsync`, `UpdateAsync`, `UpdateProfileAsync`, `DeleteAsync`
- `AddServiceAsync`, `UpdateServiceAsync`, `DeleteServiceAsync`
- `UpsertHoursAsync(shopId, hours)` — replaces all hours for a shop
- `SetStatusAsync(shopId, status)`

#### IReviewRepository methods
- `GetByShopIdAsync(shopId, page, pageSize)` — paginated
- `GetByUserIdAsync(userId)` — user's own reviews
- `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- `GetAverageRatingAsync(shopId)`, `GetTotalCountAsync()`

#### IFavoriteShopRepository methods
- `GetByUserIdAsync(userId)` — with shop + photos eager-loaded
- `AddAsync(userId, shopId)`, `RemoveAsync(userId, shopId)`
- `IsFavoriteAsync(userId, shopId)`

### File Storage (`Storage/`)

`LocalFileStorage` requires `IWebHostEnvironment` — Infrastructure csproj has `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to enable this.

Uploaded files are stored in `wwwroot/uploads/{folder}/` with a `Guid`-prefixed filename to avoid collisions. `DeleteAsync` accepts both relative URL paths and absolute disk paths.

## Design

UI designs are generated in Google Stitch then ported to Blazor manually.
Reference prototype: `wwwroot/prototypes/homepage.html`

claude --resume 8dbc8f05-5769-48b1-aec1-ad56a1673126