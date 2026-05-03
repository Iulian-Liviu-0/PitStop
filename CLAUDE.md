# CLAUDE.md

This file provides guidance to Claude Code when working with this repository.

## Pending work (not yet implemented)

- **Email sending** — `IEmailService` / `SmtpEmailService` (MailKit) is wired and called in
  `Admin/Dashboard.razor` (shop request approval), `Auth/ForgotPassword` (reset link), and
  `Contact.razor` (non-listing contact form subjects). SMTP credentials must be configured in
  `appsettings.json` (`Email:Host`, `Email:Port`, `Email:Username`, `Email:Password`,
  `Email:From`) for emails to actually send — failures are silently swallowed so no user flow
  is ever blocked.
- **Google OAuth** — Code is fully wired: `/auth/google-login` issues the challenge,
  `/auth/google-callback` handles the response (creates or links the `ApplicationUser`).
  To activate, replace `YOUR_GOOGLE_CLIENT_ID` / `YOUR_GOOGLE_CLIENT_SECRET` in
  `appsettings.json` with real credentials from the Google Cloud Console.

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
- Full profile page with photo gallery (lightbox with keyboard navigation)
- Services list with name, description, and price range per service
- Opening hours per day of week (open/closed + time range)
- Contact info: phone, email, website, address
- Embedded Google Maps for directions
- Customer reviews section with star rating, text, author, and date
- Social share links (WhatsApp, Facebook, copy link)
- Category badge and verified status indicator
- Report a problem modal

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
- Manage brand/make associations
- Red banner shown when shop is disabled by admin, including the reason

### 🔐 Admin Dashboard
- View all pending shop requests in a table
- Approve or reject requests with optional note
- View and manage all listed shops (search, edit, activate/deactivate with reason)
- View and manage all registered users (search, edit, reset password, lock, soft-disable with reason, restore)
- Overview stats: total shops, pending requests, active users, total reviews
- **SuperAdmin** tier: cannot be deleted or demoted by regular admins; only SuperAdmin can manage Admin-role users

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
| Database | PostgreSQL / SQLite (dev fallback) / Supabase (prod) |
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

> **SQLite + migrations:** The SQLite dev path uses `EnsureCreated()` (no migration history). If you add a migration and need to apply it locally, delete `PitStop.Web/pitstop_dev.db` and restart — `EnsureCreated()` will recreate the schema. Migrations apply normally to PostgreSQL.
>
> **`Directory.Build.props`** at the solution root sets `<AllowMissingPrunePackageData>true</AllowMissingPrunePackageData>` to suppress a .NET 10 RC SDK error that breaks `dotnet ef` and `dotnet build`.

## UI

- All pages in `PitStop.Web/Components/Pages/`
- Layout in `PitStop.Web/Components/Layout/` — NavBar, Footer, MainLayout
- Shared components in `PitStop.Web/Components/Shared/`
- Shared utilities in `PitStop.Web/Utilities/` — `DisplayHelpers.cs`
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

### Shared Utilities

`PitStop.Web/Utilities/DisplayHelpers.cs` — static class, imported globally via
`@using static PitStop.Web.Utilities.DisplayHelpers` in `_Imports.razor`.

| Method | Purpose |
|---|---|
| `CategoryDisplayName(ShopCategory)` | Maps enum to Romanian display string |
| `Initials(string name)` | Extracts 1–2 initials from a full name |

**Do not copy these into individual pages** — they are available everywhere via `_Imports.razor`.

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
| `/auth/access-denied` | ✅ Done | `Auth/AccessDenied.razor` |
| `/auth/forgot-password` | ✅ Done | `Auth/ForgotPassword.razor` |
| `/auth/set-password` | ✅ Done | `Auth/SetPassword.razor` |
| `/not-found` | ✅ Done | `NotFound.razor` |
| `/Error` | ✅ Done | `Error.razor` |
| `/dashboard` | ✅ Done | `User/Dashboard.razor` |
| `/shop/dashboard` | ✅ Done | `Shop/Dashboard.razor` |
| `/admin` | ✅ Done | `Admin/Dashboard.razor` |

