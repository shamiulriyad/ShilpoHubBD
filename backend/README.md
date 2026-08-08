# ShilpoHubBD Backend

ASP.NET Core 8 Web API for **ShilpoHubBD**, a marketplace connecting Bangladeshi heritage artisans and producers directly with customers. Built with Clean Architecture, repository abstractions, and a service layer, backed by PostgreSQL (Supabase) via Entity Framework Core.

## Tech stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 8 (C#) |
| Database | PostgreSQL (Supabase), via `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Auth | JWT Bearer (access + refresh tokens), BCrypt password hashing |
| Validation | FluentValidation |
| Realtime | SignalR (`/hubs/messaging`, `/hubs/live-events`) |
| API docs | Swagger / OpenAPI (`Swashbuckle`) |
| Health checks | `/health/db` |

## Architecture

Clean Architecture, five projects under `src/`, each depending only on the layers inside it:

```
ShilpoHubBD.Domain          Entities, enums, constants. No dependencies.
ShilpoHubBD.Application      DTOs, service interfaces, service implementations,
                              repository interfaces, FluentValidation validators.
ShilpoHubBD.Data             EF Core DbContext, entity configurations,
                              repository implementations, migrations.
ShilpoHubBD.Infrastructure    Cross-cutting concerns: JWT issuing, password hashing,
                              email sending, payment/recommendation providers.
ShilpoHubBD.Api               Controllers, middleware, SignalR hubs, Program.cs
                              (composition root).
```

Each feature area follows the same pattern: `Domain/Entities/<Area>` → `Application/DTOs|Interfaces|Services|Validators/<Area>` → `Data/Configurations|Repositories` → `Api/Controllers`. Dependency injection is wired per-layer in `DependencyInjection.cs` (`Application`, `Data`, `Infrastructure`), all composed in `Api/Program.cs`.

Abstract providers (swap the implementation without touching callers):
- `IPaymentProvider` → `CashOnDeliveryPaymentProvider` (Infrastructure)
- `IRecommendationProvider` → `DummyRecommendationProvider` (Infrastructure)
- `ISearchProvider` → `PostgresProductSearchProvider` (Data, native PostgreSQL full-text search)
- `IAIBusinessProvider` → `DummyAIBusinessProvider` (Infrastructure, rule-based; ready for a future Gemini/OpenAI/custom-ML implementation)
- `IMessageNotifier` → `SignalRMessageNotifier`, `ILiveEventNotifier` → `SignalRLiveEventNotifier` (Api/Realtime, keep SignalR out of the Application layer)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A PostgreSQL database (this project is set up against [Supabase](https://supabase.com))

## Setup

1. Copy the example env file from the repo root and fill in your values:
   ```
   cp .env.example .env
   ```
2. Set at minimum:
   - `ConnectionStrings__DefaultConnection` — your Supabase/Postgres connection string
   - `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key` — JWT signing config (`Jwt__Key` should be a long random secret)
   - Optionally `Seed__SuperAdminEmail` / `Seed__SuperAdminPassword` to auto-seed a SuperAdmin user on first run (Development only)

   The API loads `.env` automatically in Development from the repo root, `backend/`, or `backend/src/ShilpoHubBD.Api/` (first one found wins).

3. Restore and build:
   ```
   dotnet restore ShilpoHubBD.sln
   dotnet build ShilpoHubBD.sln
   ```

4. Apply database migrations:
   ```
   dotnet ef database update --project src/ShilpoHubBD.Data --startup-project src/ShilpoHubBD.Api
   ```

5. Run the API:
   ```
   dotnet run --project src/ShilpoHubBD.Api
   ```
   - Swagger UI (Development): `https://localhost:5001/swagger`
   - Health check: `GET /health/db`
   - SignalR hubs: `/hubs/messaging`, `/hubs/live-events`

## Database migrations

```
# Add a new migration after changing entities/configurations
dotnet ef migrations add <Name> --project src/ShilpoHubBD.Data --startup-project src/ShilpoHubBD.Api

# Apply pending migrations
dotnet ef database update --project src/ShilpoHubBD.Data --startup-project src/ShilpoHubBD.Api

# List migration history
dotnet ef migrations list --project src/ShilpoHubBD.Data --startup-project src/ShilpoHubBD.Api
```

## Authentication & roles

JWT Bearer auth (`Authorization: Bearer <token>`). Roles (`ShilpoHubBD.Domain.Constants.RoleNames`):

`Customer`, `Producer`, `BusinessPartner`, `Tourist`, `HeritageAcademyMember`, `HeritageInnovationHub`, `GovernmentNGO`, `LogisticsPartner`, `SuperAdmin`.

