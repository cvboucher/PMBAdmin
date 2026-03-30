namespace PMBAdmin.Authorization;

/// <summary>
/// Defines all application permissions.
/// Each page has a View (read-only) and Manage (insert/update/delete) permission.
/// Permissions are stored as claims in AspNetRoleClaims with ClaimType = "Permission".
/// </summary>
public static class AppPermissions
{
    public const string ClaimType = "Permission";
    public const string SuperUserClaimType = "SuperUser";

    // Administration
    public const string UsersView = "Users.View";
    public const string UsersManage = "Users.Manage";
    public const string RolesView = "Roles.View";
    public const string RolesManage = "Roles.Manage";

    // Configuration
    public const string AgenciesView = "Agencies.View";
    public const string AgenciesManage = "Agencies.Manage";
    public const string AgencyFeesView = "AgencyFees.View";
    public const string AgencyFeesManage = "AgencyFees.Manage";
    public const string AgencyVoidsView = "AgencyVoids.View";
    public const string AgencyVoidsManage = "AgencyVoids.Manage";
    public const string BatchJobTypesView = "BatchJobTypes.View";
    public const string BatchJobTypesManage = "BatchJobTypes.Manage";
    public const string CashRegistersView = "CashRegisters.View";
    public const string CashRegistersManage = "CashRegisters.Manage";
    public const string CashierCustomersView = "CashierCustomers.View";
    public const string CashierCustomersManage = "CashierCustomers.Manage";
    public const string CashierBatchesView = "CashierBatches.View";
    public const string CashierBatchesManage = "CashierBatches.Manage";
    public const string CashierView = "Cashier.View";
    public const string CashierManage = "Cashier.Manage";
    public const string CashnetPaymentCodesView = "CashnetPaymentCodes.View";
    public const string CashnetPaymentCodesManage = "CashnetPaymentCodes.Manage";

    /// <summary>
    /// All defined permissions for enumeration (e.g., admin UI checkboxes).
    /// Expand this list as pages are converted.
    /// </summary>
    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new("Administration", UsersView, "Users - View"),
        new("Administration", UsersManage, "Users - Manage"),
        new("Administration", RolesView, "Roles - View"),
        new("Administration", RolesManage, "Roles - Manage"),

        new("Configuration", AgenciesView, "Agencies - View"),
        new("Configuration", AgenciesManage, "Agencies - Manage"),
        new("Configuration", AgencyFeesView, "Agency Fees - View"),
        new("Configuration", AgencyFeesManage, "Agency Fees - Manage"),
        new("Configuration", AgencyVoidsView, "Agency Voids - View"),
        new("Configuration", AgencyVoidsManage, "Agency Voids - Manage"),
        new("Configuration", BatchJobTypesView, "Batch Job Types - View"),
        new("Configuration", BatchJobTypesManage, "Batch Job Types - Manage"),
        new("Configuration", CashRegistersView, "Cash Registers - View"),
        new("Configuration", CashRegistersManage, "Cash Registers - Manage"),
        new("Configuration", CashierCustomersView, "Cashier Customers - View"),
        new("Configuration", CashierCustomersManage, "Cashier Customers - Manage"),
        new("Configuration", CashierBatchesView, "Cashier Batches - View"),
        new("Configuration", CashierBatchesManage, "Cashier Batches - Manage"),
        new("Configuration", CashierView, "Cashier - View"),
        new("Configuration", CashierManage, "Cashier - Manage"),
        new("Configuration", CashnetPaymentCodesView, "Cashnet Payment Codes - View"),
        new("Configuration", CashnetPaymentCodesManage, "Cashnet Payment Codes - Manage"),
    ];

    /// <summary>
    /// Given a Manage permission (e.g. "Users.Manage"), returns the corresponding View permission, or null.
    /// </summary>
    public static string? GetViewPermission(string managePermission)
    {
        if (!managePermission.EndsWith(".Manage"))
            return null;
        var viewValue = managePermission.Replace(".Manage", ".View");
        return All.Any(p => p.Value == viewValue) ? viewValue : null;
    }

    /// <summary>
    /// Given a View permission (e.g. "Users.View"), returns the corresponding Manage permission, or null.
    /// </summary>
    public static string? GetManagePermission(string viewPermission)
    {
        if (!viewPermission.EndsWith(".View"))
            return null;
        var manageValue = viewPermission.Replace(".View", ".Manage");
        return All.Any(p => p.Value == manageValue) ? manageValue : null;
    }
}

/// <summary>
/// Describes a single permission for display in admin UI.
/// </summary>
public record PermissionDefinition(string Category, string Value, string DisplayName);