### Page notes

- **`Home.razor`** — loads featured shops via `IShopRepository.GetFeaturedAsync(6)`. Hero, category grid, how-it-works section, CTA all wired.
- **`Search.razor`** — fully wired to `IShopRepository.SearchAsync`. Filters: keyword, category, county, min rating, open now. Pagination (6/page). Sort by recommended/rating/review count. Accepts `?q=` and `?categorie=` query params from Home category grid links.
- **`ShopProfile.razor`** — loads shop by `Id` via `IShopRepository.GetByIdAsync`. Shows hero banner, about, services, gallery with lightbox, paginated reviews with rating breakdown and useful votes, sticky contact card, hours, Google Maps link, top 3 similar shops (same category), share popover (WhatsApp/Facebook/copy), report modal, favorites toggle. After loading the main shop, `IsFavoriteAsync` and `SearchAsync` (for similar shops) run in parallel via `Task.WhenAll`.
- **`Auth/Login.razor`** — single page with tab switching (login/register). `[SupplyParameterFromQuery] Tab` activates the register tab. Supports `?returnUrl=` query param — passes it as a hidden field to `/auth/do-login`, which redirects there after successful login (local paths only, open-redirect protected). Login calls `SignInManager.PasswordSignInAsync`; register calls `UserManager.CreateAsync` + `AddToRoleAsync("User")` + `SignInAsync`. Google OAuth button is UI-only.
- **`Contact.razor`** — Blazor form with `@onsubmit` + success/error state. All four fields (name, email, subject, message) are validated before submit; shows inline error on failure. FAQ accordion driven by `OpenFaq` (int?) — one item open at a time. Subject `"listing"` creates a `ShopRequest` in the DB; all other subjects try to send a notification email via `IEmailService` to the configured `Email:From` address (swallowed silently if SMTP is not set up).
- **`AboutUs.razor`** — static page: mission/vision, process steps, hardcoded team member cards (Alexandru Popescu/CEO, Mihai Ionescu/Lead Dev, Elena Radu/Community Manager), CTA.
- **`User/Dashboard.razor`** — favorites list, review history (with inline edit), profile settings (name/email), password change. Auth guard in `OnInitializedAsync` via `AuthenticationStateProvider`.
- **`Auth/SetPassword.razor`** — password reset / first-time setup page. `OnParametersSetAsync` validates the token via `UserManager.VerifyUserTokenAsync` on page load — shows "invalid link" UI immediately for expired or missing tokens without waiting for form submission. Form POSTs to `/auth/do-set-password`.
- **`Shop/Dashboard.razor`** — profile editing (including brands), services CRUD, hours management (per day), photos tab (upload/delete/set cover), reviews viewing. Uses internal `HourRow` class with `string OpenTime`/`CloseTime` (format `"HH:mm"`). `<input type="time">` uses `value`/`@onchange` instead of `@bind` — Blazor infers `DateTime` for time inputs when using `@bind` on string properties. Shows a red disabled banner (with reason) when `Status == Inactive`.
- **`Admin/Dashboard.razor`** — accessible to both `Admin` and `SuperAdmin` roles (`_isSuperAdmin` flag drives elevated actions). Tabs: Cereri, Servicii, Utilizatori, Recenzii. Shops tab: search, edit modal (`UpdateProfileAsync` + `SetCategoryAsync` for Category field), disable-with-reason modal, activate. Users tab: Activi/Dezactivați toggle, search, edit modal (FullName + email via `UserManager`), reset-password modal (generates `/auth/set-password` link), lock/unlock (no reason), soft-disable-with-reason modal, restore. Role enforcement: SuperAdmin rows have no actions; Admin rows only actionable by SuperAdmin; `Admin` option in role dropdown only shown to SuperAdmin. Reject panel toggled via `_rejectingId`.

### Shared Components

