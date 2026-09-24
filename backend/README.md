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
- `IHeritageAssistantProvider` → `RuleBasedHeritageAssistantProvider`, `ICounterfeitDetectionProvider` → `RuleBasedCounterfeitDetectionProvider`, `IStoryGeneratorProvider` → `RuleBasedStoryGeneratorProvider`, `ISentimentAnalysisProvider` → `RuleBasedSentimentAnalysisProvider` (Infrastructure, Cross-Platform AI features — rule-based/keyword/lexicon today, ready for a real model)
- `IBackupRunner` → `PgDumpBackupRunner` (Infrastructure, Super Admin Security — shells out to a real `pg_dump`; fails clearly if PostgreSQL client tools aren't available rather than faking success)
- `IMessageNotifier` → `SignalRMessageNotifier`, `ILiveEventNotifier` → `SignalRLiveEventNotifier` (Api/Realtime, keep SignalR out of the Application layer)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A PostgreSQL database (this project is set up against [Supabase](https://supabase.com))

## Setup

1. From the `backend` directory, copy the example env file and fill in your values:
   ```
   cp .env.example .env
   ```
2. Set at minimum:
   - `ConnectionStrings__DefaultConnection` — your Supabase/Postgres connection string
   - `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key` — JWT signing config (`Jwt__Key` should be a long random secret)
   - Optionally `Seed__SuperAdminEmail` / `Seed__SuperAdminPassword` to auto-seed a SuperAdmin user on first run (Development only)
   - Optionally `Gemini__ApiKey` (a [Google AI Studio](https://aistudio.google.com/apikey) key) to enable real AI-generated itineraries and translation for the Tourist AI Travel Planner (`api/ai-tourism/*`). Without it, those endpoints automatically fall back to the deterministic rule-based planner — nothing breaks, the itinerary is just less conversational.
   - No key needed for geocoding/routing in the Travel Planner — it uses the free, keyless public **Nominatim** (`Nominatim:*`) and **OSRM demo** (`Osrm:*`) services by default. Both are fine for development but are public demo instances not meant for production load; for production, self-host Nominatim/OSRM or point `Nominatim:BaseUrl`/`Osrm:BaseUrl` at a paid provider behind the same `IGeocodingProvider`/`IRoutingProvider` interfaces.

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
   - Swagger UI (default HTTP profile): `http://localhost:5065/swagger`
   - Health check: `GET /health/db`
   - SignalR hubs: `/hubs/messaging`, `/hubs/live-events`, `/hubs/live-classes`


### Missing database configuration / registration cannot reach the server

If startup reports `Connection string 'DefaultConnection' is not configured`, the API has exited and the browser cannot register users. In `backend/.env`, set the real PostgreSQL host, database, username and password using the `Host=...;Port=...;Database=...;Username=...;Password=...` format. The example values must be replaced; a Supabase URL or API key is not a database connection string.

Set `Jwt__Key` to a random signing key. Generate one in PowerShell and paste the result into your private `backend/.env`:

```powershell
$keyBytes = New-Object byte[] 48
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($keyBytes)
[Convert]::ToBase64String($keyBytes)
$rng.Dispose()
```

Keep `.env` private (it is ignored by Git). Apply the migrations above to your intended database, then start the default HTTP profile from `backend`:

```bash
dotnet run --project src/ShilpoHubBD.Api --launch-profile http
```

Check `http://localhost:5065/health/db`: it must return HTTP 200 / `Healthy`. The frontend now defaults to `http://localhost:5065/api`. Remove any old `VITE_API_BASE_URL` override or update it to this address, then restart Vite. If you deliberately select the HTTPS profile, set the frontend override to `https://localhost:5001/api` and trust the .NET development certificate.

## Database migrations

```
# Add a new migration after changing entities/configurations
dotnet ef migrations add <Name> --project src/ShilpoHubBD.Data --startup-project src/ShilpoHubBD.Api

# Apply pending migrations
dotnet ef database update --project src/ShilpoHubBD.Data --startup-project src/ShilpoHubBD.Api

# List migration history
dotnet ef migrations list --project src/ShilpoHubBD.Data --startup-project src/ShilpoHubBD.Api
```

## Database connection

The API talks to PostgreSQL directly over EF Core / Npgsql — **Supabase is used only as the hosted Postgres instance** (and Storage). Supabase's own Auth, PostgREST, and RLS are not used.

Connection wiring, in order:

1. `ConnectionStrings__DefaultConnection` is provided as an environment variable or in `.env` (repo root / `backend/` / `backend/src/ShilpoHubBD.Api/`). In Development, `Program.cs` loads the first `.env` it finds via `DotNetEnv` and then `AddEnvironmentVariables()`. Use the Supabase **session pooler** Npgsql connection string (Project Settings → Database → Connection string), e.g. `Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<db-password>;SSL Mode=Require;Trust Server Certificate=true`.
2. `Program.cs` calls `builder.Services.AddData(builder.Configuration)` ([`ShilpoHubBD.Data/DependencyInjection.cs`](src/ShilpoHubBD.Data/DependencyInjection.cs)), which reads `GetConnectionString("DefaultConnection")` (throws at startup if empty) and registers `ShilpoHubDbContext` with `options.UseNpgsql(connectionString)`.
3. The same string is registered as the `supabase-postgres` health check exposed at `GET /health/db`.
4. Repositories take `ShilpoHubDbContext` by DI and persist through `SaveChangesAsync()`; Npgsql manages a pooled TCP + SSL connection to the Supabase host.

The schema is created and evolved **only** by the EF Core migrations above — there is no `Database.Migrate()` / `EnsureCreated()` on startup, so run `dotnet ef database update` after pulling new migrations.

## Authentication & roles

JWT Bearer auth (`Authorization: Bearer <token>`). Roles (`ShilpoHubBD.Domain.Constants.RoleNames`):

`Customer`, `Producer`, `BusinessPartner`, `Tourist`, `HeritageAcademyMember`, `HeritageInnovationHub`, `GovernmentNGO`, `LogisticsPartner`, `SuperAdmin`.

Most write endpoints require `[Authorize]`; producer-owned resources (products, live events, auctions, QR codes, certificates, traceability records, orders, courses, mentor profiles, apprenticeship & internship programs, sustainability records, heritage identity) are additionally ownership-checked against the authenticated user, with `SuperAdmin` able to manage anything. Job listings follow the same pattern for `BusinessPartner`-role users, gated on top by their business profile's verification status. Catalog/admin-only actions (roles, categories, districts, craft stories, achievement definitions, sustainability material certification verification) require `SuperAdmin`. The **Heritage Innovation Lab** modules are gated to research roles (`HeritageInnovationHub` / `GovernmentNGO` / `SuperAdmin`) at the edge and membership-checked in the service layer; the entire **Government & NGO** module (`/api/governance/*`) is restricted to `GovernmentNGO` / `SuperAdmin`. The **Super Admin Dashboard**'s admin actions (`/api/admin/*`, `/api/cms/*` writes) are `SuperAdmin`-only; its **Cross-Platform AI** endpoints are the deliberate exception — public or any-authenticated-user, since they're meant to be used by every role. Every login attempt is recorded and a blocked IP is rejected before its credentials are even checked (Super Admin Threat Detection, `AuthService.LoginAsync`).

### Where auth data lives

This is a **self-contained identity system** — it does **not** use Supabase Auth. Accounts are rows in the application's own `public."Users"` table (plus `Roles`, `UserRoles`, `RefreshTokens`, `PasswordResetTokens`), all created by the `AddAuthIdentitySchema` migration. Passwords are stored only as BCrypt hashes (`IPasswordHasher` → `BCryptPasswordHasher`); access tokens are self-issued JWTs signed with `Jwt__Key`; refresh tokens are stored hashed and rotated on use.

Consequences:

- The Supabase dashboard's **Authentication → Users** page reads `auth.users` and will always be empty. Registered users appear under **Table Editor → `public` → `Users`**, or via `select "Id", "Email", "FullName", "CreatedAt" from public."Users";` in the SQL editor.
- `Roles` is seeded by the migration (all nine role names). No user rows are seeded by migrations. Set `Seed__SuperAdminEmail` / `Seed__SuperAdminPassword` in `.env` to have `Program.cs` create one `SuperAdmin` on startup if none exists (idempotent; leave blank to skip).

Registration flow: React form → `POST /api/auth/register` → `AuthController` → FluentValidation (`RegisterRequestValidator`: email/password rules, `SelfRegisterableRoles` only — `SuperAdmin` is rejected) → `AuthService.RegisterAsync` (lowercases email, `409` if it exists, BCrypt-hashes the password, builds the `User` + `UserRole` rows) → `SaveChangesAsync()` → Supabase Postgres → returns a JWT + refresh token.

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
| **Super Admin Dashboard** | `/admin`, `/cms`, `/ai` | Platform administration for `SuperAdmin`, plus public Cross-Platform AI features usable by every role — see the [Super Admin modules](#super-admin-modules) table below |

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

### Super Admin modules

The Super Admin dashboard (`SuperAdmin` role). Most of it reuses existing engines rather than duplicating them — Fraud Control and AI Moderation both raise `MonitoringFlag`s through the same rule-based scan engine the Government & NGO Monitoring module uses; Marketplace Monitoring is that same module, just with `SuperAdmin` already in its role list. The Cross-Platform AI features are the only part of this dashboard open to every role (most have no `[Authorize]` at all), matching the platform-wide "everyone uses" framing — real content moderation/business-intelligence AI (Price Recommendation, Demand Prediction, Fraud Detection, Policy Recommendation) stays role-gated to the team it serves. Namespaces `ShilpoHubBD.Domain.Entities.{Admin, Cms, Security}`.

| Module | Base route | Summary |
|---|---|---|
| User Management | `/admin/users`, `/roles`, `/admin/permissions`, `/identity-verifications` | User directory (search/filter, activate/deactivate), role assignment, a permission catalogue (26 codes across all 6 dashboard sections — stored and manageable, not yet enforced at runtime beyond the coarse role check), and the identity-verification review workflow |
| Heritage Management | `/categories`, `/villages`, `/districts`, `/heritage-festivals`, `/unesco-records` | Plain `SuperAdmin`-gated CRUD over craft categories, heritage villages, districts, festivals and UNESCO records |
| Marketplace Admin | `/products/pending-approval`, `/products/{id}/approval`, `/payments`, `/governance/monitoring` | Product listing-approval workflow (new products start `Pending`; only `Approved` products appear in storefront queries); a platform-wide payments/refund queue (previously only per-order lookup existed); Fraud Control and Marketplace Monitoring both reuse `/governance/monitoring`'s existing flag engine |
| CMS | `/cms/homepage`, `/cms/blogs`, `/cms/news`, `/cms/events`, `/cms/announcements` | Homepage content sections, blog posts, news items, CMS events and site-wide announcements — public reads, `SuperAdmin`-only writes, slug-based routing and a Draft/Published workflow for Blog/News/Events |
| AI Moderation | `/governance/monitoring` (new `ScanType`s) | Fake Reviews (existing `ReviewAbuse` scan), Spam Detection (duplicate messages/review comments), Content Moderation (banned-term matching over Blog/News/Review content), Image Moderation (a review photo reused by several different reviewers) — all new rule-based candidate finders on the shared monitoring engine, no new entities |
| Security | `/admin/security/audit-logs`, `/admin/security/backups`, `/admin/security/system-health`, `/admin/security/api-keys`, `/admin/security/threats` | Audit Logs (system-written, not client-POSTed); Backups (triggers a real `pg_dump`, records success/failure honestly); System Monitoring (live DB connectivity + row counts + process uptime — distinct from the Governance `MonitoringController`); API Management (issue/list/revoke API keys, SHA-256 hashed, raw key shown once — issuance/management only, not yet wired into request auth); Threat Detection (every login attempt recorded, blocked IPs rejected before credential check, computed "suspicious IPs", block/unblock) |
| Cross-Platform AI | `/ai/heritage-assistant`, `/ai/counterfeit-detection`, `/ai/story-generator`, `/ai/sentiment` | AI Heritage Assistant (keyword Q&A over districts/festivals/UNESCO data, public); AI Counterfeit Detection (risk score from verification status + price-vs-category-average anomaly, public); AI Story Generator (template-based craft story drafting, authenticated); AI Sentiment Analysis (lexicon-based scoring over text or a product's reviews, public). Voice Search and Smart Search were deliberately not duplicated — voice-to-text belongs client-side ahead of the existing `/search`, and `ISearchProvider` already documents itself as the swap-in point for a future embedding-based upgrade. `HeritageRiskController` and `KnowledgeGraphController` reads were also opened to everyone as part of this effort (writes stay steward-role-gated) |



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
