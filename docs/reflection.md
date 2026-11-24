# Week 5 reflection

## Key concepts

- **Authentication** establishes which user is signed in. **Authorization** decides what that signed-in user may do.
- **IdentityUser** is ASP.NET Core Identity's user entity. ApplicationUser extends it with first name, last name, and creation time.
- **IdentityDbContext** provides EF Core sets and mappings for users, roles, claims, logins, tokens, and their relationships.
- **UserManager** creates and updates users, validates passwords, looks up identities, and assigns roles.
- **SignInManager** handles password sign-in, persistent cookies, lockout-aware attempts, and sign-out.
- **RoleManager** manages role records. The startup seeder creates the Admin and Student roles when absent.
- **Authorize** protects a controller or action. A role constraint adds a server-side permission check. Hiding a button alone is not authorization.
- **ValidateAntiForgeryToken** rejects cross-site form submissions that do not contain the expected token.
- **Password hashing** stores a derived verifier rather than the original password. Identity owns the hash and verification process.
- **ModelState** combines model-binding and validation results. The controller returns the form when it is invalid and only persists valid input.
- **Migration** is a versioned schema change. MigrateAsync applies migrations and tracks them in __EFMigrationsHistory.
- **Role seeding** makes required roles available before registration and can safely run repeatedly when it checks for existing roles.

## Quiz answers

The assignment quiz answers, in order: **1-B, 2-A, 3-B, 4-A, 5-A, 6-B, 7-B, 8-A, 9-A, 10-B.**

## Reflection questions

1. A user role should not be accepted from a public registration form because a visitor could submit Admin and gain elevated privileges. The registration endpoint assigns Student on the server.
2. Hiding a delete button is only a presentation choice. A student can send a crafted request directly, so the delete endpoint itself requires the Admin role and validates antiforgery tokens.
3. Registration failures are added to ModelState from Identity's result errors. The form can then display those validation messages; logs should capture unexpected operational details without logging passwords.

## Implementation issue and resolution

The starter project contained only the default MVC welcome page and no Identity or database setup. The app was extended with an Identity EF Core context, SQLite migration, role seeder, controllers, and Razor views. The first database run must apply the checked-in migration before role seeding.
