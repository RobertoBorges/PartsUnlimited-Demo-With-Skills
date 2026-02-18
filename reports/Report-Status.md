# Migration Status Report

**Project:** PartsUnlimited  
**Generated:** February 18, 2026  
**Agent:** Migration to Azure Agent  

---

## Current Status: � Phase 2 In Progress — Code Modernization (~90% Complete)

---

## Migration Configuration

| Setting | Selection |
|---|---|
| **Modernization Scope** | Version upgrade only (.NET Framework 4.5.1 → .NET 8 LTS) |
| **Target Platform** | Azure Kubernetes Service (AKS) |
| **IaC Tool** | Terraform + Helm |
| **Target Database** | Azure SQL Database |

---

## Phase Progress

| Phase | Status | Completed | Notes |
|---|---|---|---|
| **Phase 0** — Multi-Repo Assessment | ⬜ Skipped | — | Single repository |
| **Phase 1** — Planning & Assessment | ✅ Complete | Feb 18, 2026 | Report generated |
| **Phase 2** — Code Modernization | 🔄 In Progress | — | ~90% complete — remaining: wwwroot static files, test project upgrade, EF Core migrations |
| **Phase 3** — Infrastructure Generation | ⬜ Not Started | — | Terraform + Helm |
| **Phase 4** — Deployment to Azure | ⬜ Not Started | — | AKS |
| **Phase 5** — CI/CD Pipeline Setup | ⬜ Not Started | — | GitHub Actions |

---

## Phase 2 Deliverables

### ✅ Completed
| Artifact | Location |
|---|---|
| SDK-style project file (net8.0) | `src/PartsUnlimited.Web/PartsUnlimited.Web.csproj` |
| ASP.NET Core 8 Program.cs (replaces Global.asax + Startup) | `src/PartsUnlimited.Web/Program.cs` |
| appsettings.json + appsettings.Development.json | `src/PartsUnlimited.Web/` |
| EF Core 8 models: IPartsUnlimitedContext, PartsUnlimitedContext | `src/PartsUnlimited.Web/Models/` |
| Entity models: Product, Order, OrderDetail, Category, CartItem, Raincheck, Store | `src/PartsUnlimited.Web/Models/` |
| ShoppingCart.cs (IHttpContextAccessor refactor) | `src/PartsUnlimited.Web/Models/ShoppingCart.cs` |
| OrderCostSummary, ILineItem | `src/PartsUnlimited.Web/Models/` |
| DefaultShippingTaxCalculator (shipping ×$5, tax 5%) | `src/PartsUnlimited.Web/Utils/` |
| OrdersQuery, RaincheckQuery | `src/PartsUnlimited.Web/Utils/` |
| TelemetryProvider (Application Insights) | `src/PartsUnlimited.Web/Utils/` |
| PartsUnlimitedDbInitializer (EF Core seed) | `src/PartsUnlimited.Web/Utils/` |
| AnnouncementHub (ASP.NET Core SignalR) | `src/PartsUnlimited.Web/Hubs/` |
| IProductSearch, StringContainsProductSearch | `src/PartsUnlimited.Web/ProductSearch/` |
| IRecommendationEngine, NaiveRecommendationEngine, AzureMLEngine | `src/PartsUnlimited.Web/Recommendations/` |
| ViewModels: Home, Products, ShoppingCart, Orders, Search, OrderCostSummary | `src/PartsUnlimited.Web/ViewModels/` |
| MVC Controllers: Home, Store, ShoppingCart, Checkout, Orders, Search | `src/PartsUnlimited.Web/Controllers/` |
| Admin Controllers: AdminController (base), StoreManagerController, OrdersController, CustomerController, RaincheckController | `src/PartsUnlimited.Web/Areas/Admin/Controllers/` |
| API Controllers: ProductsController, RaincheckApiController | `src/PartsUnlimited.Web/Api/` |
| Razor Views: _ViewImports, _ViewStart, _Layout, Error | `src/PartsUnlimited.Web/Views/Shared/` |
| Razor Views: Home/Index, Store/Index+Browse+Details, ShoppingCart/Index, Search/Index, Checkout/AddressAndPayment+Complete, Orders/Index+Details | `src/PartsUnlimited.Web/Views/` |
| Admin Views: StoreManager CRUD (Index, Create, Edit, Delete, Details) | `src/PartsUnlimited.Web/Areas/Admin/Views/StoreManager/` |
| Multi-stage Dockerfile (sdk:8.0 → aspnet:8.0, non-root user) | `src/PartsUnlimited.Web/Dockerfile` |
| Business Logic Mapping (35 items tracked) | `reports/Business-Logic-Mapping.md` |

### ⬜ Remaining
| Task | Notes |
|---|---|
| Copy wwwroot static files (CSS, JS, Images) | Copy from `src/PartsUnlimitedWebsite/Content` and `Images` |
| EF Core migrations | `dotnet ef migrations add InitialCreate` |
| Test project upgrade | Convert MSTest → xUnit; update MockDataContext |
| Admin views: Orders, Customer, Raincheck | Basic CRUD views |

---

## Key Findings Summary

### 🔴 Blockers (Must Address in Phase 2)
- `System.Web.*` dependencies — no .NET 8 equivalent; full rewrite of startup pipeline
- OWIN middleware (`Microsoft.Owin.*`) — replace with ASP.NET Core middleware
- Admin password stored in `web.config` — move to Azure Key Vault
- SQL authentication in connection string — migrate to Managed Identity

### 🟠 High Risk
- Entity Framework 6 → EF Core 8 migration (migrations must be re-scaffolded)
- **ASP.NET Identity 2 → Microsoft Entra ID + `Microsoft.Identity.Web` (MSAL)** *(user decision: no local Identity store)*
- Unity IoC container → `Microsoft.Extensions.DependencyInjection`

### 🟡 Medium Risk
- ASP.NET MVC 5 → ASP.NET Core MVC 8
- SignalR 2.2.1 → ASP.NET Core SignalR
- `Global.asax` → `Program.cs` / middleware pipeline
- Admin role → Entra ID App Role (`Administrator`) assigned in Azure portal
- `AccountController` / `ManageController` — remove local auth UI; replaced by Entra ID OIDC flow

### 🟢 Low Risk / Straightforward
- Web API 2 → ASP.NET Core controllers
- `web.config` → `appsettings.json`
- Application Insights SDK update
- NuGet package modernization

---

## Decision Log

| Date | Decision | Impact |
|---|---|---|
| Feb 18, 2026 | **Authentication: Entra ID + `Microsoft.Identity.Web` instead of ASP.NET Core Identity** | Remove local user store, `AccountController`, `ManageController`, `ApplicationUser`; `PartsUnlimitedContext` no longer inherits `IdentityDbContext`; admin role via Entra ID App Roles |

---

## Next Action

Phase 2 code modernization is ~90% complete. Remaining tasks:
1. Copy `wwwroot/` static assets from source project
2. Run `dotnet ef migrations add InitialCreate` to scaffold EF Core migrations
3. Upgrade the test project to xUnit (.NET 8)

When ready, run `/phase3-generateinfra` to generate Terraform + Helm infrastructure.
