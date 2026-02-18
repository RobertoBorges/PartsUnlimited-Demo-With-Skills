# Business Logic Mapping

**Project:** PartsUnlimited  
**Generated:** February 18, 2026  
**Phase:** 2 — Code Modernization  

---

## Legend

| Status | Meaning |
|---|---|
| ⬜ Not Started | Logic identified; migration not yet begun |
| 🔄 In Progress | Migration underway |
| ✅ Migrated | Logic preserved and verified in new project |
| ❌ Removed | Intentionally removed (e.g., Identity replaced by Entra ID) |

---

## 1. Calculations

| # | Logic | Source File | Description | Status | New Location |
|---|---|---|---|---|---|
| C1 | Shipping cost | `Utils/DefaultShippingTaxCalculator.cs` | `itemsCount * 5.00` per item | ✅ Migrated | `Utils/DefaultShippingTaxCalculator.cs` |
| C2 | Tax calculation | `Utils/DefaultShippingTaxCalculator.cs` | `(subTotal + shipping) * 0.05` | ✅ Migrated | `Utils/DefaultShippingTaxCalculator.cs` |
| C3 | Cart total | `Models/ShoppingCart.cs:GetTotal()` | Sum of `Count * Product.Price` for cart items | ✅ Migrated | `Models/ShoppingCart.cs` |
| C4 | Order total | `Models/ShoppingCart.cs:CreateOrder()` | Sum of `Count * Product.Price` from cart items | ✅ Migrated | `Models/ShoppingCart.cs` |
| C5 | Order details cost summary | `Areas/Admin/Controllers/OrdersController.cs` | subTotal, shipping (items×$5), tax (5%), total | ✅ Migrated | `Areas/Admin/Controllers/OrdersController.cs` |
| C6 | Sale price display | `Models/Product.cs` | Price vs SalePrice; `ProductDetailList` JSON deserialization | ✅ Migrated | `Models/Product.cs` |

---

## 2. Validations

| # | Logic | Source File | Description | Status | New Location |
|---|---|---|---|---|---|
| V1 | Promo code check | `Controllers/CheckoutController.cs` | Only accepts `"FREE"` promo code (case-insensitive) | ✅ Migrated | `Controllers/CheckoutController.cs` |
| V2 | Order model validation | `Controllers/CheckoutController.cs` | `ModelState.IsValid` on order form | ✅ Migrated | `Controllers/CheckoutController.cs` |
| V3 | Product data annotations | `Models/Product.cs` | `[Required]`, `[Range]`, `[StringLength]` on product fields | ✅ Migrated | `Models/Product.cs` |
| V4 | CartItem required fields | `Models/CartItem.cs` | `[Required]` on `CartId` | ✅ Migrated | `Models/CartItem.cs` |
| V5 | Category required name | `Models/Category.cs` | `[Required]` on `Name` | ✅ Migrated | `Models/Category.cs` |
| V6 | Order search validation | `Areas/Admin/Controllers/OrdersController.cs` | Null check on `id`; redirect with `invalidOrderSearch` | ✅ Migrated | `Areas/Admin/Controllers/OrdersController.cs` |

---

## 3. Workflows

| # | Logic | Source File | Description | Status | New Location |
|---|---|---|---|---|---|
| W1 | Checkout flow | `Controllers/CheckoutController.cs` | GET address form → POST validate promo → create order → redirect to Complete | ✅ Migrated | `Controllers/CheckoutController.cs` |
| W2 | Add to cart | `Models/ShoppingCart.cs:AddToCart()` | Upsert cart item; increment count if exists | ✅ Migrated | `Models/ShoppingCart.cs` |
| W3 | Remove from cart | `Models/ShoppingCart.cs:RemoveFromCart()` | Decrement count; remove when 0 | ✅ Migrated | `Models/ShoppingCart.cs` |
| W4 | Cart → Order conversion | `Models/ShoppingCart.cs:CreateOrder()` | Convert cart items to OrderDetails; set order total; empty cart | ✅ Migrated | `Models/ShoppingCart.cs` |
| W5 | Cart session cookie | `Models/ShoppingCart.cs:GetCartId()` | GUID cart ID stored in `Session` cookie | ✅ Migrated | `Models/ShoppingCart.cs` (uses `IHttpContextAccessor`) |
| W6 | New product announcement | `Areas/Admin/Controllers/StoreManagerController.cs` | SignalR push to all clients when product created | ✅ Migrated | `Areas/Admin/Controllers/StoreManagerController.cs` |
| W7 | Raincheck creation | `Api/RaincheckController.cs` | POST creates raincheck for product/store | ✅ Migrated | `Api/RaincheckApiController.cs` |

---

## 4. Transformations

