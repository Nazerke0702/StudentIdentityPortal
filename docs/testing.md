# Testing and verification

## Automated tests

Run from the repository root. These tests use WebApplicationFactory and an isolated in-memory SQLite database; they do not use or reset the development database:

~~~powershell
dotnet restore .\StudentIdentityPortal.csproj
dotnet restore .\StudentIdentityPortal.Tests\StudentIdentityPortal.Tests.csproj
dotnet build .\StudentIdentityPortal.slnx --maxcpucount:1
dotnet test .\StudentIdentityPortal.slnx --maxcpucount:1
~~~

The suite covers MVC endpoints, registration, role assignment, login, role authorization, antiforgery, logout, and return URLs, plus registration view-model validation. Browser screenshots are separate evidence and must not be marked complete until the Playwright script runs successfully.

## Manual acceptance checklist

Record the actual date, browser, and result after running each case. Leave the result blank until the case has been executed.

| # | Scenario | Expected result | Result |
| --- | --- | --- | --- |
| 1 | Open the home page as a guest | Home page loads; registration and sign-in are available | |
| 2 | Register with a valid email and strong matching password | Account is created with Student role and user reaches dashboard | |
| 3 | Register with a duplicate email, weak password, or mismatched confirmation | Form remains open with validation feedback; no second account is created | |
| 4 | Sign out and sign in with the new student account | Dashboard opens; remember-me option is available | |
| 5 | Visit dashboard, profile, directory, and a student detail as guest | Guest is redirected to sign-in | |
| 6 | Try /Admin as a student | Access denied; no management action is available | |
| 7 | Submit an admin delete request as a student or without an antiforgery token | Request is rejected; account is unchanged | |
| 8 | Sign in with configured administrator and remove a student | Admin list updates; student can no longer sign in | |
| 9 | Change language selector and reload | Culture cookie persists and request culture changes | |
| 10 | Toggle dark theme and reload | Theme preference persists in browser storage | |

## Browser screenshots

The Playwright automation and isolated-server launcher are in scripts/capture-evidence.mjs and scripts/Capture-Evidence.ps1. Run them from a local PowerShell session:

~~~powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Capture-Evidence.ps1
~~~

The launcher uses a unique temporary SQLite file and temporary Development-only administrator credentials. It does not connect to the project's existing database. npm and Chromium must be downloadable on the machine running the script.

The script captures these real browser states:

- 01-home.png
- 02-register.png
- 03-register-validation.png
- 04-register-success.png
- 05-login.png
- 06-login-validation.png
- 07-student-dashboard.png
- 08-student-access-denied.png
- 09-admin-dashboard.png
- 10-admin-protected-action.png
- 11-logout.png

Use disposable test accounts and hide personal data. Do not create fabricated screenshots or describe unrun checks as successful.
