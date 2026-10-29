# FullStackDemo: a small hair-salon site

A demo content management site for a made-up hair salon ("Sample Hair Studio"). It shows a typical full-stack setup:
a **React UI**, an **ASP.NET Core API** using **Entity Framework Core**, and a **MySQL** database, all started with
Docker. It has Playwright UI tests and a few very basic C# tests written to be read and learned from.

All names, addresses and passwords in this repo are made up. Do not reuse them for anything real.

## What it does

| Page | Who | What |
| --- | --- | --- |
| Home, Contact | everyone | Text that comes from the database |
| Booking | everyone | Read-only list of upcoming bookings (first name, service, time; no surnames, no private notes) |
| Staff login | staff | Username and password, gives a JWT token |
| Admin | staff | Add and delete bookings; edit the Home and Contact text |

## Run it

You need Docker. One command starts the database, the API and the UI:

```bash
docker compose up --build
```

| Part | Address |
| --- | --- |
| UI | http://localhost:5300 |
| API | http://localhost:5301 (the UI also reaches it at `/api`) |
| MySQL | localhost:3307, user `salon`, password `salon_demo_pw`, database `salon` |

Demo staff login: **admin** / **Salon123!** (set in `docker-compose.yml`; it is created on first start).
Stop with `docker compose down` (add `-v` to also delete the database).

On Windows, `demo.bat` wraps the same commands: `demo.bat start | stop | test | status`.

## Tests

**C# tests** (xUnit with EF Core's in-memory database, no MySQL needed). They are short and commented to explain
what each one shows:

```bash
cd api
dotnet test SalonApi.sln
```

**Playwright UI tests** (run in a real browser against the running app, so start Docker first):

```bash
cd ui
npm install
npx playwright install chromium
npx playwright test
```

## How it is put together

```
ui/                  React + TypeScript (Vite), served by nginx, which forwards /api to the API
api/SalonApi/        ASP.NET Core 9 minimal API
  Models/            Booking, ContentPage, AdminUser and the request/response shapes (DTOs)
  Data/              SalonDbContext (EF Core) and the starter-data seeder
  Services/          BookingService, ContentService, AuthService, PasswordHasher
  Endpoints/         the HTTP routes (public ones and the login-protected /api/admin ones)
api/SalonApi.Tests/  basic xUnit tests
docker-compose.yml   db (MySQL 8.4) + api + ui
```

Notes for learning:

- **EF Core**: `SalonDbContext` has one `DbSet` per table; the services query them with LINQ. The tables are created
  on first start with `EnsureCreated()` to keep the demo simple. A real project would use migrations.
- **Passwords** are stored as PBKDF2 hashes, never as plain text.
- **The public Booking list** uses a DTO that has no notes or surname field, so that data cannot leak by mistake.

## License

Published for viewing only. See `LICENSE`.
