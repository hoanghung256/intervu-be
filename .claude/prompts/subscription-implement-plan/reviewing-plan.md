# Subscription & Feature-Gating Implementation Plan

## Context

The Intervu platform needs a subscription billing system with Stripe to enable tiered access to features (AI interviews, coach bookings, etc.). Currently, the platform has **no subscription entities or Stripe integration**. The existing PayOS payment system handles one-time interview booking transactions and will continue to coexist alongside Stripe subscriptions.

**Key requirement**: Dynamic plan management -- admin can create/update plans and features via the database without code changes. No hardcoded plan limits.

---

## 1. Domain Layer -- New Entities

All in `intervu-be/Intervu.Domain/Entities/`

### Enums (`Intervu.Domain/Entities/Constants/`)

| File | Values |
|---|---|
| `SubscriptionStatus.cs` | `Active`, `PastDue`, `Canceled`, `Trialing`, `Incomplete` |
| `BillingInterval.cs` | `None` (Free), `Monthly`, `Yearly` |

### Entities

**`SubscriptionPlan.cs`** -- extends `EntityAuditable<Guid>`
```
Id, Name, Description, MonthlyPrice (decimal), YearlyPrice (decimal?),
StripeProductId, StripePriceIdMonthly, StripePriceIdYearly,
Level (int, for upgrade/downgrade logic), IsActive (bool),
Nav: ICollection<PlanFeature>, ICollection<UserSubscription>
```

**`Feature.cs`** -- extends `EntityBase<Guid>`
```
Id, Code (string, unique, e.g. "MOCK_INTERVIEW"), Name (string)
Nav: ICollection<PlanFeature>
```

**`PlanFeature.cs`** -- join entity (many-to-many), extends `EntityBase<Guid>`
```
Id, PlanId (Guid FK), FeatureId (Guid FK), MaxLimit (int?, null = unlimited)
Nav: SubscriptionPlan, Feature
```

**`UserSubscription.cs`** -- extends `EntityAuditable<Guid>`
```
Id, UserId (Guid FK), PlanId (Guid FK),
StripeCustomerId, StripeSubscriptionId,
Status (SubscriptionStatus), BillingInterval (BillingInterval),
CurrentPeriodStart (DateTime?), CurrentPeriodEnd (DateTime?),
CancelAtPeriodEnd (bool), CanceledAt (DateTime?)
Nav: User, SubscriptionPlan
```

**`UserFeatureUsage.cs`** -- extends `EntityBase<Guid>`
```
Id, UserId (Guid FK), FeatureId (Guid FK),
UsedCount (int), PeriodStart (DateTime), PeriodEnd (DateTime)
Nav: User, Feature
```

### Why dynamic Feature/PlanFeature instead of hardcoded columns

The user explicitly requires "No hardcoded plan limits." The Feature + PlanFeature M2M design lets admin add new gated features (e.g., "RESUME_REVIEW", "ADVANCED_ANALYTICS") via database without code deployment. `MaxLimit = null` means unlimited access.

---

## 2. Repository Interfaces

All in `intervu-be/Intervu.Domain/Repositories/`

| Interface | Key Methods (beyond IRepositoryBase) |
|---|---|
| `ISubscriptionPlanRepository` | `GetActivePlansWithFeaturesAsync()`, `GetByIdWithFeaturesAsync(Guid)`, `GetFreePlanAsync()` |
| `IFeatureRepository` | `GetByCodeAsync(string code)`, `GetAllAsync()` |
| `IPlanFeatureRepository` | `GetByPlanIdAsync(Guid planId)` |
| `IUserSubscriptionRepository` | `GetActiveByUserIdAsync(Guid userId)`, `GetByStripeSubscriptionIdAsync(string)` |
| `IUserFeatureUsageRepository` | `GetByUserAndFeatureAsync(Guid userId, Guid featureId)`, `GetAllByUserIdAsync(Guid userId)` |

---

## 3. Infrastructure Layer

### DbContext Update
**File**: `intervu-be/Intervu.Infrastructure/Persistence/PostgreSQL/DataContext/IntervuPostgreDbContext.cs`

- Add 5 new `DbSet<>` properties
- Fluent API config in `OnModelCreating`:
  - `SubscriptionPlan`: unique index on `Name`
  - `Feature`: unique index on `Code`
  - `PlanFeature`: composite unique on `(PlanId, FeatureId)`, cascade delete from Plan/Feature
  - `UserSubscription`: index on `(UserId, Status)`, unique on `StripeSubscriptionId`
  - `UserFeatureUsage`: composite unique on `(UserId, FeatureId, PeriodStart)`
- **Seed data**: Default Free plan + core Feature records ("MOCK_INTERVIEW", "COACH_BOOKING", "RESUME_REVIEW") + PlanFeature links with limits

