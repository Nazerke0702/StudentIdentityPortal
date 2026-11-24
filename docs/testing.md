# Testing and verification

## Automated tests

Run from the repository root:

~~~powershell
dotnet test .\StudentIdentityPortal.slnx --maxcpucount:1
~~~

The test project checks the registration view model's valid input, password policy, and password confirmation. These are unit-level checks; they do not replace browser testing of Identity, cookies, authorization, or SQLite.

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

Capture real browser states after the matching acceptance case has passed. Suggested names:

- 01-home.png
- 02-register.png
- 03-login.png
- 04-student-dashboard.png
- 05-student-directory.png
- 06-access-denied.png
- 07-admin-dashboard.png
- 08-admin-delete.png

Use disposable test accounts and hide personal data. Do not create fabricated screenshots or describe unrun checks as successful.
