# Security Playbook (ASP.NET Core Identity + Roles + Permission Claims)

This document summarizes how **MyCampusPermitAdmin** implements security using ASP.NET Core Identity, roles, and permission claims. Use it as a reference for this app or other Blazor Server projects.

---

## 1) Core Concepts

- **IdentityUser (`ApplicationUser`)** holds basic profile + security flags.
- **Roles** hold **permission claims** (ClaimType = `Permission`).
- **Policies** are generated dynamically for each permission.
- **Super Admin** bypasses permission checks when `IsSuperAdmin = true`.

---

## 2) Key Files & Responsibilities

### `Data/ApplicationUser.cs`
Adds custom fields to Identity user:
- `FirstName`, `LastName`, `CampusId`, `FilterByCampus`
- `IsSuperAdmin` (bypass permission checks)

### `Authorization/AppPermissions.cs`
- Defines **all** permission constants (e.g., `Customers.View`, `Customers.Manage`).
- `ClaimType = "Permission"`
- `SuperAdminClaimType = "IsSuperAdmin"`

### `Authorization/PermissionPolicyProvider.cs`
- Creates policies on the fly for each permission value.
- Policy passes when:
  - user has `IsSuperAdmin = true`, **or**
  - user has the exact permission claim, **or**
  - **Manage permission** satisfies **View** permission.

### `Data/ApplicationUserClaimsPrincipalFactory.cs`
- Adds custom claims:
  - `FirstName`, `LastName`, `CampusId`, `FilterByCampus`
  - `IsSuperAdmin`

### UI usage
- Pages: `[Authorize(Policy = AppPermissions.<X>View)]`
- Buttons/actions: `<AuthorizeView Policy="@AppPermissions.<X>Manage">`
- Nav menu visibility checks permission claims.

---

## 3) Permission Model

Each feature has two permissions:
- `Feature.View` (read-only)
- `Feature.Manage` (insert/update/delete)

**Rule:** If user has `Feature.Manage`, they’re allowed to access `Feature.View`.

---

## 4) Super Admin Bypass

If `ApplicationUser.IsSuperAdmin = true`:
- User can access **all** permission-protected pages/actions.
- This is checked in the policy provider and nav menu.

---

## 5) Where Policies Are Enforced

### Page-level access
```razor
@attribute [Authorize(Policy = AppPermissions.CustomersView)]
```

### Manage actions
```razor
<AuthorizeView Policy="@AppPermissions.CustomersManage">
    <RadzenButton Text="Add" ... />
</AuthorizeView>
```

### Nav menu visibility
```razor
private bool Has(string permission) =>
    _user.HasClaim(AppPermissions.SuperAdminClaimType, bool.TrueString) ||
    _user.HasClaim(AppPermissions.ClaimType, permission);
```

---

## 6) Roles + Permission Claims

Roles store permissions as **claims**:
- **ClaimType** = `Permission`
- **ClaimValue** = `Customers.View`, `Customers.Manage`, etc.

The Role dialog uses a multi-select list of all permissions and saves them as role claims.

---

## 7) Manage → View Behavior

The policy provider allows Manage to satisfy View.
This avoids needing to always select both.

Logic:
```csharp
if (policyName.EndsWith(".View"))
    allow if user has matching ".Manage" claim
```

---

## 8) User Management

- **Users page** lists identity users + `IsSuperAdmin`.
- **User dialog** supports:
  - Name, campus, FilterByCampus
  - Super Admin checkbox
  - Role assignments

---

## 9) Adding a New Permission

1. Add constants in `AppPermissions.cs`
2. Add to `AppPermissions.All`
3. Use in page attributes / `AuthorizeView`
4. Update Role dialog (auto uses `AppPermissions.All`)

---

## 10) Reuse in Other Projects

- Replace `AppPermissions` values with your feature set
- Keep the **permission claim** pattern
- Keep the **policy provider** (dynamic policies)
- Keep optional **Super Admin** claim for bypass

---

If you want diagrams or a quick-start checklist, say the word.