### Repository Implementations
In `intervu-be/Intervu.Infrastructure/Persistence/PostgreSQL/Repositories/` -- 5 new files, each extending `RepositoryBase<T>` with `IntervuPostgreDbContext` injection (same pattern as existing repos).

### Stripe External Service

**Interface**: `intervu-be/Intervu.Application/Interfaces/ExternalServices/IStripeService.cs`
```csharp
Task<string> CreateCustomerAsync(string email, string name);
Task<string> CreateCheckoutSessionAsync(string stripeCustomerId, string stripePriceId, string successUrl, string cancelUrl, Dictionary<string, string> metadata);
Task<Subscription> GetSubscriptionAsync(string stripeSubscriptionId);
Task<Subscription> UpdateSubscriptionPriceAsync(string stripeSubscriptionId, string newStripePriceId);
Task<Subscription> CancelAtPeriodEndAsync(string stripeSubscriptionId);
Task<Subscription> ReactivateAsync(string stripeSubscriptionId);
bool VerifyWebhookSignature(string json, string stripeSignature, out Event stripeEvent);
```

**Implementation**: `intervu-be/Intervu.Infrastructure/ExternalServices/StripeService/StripeService.cs`
- Uses `Stripe.net` NuGet package
- Config from `appsettings.json` section `"Stripe": { "SecretKey", "WebhookSecret" }`
- `VerifyWebhookSignature` uses `EventUtility.ConstructEvent()` for signature validation

### EF Migration
```bash
dotnet ef migrations add AddSubscriptionEntities --project Intervu.Infrastructure --startup-project Intervu.API -o Persistence/PostgreSQL/Migrations
```

Include SQL for existing users:
```sql
INSERT INTO "UserSubscriptions" (...)
SELECT gen_random_uuid(), u."Id", <FreePlanId>, 0, 0, NOW(), false, ...
FROM "Users" u WHERE NOT EXISTS (SELECT 1 FROM "UserSubscriptions" us WHERE us."UserId" = u."Id");
```

---

## 4. Application Layer -- DTOs

In `intervu-be/Intervu.Application/DTOs/Subscription/`:

| DTO | Purpose |
|---|---|
| `SubscriptionPlanDto` | Plan + list of `PlanFeatureDto` (featureCode, featureName, maxLimit) |
| `PlanFeatureDto` | FeatureCode, FeatureName, MaxLimit |
| `UserSubscriptionStatusDto` | PlanName, Status, BillingInterval, CurrentPeriodEnd, CancelAtPeriodEnd, `List<FeatureUsageDto>` |
| `FeatureUsageDto` | FeatureCode, FeatureName, UsedCount, MaxLimit |
| `CreateCheckoutDto` | PlanId, BillingInterval, SuccessUrl, CancelUrl |
| `ChangePlanDto` | NewPlanId, BillingInterval |
| `CreateSubscriptionPlanDto` | Admin: Name, Description, Prices, StripeIds, Level, Features[] |
| `UpdateSubscriptionPlanDto` | Admin: same as Create |

AutoMapper profile addition in `intervu-be/Intervu.Application/Mappings/MappingProfile.cs`.

---

## 5. Application Layer -- UseCases

All interfaces in `intervu-be/Intervu.Application/Interfaces/UseCases/Subscription/`
All implementations in `intervu-be/Intervu.Application/UseCases/Subscription/`

### Public UseCases

| UseCase | Signature | Notes |
|---|---|---|
| `IGetSubscriptionPlans` | `Task<List<SubscriptionPlanDto>> ExecuteAsync()` | Returns active plans with features. No auth required. |
| `IGetMySubscriptionStatus` | `Task<UserSubscriptionStatusDto> ExecuteAsync(Guid userId)` | Calls `IEnsureUserSubscription` if no record. Returns plan + usage. |
| `ICreateCheckoutSession` | `Task<string> ExecuteAsync(Guid userId, CreateCheckoutDto dto)` | Creates Stripe customer if needed, returns checkout URL. Stores userId in Stripe session metadata. |
| `IChangePlan` | `Task ExecuteAsync(Guid userId, ChangePlanDto dto)` | Uses Level field for upgrade/downgrade. Calls Stripe proration API. |
| `ICancelSubscription` | `Task ExecuteAsync(Guid userId)` | Sets `cancel_at_period_end = true` on Stripe. |
| `IReactivateSubscription` | `Task ExecuteAsync(Guid userId)` | Un-cancels before period end. |

### Admin UseCases

| UseCase | Signature |
|---|---|
| `ICreateSubscriptionPlan` | `Task<SubscriptionPlanDto> ExecuteAsync(CreateSubscriptionPlanDto dto)` |
| `IUpdateSubscriptionPlan` | `Task ExecuteAsync(Guid planId, UpdateSubscriptionPlanDto dto)` |
| `IDeleteSubscriptionPlan` | `Task ExecuteAsync(Guid planId)` -- soft delete |
| `IAdminGetAllSubscriptions` | `Task<PagedResult> ExecuteAsync(int page, int pageSize)` |
| `IManageFeatures` | `Task<List<FeatureDto>> ExecuteAsync()` + CRUD for Feature entities |

