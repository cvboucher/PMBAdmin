# Grid & Update Page Playbook (Blazor + Radzen)

This document captures the patterns used in **MyCampusPermitAdmin** for grid pages and modal update dialogs. It can be reused for new pages in this app and adapted for other Blazor projects.

---

## 1) Standard Grid Page Pattern

**File:** `Components/Pages/<EntityPlural>.razor`

**Core elements**
- `@page` route(s)
- `[Authorize(Policy = AppPermissions.<Entity>View)]`
- `AppDataGrid<TItem>` with `IQueryable` data
- `RowSelect` opens a modal dialog
- “Add” button opens modal dialog (Manage policy required)
- First column default sort: `SortOrder="SortOrder.Ascending"`
- Related-entity filter columns use **RadzenDropDown** (or **RadzenDropDownDataGrid** for Customer)
- Filters call `_grid!.FirstPage(true)`

### Skeleton

```razor
@page "/entities"
@attribute [Authorize(Policy = AppPermissions.EntitiesView)]
@implements IDisposable

@inject IDbContextFactory<ParkingPermitContext> DbFactory
@inject DialogService DialogService

<PageTitle>Entities</PageTitle>

<RadzenCard>
    <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" JustifyContent="JustifyContent.SpaceBetween" class="rz-mb-3">
        <RadzenText Text="Entities" Style="font-weight: 600;" />
        <AuthorizeView Policy="@AppPermissions.EntitiesManage">
            <RadzenButton Text="Add" Icon="add_circle" ButtonStyle="ButtonStyle.Primary" Click="AddAsync" />
        </AuthorizeView>
    </RadzenStack>

    <AppDataGrid @ref="_grid" TItem="Entity" Data="_query" RowSelect="OnRowSelect">
        <Columns>
            <RadzenDataGridColumn TItem="Entity" Property="Name" Title="Name" SortOrder="SortOrder.Ascending" />
            <!-- Related column with dropdown filter -->
            <RadzenDataGridColumn TItem="Entity" Property="Campus.CampusName" Title="Campus">
                <FilterTemplate>
                    <RadzenDropDown Data="@_db!.Campuses.AsNoTracking().OrderBy(x => x.CampusName)
                                   .Select(x => x.CampusName).Distinct().ToList()"
                                    Value="@context.FilterValue"
                                    Change="@(value => { context.FilterValue = value; _grid!.FirstPage(true); })"
                                    AllowFiltering="true" AllowClear="true" Style="width: 100%" />
                </FilterTemplate>
            </RadzenDataGridColumn>
        </Columns>
    </AppDataGrid>
</RadzenCard>

@code {
    private ParkingPermitContext? _db;
    private AppDataGrid<Entity>? _grid;
    private IQueryable<Entity>? _query;

    protected override async Task OnInitializedAsync()
    {
        _db = await DbFactory.CreateDbContextAsync();
        _query = _db.Entities.AsNoTracking();
    }

    private async Task AddAsync()
    {
        await DialogService.OpenDialogAsync<EntityDialog>("Add Entity");
        _db?.ChangeTracker.Clear();
    }

    private async Task OnRowSelect(Entity row)
    {
        await DialogService.OpenDialogAsync<EntityDialog>("Edit Entity",
            new Dictionary<string, object?> { ["EntityId"] = row.EntityId });
        _db?.ChangeTracker.Clear();
    }

    public void Dispose() => _db?.Dispose();
}
```

### Customer-related dropdown filter (virtualized)

```razor
<FilterTemplate>
    <RadzenDropDownDataGrid Data="@_db!.Customers.AsNoTracking()"
                            TextProperty="Email" ValueProperty="Email"
                            Value="@context.FilterValue"
                            Change="@(value => { context.FilterValue = value; _grid!.FirstPage(true); })"
                            AllowFiltering="true" AllowClear="true" Style="width: 100%" Virtualization="true">
        <Columns>
            <RadzenDataGridColumn TItem="Customer" Property="Email" Title="Email" />
            <RadzenDataGridColumn TItem="Customer" Property="FirstName" Title="First Name" />
            <RadzenDataGridColumn TItem="Customer" Property="LastName" Title="Last Name" />
        </Columns>
    </RadzenDropDownDataGrid>
</FilterTemplate>
```

---

## 2) Modal Update Dialog Pattern