| Component | File | Used In |
|---|---|---|
| `ShopCard` | `Shared/ShopCard.razor` | Home, Search — `Variant`: `Grid` (vertical) or `Row` (horizontal) |
| `StatsSection` | `Shared/StatsSection.razor` | Home, AboutUs — `Large`: compact inline (Home) or full section with bg (AboutUs) |
| `CTASection` | `Shared/CTASection.razor` | Home, AboutUs, Contact — `Variant`: `DarkHorizontal` or `RedCentered` |
| `TeamSection` | `Shared/TeamSection.razor` | AboutUs |
| `ShopReviewsSection` | `Shared/ShopProfile/ShopReviewsSection.razor` | ShopProfile — shows rating breakdown, reviews list, and write-review flow. Unauthenticated users see a note below the button and are redirected to login (with `returnUrl`) on click. |

`ShopCardModel` and `ShopCardVariant` are defined in `Shared/ShopCard.cs` — use this record for all shop data, not page-local records.

## Blazor conventions

- Every page or component with `@onclick`, `@bind`, or any other interactivity **must** have `@rendermode InteractiveServer` — either declared at the top of the component or inherited from `Routes.razor`.
- Never use raw hex color values (`bg-[#C0392B]`) — always use token classes (`bg-brand-red`). The token table above is the single source of truth.
- `ShopCardModel` is the unified data model for shop data across all pages — do not define page-local shop records.
- **`<input type="time">` with a `string` property** — do NOT use `@bind` (Blazor infers `DateTime`). Use `value="@field" @onchange="e => field = e.Value?.ToString() ?? string.Empty"` instead.
- **Inline `@onclick` lambdas with double quotes** — if the lambda body contains `""` (empty string), wrap the attribute in single quotes: `@onclick='() => { x = y ?? ""; }'`.
- **`CategoryDisplayName` and `Initials`** — do NOT copy these into individual pages; use the shared `DisplayHelpers` class available via `_Imports.razor`.
- **`<base href="/" />`** — do NOT use `<BasePath />` in `App.razor`. `<BasePath />` renders nothing when no path base is configured, which causes all `@Assets[...]` relative URLs to resolve against the current route path (e.g. `/auth/css/...` instead of `/css/...`). Use a hardcoded `<base href="/" />` instead. Similarly, script `src` values should use absolute paths (leading `/`).

### Auth conventions

- **Do NOT use `AuthorizeRouteView` or `CascadingAuthenticationState`** — these cause cascading parameter errors and redirect loops in this setup. Use plain `RouteView` in `Routes.razor`.
- **Do NOT use `<AuthorizeView>` component** — it requires a cascading `Task<AuthenticationState>`. Instead, inject `AuthenticationStateProvider` directly and use `_isAuthenticated` / `_userName` fields set in `OnInitializedAsync`.
- **Do NOT use `@attribute [Authorize]` on Blazor pages** — without `AuthorizeRouteView`, the attribute is not enforced and causes errors. Guard pages manually in `OnInitializedAsync` via `AuthenticationStateProvider`.
- **NavBar** reads auth state via injected `AuthenticationStateProvider` in `OnInitializedAsync`. After login/register/logout, always use `NavigateTo(..., forceLoad: true)` so the NavBar re-initializes with the new auth state.
- **`_Imports.razor`** already has `@using Microsoft.AspNetCore.Authorization`, `@using Microsoft.AspNetCore.Identity`, `@using PitStop.Infrastructure.Identity` — no need to repeat these in individual components.
- **Getting the user's display name** — `state.User.Identity.Name` returns the username/email, not the full name. Inject `UserManager<ApplicationUser>` and call `await UserManager.FindByIdAsync(userId)` to get `FullName`. If `IsAuthenticated` is true the NameIdentifier claim is always present, so no null-guard on the userId is needed before calling `FindByIdAsync`.

### Client-side JS

`wwwroot/js/app.js` — loaded in `App.razor` before `blazor.web.js`.

| Function | Purpose |
|---|---|
| `copyToClipboard(text)` | Wraps `navigator.clipboard.writeText`, called from `ShopProfile.razor` via `IJSRuntime` |

## Data

All public pages are wired to real repository calls. Hardcoded data remains only in `AboutUs.razor` (team members).

### Connection string