### Internal UseCases

| UseCase | Signature | Notes |
|---|---|---|
| `IHandleStripeWebhook` | `Task ExecuteAsync(string json, string stripeSignature)` | See webhook events below |
| `ICheckFeatureAccess` | `Task<(bool allowed, int remaining)> ExecuteAsync(Guid userId, string featureCode)` | Lazy-resets usage if period expired |
| `IIncrementFeatureUsage` | `Task ExecuteAsync(Guid userId, string featureCode)` | Increments UsedCount |
| `IEnsureUserSubscription` | `Task ExecuteAsync(Guid userId)` | Creates Free plan sub if none exists |

### Webhook Events Handled by `HandleStripeWebhook`

| Event | Action |
|---|---|
| `checkout.session.completed` | Create/activate UserSubscription, init UserFeatureUsage records |
| `invoice.paid` | Advance CurrentPeriodStart/End, reset all UserFeatureUsage for user |
| `invoice.payment_failed` | Set status to `PastDue` |
| `customer.subscription.updated` | Sync plan, status, period, cancel_at_period_end |
| `customer.subscription.deleted` | Set status `Canceled`, revert to Free plan |

### Feature Gating Strategy

**Explicit UseCase-level checks** (not middleware). Consistent with how the codebase validates business rules by throwing `BusinessException` subclasses.

Integration points in existing code:
1. **AI Interview creation**: Inject `ICheckFeatureAccess` + `IIncrementFeatureUsage`, check `"MOCK_INTERVIEW"` before creating
2. **Coach booking** (`CreateBookingCheckoutUrl`): Check `"COACH_BOOKING"` before PayOS checkout
3. Throw `ForbiddenException("Monthly limit reached. Upgrade your plan.")` when denied

---

## 6. API Layer -- Controllers

### `SubscriptionController.cs` (`api/v1/subscription/`)

| Method | Path | Auth | UseCase |
|---|---|---|---|
| GET | `/plans` | Public | `IGetSubscriptionPlans` |
| GET | `/my-status` | AllRoles | `IGetMySubscriptionStatus` |
| POST | `/checkout` | Candidate | `ICreateCheckoutSession` |
| POST | `/change-plan` | Candidate | `IChangePlan` |
| POST | `/cancel` | Candidate | `ICancelSubscription` |
| POST | `/reactivate` | Candidate | `IReactivateSubscription` |

### `StripeWebhookController.cs` (`api/v1/stripe/`)

| Method | Path | Auth | Notes |
|---|---|---|---|
| POST | `/webhook` | None | Reads raw body, verifies Stripe-Signature header |

### Admin endpoints -- add to `AdminController.cs` or new `AdminSubscriptionController.cs`

| Method | Path | UseCase |
|---|---|---|
| GET | `/admin/subscription/plans` | Get all plans (incl. inactive) |
| POST | `/admin/subscription/plans` | Create plan |
| PUT | `/admin/subscription/plans/{id}` | Update plan |
| DELETE | `/admin/subscription/plans/{id}` | Soft-delete plan |
| GET | `/admin/subscription/features` | List features |
| POST | `/admin/subscription/features` | Create feature |
| GET | `/admin/subscriptions` | List all user subscriptions |

Response format: `Ok(new { success, message, data })` -- same as all existing controllers.

---

## 7. DI Registration

**`Intervu.Application/DependencyInjection.cs`** -- add all UseCase registrations as `AddScoped<>`
**`Intervu.Infrastructure/DependencyInjection.cs`**:
- Add 5 repository registrations in `AddPersistenceSqlServer()`
- Add `IStripeService` as `AddSingleton<>` in `AddInfrastructureExternalServices()`

**NuGet**: Add `Stripe.net` to `Intervu.Infrastructure.csproj`

---

## 8. Usage Reset Mechanism

**Hangfire recurring job** following existing `IRecurringJob` pattern.

File: `intervu-be/Intervu.Infrastructure/BackgroundJobs/UsageResetJob.cs`
- Implements `IRecurringJob` (JobId: `"subscription-usage-reset"`, Cron: `"0 * * * *"` hourly)
- Queries `UserFeatureUsage` where `PeriodEnd < UtcNow`, resets `UsedCount = 0`, advances period
- Also done lazily in `CheckFeatureAccess` (inline reset if expired) -- Hangfire is the safety net

Register in `Intervu.Infrastructure/DependencyInjection.cs` same as `InterviewMonitorJob`.

---

## 9. Frontend

