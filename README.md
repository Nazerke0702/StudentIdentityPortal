# StudentIdentityPortal

An ASP.NET Core MVC student identity portal for Nazerke's Week 5 assignment. It demonstrates registration, sign-in, role-based access, student profiles, a student directory, and admin account management using ASP.NET Core Identity and SQLite.

## Project objectives

- Use ASP.NET Core MVC with Razor views and no client-side SPA framework.
- Store users and Identity roles in SQLite through EF Core migrations.
- Apply role-based authorization on server endpoints, including destructive actions.
- Demonstrate safe password handling, antiforgery protection, validation, lockout, localization middleware, and a responsive light/dark interface.

## Features

- Student self-registration. New public accounts are always assigned the Student role.
- Login with remember-me support, safe local return URLs, failed-attempt lockout, and POST-only logout.
- Authenticated student dashboard, profile update, student directory, and student details.
- Admin dashboard to review and remove student accounts. Removal is POST-only, antiforgery protected, and restricted to the Admin role.
- Idempotent Admin and Student role seeding and optional admin account seeding from User Secrets or environment variables.
- English, Kazakh, and Russian culture selection using the ASP.NET Core request-localization cookie. The current UI copy is English; the culture pipeline is ready for translated resource strings.
- Responsive custom CSS, dark-mode preference, accessible focus states, and password visibility controls.

## Technologies and architecture

- .NET 10 / ASP.NET Core MVC / C#
- ASP.NET Core Identity with ApplicationUser : IdentityUser
- EF Core 10 and SQLite
- Razor Views, CSS, and JavaScript

The web project is organized into Controllers, Data, Models, ViewModels, Constants, Views, and wwwroot. AppDbContext inherits from IdentityDbContext<ApplicationUser>. IdentitySeeder creates the roles and, only when configured, the initial administrator.

## Database structure

The checked-in Data/Migrations folder contains InitialCreate. It creates the Identity tables, including AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins, and AspNetUserTokens. EF Core migration history is recorded in __EFMigrationsHistory.

The local database file is student-identity-portal.db in the app working directory. It is ignored by Git. Startup applies checked-in migrations with MigrateAsync; the application does not use EnsureCreated.

## Authentication and permissions

| Capability | Guest | Student | Admin |
| --- | --- | --- | --- |
| Home, registration, sign-in | Yes | Yes | Yes |
| Dashboard, profile, student directory/details | No | Yes | Yes |
| Admin student management | No | No | Yes |
| Remove student | No | No | Yes |

The server enforces these permissions with Authorize, Authorize(Roles = ...), and AllowAnonymous. Hiding a link in the UI is never the access control. Every state-changing form uses an antiforgery token. No password is stored or logged by application code; Identity handles password hashing.

## Requirements

- .NET 10 SDK
- A terminal in this repository folder

Check the installed SDK with dotnet --version.

## Installation and run

From the repository root:

~~~powershell
dotnet tool restore
dotnet restore
dotnet build
dotnet run --project .\StudentIdentityPortal.csproj
~~~

Open the local URL printed by dotnet run. On first startup, EF Core applies InitialCreate and creates the local SQLite database. The site can run without an administrator account; student registration remains available.

## Configure an initial administrator

Do not put admin credentials in appsettings.json, source code, screenshots, or Git. Configure them locally with User Secrets:

~~~powershell
dotnet user-secrets set "Admin:Email" "admin@example.edu" --project .\StudentIdentityPortal.csproj
dotnet user-secrets set "Admin:Password" "Replace-With-A-Unique-Strong-Password!" --project .\StudentIdentityPortal.csproj
~~~

Restart the app. The seeder creates the account only when it does not already exist and ensures that it has the Admin role. Alternatively set Admin__Email and Admin__Password as environment variables. Remove local secrets with dotnet user-secrets clear --project .\StudentIdentityPortal.csproj.

## Migrations

The initial migration is already checked in. To apply it explicitly:

~~~powershell
dotnet ef database update --project .\StudentIdentityPortal.csproj
~~~

To add a future schema change:

~~~powershell
dotnet ef migrations add DescriptiveChangeName --project .\StudentIdentityPortal.csproj
dotnet ef database update --project .\StudentIdentityPortal.csproj
~~~

If InitialCreate already exists, do not add it again.

## Tests and manual checks

Run automated validation tests with:

~~~powershell
dotnet test .\StudentIdentityPortal.slnx --maxcpucount:1
~~~

Manual cases and a results log template are in docs/testing.md. Do not record a case as passed until it has actually been run. Save genuine, redacted browser screenshots to docs/screenshots/ when capturing them locally.

## Security notes

- Password requirements: minimum 8 characters, uppercase, lowercase, digit, and symbol.
- Identity lockout: five failed attempts, then 15 minutes.
- Unique email addresses; registration cannot choose a role.
- Cookies are HTTP-only and SameSite=Lax; secure-only cookies are enabled automatically over HTTPS.
- Logout and deletion use POST plus antiforgery validation.
- Login return URLs are restricted to local URLs.
- Admin credentials are supplied out of band. No sample password is committed.
- SQLite databases, local secrets, build output, and IDE files are ignored.

## Screenshots

The home page capture is in docs/screenshots/01-home.png. Capture real registration, login, student dashboard, profile, directory, access denied, admin dashboard, and account removal states locally after the matching tests pass. Use test accounts only and redact personal information before committing.

## Conclusion

The project provides a runnable MVC example of Identity registration, sign-in, role assignment, profile access, and server-enforced admin actions. It is intentionally sized for an introductory assignment and does not include production account recovery, email confirmation delivery, or a deployment configuration.

## Publish to GitHub

Create an empty repository on GitHub, then from this folder:

~~~powershell
git init
git add .
git commit -m "Build student identity portal"
git branch -M main
git remote add origin https://github.com/YOUR-USERNAME/StudentIdentityPortal.git
git push -u origin main
~~~

Check git status and review git diff --cached before pushing. Never commit the SQLite database, User Secrets, real passwords, or unredacted screenshots.