`appsettings.json`:
```
"ConnectionStrings": { "DefaultConnection": "Host=localhost;Database=pitstop_dev;Username=postgres;Password=postgres" }
```
`appsettings.Development.json` overrides the password (`Password=juliandev`) and enables `Microsoft.EntityFrameworkCore.Database.Command: Information` to log SQL.

### SQLite dev fallback

Set `"DatabaseProvider": "Sqlite"` in `appsettings.Development.json` to use SQLite instead of PostgreSQL. The DB file (`pitstop_dev.db`) is created automatically via `EnsureCreated()` on startup — no migrations needed. Remove the key (or set to anything else) to switch back to PostgreSQL. Never use `EnsureCreated()` in production; it doesn't track schema evolution.

### Program.cs registration order

1. `AddRazorComponents` + `AddInteractiveServerComponents`
2. `AddDbContextFactory<AppDbContext>` — provider selected by `DatabaseProvider` config key (`"Sqlite"` or default Postgres)
3. `AddIdentity<ApplicationUser, IdentityRole>` → `AddEntityFrameworkStores<AppDbContext>` → `AddDefaultTokenProviders`
   - Password: `RequireDigit=true`, `MinimumLength=8`, `RequireUppercase=false`
   - User: `RequireUniqueEmail=true`
4. `ConfigureApplicationCookie` — `LoginPath=/auth/login`, `AccessDeniedPath=/auth/access-denied`, 30-day sliding expiration
5. `AddAuthorization` with policies: `"ShopOwner"` (requires role ShopOwner), `"Admin"` (requires role Admin), `"SuperAdmin"` (requires role SuperAdmin)
6. All repositories as `AddScoped`; `IFileStorage → LocalFileStorage` as `AddScoped`

### Middleware order

`UseStaticFiles` → `UseAuthentication` → `UseAuthorization` → `UseAntiforgery` → `MapStaticAssets` → `MapRazorComponents`

`UseStaticFiles` is before auth so that `wwwroot/uploads/` is public without login.

### Minimal API endpoints

`/auth/do-login` (POST) — reads `email`, `password`, `rememberMe`, `returnUrl` from form. On success, redirects to `returnUrl` if it is a local path (starts with `/`), otherwise redirects to `/`. Prevents open redirect by rejecting non-local paths.

`/auth/do-register` (POST) — creates `ApplicationUser`, assigns `User` role, signs in, redirects to `/`.

### Role seeding

`SeedRolesAsync` runs after `app.Build()`, creates roles `SuperAdmin`, `Admin`, `ShopOwner`, `User` if they don't exist.

`SeedAdminAsync` creates `julian@pitstop.ro` / `juliandev0011!` with role `Admin` if it doesn't exist.

`SeedSuperAdminAsync` creates `superadmin@pitstop.ro` / `SuperAdmin1234!` with role `SuperAdmin` if it doesn't exist.

All seeders are no-ops if the user already exists — safe to leave enabled in production.

## Domain

Entities in `PitStop.Domain/Entities/`:

| Entity | Inherits | Key fields |
|---|---|---|
| `BaseEntity` | — | `Id` (int), `CreatedAt`, `UpdatedAt` |
| `Shop` | `BaseEntity` | `Name`, `Description`, `Address`, `City`, `County`, `Phone`, `Email`, `Website?`, `CoverImage?`, `Category`, `Status`, `DisabledReason?`, `OwnerId?`, `AverageRating`, `ReviewCount` |
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
- `AverageRating` and `ReviewCount` on `Shop` are denormalized — kept in sync by calling `ShopRepository.RecalcRatingAsync(shopId)` after any review create/update/delete

## Infrastructure

### Identity
`PitStop.Infrastructure/Identity/ApplicationUser.cs` — extends `IdentityUser`, adds `FullName`, `CreatedAt`, `ProfilePhoto?`, `IsDisabled`, `DisabledReason?`, `DisabledAt?`, `DisabledByAdminId?`.

Soft-disable pattern: when an admin disables a user, `IsDisabled = true`, reason and metadata are stored, and `LockoutEnd = DateTimeOffset.MaxValue` prevents login. Restore clears all four fields and unlocks.