### Feature Structure
```
intervu-fe/src/features/subscription/
    services/subscriptionApi.js
    pages/
        PricingPage.jsx
        SubscriptionDashboard.jsx
    components/
        PlanCard.jsx
        UsageProgressBar.jsx
        PlanComparisonTable.jsx
        ChangePlanModal.jsx
```

### API Service (`subscriptionApi.js`)
Endpoints object + helper functions using `callApi()` from `src/common/utils/apiConnector.js`.

### PricingPage
- Fetches plans via `GET /subscription/plans`
- Renders `PlanCard` components in MUI `Grid`
- Current plan highlighted with `secondary.main` accent
- "Upgrade" button redirects to Stripe Checkout URL from `POST /subscription/checkout`
- Reuses: `PrimaryButton`, `SecondaryButton`, `BaseCard`, `CommonLoader`
- Theme tokens only -- no hardcoded colors

### SubscriptionDashboard
- Fetches status via `GET /subscription/my-status`
- Shows plan name + status with `StatusChip`
- MUI `LinearProgress` bars for each feature usage
- "Change Plan" opens `ChangePlanModal` (uses `ConfirmModal` pattern)
- "Cancel" with confirmation dialog
- "Reactivate" button if `cancelAtPeriodEnd = true`

### Admin PlanManagementPage
- CRUD for plans and features
- Table with plan list, edit/delete actions
- Dialog for creating/editing plans with feature limit assignments

### Routes
Add to `intervu-fe/src/app/routes/index.jsx`:
- `/pricing` -- public, `MainLayout`
- `/subscription` -- protected (CANDIDATE, INTERVIEWER), `MainLayout`

Add to `adminRoutes.jsx`:
- `/admin/subscriptions` -- protected (ADMIN)

### useSubscription Hook
`src/common/hooks/useSubscription.js` -- fetches `/subscription/my-status`, exposes `canUse(featureCode)`, `remaining(featureCode)`. Used by components to show upgrade prompts when limits reached.

### npm Package
No Stripe npm packages needed -- we redirect to Stripe hosted Checkout page.

---

## 10. PayOS Coexistence

- `IPaymentService` (PayOS) remains unchanged for booking payments
- `IStripeService` (new) handles subscriptions only
- No shared entities, controllers, or code between the two
- Only interaction: existing booking UseCases gain an `ICheckFeatureAccess` call before processing

---

## 11. Execution Order

| Phase | Tasks | Depends On |
|---|---|---|
| **1. Domain** | Enums, Entities (5), Repository interfaces (5) | -- |
| **2. Infrastructure DB** | DbContext update, repo implementations, EF migration, seed data | Phase 1 |
| **3. Stripe Service** | `Stripe.net` NuGet, `IStripeService`, `StripeService`, appsettings config | Phase 1 |
| **4. Application** | DTOs, AutoMapper, all UseCase interfaces + implementations, DI registration | Phase 2+3 |
| **5. API** | `SubscriptionController`, `StripeWebhookController`, admin endpoints | Phase 4 |
| **6. Feature Gating** | Modify existing booking/AI UseCases, `UsageResetJob` | Phase 4 |
| **7. Frontend** | Services, components, pages, routes, hook | Phase 5 |
| **8. Testing** | Integration tests, Stripe test mode E2E, verify PayOS unchanged | Phase 7 |

---

## 12. Verification

1. **Backend build**: `cd intervu-be && dotnet build` -- no errors
2. **Migration**: Apply migration, verify tables + seed data created
3. **Stripe test mode**: Create checkout session with test keys, complete payment, verify webhook updates UserSubscription
4. **Feature gating**: Hit a gated endpoint with Free plan user at limit, verify 403
5. **Frontend**: Pricing page loads plans, checkout redirects to Stripe, dashboard shows usage
6. **PayOS**: Verify existing booking payment flow unchanged
7. **Admin**: Create/update plans via admin UI, verify reflected on pricing page

---

## Key Files to Modify (Existing)

| File | Change |
|---|---|
| `Intervu.Infrastructure/Persistence/PostgreSQL/DataContext/IntervuPostgreDbContext.cs` | Add 5 DbSets + config + seed |
| `Intervu.Application/DependencyInjection.cs` | Register ~15 new UseCases |
| `Intervu.Infrastructure/DependencyInjection.cs` | Register 5 repos + StripeService |
| `Intervu.Infrastructure/Intervu.Infrastructure.csproj` | Add Stripe.net NuGet |
| `Intervu.Application/Mappings/MappingProfile.cs` | Add subscription mappings |
| `intervu-fe/src/app/routes/index.jsx` | Add /pricing, /subscription routes |
| `intervu-fe/src/app/routes/adminRoutes.jsx` | Add /admin/subscriptions route |
| Existing booking/AI UseCases | Add ICheckFeatureAccess + IIncrementFeatureUsage calls |
