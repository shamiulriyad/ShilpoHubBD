# ShilpoHubBD Backend

ASP.NET Core 8 Web API for **ShilpoHubBD**, a marketplace connecting Bangladeshi heritage artisans and producers directly with customers. Built with Clean Architecture, repository abstractions, and a service layer, backed by PostgreSQL (Supabase) via Entity Framework Core.

## Tech stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 8 (C#) |
| Database | PostgreSQL (Supabase), via `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Auth | JWT Bearer (access + refresh tokens), BCrypt password hashing |
| Validation | FluentValidation |
| Realtime | SignalR (`/hubs/messaging`, `/hubs/live-events`, `/hubs/live-classes`) |
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
   - SignalR hubs: `/hubs/messaging`, `/hubs/live-events`, `/hubs/live-classes`

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

Most write endpoints require `[Authorize]`; producer-owned resources (products, live events, auctions, QR codes, certificates, traceability records, orders, courses, mentor profiles, apprenticeship & internship programs, sustainability records, heritage identity) are additionally ownership-checked against the authenticated user, with `SuperAdmin` able to manage anything. Job listings follow the same pattern for `BusinessPartner`-role users, gated on top by their business profile's verification status. Catalog/admin-only actions (roles, categories, districts, craft stories, achievement definitions, sustainability material certification verification) require `SuperAdmin`.

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
| **Heritage Academy** | | Learner/trainer/mentor education platform — see the [Heritage Academy modules](#heritage-academy-modules) table below for the full breakdown |
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

### Heritage Academy modules

A full learner → trainer/mentor → certified-professional pipeline for heritage skills, built on top of the existing `User`/`Producer`/`Certificate` system rather than duplicating it. A course/program/job listing is authored by either a `MentorProfile` (a `Producer` who opted into mentoring via `POST /mentors`) or an `AcademyMemberProfile` with `Role = Trainer` — never a new "instructor" concept. All AI-flavored features (skill assessment, learning roadmap, mentor matching, job matching) are rule-based today behind a swappable provider interface, exactly like the marketplace's `IRecommendationProvider`/`IAIBusinessProvider` pattern — ready for a real model later without touching callers.

| Module | Base route | Summary |
|---|---|---|
| Academy Profile | `/academy-profiles` | Learner/Trainer/Mentor role profile, heritage skills with level (Beginner→Expert), learning preferences, learning history |
| Mentors | `/mentors` | A `Producer` becomes a mentor, manages their mentor profile & taught skills |
| Heritage Skills | `/heritage-skills` | Shared skill catalog referenced by courses, member/mentor skills, apprenticeship requirements, job requirements, and certificates — one lookup table, no duplication |
| Courses | `/courses`, `/course-categories` | Mentor- or Trainer-authored courses, modules, lessons, downloadable materials, categories; Draft → Published → Archived lifecycle |
| Enrollments | `/enrollments` | Course enrollment with capacity limits, per-lesson progress tracking, completion (auto-issues a Course certificate) |
| Live Classes | `/live-classes` | Scheduled live classes, participants, live Q&A, attendance, class history; realtime via `/hubs/live-classes` |
| Assignments / Quizzes / Exams | `/assignments`, `/quizzes`, `/exams` | Assignment submission & manual grading; quiz/exam attempts with attempt limits, automatic MCQ scoring, and manual essay evaluation |
| AI Skill Assessment | `/skill-assessments` | Rule-based skill level, strengths/weaknesses, and recommended-skill assessment from a learner's courses, quizzes, exams and assignments — behind `IAISkillAssessmentProvider` for a future real model |
| Learning Roadmap | `/learning-roadmaps` | Rule-based personalized roadmap — goals, skill milestones, recommended courses/lessons, next step — behind `ILearningRoadmapProvider` |
| Mentor Matching | `/mentor-matching` | Rule-based mentor discovery, scored on skill match, skill level, location, experience, availability, category |
| Mentorship | `/mentorship-requests` | Request a mentor, accept/reject/complete, mentorship history |
| Internship & Apprenticeship | `/apprenticeship-programs`, `/program-applications`, `/apprentice-enrollments` | Mentor/Trainer-run internship & apprenticeship programs with eligibility requirements, applications, training milestones, and completion (auto-issues an Apprenticeship certificate) |
| Certificates | `/training-certificates` | Unified digital certificate system — Course, Skill (mentor/trainer-issued), and Apprenticeship certificates all issued, verified, downloaded and revoked through one entity/API, each with its own `SH-TRN-`/`SH-SKL-`/`SH-APR-` numbered ID |
| Digital Portfolio | `/portfolios`, `/mentor-feedback` | Aggregated member portfolio — heritage skills, completed courses, certificates, showcase projects, graded assignments, achievements, apprenticeship experience, mentor feedback — with member-controlled public/private visibility |
| Employment | `/job-listings`, `/job-applications`, `/job-matching` | Job listings from verified `BusinessPartner` employers with skill requirements, applications through hire/reject, and rule-based job recommendations matched against a member's skills, plus employer review of an applicant's portfolio |

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
