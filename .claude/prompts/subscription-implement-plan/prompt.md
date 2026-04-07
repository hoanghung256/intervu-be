Act as a Senior Full-stack Developer (ASP.NET Core & React). I need to implement a dynamic subscription and feature-gating system integrated with Stripe for my application. 

Here are the complete business rules and architectural requirements:

### 1. OVERALL REQUIREMENTS
- **Stripe Integration**: Use Stripe Checkout for payments and Stripe Webhooks as the source of truth for subscription states.
- **Dynamic Plan Management**: Admin can dynamically create/update plans, define access to specific features, and set usage limits (e.g., AI Mock Interviews: 5 times/month) via the database. No hardcoded plan limits.
- **User Lifecycle**: 
  - New users get a default "Free" plan automatically.
  - Users can Upgrade, Downgrade, or Cancel (CancelAtPeriodEnd = true) their subscription.
- **Custom React UI**: Do NOT use the pre-built Stripe Customer Portal. I want to build a custom Pricing Page and Subscription Dashboard in React that communicates with my ASP.NET Core backend.

### 2. DATABASE SCHEMA
Please implement the following entities to support dynamic feature gating:
1. `SubscriptionPlan`: Name, StripeProductId, StripePriceId, Price, Level (for upgrade/downgrade logic).
2. `Feature`: Code (e.g., "MOCK_INTERVIEW"), Name.
3. `PlanFeature` (Many-to-Many): PlanId, FeatureId, MaxLimit (nullable for unlimited).
4. `UserSubscription`: UserId, PlanId, StripeCustomerId, StripeSubscriptionId, Status, CurrentPeriodEnd, CancelAtPeriodEnd.
5. `UserFeatureUsage`: UserId, FeatureId, UsedCount, ResetDate.

### 3. BACKEND LOGIC
Please implement the necessary Controllers and Services:
- **Subscription Service**: Handle creating Stripe Checkout Sessions.
- **Webhook Handler**: An endpoint `POST /webhook` to handle Stripe events (`checkout.session.completed`, `customer.subscription.updated`, `customer.subscription.deleted`, `invoice.payment_succeeded`). This must sync the `UserSubscription` status and reset `UserFeatureUsage` on new billing cycles.
- **Feature Gating**: A service/method `CanUseFeatureAsync(userId, featureCode)` to check if a user's current plan allows access to a feature, and if they haven't exceeded their `MaxLimit`.
- **APIs for React**:
  - `GET /api/v1/subscriptions/plans`: List all plans and their features.
  - `GET /api/v1/subscriptions/my-status`: Get current user's plan, usage stats, and billing end date.
  - `POST /api/v1/subscriptions/create-checkout`: Return Stripe session URL.
  - `POST /api/v1/subscriptions/change-plan`: Handle upgrade/downgrade (including Stripe proration).
  - `POST /api/v1/subscriptions/cancel`: Set cancel_at_period_end = true.

### 4. FRONTEND LOGIC
Please provide the React components:
- **PlanManagementPage**: Management page for Admin
- **PricingPage**: Fetch plans, display them, and handle the "Upgrade" button (redirects to Stripe Checkout).
- **SubscriptionDashboard**: Dashboard for candidate
  - Display current plan status and next billing date.
  - Render progress bars for feature usage (e.g., "Mock Interviews: 2/5 used").
  - Provide "Change Plan" and "Cancel Auto-renewal" buttons calling the respective backend APIs.

### 5. EXECUTION PLAN
Please provide the code in the following order:
1. EF Core Entity classes and DbContext configurations.
2. The core backend services (Stripe integration, Webhook handling, Feature Gating logic).
3. The ASP.NET Core Controller exposing the endpoints.
4. The React components for the Plan Management Pricing and Dashboard pages.

Ensure the code is robust, handles Stripe API exceptions gracefully, and follows best practices for a scalable architecture.