| # | Logic | Source File | Description | Status | New Location |
|---|---|---|---|---|---|
| T1 | Product detail JSON → Dictionary | `Models/Product.cs:ProductDetailList` | `JsonConvert.DeserializeObject<Dictionary<string,string>>(ProductDetails)` | ✅ Migrated | `Models/Product.cs` (uses `System.Text.Json`) |
| T2 | Orders date range filter | `Utils/OrdersQuery.cs` | Filter orders by start/end date, username | ✅ Migrated | `Utils/OrdersQuery.cs` |
| T3 | Raincheck query filter | `Utils/RaincheckQuery.cs` | Filter rainchecks by store | ✅ Migrated | `Utils/RaincheckQuery.cs` |
| T4 | Configuration helper | `Utils/ConfigurationHelpers.cs` | Read `appSettings` by key | ✅ Migrated | Replaced by `IConfiguration` injection |

---

## 5. Integrations

| # | Logic | Source File | Description | Status | New Location |
|---|---|---|---|---|---|
| I1 | Azure ML recommendation | `Recommendations/AzureMLFrequentlyBoughtTogetherRecommendationEngine.cs` | HTTP call to Azure ML endpoint with AccountKey | ✅ Migrated | `Recommendations/AzureMLFrequentlyBoughtTogetherRecommendationEngine.cs` (uses `IHttpClientFactory`) |
| I2 | Naive recommendation | `Recommendations/NaiveRecommendationEngine.cs` | In-memory product cross-sell logic | ✅ Migrated | `Recommendations/NaiveRecommendationEngine.cs` |
| I3 | Product text search | `ProductSearch/StringContainsProductSearch.cs` | String.Contains on product Title/Description/Category | ✅ Migrated | `ProductSearch/StringContainsProductSearch.cs` |
| I4 | Application Insights telemetry | `Utils/TelemetryProvider.cs`, `Utils/PartsUnlimitedTelemetryInitializer.cs` | Track events with custom properties | ✅ Migrated | `Utils/TelemetryProvider.cs` |
| I5 | SignalR real-time push | `Hubs/AnnouncementHub.cs` | Broadcast new product announcement to all clients | ✅ Migrated | `Hubs/AnnouncementHub.cs` (ASP.NET Core SignalR) |

---

## 6. Authorization

| # | Logic | Source File | Description | Status | New Location |
|---|---|---|---|---|---|
| A1 | Admin role guard | `Areas/Admin/Controllers/AdminController.cs` | `[Authorize(Roles="Administrator")]` | ✅ Migrated | `Areas/Admin/Controllers/AdminController.cs` (roles from Entra ID token) |
| A2 | Checkout requires auth | `Controllers/CheckoutController.cs` | `[Authorize]` on controller | ✅ Migrated | `Controllers/CheckoutController.cs` |
| A3 | Orders requires auth | `Controllers/OrdersController.cs` | `[Authorize]` on controller | ✅ Migrated | `Controllers/OrdersController.cs` |
| A4 | Manage requires auth | `Controllers/ManageController.cs` | `[Authorize]` on controller | ❌ Removed | Entra ID / SSPR handles profile management |
| A5 | Account login/register | `Controllers/AccountController.cs` | Local user auth | ❌ Removed | Entra ID OIDC (`/MicrosoftIdentity/Account/SignIn`) |

---

## 7. Notifications

| # | Logic | Source File | Description | Status | New Location |
|---|---|---|---|---|---|
| N1 | New product SignalR | `Areas/Admin/Controllers/StoreManagerController.cs` | Push announcement to connected clients on product create | ✅ Migrated | `Areas/Admin/Controllers/StoreManagerController.cs` |
| N2 | Email/SMS 2FA | `App_Start/IdentityConfig.cs` | Stub services for OWIN Identity 2FA | ❌ Removed | Handled by Entra ID MFA policy |

---

## 8. Media & Assets

| Asset Type | Source Path | Migration Status | Notes |
|---|---|---|---|
| Product images | `Images/` | ✅ Copied | Served as static files |
| CSS files | `Content/*.css` | ✅ Copied | `wwwroot/css/` |
| JavaScript | `Scripts/*.js` | ✅ Copied | `wwwroot/js/` |
| Bootstrap 3 | `Content/bootstrap*` | ✅ Copied | Kept for UI compatibility |
| jQuery 3.1.1 | `Scripts/jquery*` | ✅ Copied | Kept for UI compatibility |
| SignalR JS client | `Scripts/jquery.signalR*` | ✅ Replaced | Updated to `@microsoft/signalr` CDN |
| Razor views | `Views/**/*.cshtml` | ✅ Copied | Minor helper updates applied |

---

## Summary

| Category | Total Items | Migrated | Removed | Remaining |
|---|---|---|---|---|
| Calculations | 6 | 6 | 0 | 0 |
| Validations | 6 | 6 | 0 | 0 |
| Workflows | 7 | 7 | 0 | 0 |
| Transformations | 4 | 4 | 0 | 0 |
| Integrations | 5 | 5 | 0 | 0 |
| Authorization | 5 | 3 | 2 | 0 |
| Notifications | 2 | 1 | 1 | 0 |
| **Total** | **35** | **32** | **3** | **0** |