Most write endpoints require `[Authorize]`; producer-owned resources (products, live events, auctions, QR codes, certificates, traceability records, orders, courses, mentor profiles, sustainability records, heritage identity) are additionally ownership-checked against the authenticated user, with `SuperAdmin` able to manage anything. Catalog/admin-only actions (roles, categories, districts, craft stories, achievement definitions, sustainability material certification verification) require `SuperAdmin`.

## API modules

All routes are prefixed `/api`. Auth requirement: **Public** (no token needed), **Auth** (any logged-in user), or **Role** (specific role(s), e.g. `Producer`/`SuperAdmin`) — most modules mix all three across their endpoints.

| Module | Base route | Summary |
|---|---|---|
| Auth | `/auth` | Register, login, refresh, logout, password reset |
| Roles | `/roles` | Role catalog (SuperAdmin-managed) |
| Categories / Districts | `/categories`, `/districts` | Marketplace reference data |
| Products | `/products` | Product CRUD, variants, bulk upload, production videos, 360° images, handmade verification, featured/trending lists |
| Inventory | `/inventory` | Stock adjustments with audit history, low-stock alerts |
| Custom Orders | `/custom-orders` | Customer-to-producer custom order requests, quotes, accept/reject/cancel |
| Craft Stories / Producer Stories | `/craft-stories`, `/producer-stories` | Editorial heritage content per category/producer |
| Heritage Identity | `/heritage-identity` | Digital heritage ID, government verification, family heritage tree, skill timeline, awards, certifications, workshop profile, story archive, and the **Heritage Legacy Score** (configurable rule-based weights across experience/verification/awards/certifications/products/reviews/apprentices trained/courses published/cultural contribution, with score history and a recalculation endpoint) |
| Workshop Gallery | `/workshop-gallery` | Producer workshop media |
| Wishlist / Cart | `/wishlist`, `/cart` | Per-user shopping state |
| Orders | `/orders` | Checkout, tracking, cancel/return, fulfillment lifecycle |
| Payments | `/payments` | Initiate, verify, callback, refund (provider-abstracted) |
| Reviews | `/reviews` | Ratings, review images, edit/delete |
| Questions / Discussions | `/questions`, `/discussions` | Community Q&A and threaded discussions |
| Producer Follows / Villages | `/producer-follows`, `/villages` | Follow producers, favorite villages |
| Messaging | `/messaging` | Conversations, messages, read receipts, realtime via SignalR |
| Producer Orders | `/producer/orders` | Producer-side order fulfillment (accept/reject/processing/ship/deliver, scoped to a producer's own products), customer list, revenue dashboard, sales analytics, visitor analytics, income reports, product performance |
| AI Business Assistant | `/producer/ai-business` | Price suggestion, product description generator, translation, demand forecast, production planner, material forecast, seasonal prediction, sales insights — all rule-based/dummy today, provider-abstracted for a future real model |
| Live Shopping | `/live-events` | Live events, comments, reactions, buy-during-live, optional linked live auction, realtime updates via `/hubs/live-events` |
| Learning & Mentorship | `/mentors`, `/courses`, `/enrollments`, `/training-certificates` | Become a mentor, publish courses & lessons, apprentice enrollment, lesson-progress tracking, auto-issued training certificates on course completion (verify/download, same pattern as product certificates) |
| Sustainability | `/sustainability` | Eco Score, Green Production Badge, Carbon Savings, sustainable material records and certifications — configurable rule-based scoring, no AI |
| Auctions | `/auctions` | Auctions, bids, timer-driven winner resolution |
| QR Verification | `/qr-verification` | Generate/verify/revoke product QR codes, scan history |
| Certificates | `/certificates` | Generate/verify/download certificates of authenticity |
| Traceability | `/traceability` | Product journey, material sources, timeline |
| Recommendations | `/recommendations` | Provider-based product recommendations (rule-based, no AI) |
| Search | `/search` | PostgreSQL full-text product search (no AI) |
| AI Shopping | `/ai-shopping` | Placeholder gift/fashion/interior/translation endpoints (mock data, no AI) |
| Passport | `/passport` | District, festival, and purchase badges |
| Achievements | `/achievements` | XP, levels, achievement unlocks |
| Analytics | `/analytics` | Personal purchase analytics, spending, favorite categories |
| Impact | `/impact` | Heritage score, families supported, estimated CO₂ savings |

## Project structure

```
backend/
├── ShilpoHubBD.sln
├── Directory.Build.props        # shared MSBuild settings (net8.0, nullable, warnings-as-errors)
└── src/
    ├── ShilpoHubBD.Domain/
    ├── ShilpoHubBD.Application/
    ├── ShilpoHubBD.Data/
    │   └── Migrations/
    ├── ShilpoHubBD.Infrastructure/
    └── ShilpoHubBD.Api/
        ├── Controllers/
        ├── Hubs/
        ├── Middlewares/
        ├── Realtime/            # SignalR notifier implementations (IMessageNotifier, ILiveEventNotifier)
        └── Program.cs
```
