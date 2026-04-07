# shop-dotnet

A .NET 10 ASP.NET Core MVC port of a PHP e-commerce demo shop. Faithfully replicates all pages, routes, functionality, and UI of the original PHP application.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

- Product catalogue with categories, search, and hierarchy navigation
- Session-based shopping cart with AJAX add-to-cart
- Customer registration, login, and account management
- Checkout with delivery options and order confirmation
- Full admin panel: products, categories, users, orders, delivery options, settings
- Rate limiting on login and registration
- Custom CSRF protection (session token, compatible with original shop.js)
- Product image upload in admin

## Tech stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core MVC, .NET 10 |
| Database | SQLite via Dapper (raw SQL) |
| Auth | Session-based (JSON in session, no ASP.NET Identity) |
| Passwords | BCrypt.Net-Next |
| ORM | None — Dapper with snake_case mapping |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Getting started

```bash
git clone <repo>
cd shop-dotnet
dotnet run
```

The app listens on `http://localhost:5175`.

The SQLite database (`shop.db`) is created and seeded automatically on first run.

## Configuration

`appsettings.json`:

```json
{
  "Database": { "Path": "shop.db" },
  "ImagesPath": "../shop-demo/public/images"
}
```

- **`Database.Path`** — path to the SQLite file (relative to the project root, or absolute)
- **`ImagesPath`** — directory from which product images are served at `/images/`. Defaults to the sibling PHP shop's image folder; change this to any directory containing your product images.

## Default credentials

| Role | Email | Password |
|---|---|---|
| Admin | admin@shop.local | password |
| Customer | jane@example.com | password |

## Project structure

```
Controllers/
  BaseController.cs          # Shared ViewData injection (nav, cart, user, CSRF)
  StorefrontController.cs    # /, /search, /category/{slug}, /product/{slug}
  AuthController.cs          # /login, /register, /logout
  CartController.cs          # /cart, /cart/update, POST /product/{slug} (add)
  CheckoutController.cs      # /checkout, /checkout/confirm/{id}
  AccountController.cs       # /account, /account/orders
  Admin/
    AdminBaseController.cs   # Admin auth gate
    DashboardController.cs
    ProductsController.cs
    CategoriesController.cs
    OrdersController.cs
    UsersController.cs
    DeliveryController.cs
    SettingsController.cs
Data/
  schema.sql                 # SQLite schema + seed data
Helpers/
  ViewHelpers.cs             # Static V class: V.Money(), V.ImgSrc(), etc.
Models/
  Models.cs                  # All model classes
Services/
  DatabaseService.cs         # DB init and seeding
  SettingsService.cs         # Cached key/value settings
  SecurityService.cs         # CSRF + rate limiting
  AuthService.cs             # Session login/logout helpers
  CartService.cs             # Cart read/write
Views/
  Shared/
    _Layout.cshtml           # Storefront layout
    _AdminLayout.cshtml      # Admin layout
  ...                        # Per-controller view folders
wwwroot/
  css/shop.css
  css/admin.css
  js/shop.js
  images/placeholder.svg
```

## Database settings

Runtime settings are stored in the `settings` table and editable via the admin panel:

| Key | Default | Description |
|---|---|---|
| site_name | My Shop | Site name shown in the header |
| currency_symbol | £ | Currency symbol for prices |
| password_min_length | 8 | Minimum password length |
| login_max_attempts | 5 | Failed logins before rate limit |
| login_window_seconds | 300 | Rate limit window (seconds) |
| register_max_attempts | 3 | Registrations before rate limit |
| register_window_seconds | 3600 | Rate limit window (seconds) |
| low_stock_threshold | 5 | Stock level shown as "low stock" |