### DbContext
`PitStop.Infrastructure/Data/AppDbContext.cs` — extends `IdentityDbContext<ApplicationUser>`.

DbSets: `Shops`, `Reviews`, `ShopPhotos`, `ShopServices`, `ShopHours`, `ShopRequests`, `FavoriteShops`, `ShopBrands`.

`SaveChangesAsync` automatically sets `CreatedAt` + `UpdatedAt` on `BaseEntity` entries (Added sets both; Modified sets only `UpdatedAt`).

`OnModelCreating` calls `base` first, then `ApplyConfigurationsFromAssembly` — all configurations are picked up automatically. It also applies the `Rating BETWEEN 1 AND 5` check constraint conditionally — quoted `"Rating"` for Postgres, unquoted for SQLite. Do NOT put provider-specific SQL inside `IEntityTypeConfiguration` — it has no access to `Database.IsNpgsql()` / `Database.IsSqlite()`.

### EF Core Configurations (`Data/Configurations/`)

| File | Entity | Key rules |
|---|---|---|
| `ShopConfiguration` | `Shop` | Required + max lengths; index on `City`, index on `Category`; `OwnerId` max 450 |
| `ReviewConfiguration` | `Review` | Unique index on `(ShopId, UserId)` — one review per user per shop; check constraint in `AppDbContext.OnModelCreating` |
| `ShopServiceConfiguration` | `ShopService` | `PriceMin` + `PriceMax` precision `(10, 2)` |
| `ShopRequestConfiguration` | `ShopRequest` | Required + max lengths for all string fields |
| `FavoriteShopConfiguration` | `FavoriteShop` | Unique index on `(UserId, ShopId)` |

`ShopBrand`, `ShopPhoto`, and `ShopHour` use EF Core conventions (no explicit configuration files needed yet).

### Repositories (`Repositories/`)

Interfaces live in `PitStop.Application/Interfaces/`. Implementations take `IDbContextFactory<AppDbContext>` via primary constructor, plus `ILogger<T>`. All methods use `await factory.CreateDbContextAsync()` — never the synchronous `factory.CreateDbContext()`.

**Logging levels:** use `LogInformation` for high-value writes (shop create, status change, review create/delete, shop request create/update); use `LogDebug` for frequent or low-value operations (favorite add/remove, rating recalc).

| Interface | Implementation | Notes |
|---|---|---|
| `IShopRepository` | `ShopRepository` | `SearchAsync` builds `IQueryable` pipeline; `openNow` maps .NET `DayOfWeek` (Sunday=0) → project convention (Monday=0) via `((int)dow + 6) % 7` |
| `IReviewRepository` | `ReviewRepository` | `IncrementUsefulAsync` uses `ExecuteUpdateAsync` for atomic increment; `DeleteAsync` uses `ExecuteDeleteAsync` |
| `IShopRequestRepository` | `ShopRequestRepository` | `GetPendingAsync` ordered by `CreatedAt ASC` (oldest first for FIFO admin queue) |
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
- `SetStatusAsync(shopId, status, reason?)` — writes `DisabledReason` when setting `Inactive`, clears it otherwise
- `SetCategoryAsync(shopId, category)` — updates `Category` via `ExecuteUpdateAsync`; used by admin edit shop modal
- `GetSiteStatsAsync()` — returns `(ShopCount, CountyCount, AvgRating, ReviewCount)` for StatsSection
- `RecalcRatingAsync(shopId)` — single GroupBy query to recompute `AverageRating` + `ReviewCount` on Shop; call after any review write
- `AddPhotoAsync(photo)`, `DeletePhotoAsync(photoId)`, `SetCoverImageAsync(shopId, url?)`
- `AddBrandAsync(brand)`, `DeleteBrandAsync(brandId)`

#### IReviewRepository methods
- `GetByShopIdAsync(shopId, page, pageSize)` — paginated
- `GetByUserIdAsync(userId)` — user's own reviews, includes Shop navigation
- `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- `GetAverageRatingAsync(shopId)`, `GetTotalCountAsync()`
- `IncrementUsefulAsync(reviewId)` — atomic `UsefulCount + 1` via `ExecuteUpdateAsync`

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