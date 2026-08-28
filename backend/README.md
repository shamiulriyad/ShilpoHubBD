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
- `IAISkillAssessmentProvider` → `DummySkillAssessmentProvider`, `ILearningRoadmapProvider` → `RuleBasedLearningRoadmapProvider` (Application, Heritage Academy)
- `IResearchAIProvider` → `DummyResearchAIProvider` (Infrastructure, rule-based statistics; Heritage Innovation Lab's AI Research Assistant)
- `IHeritageIntelligenceProvider` → `RuleBasedHeritageIntelligenceProvider`, `IPolicySimulationProvider` → `RuleBasedPolicySimulationProvider`, `IGovForecastProvider` → `RuleBasedGovForecastProvider` (Infrastructure, Government & NGO analytics — all rule-based today, ready for a real ML/forecasting model)
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

Most write endpoints require `[Authorize]`; producer-owned resources (products, live events, auctions, QR codes, certificates, traceability records, orders, courses, mentor profiles, apprenticeship & internship programs, sustainability records, heritage identity) are additionally ownership-checked against the authenticated user, with `SuperAdmin` able to manage anything. Job listings follow the same pattern for `BusinessPartner`-role users, gated on top by their business profile's verification status. Catalog/admin-only actions (roles, categories, districts, craft stories, achievement definitions, sustainability material certification verification) require `SuperAdmin`. The **Heritage Innovation Lab** modules are gated to research roles (`HeritageInnovationHub` / `GovernmentNGO` / `SuperAdmin`) at the edge and membership-checked in the service layer; the entire **Government & NGO** module (`/api/governance/*`) is restricted to `GovernmentNGO` / `SuperAdmin`.

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
| **Heritage Innovation Lab** | `/research`, `/heritage-database`, `/field-research`, `/knowledge-graph`, `/innovation-lab` | Researcher workspace for creating and analysing heritage knowledge — see the [Heritage Innovation Lab modules](#heritage-innovation-lab-modules) table below |
| **Government & NGO** | `/governance` | National oversight platform for `GovernmentNGO` / `SuperAdmin` — see the [Government & NGO modules](#government--ngo-modules) table below |

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

### Heritage Innovation Lab modules

A researcher-facing workspace for creating and analysing heritage knowledge, built on top of the existing `User`/`Producer`/`Product`/`Village`/`HeritagePlace`/`Family` entities rather than duplicating them — cross-module links use nullable `SetNull` foreign keys plus resolver services. Membership is the primary access gate for research projects; the AI features are rule-based today behind swappable provider interfaces. Namespaces `ShilpoHubBD.Domain.Entities.{Research, HeritageDatabase, FieldResearch, KnowledgeGraph, Innovation}`.

| Module | Base route | Summary |
|---|---|---|
| Research Workspace | `/research/projects` (+ `/tasks`, `/milestones`, `/notes`, `/papers`, `/publications`), `/research/publications` | Research projects with a `Viewer < Contributor < Researcher < Admin < Owner` role ladder, project members, tasks, milestones, notes, paper management, a global publication repository, and per-project activity history. Project creation gated to `HeritageInnovationHub` / `GovernmentNGO` / `SuperAdmin`; everything else is membership-gated |
| National Heritage Database | `/heritage-database/datasets`, `/heritage-database/live/*`, `/heritage-database/risk`, `/heritage-database/exports/mine` | Structured heritage datasets with versioning and import metadata, researcher access grants, live read projections over Product/Village/HeritagePlace/TouristService/Producer, a new `HeritageRiskRecord` store, and metadata-only export analytics. Live + risk-read gated to research roles |
| AI Research Assistant | `/research/projects/{projectId}/ai` | Rule-based automatic insights, trend discovery, correlation detection, report generation and a citation generator (APA/MLA/Chicago/IEEE/BibTeX) over selected project/dataset/paper data — behind `IResearchAIProvider`; requests and results are persisted and linked to the project |
| Survey & Field Data Collection | `/field-research/surveys` (+ `/{id}/responses`, `/{id}/evidence`) | Owner-managed digital surveys, questions, field-researcher assignments (Collector/Supervisor/Reviewer), GPS-tagged responses, and field evidence (photo/audio/video/interview transcript/document/waypoint/note) — media metadata and file URLs only, no processing |
| Heritage Knowledge Graph | `/knowledge-graph` | Flexible node/relationship model mapping Producer ↔ Village ↔ Craft ↔ Material ↔ Culture ↔ Family ↔ Product, 12 relationship types with metadata, BFS traversal / shortest-path / preset network queries (Producer Relationships, Village Connections, Material/Cultural Network, Family Tree). Whole controller gated to research roles |
| Innovation Lab | `/innovation-lab/{experiments, preservation-strategies, prototypes, submissions}` | AI Model Builder (experiment + version + training-run **metadata only**), Preservation Strategy designer (objectives, actions, timeline), Prototype Testing (iterations, test cases, runs, results, issues), and Heritage Innovation Submissions with team members, reviews and an approval workflow (reviewers = `GovernmentNGO` / `SuperAdmin`) |

### Government & NGO modules

A national oversight platform, entirely gated to `GovernmentNGO` / `SuperAdmin`. Every figure is aggregated live from the existing marketplace / employment / tourism / community tables — no transactional data is duplicated. The three AI-flavoured features are rule-based today behind swappable provider interfaces. Namespace `ShilpoHubBD.Domain.Entities.Governance`; all routes under `/api/governance`.

| Module | Base route | Summary |
|---|---|---|
| National Dashboard | `/governance/dashboard` | Live overview (producers, employment, export growth with preceding-window %, tourism, heritage economy, coverage), district rankings by sales/producers/products/villages/orders, captured snapshots for trend charting, and per-metric trends over snapshots |
| Heritage Intelligence | `/governance/heritage-intelligence` | Six explainable composite indices — Heritage Risk, Living Heritage, Craft Health, Village Survival, Youth Participation, Climate Risk — computed by `IHeritageIntelligenceProvider` for National / District / Village / Craft scope, each with a weighted component breakdown; records are stored and trendable |
| Policy Simulator | `/governance/policy-simulator` | "What-if" scenarios (Grant, Training, Tourism Campaign, Export Strategy, Employment Prediction) run through `IPolicySimulationProvider` against a captured live baseline, producing projected outcomes (baseline vs projected vs delta), per-metric confidence and recommendations |
| Monitoring | `/governance/monitoring`, `/governance/complaints`, `/governance/compliance` | Rule-based fraud / fake-product / review-abuse / QR-anomaly scans that raise triage-workflow `MonitoringFlag`s (with de-dup); complaint intake → triage → update thread → resolution, linkable to a flag; a read-only QR-verification overview; and compliance records with a requirement checklist and auto-derived score/status |
| Funding | `/governance/funding/programs`, `/governance/funding/applications` | Grant / Loan / Scholarship / Equipment-Support and Village / Producer Sponsorship programmes with a budget envelope, then application → review → approve/reject (budget-checked) → scheduled disbursement → (loan) repayment tracking, with live `AllocatedAmount` / `DisbursedAmount` counters and a full audit trail |
| Reports | `/governance/reports`, `/governance/analytics`, `/governance/forecasts` | Generated period reports (Monthly/Quarterly/Annual) assembling dashboard, monitoring and funding data into stored sections; **AI Predictions** projecting six national metrics forward via `IGovForecastProvider` (OLS trend over snapshot history, widening confidence bands); a district-keyed GIS map payload (attributes only, join client-side to boundaries); and metadata-only downloadable-analytics export requests completed by an external worker |

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