**File:** `Components/Pages/<Entity>Dialog.razor`

**Core elements**
- `RadzenTemplateForm` bound to `_model`
- Modal is used for both add/edit; primary key determines edit
- Delete button hidden for new records
- Use `RadzenFormField` with `AllowFloatingLabel="false"`
- Checkboxes shown in **horizontal RadzenStack** with label

### Skeleton

```razor
@inject IDbContextFactory<ParkingPermitContext> DbFactory
@inject DialogService DialogService

<RadzenTemplateForm Data="_model" Submit="@((Entity _) => SaveAsync())">
    <RadzenStack Gap="0.75rem">
        <RadzenFormField Text="Name" AllowFloatingLabel="false">
            <RadzenTextBox @bind-Value="_model.Name" Style="width: 100%" />
        </RadzenFormField>

        <RadzenStack Orientation="Orientation.Horizontal" Gap="0.5rem" AlignItems="AlignItems.Center">
            <RadzenCheckBox @bind-Value="_model.IsActive" />
            <RadzenLabel Text="Active" />
        </RadzenStack>
    </RadzenStack>

    <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.End" Gap="0.5rem" class="rz-mt-3">
        <AuthorizeView Context="authContext" Policy="@AppPermissions.EntitiesManage">
            <RadzenButton Text="Delete" ButtonStyle="ButtonStyle.Danger" Click="DeleteAsync" Visible="_model.EntityId != 0" />
            <RadzenButton Text="Save" ButtonStyle="ButtonStyle.Primary" ButtonType="ButtonType.Submit" />
        </AuthorizeView>
        <RadzenButton Text="Cancel" ButtonStyle="ButtonStyle.Light" Click="() => DialogService.Close()" />
    </RadzenStack>
</RadzenTemplateForm>

@code {
    [Parameter] public int? EntityId { get; set; }

    private ParkingPermitContext? _db;
    private Entity _model = new();

    protected override async Task OnInitializedAsync()
    {
        _db = await DbFactory.CreateDbContextAsync();
        if (EntityId.HasValue)
        {
            var existing = await _db.Entities.FirstOrDefaultAsync(x => x.EntityId == EntityId.Value);
            if (existing != null)
                _model = existing;
        }
    }

    private async Task SaveAsync()
    {
        if (_model.EntityId == 0)
            _db!.Entities.Add(_model);
        else
            _db!.Entities.Update(_model);

        await _db!.SaveChangesAsync();
        DialogService.Close(true);
    }

    private async Task DeleteAsync()
    {
        var confirmed = await DialogService.Confirm("Delete this record?", "Confirm",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });
        if (confirmed != true) return;

        _db!.Entities.Remove(_model);
        await _db.SaveChangesAsync();
        DialogService.Close(true);
    }
}
```

---

## 3) Shared UI & Behavior Conventions

- **Layout:** `RadzenLayout` with a left nav (`NavMenu.razor`).
- **Dialogs:** Always use `DialogService.OpenDialogAsync<T>()` (default dialog options are Resizable/Draggable).
- **Filtering:** First column has default sort; related fields use dropdown filters; always reset to first page on filter change.
- **Security:** `[Authorize(Policy = AppPermissions.<X>View)]` on page; `AuthorizeView` with `Manage` policy for buttons.
- **Db access:** `IDbContextFactory<ParkingPermitContext>`; use `AsNoTracking()` for grids.
- **Campus security:** When applicable, respect `ReportParameterState.FilterByCampus` + `CampusId`.

---

## 4) Quick Checklist (New Page)

1. Create `Pages/<EntityPlural>.razor` with `AppDataGrid<T>` and filters
2. Create `Pages/<Entity>Dialog.razor` with modal form
3. Add route in NavMenu + permissions in `AppPermissions`
4. Use `OpenDialogAsync<T>` for Add/Edit
5. Add default sort to first column
6. Build + fix errors

---

## 5) Reuse in Other Projects

You can reuse this pattern in any Blazor Server app with Radzen:
- Replace `ParkingPermitContext` with your DbContext
- Replace permissions with your auth model
- Keep `AppDataGrid` wrapper for consistent paging/filter defaults
- Keep `OpenDialogAsync<T>` extension for centralized dialog options

---

If you want this split into a **template + checklist + examples** or converted into a README for new projects, say the word.
