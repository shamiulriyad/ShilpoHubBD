# ShilpoHubBD Unit Test Plan

**Status:** plan only. No test code has been written yet.
**Built from:** the source code at commit `b718e737` on 2026-09-29. Every unit listed in section 12 onwards was found by scanning the code, not typed by hand.
**Why this file is called README.md:** the repo's `.gitignore` ignores every `.md` file except `README.md`, so any other name would not be committed.

## Contents

1. [Goal and scope](#1-goal-and-scope)
2. [Ground rules](#2-ground-rules)
3. [Where the tests live](#3-where-the-tests-live)
4. [Tools and one-time setup](#4-tools-and-one-time-setup)
5. [Naming conventions](#5-naming-conventions)
6. [What each test file must check](#6-what-each-test-file-must-check)
7. [Known testing constraints](#7-known-testing-constraints)
8. [Order of work](#8-order-of-work)
9. [Existing tests (kept unchanged)](#9-existing-tests-kept-unchanged)
10. [Not covered by this plan](#10-not-covered-by-this-plan)
11. [Tracking progress](#11-tracking-progress)
12. [Backend features and units](#12-backend-features-and-units)
13. [Frontend features and units](#13-frontend-features-and-units)
14. [AI chat service features and units](#14-ai-chat-service-features-and-units)

---

## 1. Goal and scope

Write **one unit test file for every unit** in the program. Put the test files in **one folder per feature**, so the tests for any feature are easy to find.

A **unit** is:

- **Backend (ASP.NET Core):** one class, for example a service, validator, provider, repository, controller or SignalR hub. A source file that declares several classes gives several units. For example, `ProductSearchControllers.cs` declares 5 controllers, and `ReturnHandlingValidators.cs` declares 14 validators.
- **Frontend (React):** one module, meaning a page, component, hook, API service file, store, util, route guard or layout.
- **AI chat service (Python, `rag/`):** one module (`.py` file).

### Size of the work

| Codebase | Feature folders | Units | Already partly tested | **New test files** |
|---|---|---|---|---|
| Backend (ASP.NET Core) | 73 | 1011 | 16 | **1011** |
| Frontend (React) | 26 | 611 | 4 | **611** |
| AI chat service (Python) | 6 | 44 | 15 | **44** |
| **Total** | **105** | **1666** | **35** | **1666** |

"Already partly tested" means an existing test touches the unit (see [section 9](#9-existing-tests-kept-unchanged)). These units still get their own file in the new structure, but they come last in the [order of work](#8-order-of-work).

### Units by type

| Codebase | Unit type | Count |
|---|---|---|
| Backend | Service | 163 |
| Backend | Provider/Handler | 3 |
| Backend | Validator | 464 |
| Backend | Infrastructure | 40 |
| Backend | Repository | 144 |
| Backend | Data component | 2 |
| Backend | Seeder | 6 |
| Backend | DbContext | 1 |
| Backend | DbContext (notifications) | 1 |
| Backend | Controller | 172 |
| Backend | SignalR hub | 3 |
| Backend | Realtime notifier | 3 |
| Backend | Background service | 1 |
| Backend | Middleware | 2 |
| Backend | JSON converter | 2 |
| Backend | Helper | 1 |
| Backend | DI registration | 3 |
| Frontend | Page | 215 |
| Frontend | Page config | 1 |
| Frontend | Component | 103 |
| Frontend | Hook | 134 |
| Frontend | API service | 130 |
| Frontend | Store | 3 |
| Frontend | Context | 1 |
| Frontend | Layout | 3 |
| Frontend | Route | 4 |
| Frontend | Util | 11 |
| Frontend | Lib | 2 |
| Frontend | Config | 2 |
| Frontend | Navigation data | 1 |
| Frontend | App root | 1 |
| AI chat service | Module | 44 |

> **Why this is more than the earlier estimate of ~1,200 files:** that estimate grouped validators by folder (≈65 files). Under the one-file-per-unit rule each validator gets its own file (**464**). Files that declare several classes are also counted per class, and some small units (seeders, SignalR notifiers, JSON converters, DI registration) are now counted. End-to-end journey tests are not unit tests, so they are left out (see [section 10](#10-not-covered-by-this-plan)).

---

## 2. Ground rules

1. **Do not change any existing file.** This includes:
   - everything in `backend/src/`, `frontend/src/` and `rag/` outside the new test folders;
   - `backend/ShilpoHubBD.sln`;
   - `frontend/package.json` and `frontend/package-lock.json`;
   - `rag/requirements.txt`;
   - `.gitignore`;
   - the existing tests: `backend/tests/*Regression/`, `rag/tests/*.py`, and `frontend/scripts/*`.

   The testing work only **adds** new files in the new folders from [section 3](#3-where-the-tests-live).
2. **One unit, one test file.** Never put two units' tests in one file. Never split one unit across files.
3. **Test through the public surface.** Do not use `InternalsVisibleTo`, reflection to call private methods, or edits that make a class easier to test. Every unit in this plan is a public class. The few `internal` classes in the code (such as `*Mappings`, `*Access` and small helpers inside service, provider and repository files) are tested through the public unit that uses them.
4. **If a unit cannot be tested without changing source code, do not change the code.** Record it in the **Blocked units** table ([section 11](#11-tracking-progress)) with the reason, and move on.
5. **No real external calls.** Gemini, Nominatim, OSRM, Overpass, the Python RAG and product-search services, e-mail and payment gateways are always faked.
6. **Never use the development database.** Tests that need Postgres use a separate test database (see [section 4](#4-tools-and-one-time-setup)), never `shilpohub`.
7. **Tests are independent.** Each test builds its own data, can run alone or in any order, and cleans up any files or rows it creates.

---

## 3. Where the tests live

All new tests go in three new roots, one per codebase. Inside each root there is **one folder per feature**, and inside that one sub-folder per unit type.

### Backend: `backend/tests/ShilpoHubBD.UnitTests/`

```text
backend/tests/
├── ShilpoHubBD.UnitTests/                  ← NEW xUnit project (sits next to the existing *Regression folders)
│   ├── ShilpoHubBD.UnitTests.csproj
│   ├── Common/                             ← shared helpers only, no tests: fakes, test data builders,
│   │                                         ClaimsPrincipal builder, stub HttpMessageHandler, test DB fixture
│   └── Features/
│       ├── Commerce/                       ← one folder per feature (section 12)
│       │   ├── Services/OrderServiceTests.cs
│       │   ├── Validators/…Tests.cs
│       │   ├── Infrastructure/CashOnDeliveryPaymentProviderTests.cs
│       │   ├── Repositories/OrderRepositoryTests.cs
│       │   └── Controllers/OrdersControllerTests.cs
│       ├── Auth/
│       ├── …
│       └── Platform/                       ← cross-cutting: middleware, JSON converters, DI, DbContext
├── ProductSearchRegression/                ← existing, untouched
└── …
```

Sub-folders used inside a feature: `Services`, `Validators`, `Infrastructure`, `Repositories`, `Controllers`, `Hubs`, `Realtime`, `BackgroundServices`, `Seed`, `Data`, plus `Middlewares`, `Json`, `Common` and `DependencyInjection` in `Platform`. The namespace follows the folder, for example `ShilpoHubBD.UnitTests.Features.Commerce.Services`.

### Frontend: `frontend/tests/`

```text
frontend/tests/                             ← NEW, a separate npm package (frontend/package.json is not touched)
├── package.json                            ← test-only devDependencies
├── vitest.config.js
├── setup.js                                ← jest-dom matchers, cleanup, Leaflet mock
├── common/                                 ← shared helpers only: renderWithProviders, fake apiClient, fake auth store
└── features/
    ├── Customer/                           ← one folder per feature (section 13)
    │   ├── pages/CustomerDashboard.test.jsx
    │   ├── components/…test.jsx
    │   ├── hooks/useOrders.test.js
    │   └── services/…test.js
    ├── AppShell/                           ← routes, layouts, stores, utils, lib, config, apiClient
    ├── SharedUI/                           ← reusable UI in components/ui, cards, forms, media, brand, shared
    └── …
```

### AI chat service: `rag/tests/unit/`

```text
rag/tests/
├── unit/                                   ← NEW
│   ├── __init__.py
│   ├── common/                             ← shared fakes: Gemini, embeddings, backend HTTP
│   ├── api/test_chat.py                    ← one folder per feature (section 14), each with __init__.py
│   ├── heritage_rag/
│   ├── travel_planner/
│   ├── product_search/
│   ├── ingestion/
│   └── service/
├── test_json_pipeline.py                   ← existing, untouched
└── …
```

---

## 4. Tools and one-time setup

### Backend

| Need | Tool |
|---|---|
| Test framework | xUnit v3 (3.2.2) |
| Fakes | NSubstitute (5.3.0) |
| Validator tests | `FluentValidation.TestHelper` (ships inside the FluentValidation package the app already uses) |
| Postgres for repository tests | Any Postgres 16 server; the tests create and drop their own throwaway database on it (see below) |
| Coverage | coverlet.collector (6.0.4) |

- `ShilpoHubBD.UnitTests.csproj` targets `net8.0` and uses `ProjectReference` to the five projects in `backend/src`.
- It is **not** added to `backend/ShilpoHubBD.sln`, because that would change an existing file. Run it directly:

  ```sh
  dotnet test backend/tests/ShilpoHubBD.UnitTests                                  # everything
  dotnet test backend/tests/ShilpoHubBD.UnitTests --filter "Needs!=Database"       # no Postgres needed
  dotnet test backend/tests/ShilpoHubBD.UnitTests --filter "Feature=Commerce"      # one feature
  ```

- **Test database** (`Common/Database/TestDatabaseFixture.cs`):
  - Set `SHILPOHUB_TEST_DB` to a connection string for a Postgres **server**, for example `Host=127.0.0.1;Port=5432;Username=postgres;Password=…;Database=postgres`. The database it names is only used to run `CREATE DATABASE` and `DROP DATABASE`; nothing is ever written to it.
  - At the start of the run the fixture creates a new database named `shilpohub_unit_<random>` and builds its schema from the real EF migrations (`Database.MigrateAsync()`). At the end of the run it drops that database again, so no test data is left behind.
  - Each test runs inside a transaction that is rolled back when the test ends (`DatabaseScope`), so tests never see each other's rows.
  - Without `SHILPOHUB_TEST_DB`, database tests are **skipped** with a message instead of failing.
- **The build treats warnings as errors.** `backend/Directory.Build.props` sets `TreatWarningsAsErrors` and applies to this project too, so test code must compile with zero warnings. Pass `TestContext.Current.CancellationToken` to async calls to keep the xUnit analyzers quiet.
- **Shared helpers** in `Common/`:
  - `TestUsers`: builds users and roles.
  - `ControllerTestExtensions` / `AccessRules`: sign in a fake user, and read `[Authorize]` rules and routes.
  - `TestConfiguration`: in-memory `IConfiguration`.
  - `CapturingLogger<T>`: records log entries.

### Frontend

- `frontend/tests/package.json` holds only test devDependencies: `vitest`, `jsdom`, `@testing-library/react`, `@testing-library/user-event`, `@testing-library/jest-dom`, `@vitejs/plugin-react` and `@vitest/coverage-v8`.
- `vitest.config.js` uses the React plugin, `environment: 'jsdom'` and `include: ['features/**/*.test.{js,jsx}']`.
  - It **aliases** `react`, `react-dom`, `react-router-dom`, `@tanstack/react-query`, `zustand`, `axios` and `leaflet` to `../node_modules/...`, so the app and the tests share one copy of React. Two copies break hooks.
  - It sets `VITE_API_BASE_URL` to a dummy value.
- Run it:

  ```sh
  cd frontend && npm install              # app dependencies (as today)
  cd tests && npm install && npx vitest run
  npx vitest run features/Customer        # one feature
  ```

### AI chat service

- Python's built-in `unittest` (the style the existing tests use), plus `unittest.mock`. API endpoint tests use FastAPI's `TestClient` (FastAPI is already a dependency).
- Run only the new folder:

  ```sh
  cd rag && python -m unittest discover -s tests/unit -t .
  ```

  Do **not** run discovery on the whole `tests/` folder: the existing `test_bge.py` downloads the BGE-M3 model when it is imported.
- For coverage, `pip install coverage` locally. It is not added to `requirements.txt`.

---

## 5. Naming conventions

| | Backend | Frontend | AI chat service |
|---|---|---|---|
| File | `<ClassName>Tests.cs` | `<moduleName>.test.jsx` / `.test.js` | `test_<module>.py` |
| Class / suite | `public class <ClassName>Tests` | `describe('<moduleName>')` | `class Test<Thing>(unittest.TestCase)` |
| Test name | `Method_Scenario_Expected`<br>e.g. `PlaceOrder_CartIsEmpty_ThrowsConflictException` | `it('shows the empty state when there are no orders')` | `test_returns_refusal_for_out_of_scope_question` |
| Tags | `[Trait("Feature","Commerce")]`, `[Trait("Layer","Service")]`, `[Trait("Needs","Database")]` for DB tests | folder path | folder path |

---

## 6. What each test file must check

Use the checklist for the unit's type. Every public method, exported function or user-visible behaviour needs at least one success case and one failure case.

### Backend

**Service** (`Application/Services`)
- [ ] Build the service with NSubstitute fakes for every constructor dependency. No database.
- [ ] Each public method, success path: returns the right DTO and values.
- [ ] Missing records throw `NotFoundException`. Conflicting state (such as a duplicate, or an item already sold or closed) throws `ConflictException`. An unavailable AI dependency throws `AiServiceUnavailableException` or falls back, whichever the service does.
- [ ] Ownership and role rules: acting on another user's record throws `UnauthorizedAccessException` (for example, a producer editing someone else's product).
- [ ] Status changes: every allowed transition works and every forbidden one is refused.
- [ ] Side effects: the right repository add/update/save calls happen with the right values, and **nothing is saved when the method fails**. Notifiers and providers are called only when they should be.
- [ ] Calculations (totals, prices, scores, forecasts): normal values plus limits (0, negative, empty list, very large numbers).

**Validator** (`Application/Validators`)
- [ ] Use `TestValidate`. A fully valid request has no errors.
- [ ] Each rule has one failing case that checks the property name (`ShouldHaveValidationErrorFor`).
- [ ] Limits: a value exactly at the maximum length or range passes; one past it fails. Empty and whitespace strings are covered.
- [ ] Conditional rules (`When` / `Unless`): both branches.

**Infrastructure provider** (`Infrastructure/*`)
- [ ] HTTP-based providers (Gemini, Nominatim, OSRM, Overpass, the Python RAG and product-search services) take `HttpClient` in their constructor. Give them one with a **stub `HttpMessageHandler`**, and check the request URL, method, headers and body.
- [ ] Map a successful response correctly. Handle a non-2xx status, a timeout and malformed JSON.
- [ ] Missing configuration (empty API key or URL) gives the fallback or a clear error. The Gemini providers take their Dummy or RuleBased fallback in the constructor, so check that it is used.
- [ ] Rule-based and dummy providers give stable output for fixed input. Check edge input such as empty lists or unknown districts.
- [ ] `JwtTokenService`: the token carries `sub`, role claims and the expiry from `JwtSettings`, and validates with the signing key. `BCryptPasswordHasher`: the hash differs from the password and verifies correctly both ways.

**Repository** (`Data/Repositories`), tag `Needs=Database`
- [ ] Run against the migrated test database. Each test starts clean, either with a transaction rolled back at the end or a per-class reset.
- [ ] Queries return only matching rows: filters, text search (`ILike`), status, ownership (`ProducerId` / `UserId`), and active or deleted flags.
- [ ] Paging (page size, total count), ordering, and loading of related data (`Include`).
- [ ] Add, update and delete persist as expected.

**Controller** (`Api/Controllers`)
- [ ] Build the controller with NSubstitute service fakes. Set `ControllerContext.HttpContext.User` to a `ClaimsPrincipal` with `NameIdentifier`/`sub` and role claims.
- [ ] Each action returns the right result type and status code (200, 201, 204, 400, 403, 404) for the service's result.
- [ ] The current user's ID, route values, query values and body reach the service unchanged.
- [ ] **Access rules:** read `[Authorize]`, `[Authorize(Roles = …)]` and `[AllowAnonymous]` on the class and on each action through reflection, and check them against the expected roles. This locks the permission design, so any change to it is deliberate.
- [ ] Exceptions from the service **propagate**. Mapping them to HTTP responses is `GlobalExceptionHandler`'s job and is tested there.

**SignalR hub** (`Api/Hubs`): fake `HubCallerContext` (user), `IGroupManager` and `IHubCallerClients`. Check joining and leaving the right group names, broadcasting to the right group, and refusing users who are not allowed.

**Realtime notifier** (`Api/Realtime`): fake `IHubContext<THub>`. Check the client method name, target group or user, and payload.

**Background service** (`Api/BackgroundServices`): fake `IServiceScopeFactory`. Run one cycle and cancel it. Check that reports are generated once per period, and that an error in one cycle is logged without stopping the service.

**Seeder** (`Data/Seed`), tag `Needs=Database`: seeds the expected rows, and running it twice creates no duplicates.

**Platform units**
- [ ] `GlobalExceptionHandler` maps:
  - FluentValidation `ValidationException` → 400 with the field errors;
  - `NotFoundException` → 404;
  - `ConflictException` → 409;
  - `UnauthorizedAccessException` → 401;
  - `AiServiceUnavailableException` → 503;
  - anything else → 500 with the fixed text "An unexpected error occurred." (the real message must not leak). `DbUpdateConcurrencyException` details are logged.
- [ ] `ValidationFilter`: an invalid request gives 400 with field errors; a valid one passes through.
- [ ] JSON converters: read and write UTC, handle the date kind correctly, and handle `null`.
- [ ] DI registration: build a `ServiceCollection` with the layer's `Add…` method and test configuration, and check that every registered service resolves. This catches missing registrations.
- [ ] `ShilpoHubDbContext`: the EF model builds. `ShilpoHubDbContext.Notifications`: saving tracked changes creates the expected notification rows.

### Frontend

**Page**
- [ ] Render inside `MemoryRouter` and `QueryClientProvider` (retries off), with the page's hooks mocked through `vi.mock`.
- [ ] Loading, empty, error (with retry) and data states.
- [ ] Main actions: buttons and forms call the right mutation with the right payload. Validation messages show.
- [ ] Role-dependent parts (from the auth store) show or hide correctly.
- [ ] Links point at the right `routePaths` entries.

**Component**
- [ ] Renders correctly from its props, including optional and missing props.
- [ ] User interactions (`user-event`) call the right callbacks.
- [ ] Conditional parts render only when they should. Key elements are reachable by role or label.

**Hook**
- [ ] `renderHook` with a fresh `QueryClient`. Calls the right service function with the right arguments.
- [ ] Query key, and `enabled` conditions (for example, no request until an ID exists).
- [ ] Mutations invalidate the right queries. Errors reach the caller.

**API service**
- [ ] Mock `apiClient`. Check the HTTP method, URL, query parameters and body.
- [ ] Response unwrapping and mapping. Errors propagate in the shape `utils/apiError` expects.

**Store** (Zustand): initial state, each action, saving to and restoring from `localStorage` (the auth store uses `persist`), and logout clearing the state.

**Util, lib and config**: pure-function tests covering normal, edge and invalid input. For example: every role in `roles.js` (`resolveActiveRole`, `roleHomePath`), token decoding and expiry in `jwt.js`, and each adapter's mapping.

**Route and layout**
- [ ] `ProtectedRoute` sends signed-out users to login.
- [ ] `RoleBasedRoute` sends the wrong role to the unauthorized page.
- [ ] The router maps each role's paths to the expected page.
- [ ] `DashboardLayout` shows the sidebar for the active role.

### AI chat service

**Module**
- [ ] `unittest`, with fakes for Gemini, embeddings and backend HTTP. Never load the real BGE-M3 model.
- [ ] Use `QdrantClient(":memory:")` for vector-store code, as the existing tests do.
- [ ] Pure functions: normal, edge and invalid input.
- [ ] API endpoints (`FastAPI` `TestClient`): status codes, response shape, validation errors, and behaviour when the index or collection is missing.
- [ ] Refusal behaviour: out-of-scope questions give "not available" answers, with no invented content.

---

## 7. Known testing constraints

These were found in the code while building this plan. They change **how** some units are tested, not whether they are.

| Constraint | Affected units | How to handle it without changing code |
|---|---|---|
| `DateTime.UtcNow` is called directly in about 160 Application files (no `TimeProvider`) | most services | Do not assert exact timestamps. Capture the time before and after the call and assert the value falls between them. |
| Starts an external `pg_dump` process | `PgDumpBackupRunner` | Point `Backup:PgDumpPath` at a small stand-in shell script that records its arguments and environment. This covers the success path, exit-code failures and connection-string parsing without a real `pg_dump`. The stand-in is a POSIX script, so those tests are skipped on Windows. |
| Writes files to disk (uploads, backups, previews) | `MediaController`, `ChatMediaController`, `ProfileController`, `ArtisanSupportController`, `BackupService`, `GeminiInteriorPreviewProvider` | Point the storage path (options or a fake `IWebHostEnvironment`) at a temporary folder, and delete it after the test. |
| Queries `ShilpoHubDbContext` directly with Postgres `ILike` | `NotificationsController`, `UserLookupController` | Tag `Needs=Database` and use the test database. The EF in-memory provider does not support `ILike`. |
| Map pages use Leaflet, which needs a real browser | Tourism and heritage map pages | Mock `leaflet` in `setup.js`, and test the page's data and controls, not the map drawing. |
| The existing `test_bge.py` downloads a model on import | AI chat service test runs | Run discovery on `tests/unit` only. |

---

## 8. Order of work

Riskiest code comes first: access control, then money, then role workspaces. Inside each phase, do backend services and controllers first, then repositories, then the frontend.

| Phase | Theme | Backend features | Frontend features | AI chat service features | New test files |
|---|---|---|---|---|---|
| 1 | Access control and platform | [Admin](#be-admin), [Auth](#be-auth), [Notifications](#be-notifications), [Platform](#be-platform), [Profiles](#be-profiles), [Security](#be-security) | [Admin](#fe-admin), [AppShell](#fe-appshell), [Auth](#fe-auth), [Notifications](#fe-notifications) | · | 151 |
| 2 | Shopping and money | [Auction](#be-auction), [Certificate](#be-certificate), [Certificates](#be-certificates), [Commerce](#be-commerce), [Complaints](#be-complaints), [CustomOrders](#be-customorders), [Inventory](#be-inventory), [LiveShopping](#be-liveshopping), [Marketplace](#be-marketplace), [ProducerBusiness](#be-producerbusiness), [ProductSearch](#be-productsearch), [QRVerification](#be-qrverification), [Reviews](#be-reviews), [Search](#be-search), [Traceability](#be-traceability) | [Customer](#fe-customer), [Marketplace](#fe-marketplace), [Producer](#fe-producer), [SharedData](#fe-shareddata), [SharedUI](#fe-sharedui) | [API endpoints](#ai-api-endpoints), [Product search](#ai-product-search) | 405 |
| 3 | Business partners and logistics | [AIBusiness](#be-aibusiness), [AIBusinessPartner](#be-aibusinesspartner), [BusinessPartner](#be-businesspartner), [BusinessPartnerAnalytics](#be-businesspartneranalytics), [Contracts](#be-contracts), [CSRSponsorship](#be-csrsponsorship), [DesignCollaboration](#be-designcollaboration), [Investment](#be-investment), [Logistics](#be-logistics), [ManufacturingPartnership](#be-manufacturingpartnership), [Procurement](#be-procurement), [ProducerComparison](#be-producercomparison), [ProducerPartnership](#be-producerpartnership), [ProductDevelopment](#be-productdevelopment), [ProductIntelligence](#be-productintelligence), [Quotations](#be-quotations), [SupplierDiscovery](#be-supplierdiscovery), [SupplierMatching](#be-suppliermatching) | [BusinessPartner](#fe-businesspartner), [LogisticsPartner](#fe-logisticspartner) | · | 307 |
| 4 | Learning, tourism, government and research | [Achievement](#be-achievement), [AITourism](#be-aitourism), [Analytics](#be-analytics), [Apprenticeship](#be-apprenticeship), [Employment](#be-employment), [FieldResearch](#be-fieldresearch), [Governance](#be-governance), [HeritageDatabase](#be-heritagedatabase), [Innovation](#be-innovation), [KnowledgeGraph](#be-knowledgegraph), [Learning](#be-learning), [MentorMatching](#be-mentormatching), [Mentorship](#be-mentorship), [Passport](#be-passport), [Portfolio](#be-portfolio), [Research](#be-research), [Roadmap](#be-roadmap), [SkillAssessment](#be-skillassessment), [Tourism](#be-tourism), [TouristBooking](#be-touristbooking) | [Academy](#fe-academy), [ApprenticeStudent](#fe-apprenticestudent), [Dashboard](#fe-dashboard), [Government](#fe-government), [NGO](#fe-ngo), [Research](#fe-research), [Researcher](#fe-researcher), [Tourism](#fe-tourism), [Tourist](#fe-tourist), [TrainerMasterArtisan](#fe-trainermasterartisan) | [Heritage Q&A pipeline](#ai-heritage-q-a-pipeline), [Travel planner](#ai-travel-planner) | 572 |
| 5 | Heritage content, community and AI helpers | [AIShopping](#be-aishopping), [ArVr](#be-arvr), [Cms](#be-cms), [Community](#be-community), [CounterfeitDetection](#be-counterfeitdetection), [HeritageAssistant](#be-heritageassistant), [HeritageDiscovery](#be-heritagediscovery), [HeritageIdentity](#be-heritageidentity), [Impact](#be-impact), [Messaging](#be-messaging), [Recommendation](#be-recommendation), [SentimentAnalysis](#be-sentimentanalysis), [StoryGenerator](#be-storygenerator), [Sustainability](#be-sustainability) | [About](#fe-about), [Explore](#fe-explore), [Home](#fe-home), [Messaging](#fe-messaging), [News](#fe-news) | [Ingestion scripts](#ai-ingestion-scripts), [Service entry points & config](#ai-service-entry-points-config) | 196 |
| 6 | Units that already have some tests | the units marked *partly tested* in any feature | same | same | 35 |
| | **Total** | | | | **1666** |

**Phase 0 (before everything):** create the three test roots, the shared helpers in each `Common/` / `common/` folder, and the test database fixture. Then add one passing test file to prove the setup works.

---

## 9. Existing tests (kept unchanged)

These already exist and are **not** moved, edited or deleted. Units they touch are marked *(partly tested)* in the lists below, and their new test files are scheduled in Phase 6.

| Existing test | Kind | What it checks |
|---|---|---|
| `backend/tests/*Regression/` (13 console programs, run with `dotnet run --project …`) | PASS/FAIL console apps, no framework | Producer partnerships (agreements, auctions, settlements, end-to-end), product search, product intelligence, tourism budget and route planning, fashion matching, interior preview, translation, notifications, product image replacement |
| `rag/tests/test_json_pipeline.py`, `test_product_search.py`, `test_product_sync.py`, `test_product_suggest.py` | `unittest` | Heritage Q&A pipeline steps 1–4, 6, 7, 9–11; product search analysis, retrieval, store, sync, suggestions |
| `rag/tests/test_bge.py` | script | Loads the BGE-M3 model (a smoke check, not a test) |
| `rag/eval/run_eval.py` | evaluation tool | Answer quality against `questions.json` (calls Gemini) |
| `frontend/scripts/test-admin-contracts.mjs`, `test-launch-contracts.mjs`, `test-design-navigation.mjs`, `validate-source.mjs` | Node scripts | Admin config, sidebar logic, source text checks, syntax check |
| `frontend/scripts/*-fixture/` | manual browser pages | Pages shown with fake data for manual checking |

---

## 10. Not covered by this plan

| Not included | Why |
|---|---|
| DTOs, domain entities, domain constants, `Options` classes | Plain data with no logic. They are checked through the services and validators that use them. |
| `*Mappings.cs`, `*Access.cs` and abstract `*Base` classes in `Application/Services` | Internal static helpers and base classes, tested through the services that use them. |
| Helper classes inside repository files (for example `ParameterReplacer`) | Tested through their repository. |
| EF migrations and entity configurations | Applied to the test database, so every repository test exercises them. |
| `main.jsx`, styles, images, static data files (`craftHeritage.js`, `heritageDemoQuestions.js`) | Bootstrap code or static content with no logic. |
| `rag/eval/`, frontend fixtures, `run-all.ps1` | Tools, not program code. |
| End-to-end journey tests (Playwright or Selenium, about 12 journeys) | Not unit tests. Plan them separately once the unit tests exist. |

---

## 11. Tracking progress

- In the lists below, change **☐** to **✅** when a unit's test file is merged and passing.
- A unit is **done** when every item in its checklist from [section 6](#6-what-each-test-file-must-check) is covered, the file follows [section 5](#5-naming-conventions), and the test passes on a clean checkout.
- A feature is **done** when all its units are ✅ (or listed below as Blocked).

### Completed features

| Feature | Codebase | Units | Test cases | Test folder | Completed |
|---|---|---|---|---|---|
| [Auth](#be-auth) | Backend | 20 / 20 | 224 | `backend/tests/ShilpoHubBD.UnitTests/Features/Auth/` | 2026-09-29 |
| [Security](#be-security) | Backend | 18 / 18 | 169 | `backend/tests/ShilpoHubBD.UnitTests/Features/Security/` | 2026-09-29 |
| [Admin](#be-admin) | Backend | 13 / 13 | 162 | `backend/tests/ShilpoHubBD.UnitTests/Features/Admin/` | 2026-09-29 |
| [Notifications](#be-notifications) | Backend | 2 / 2 | 27 | `backend/tests/ShilpoHubBD.UnitTests/Features/Notifications/` | 2026-09-29 |
| [Platform](#be-platform) | Backend | 10 / 10 | 64 | `backend/tests/ShilpoHubBD.UnitTests/Features/Platform/` | 2026-09-29 |
| [Profiles](#be-profiles) | Backend | 4 / 4 | 97 | `backend/tests/ShilpoHubBD.UnitTests/Features/Profiles/` | 2026-09-29 |
| [Certificates](#be-certificates) | Backend | 3 / 3 | 47 | `backend/tests/ShilpoHubBD.UnitTests/Features/Certificates/` | 2026-09-29 |
| [Search](#be-search) | Backend | 3 / 3 | 21 | `backend/tests/ShilpoHubBD.UnitTests/Features/Search/` | 2026-09-29 |
| [Inventory](#be-inventory) | Backend | 4 / 4 | 40 | `backend/tests/ShilpoHubBD.UnitTests/Features/Inventory/` | 2026-09-29 |
| [Certificate](#be-certificate) | Backend | 5 / 5 | 57 | `backend/tests/ShilpoHubBD.UnitTests/Features/Certificate/` | 2026-09-29 |
| [Auction](#be-auction) | Backend | 6 / 6 | 92 | `backend/tests/ShilpoHubBD.UnitTests/Features/Auction/` | 2026-09-29 |
| [Reviews](#be-reviews) | Backend | 6 / 6 | 95 | `backend/tests/ShilpoHubBD.UnitTests/Features/Reviews/` | 2026-09-29 |
| [Complaints](#be-complaints) | Backend | 6 / 6 | 70 | `backend/tests/ShilpoHubBD.UnitTests/Features/Complaints/` | 2026-09-29 |
| [Traceability](#be-traceability) | Backend | 7 / 7 | 63 | `backend/tests/ShilpoHubBD.UnitTests/Features/Traceability/` | 2026-09-29 |
| [Achievement](#be-achievement) | Backend | 5 / 5 | 61 | `backend/tests/ShilpoHubBD.UnitTests/Features/Achievement/` | 2026-09-30 |
| [Impact](#be-impact) | Backend | 3 / 3 | 17 | `backend/tests/ShilpoHubBD.UnitTests/Features/Impact/` | 2026-09-30 |
| [QRVerification](#be-qrverification) | Backend | 6 / 6 | 58 | `backend/tests/ShilpoHubBD.UnitTests/Features/QRVerification/` | 2026-09-30 |

### Blocked units

Record units here that cannot be tested without changing source code. Don't change the code; write down why.

| Unit | Feature | Reason | Date |
|---|---|---|---|
| | | | |

---

## 12. Backend features and units

Root folder: `backend/tests/ShilpoHubBD.UnitTests/Features/`. Source paths are relative to `backend/src/ShilpoHubBD.`, and test paths are relative to the feature folder.

| Feature | Units | Services | Validators | Infra | Repos | Controllers | Other | Phase |
|---|---|---|---|---|---|---|---|---|
| [Achievement](#be-achievement) | 5 | 1 | 2 | · | 1 | 1 | · | 4 |
| [Admin](#be-admin) | 13 | 3 | 4 | · | 3 | 3 | · | 1 |
| [AIBusiness](#be-aibusiness) | 9 | 1 | 6 | 1 | · | 1 | · | 3 |
| [AIBusinessPartner](#be-aibusinesspartner) | 9 | 1 | 5 | 1 | 1 | 1 | · | 3 |
| [AIShopping](#be-aishopping) | 9 | 2 | 3 | 3 | · | 1 | · | 5 |
| [AITourism](#be-aitourism) | 17 | 2 | 5 | 6 | 2 | 1 | 1 | 4 |
| [Analytics](#be-analytics) | 6 | 2 | · | · | 2 | 2 | · | 4 |
| [Apprenticeship](#be-apprenticeship) | 16 | 3 | 7 | · | 3 | 3 | · | 4 |
| [ArVr](#be-arvr) | 18 | 4 | 7 | · | 3 | 4 | · | 5 |
| [Auction](#be-auction) | 6 | 1 | 3 | · | 1 | 1 | · | 2 |
| [Auth](#be-auth) | 20 | 2 | 9 | 3 | 4 | 2 | · | 1 |
| [BusinessPartner](#be-businesspartner) | 7 | 1 | 4 | · | 1 | 1 | · | 3 |
| [BusinessPartnerAnalytics](#be-businesspartneranalytics) | 4 | 1 | 1 | · | 1 | 1 | · | 3 |
| [Certificate](#be-certificate) | 5 | 1 | 2 | · | 1 | 1 | · | 2 |
| [Certificates](#be-certificates) | 3 | 1 | · | · | 1 | 1 | · | 2 |
| [Cms](#be-cms) | 30 | 6 | 11 | · | 6 | 6 | 1 | 5 |
| [Commerce](#be-commerce) | 26 | 4 | 13 | 1 | 4 | 4 | · | 2 |
| [Community](#be-community) | 19 | 4 | 7 | · | 4 | 4 | · | 5 |
| [Complaints](#be-complaints) | 6 | 1 | 3 | · | 1 | 1 | · | 2 |
| [Contracts](#be-contracts) | 11 | 1 | 8 | · | 1 | 1 | · | 3 |
| [CounterfeitDetection](#be-counterfeitdetection) | 3 | 1 | · | 1 | · | 1 | · | 5 |
| [CSRSponsorship](#be-csrsponsorship) | 12 | 1 | 9 | · | 1 | 1 | · | 3 |
| [CustomOrders](#be-customorders) | 8 | 2 | 4 | · | 1 | 1 | · | 2 |
| [DesignCollaboration](#be-designcollaboration) | 10 | 1 | 7 | · | 1 | 1 | · | 3 |
| [Employment](#be-employment) | 14 | 3 | 6 | · | 2 | 3 | · | 4 |
| [FieldResearch](#be-fieldresearch) | 19 | 3 | 12 | · | 1 | 3 | · | 4 |
| [Governance](#be-governance) | 69 | 11 | 32 | 5 | 9 | 12 | · | 4 |
| [HeritageAssistant](#be-heritageassistant) | 5 | 1 | 1 | 2 | · | 1 | · | 5 |
| [HeritageDatabase](#be-heritagedatabase) | 16 | 3 | 7 | · | 3 | 3 | · | 4 |
| [HeritageDiscovery](#be-heritagediscovery) | 37 | 7 | 15 | · | 7 | 7 | 1 | 5 |
| [HeritageIdentity](#be-heritageidentity) | 10 | 1 | 7 | · | 1 | 1 | · | 5 |
| [Impact](#be-impact) | 3 | 1 | · | · | 1 | 1 | · | 5 |
| [Innovation](#be-innovation) | 36 | 4 | 24 | · | 4 | 4 | · | 4 |
| [Inventory](#be-inventory) | 4 | 1 | 1 | · | 1 | 1 | · | 2 |
| [Investment](#be-investment) | 11 | 1 | 8 | · | 1 | 1 | · | 3 |
| [KnowledgeGraph](#be-knowledgegraph) | 8 | 1 | 5 | · | 1 | 1 | · | 4 |
| [Learning](#be-learning) | 74 | 11 | 39 | · | 11 | 11 | 2 | 4 |
| [LiveShopping](#be-liveshopping) | 10 | 1 | 5 | · | 1 | 1 | 2 | 2 |
| [Logistics](#be-logistics) | 88 | 8 | 59 | 4 | 8 | 9 | · | 3 |
| [ManufacturingPartnership](#be-manufacturingpartnership) | 8 | 1 | 5 | · | 1 | 1 | · | 3 |
| [Marketplace](#be-marketplace) | 36 | 6 | 17 | · | 6 | 6 | 1 | 2 |
| [MentorMatching](#be-mentormatching) | 4 | 1 | 1 | · | 1 | 1 | · | 4 |
| [Mentorship](#be-mentorship) | 5 | 1 | 2 | · | 1 | 1 | · | 4 |
| [Messaging](#be-messaging) | 9 | 1 | 3 | · | 1 | 2 | 2 | 5 |
| [Notifications](#be-notifications) | 2 | · | · | · | · | 1 | 1 | 1 |
| [Passport](#be-passport) | 11 | 1 | 6 | · | 3 | 1 | · | 4 |
| [Platform](#be-platform) | 10 | · | · | · | · | 1 | 9 | 1 |
| [Portfolio](#be-portfolio) | 11 | 2 | 5 | · | 2 | 2 | · | 4 |
| [Procurement](#be-procurement) | 11 | 1 | 6 | · | 1 | 3 | · | 3 |
| [ProducerBusiness](#be-producerbusiness) | 12 | 3 | 2 | · | 2 | 4 | 1 | 2 |
| [ProducerComparison](#be-producercomparison) | 4 | 1 | 1 | · | 1 | 1 | · | 3 |
| [ProducerPartnership](#be-producerpartnership) | 25 | 6 | 8 | · | 5 | 6 | · | 3 |
| [ProductDevelopment](#be-productdevelopment) | 13 | 1 | 10 | · | 1 | 1 | · | 3 |
| [ProductIntelligence](#be-productintelligence) | 4 | 1 | · | 2 | · | 1 | · | 3 |
| [ProductSearch](#be-productsearch) | 22 | 4 | 5 | 2 | 4 | 5 | 2 | 2 |
| [Profiles](#be-profiles) | 4 | 1 | 1 | · | 1 | 1 | · | 1 |
| [QRVerification](#be-qrverification) | 6 | 1 | 3 | · | 1 | 1 | · | 2 |
| [Quotations](#be-quotations) | 9 | 1 | 6 | · | 1 | 1 | · | 3 |
| [Recommendation](#be-recommendation) | 3 | 1 | · | 1 | · | 1 | · | 5 |
| [Research](#be-research) | 37 | 7 | 18 | 1 | 2 | 9 | · | 4 |
| [Reviews](#be-reviews) | 6 | 1 | 3 | · | 1 | 1 | · | 2 |
| [Roadmap](#be-roadmap) | 5 | 2 | 1 | · | 1 | 1 | · | 4 |
| [Search](#be-search) | 3 | 1 | · | · | · | 1 | 1 | 2 |
| [Security](#be-security) | 18 | 5 | 2 | 1 | 5 | 5 | · | 1 |
| [SentimentAnalysis](#be-sentimentanalysis) | 4 | 1 | 1 | 1 | · | 1 | · | 5 |
| [SkillAssessment](#be-skillassessment) | 4 | 2 | · | · | 1 | 1 | · | 4 |
| [StoryGenerator](#be-storygenerator) | 4 | 1 | 1 | 1 | · | 1 | · | 5 |
| [SupplierDiscovery](#be-supplierdiscovery) | 4 | 1 | 1 | · | 1 | 1 | · | 3 |
| [SupplierMatching](#be-suppliermatching) | 4 | 1 | 1 | · | 1 | 1 | · | 3 |
| [Sustainability](#be-sustainability) | 5 | 1 | 2 | · | 1 | 1 | · | 5 |
| [Tourism](#be-tourism) | 11 | 1 | 2 | 4 | 1 | 2 | 1 | 4 |
| [TouristBooking](#be-touristbooking) | 14 | 3 | 6 | · | 3 | 2 | · | 4 |
| [Traceability](#be-traceability) | 7 | 1 | 4 | · | 1 | 1 | · | 2 |
| **Total (73 features)** | **1011** | **166** | **464** | **40** | **144** | **172** | **25** | |

<a id="be-achievement"></a>

### Achievement

**Folder:** `Features/Achievement/` · **Units:** 5 · **Phase:** 4

Contains: 1 service, 2 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `AchievementService` | Service | `Application/Services/Achievement/AchievementService.cs` | `Services/AchievementServiceTests.cs` |  |
| ✅ | `AwardXpRequestValidator` | Validator | `Application/Validators/Achievement/AwardXpRequestValidator.cs` | `Validators/AwardXpRequestValidatorTests.cs` |  |
| ✅ | `CreateAchievementRequestValidator` | Validator | `Application/Validators/Achievement/CreateAchievementRequestValidator.cs` | `Validators/CreateAchievementRequestValidatorTests.cs` |  |
| ✅ | `AchievementRepository` | Repository | `Data/Repositories/AchievementRepository.cs` | `Repositories/AchievementRepositoryTests.cs` | needs the test database |
| ✅ | `AchievementsController` | Controller | `Api/Controllers/AchievementsController.cs` | `Controllers/AchievementsControllerTests.cs` |  |

<a id="be-admin"></a>

### Admin

**Folder:** `Features/Admin/` · **Units:** 13 · **Phase:** 1

Contains: 3 services, 4 validators, 3 repositories, 3 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `AdminUserService` | Service | `Application/Services/Admin/AdminUserService.cs` | `Services/AdminUserServiceTests.cs` |  |
| ✅ | `IdentityVerificationService` | Service | `Application/Services/Admin/IdentityVerificationService.cs` | `Services/IdentityVerificationServiceTests.cs` |  |
| ✅ | `PermissionService` | Service | `Application/Services/Admin/PermissionService.cs` | `Services/PermissionServiceTests.cs` |  |
| ✅ | `CreatePermissionRequestValidator` | Validator | `Application/Validators/Admin/PermissionValidators.cs` | `Validators/CreatePermissionRequestValidatorTests.cs` |  |
| ✅ | `RejectIdentityVerificationRequestValidator` | Validator | `Application/Validators/Admin/IdentityVerificationValidators.cs` | `Validators/RejectIdentityVerificationRequestValidatorTests.cs` |  |
| ✅ | `SubmitIdentityVerificationRequestValidator` | Validator | `Application/Validators/Admin/IdentityVerificationValidators.cs` | `Validators/SubmitIdentityVerificationRequestValidatorTests.cs` |  |
| ✅ | `SyncRolePermissionsRequestValidator` | Validator | `Application/Validators/Admin/PermissionValidators.cs` | `Validators/SyncRolePermissionsRequestValidatorTests.cs` |  |
| ✅ | `AdminUserRepository` | Repository | `Data/Repositories/AdminUserRepository.cs` | `Repositories/AdminUserRepositoryTests.cs` | needs the test database |
| ✅ | `IdentityVerificationRepository` | Repository | `Data/Repositories/IdentityVerificationRepository.cs` | `Repositories/IdentityVerificationRepositoryTests.cs` | needs the test database |
| ✅ | `PermissionRepository` | Repository | `Data/Repositories/PermissionRepository.cs` | `Repositories/PermissionRepositoryTests.cs` | needs the test database |
| ✅ | `AdminUsersController` | Controller | `Api/Controllers/AdminUsersController.cs` | `Controllers/AdminUsersControllerTests.cs` |  |
| ✅ | `IdentityVerificationsController` | Controller | `Api/Controllers/IdentityVerificationsController.cs` | `Controllers/IdentityVerificationsControllerTests.cs` |  |
| ✅ | `PermissionsController` | Controller | `Api/Controllers/PermissionsController.cs` | `Controllers/PermissionsControllerTests.cs` |  |

<a id="be-aibusiness"></a>

### AIBusiness

**Folder:** `Features/AIBusiness/` · **Units:** 9 · **Phase:** 3

Contains: 1 service, 6 validators, 1 infrastructure class, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AIBusinessService` | Service | `Application/Services/AIBusiness/AIBusinessService.cs` | `Services/AIBusinessServiceTests.cs` |  |
| ☐ | `BusinessTranslationRequestValidator` | Validator | `Application/Validators/AIBusiness/BusinessTranslationRequestValidator.cs` | `Validators/BusinessTranslationRequestValidatorTests.cs` |  |
| ☐ | `DemandForecastRequestValidator` | Validator | `Application/Validators/AIBusiness/DemandForecastRequestValidator.cs` | `Validators/DemandForecastRequestValidatorTests.cs` |  |
| ☐ | `MaterialForecastRequestValidator` | Validator | `Application/Validators/AIBusiness/MaterialForecastRequestValidator.cs` | `Validators/MaterialForecastRequestValidatorTests.cs` |  |
| ☐ | `PriceSuggestionRequestValidator` | Validator | `Application/Validators/AIBusiness/PriceSuggestionRequestValidator.cs` | `Validators/PriceSuggestionRequestValidatorTests.cs` |  |
| ☐ | `ProductDescriptionRequestValidator` | Validator | `Application/Validators/AIBusiness/ProductDescriptionRequestValidator.cs` | `Validators/ProductDescriptionRequestValidatorTests.cs` |  |
| ☐ | `ProductionPlannerRequestValidator` | Validator | `Application/Validators/AIBusiness/ProductionPlannerRequestValidator.cs` | `Validators/ProductionPlannerRequestValidatorTests.cs` |  |
| ☐ | `DummyAIBusinessProvider` | Infrastructure | `Infrastructure/AIBusiness/DummyAIBusinessProvider.cs` | `Infrastructure/DummyAIBusinessProviderTests.cs` |  |
| ☐ | `AIBusinessAssistantController` | Controller | `Api/Controllers/AIBusinessAssistantController.cs` | `Controllers/AIBusinessAssistantControllerTests.cs` |  |

<a id="be-aibusinesspartner"></a>

### AIBusinessPartner

**Folder:** `Features/AIBusinessPartner/` · **Units:** 9 · **Phase:** 3

Contains: 1 service, 5 validators, 1 infrastructure class, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `BusinessPartnerAIService` | Service | `Application/Services/AIBusinessPartner/BusinessPartnerAIService.cs` | `Services/BusinessPartnerAIServiceTests.cs` |  |
| ☐ | `DeliveryPredictionRequestValidator` | Validator | `Application/Validators/AIIntelligence/DeliveryPredictionRequestValidator.cs` | `Validators/DeliveryPredictionRequestValidatorTests.cs` |  |
| ☐ | `PriceForecastRequestValidator` | Validator | `Application/Validators/AIIntelligence/PriceForecastRequestValidator.cs` | `Validators/PriceForecastRequestValidatorTests.cs` |  |
| ☐ | `QualityPredictionRequestValidator` | Validator | `Application/Validators/AIIntelligence/QualityPredictionRequestValidator.cs` | `Validators/QualityPredictionRequestValidatorTests.cs` |  |
| ☐ | `RiskAssessmentRequestValidator` | Validator | `Application/Validators/AIIntelligence/RiskAssessmentRequestValidator.cs` | `Validators/RiskAssessmentRequestValidatorTests.cs` |  |
| ☐ | `SupplierRankingRequestValidator` | Validator | `Application/Validators/AIIntelligence/SupplierRankingRequestValidator.cs` | `Validators/SupplierRankingRequestValidatorTests.cs` |  |
| ☐ | `DummyBusinessPartnerAIProvider` | Infrastructure | `Infrastructure/AIBusinessPartner/DummyBusinessPartnerAIProvider.cs` | `Infrastructure/DummyBusinessPartnerAIProviderTests.cs` |  |
| ☐ | `AIIntelligenceRepository` | Repository | `Data/Repositories/AIIntelligenceRepository.cs` | `Repositories/AIIntelligenceRepositoryTests.cs` | needs the test database |
| ☐ | `AIIntelligenceController` | Controller | `Api/Controllers/AIIntelligenceController.cs` | `Controllers/AIIntelligenceControllerTests.cs` |  |

<a id="be-aishopping"></a>

### AIShopping

**Folder:** `Features/AIShopping/` · **Units:** 9 · **Phase:** 5 · **Partly tested already:** 4

Contains: 2 services, 3 validators, 3 infrastructure classes, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `FashionMatchingService` | Service | `Application/Services/AIShopping/FashionMatchingService.cs` | `Services/FashionMatchingServiceTests.cs` | partly tested by `backend/tests/FashionMatchingRegression` |
| ☐ | `GiftRecommendationService` | Service | `Application/Services/AIShopping/GiftRecommendationService.cs` | `Services/GiftRecommendationServiceTests.cs` |  |
| ☐ | `FashionMatchRequestValidator` | Validator | `Application/Validators/AIShopping/FashionMatchRequestValidator.cs` | `Validators/FashionMatchRequestValidatorTests.cs` |  |
| ☐ | `GiftRecommendationRequestValidator` | Validator | `Application/Validators/AIShopping/GiftRecommendationRequestValidator.cs` | `Validators/GiftRecommendationRequestValidatorTests.cs` |  |
| ☐ | `TranslationRequestValidator` | Validator | `Application/Validators/AIShopping/TranslationRequestValidator.cs` | `Validators/TranslationRequestValidatorTests.cs` | partly tested by `backend/tests/TranslationRegression` |
| ☐ | `GeminiInteriorPreviewProvider` | Infrastructure | `Infrastructure/AIShopping/GeminiInteriorPreviewProvider.cs` | `Infrastructure/GeminiInteriorPreviewProviderTests.cs` | partly tested by `backend/tests/InteriorPreviewRegression` |
| ☐ | `GeminiTranslationProvider` | Infrastructure | `Infrastructure/AIShopping/GeminiTranslationProvider.cs` | `Infrastructure/GeminiTranslationProviderTests.cs` | partly tested by `backend/tests/TranslationRegression` |
| ☐ | `PythonProductSearchProvider` | Infrastructure | `Infrastructure/ProductSearch/PythonProductSearchProvider.cs` | `Infrastructure/PythonProductSearchProviderTests.cs` |  |
| ☐ | `AIShoppingController` | Controller | `Api/Controllers/AIShoppingController.cs` | `Controllers/AIShoppingControllerTests.cs` |  |

<a id="be-aitourism"></a>

### AITourism

**Folder:** `Features/AITourism/` · **Units:** 17 · **Phase:** 4 · **Partly tested already:** 2

Contains: 2 services, 5 validators, 6 infrastructure classes, 2 repositories, 1 seeder, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AITourismService` | Service | `Application/Services/AITourism/AITourismService.cs` | `Services/AITourismServiceTests.cs` | partly tested by `backend/tests/TourismBudgetRegression`, `backend/tests/TourismRouteRegression` |
| ☐ | `SavedTourPlanService` | Service | `Application/Services/AITourism/SavedTourPlanService.cs` | `Services/SavedTourPlanServiceTests.cs` |  |
| ☐ | `BudgetPlanRequestValidator` | Validator | `Application/Validators/AITourism/BudgetPlanRequestValidator.cs` | `Validators/BudgetPlanRequestValidatorTests.cs` |  |
| ☐ | `CulturalRecommendationRequestValidator` | Validator | `Application/Validators/AITourism/CulturalRecommendationRequestValidator.cs` | `Validators/CulturalRecommendationRequestValidatorTests.cs` |  |
| ☐ | `RouteOptimizationRequestValidator` | Validator | `Application/Validators/AITourism/RouteOptimizationRequestValidator.cs` | `Validators/RouteOptimizationRequestValidatorTests.cs` |  |
| ☐ | `TourismTranslationRequestValidator` | Validator | `Application/Validators/AITourism/TourismTranslationRequestValidator.cs` | `Validators/TourismTranslationRequestValidatorTests.cs` |  |
| ☐ | `TourPlanRequestValidator` | Validator | `Application/Validators/AITourism/TourPlanRequestValidator.cs` | `Validators/TourPlanRequestValidatorTests.cs` |  |
| ☐ | `DummyAITourismProvider` | Infrastructure | `Infrastructure/AITourism/DummyAITourismProvider.cs` | `Infrastructure/DummyAITourismProviderTests.cs` | partly tested by `backend/tests/TourismRouteRegression`, `backend/tests/TourismBudgetRegression` |
| ☐ | `GeminiAITourismProvider` | Infrastructure | `Infrastructure/AITourism/GeminiAITourismProvider.cs` | `Infrastructure/GeminiAITourismProviderTests.cs` |  |
| ☐ | `NominatimGeocodingProvider` | Infrastructure | `Infrastructure/Geocoding/NominatimGeocodingProvider.cs` | `Infrastructure/NominatimGeocodingProviderTests.cs` |  |
| ☐ | `NominatimRateGate` | Infrastructure | `Infrastructure/Geocoding/NominatimRateGate.cs` | `Infrastructure/NominatimRateGateTests.cs` |  |
| ☐ | `OsrmRoutingProvider` | Infrastructure | `Infrastructure/Routing/OsrmRoutingProvider.cs` | `Infrastructure/OsrmRoutingProviderTests.cs` |  |
| ☐ | `RagTravelPlannerProvider` | Infrastructure | `Infrastructure/AITourism/RagTravelPlannerProvider.cs` | `Infrastructure/RagTravelPlannerProviderTests.cs` |  |
| ☐ | `SavedTourPlanRepository` | Repository | `Data/Repositories/SavedTourPlanRepository.cs` | `Repositories/SavedTourPlanRepositoryTests.cs` | needs the test database |
| ☐ | `TransportOptionRepository` | Repository | `Data/Repositories/TransportOptionRepository.cs` | `Repositories/TransportOptionRepositoryTests.cs` | needs the test database |
| ☐ | `TransportOptionSeeder` | Seeder | `Data/Seed/TransportOptionSeeder.cs` | `Seed/TransportOptionSeederTests.cs` |  |
| ☐ | `AITourismController` | Controller | `Api/Controllers/AITourismController.cs` | `Controllers/AITourismControllerTests.cs` |  |

<a id="be-analytics"></a>

### Analytics

**Folder:** `Features/Analytics/` · **Units:** 6 · **Phase:** 4

Contains: 2 services, 2 repositories, 2 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AnalyticsService` | Service | `Application/Services/Analytics/AnalyticsService.cs` | `Services/AnalyticsServiceTests.cs` |  |
| ☐ | `TouristAnalyticsService` | Service | `Application/Services/Analytics/TouristAnalyticsService.cs` | `Services/TouristAnalyticsServiceTests.cs` |  |
| ☐ | `AnalyticsRepository` | Repository | `Data/Repositories/AnalyticsRepository.cs` | `Repositories/AnalyticsRepositoryTests.cs` | needs the test database |
| ☐ | `TouristAnalyticsRepository` | Repository | `Data/Repositories/TouristAnalyticsRepository.cs` | `Repositories/TouristAnalyticsRepositoryTests.cs` | needs the test database |
| ☐ | `AnalyticsController` | Controller | `Api/Controllers/AnalyticsController.cs` | `Controllers/AnalyticsControllerTests.cs` |  |
| ☐ | `TouristAnalyticsController` | Controller | `Api/Controllers/TouristAnalyticsController.cs` | `Controllers/TouristAnalyticsControllerTests.cs` |  |

<a id="be-apprenticeship"></a>

### Apprenticeship

**Folder:** `Features/Apprenticeship/` · **Units:** 16 · **Phase:** 4

Contains: 3 services, 7 validators, 3 repositories, 3 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ApprenticeEnrollmentService` | Service | `Application/Services/Apprenticeship/ApprenticeEnrollmentService.cs` | `Services/ApprenticeEnrollmentServiceTests.cs` |  |
| ☐ | `ApprenticeshipProgramService` | Service | `Application/Services/Apprenticeship/ApprenticeshipProgramService.cs` | `Services/ApprenticeshipProgramServiceTests.cs` |  |
| ☐ | `ProgramApplicationService` | Service | `Application/Services/Apprenticeship/ProgramApplicationService.cs` | `Services/ProgramApplicationServiceTests.cs` |  |
| ☐ | `CreateApprenticeshipProgramRequestValidator` | Validator | `Application/Validators/Apprenticeship/CreateApprenticeshipProgramRequestValidator.cs` | `Validators/CreateApprenticeshipProgramRequestValidatorTests.cs` |  |
| ☐ | `CreateProgramApplicationRequestValidator` | Validator | `Application/Validators/Apprenticeship/CreateProgramApplicationRequestValidator.cs` | `Validators/CreateProgramApplicationRequestValidatorTests.cs` |  |
| ☐ | `CreateTrainingMilestoneRequestValidator` | Validator | `Application/Validators/Apprenticeship/CreateTrainingMilestoneRequestValidator.cs` | `Validators/CreateTrainingMilestoneRequestValidatorTests.cs` |  |
| ☐ | `RespondProgramApplicationRequestValidator` | Validator | `Application/Validators/Apprenticeship/RespondProgramApplicationRequestValidator.cs` | `Validators/RespondProgramApplicationRequestValidatorTests.cs` |  |
| ☐ | `UpdateApprenticeshipProgramRequestValidator` | Validator | `Application/Validators/Apprenticeship/UpdateApprenticeshipProgramRequestValidator.cs` | `Validators/UpdateApprenticeshipProgramRequestValidatorTests.cs` |  |
| ☐ | `UpdateMilestoneProgressRequestValidator` | Validator | `Application/Validators/Apprenticeship/UpdateMilestoneProgressRequestValidator.cs` | `Validators/UpdateMilestoneProgressRequestValidatorTests.cs` |  |
| ☐ | `UpdateTrainingMilestoneRequestValidator` | Validator | `Application/Validators/Apprenticeship/UpdateTrainingMilestoneRequestValidator.cs` | `Validators/UpdateTrainingMilestoneRequestValidatorTests.cs` |  |
| ☐ | `ApprenticeEnrollmentRepository` | Repository | `Data/Repositories/ApprenticeEnrollmentRepository.cs` | `Repositories/ApprenticeEnrollmentRepositoryTests.cs` | needs the test database |
| ☐ | `ApprenticeshipProgramRepository` | Repository | `Data/Repositories/ApprenticeshipProgramRepository.cs` | `Repositories/ApprenticeshipProgramRepositoryTests.cs` | needs the test database |
| ☐ | `ProgramApplicationRepository` | Repository | `Data/Repositories/ProgramApplicationRepository.cs` | `Repositories/ProgramApplicationRepositoryTests.cs` | needs the test database |
| ☐ | `ApprenticeEnrollmentsController` | Controller | `Api/Controllers/ApprenticeEnrollmentsController.cs` | `Controllers/ApprenticeEnrollmentsControllerTests.cs` |  |
| ☐ | `ApprenticeshipProgramsController` | Controller | `Api/Controllers/ApprenticeshipProgramsController.cs` | `Controllers/ApprenticeshipProgramsControllerTests.cs` |  |
| ☐ | `ProgramApplicationsController` | Controller | `Api/Controllers/ProgramApplicationsController.cs` | `Controllers/ProgramApplicationsControllerTests.cs` |  |

<a id="be-arvr"></a>

### ArVr

**Folder:** `Features/ArVr/` · **Units:** 18 · **Phase:** 5

Contains: 4 services, 7 validators, 3 repositories, 4 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ArCraftScanService` | Service | `Application/Services/ArVr/ArCraftScanService.cs` | `Services/ArCraftScanServiceTests.cs` |  |
| ☐ | `CulturalStoryService` | Service | `Application/Services/ArVr/CulturalStoryService.cs` | `Services/CulturalStoryServiceTests.cs` |  |
| ☐ | `MuseumItemService` | Service | `Application/Services/ArVr/MuseumItemService.cs` | `Services/MuseumItemServiceTests.cs` |  |
| ☐ | `VillageTourService` | Service | `Application/Services/ArVr/VillageTourService.cs` | `Services/VillageTourServiceTests.cs` |  |
| ☐ | `ArCraftScanRequestValidator` | Validator | `Application/Validators/ArVr/ArCraftScanRequestValidator.cs` | `Validators/ArCraftScanRequestValidatorTests.cs` |  |
| ☐ | `CreateCulturalStoryRequestValidator` | Validator | `Application/Validators/ArVr/CreateCulturalStoryRequestValidator.cs` | `Validators/CreateCulturalStoryRequestValidatorTests.cs` |  |
| ☐ | `CreateMuseumItemRequestValidator` | Validator | `Application/Validators/ArVr/CreateMuseumItemRequestValidator.cs` | `Validators/CreateMuseumItemRequestValidatorTests.cs` |  |
| ☐ | `CreateVillageTourStopRequestValidator` | Validator | `Application/Validators/ArVr/CreateVillageTourStopRequestValidator.cs` | `Validators/CreateVillageTourStopRequestValidatorTests.cs` |  |
| ☐ | `UpdateCulturalStoryRequestValidator` | Validator | `Application/Validators/ArVr/UpdateCulturalStoryRequestValidator.cs` | `Validators/UpdateCulturalStoryRequestValidatorTests.cs` |  |
| ☐ | `UpdateMuseumItemRequestValidator` | Validator | `Application/Validators/ArVr/UpdateMuseumItemRequestValidator.cs` | `Validators/UpdateMuseumItemRequestValidatorTests.cs` |  |
| ☐ | `UpdateVillageTourStopRequestValidator` | Validator | `Application/Validators/ArVr/UpdateVillageTourStopRequestValidator.cs` | `Validators/UpdateVillageTourStopRequestValidatorTests.cs` |  |
| ☐ | `CulturalStoryRepository` | Repository | `Data/Repositories/CulturalStoryRepository.cs` | `Repositories/CulturalStoryRepositoryTests.cs` | needs the test database |
| ☐ | `MuseumItemRepository` | Repository | `Data/Repositories/MuseumItemRepository.cs` | `Repositories/MuseumItemRepositoryTests.cs` | needs the test database |
| ☐ | `VillageTourStopRepository` | Repository | `Data/Repositories/VillageTourStopRepository.cs` | `Repositories/VillageTourStopRepositoryTests.cs` | needs the test database |
| ☐ | `ArCraftScanController` | Controller | `Api/Controllers/ArCraftScanController.cs` | `Controllers/ArCraftScanControllerTests.cs` |  |
| ☐ | `CulturalStoriesController` | Controller | `Api/Controllers/CulturalStoriesController.cs` | `Controllers/CulturalStoriesControllerTests.cs` |  |
| ☐ | `MuseumItemsController` | Controller | `Api/Controllers/MuseumItemsController.cs` | `Controllers/MuseumItemsControllerTests.cs` |  |
| ☐ | `VillageTourController` | Controller | `Api/Controllers/VillageTourController.cs` | `Controllers/VillageTourControllerTests.cs` |  |

<a id="be-auction"></a>

### Auction

**Folder:** `Features/Auction/` · **Units:** 6 · **Phase:** 2

Contains: 1 service, 3 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `AuctionService` | Service | `Application/Services/Auction/AuctionService.cs` | `Services/AuctionServiceTests.cs` |  |
| ✅ | `AuctionQueryParametersValidator` | Validator | `Application/Validators/Auction/AuctionQueryParametersValidator.cs` | `Validators/AuctionQueryParametersValidatorTests.cs` |  |
| ✅ | `CreateAuctionRequestValidator` | Validator | `Application/Validators/Auction/CreateAuctionRequestValidator.cs` | `Validators/CreateAuctionRequestValidatorTests.cs` |  |
| ✅ | `PlaceBidRequestValidator` | Validator | `Application/Validators/Auction/PlaceBidRequestValidator.cs` | `Validators/PlaceBidRequestValidatorTests.cs` |  |
| ✅ | `AuctionRepository` | Repository | `Data/Repositories/AuctionRepository.cs` | `Repositories/AuctionRepositoryTests.cs` | needs the test database |
| ✅ | `AuctionsController` | Controller | `Api/Controllers/AuctionsController.cs` | `Controllers/AuctionsControllerTests.cs` |  |

<a id="be-auth"></a>

### Auth

**Folder:** `Features/Auth/` · **Units:** 20 · **Phase:** 1

Contains: 2 services, 9 validators, 3 infrastructure classes, 4 repositories, 2 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `AuthService` | Service | `Application/Services/Auth/AuthService.cs` | `Services/AuthServiceTests.cs` |  |
| ✅ | `RoleService` | Service | `Application/Services/Auth/RoleService.cs` | `Services/RoleServiceTests.cs` |  |
| ✅ | `AssignRoleRequestValidator` | Validator | `Application/Validators/Roles/AssignRoleRequestValidator.cs` | `Validators/AssignRoleRequestValidatorTests.cs` |  |
| ✅ | `ForgotPasswordRequestValidator` | Validator | `Application/Validators/Auth/ForgotPasswordRequestValidator.cs` | `Validators/ForgotPasswordRequestValidatorTests.cs` |  |
| ✅ | `LoginRequestValidator` | Validator | `Application/Validators/Auth/LoginRequestValidator.cs` | `Validators/LoginRequestValidatorTests.cs` |  |
| ✅ | `LogoutRequestValidator` | Validator | `Application/Validators/Auth/LogoutRequestValidator.cs` | `Validators/LogoutRequestValidatorTests.cs` |  |
| ✅ | `RefreshTokenRequestValidator` | Validator | `Application/Validators/Auth/RefreshTokenRequestValidator.cs` | `Validators/RefreshTokenRequestValidatorTests.cs` |  |
| ✅ | `RegisterRequestValidator` | Validator | `Application/Validators/Auth/RegisterRequestValidator.cs` | `Validators/RegisterRequestValidatorTests.cs` |  |
| ✅ | `RemoveRoleRequestValidator` | Validator | `Application/Validators/Roles/RemoveRoleRequestValidator.cs` | `Validators/RemoveRoleRequestValidatorTests.cs` |  |
| ✅ | `ResetPasswordRequestValidator` | Validator | `Application/Validators/Auth/ResetPasswordRequestValidator.cs` | `Validators/ResetPasswordRequestValidatorTests.cs` |  |
| ✅ | `SwitchRoleRequestValidator` | Validator | `Application/Validators/Auth/SwitchRoleRequestValidator.cs` | `Validators/SwitchRoleRequestValidatorTests.cs` |  |
| ✅ | `BCryptPasswordHasher` | Infrastructure | `Infrastructure/Security/BCryptPasswordHasher.cs` | `Infrastructure/BCryptPasswordHasherTests.cs` |  |
| ✅ | `ConsoleEmailSender` | Infrastructure | `Infrastructure/Email/ConsoleEmailSender.cs` | `Infrastructure/ConsoleEmailSenderTests.cs` |  |
| ✅ | `JwtTokenService` | Infrastructure | `Infrastructure/Security/JwtTokenService.cs` | `Infrastructure/JwtTokenServiceTests.cs` |  |
| ✅ | `PasswordResetTokenRepository` | Repository | `Data/Repositories/PasswordResetTokenRepository.cs` | `Repositories/PasswordResetTokenRepositoryTests.cs` | needs the test database |
| ✅ | `RefreshTokenRepository` | Repository | `Data/Repositories/RefreshTokenRepository.cs` | `Repositories/RefreshTokenRepositoryTests.cs` | needs the test database |
| ✅ | `RoleRepository` | Repository | `Data/Repositories/RoleRepository.cs` | `Repositories/RoleRepositoryTests.cs` | needs the test database |
| ✅ | `UserRepository` | Repository | `Data/Repositories/UserRepository.cs` | `Repositories/UserRepositoryTests.cs` | needs the test database |
| ✅ | `AuthController` | Controller | `Api/Controllers/AuthController.cs` | `Controllers/AuthControllerTests.cs` |  |
| ✅ | `RolesController` | Controller | `Api/Controllers/RolesController.cs` | `Controllers/RolesControllerTests.cs` |  |

<a id="be-businesspartner"></a>

### BusinessPartner

**Folder:** `Features/BusinessPartner/` · **Units:** 7 · **Phase:** 3

Contains: 1 service, 4 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `BusinessPartnerService` | Service | `Application/Services/BusinessPartner/BusinessPartnerService.cs` | `Services/BusinessPartnerServiceTests.cs` |  |
| ☐ | `BusinessDocumentInputValidator` | Validator | `Application/Validators/BusinessPartner/BusinessDocumentInputValidator.cs` | `Validators/BusinessDocumentInputValidatorTests.cs` |  |
| ☐ | `BusinessPartnerQueryParametersValidator` | Validator | `Application/Validators/BusinessPartner/BusinessPartnerQueryParametersValidator.cs` | `Validators/BusinessPartnerQueryParametersValidatorTests.cs` |  |
| ☐ | `UpsertBusinessPartnerProfileRequestValidator` | Validator | `Application/Validators/BusinessPartner/UpsertBusinessPartnerProfileRequestValidator.cs` | `Validators/UpsertBusinessPartnerProfileRequestValidatorTests.cs` |  |
| ☐ | `VerifyBusinessPartnerRequestValidator` | Validator | `Application/Validators/BusinessPartner/VerifyBusinessPartnerRequestValidator.cs` | `Validators/VerifyBusinessPartnerRequestValidatorTests.cs` |  |
| ☐ | `BusinessPartnerRepository` | Repository | `Data/Repositories/BusinessPartnerRepository.cs` | `Repositories/BusinessPartnerRepositoryTests.cs` | needs the test database |
| ☐ | `BusinessPartnersController` | Controller | `Api/Controllers/BusinessPartnersController.cs` | `Controllers/BusinessPartnersControllerTests.cs` |  |

<a id="be-businesspartneranalytics"></a>

### BusinessPartnerAnalytics

**Folder:** `Features/BusinessPartnerAnalytics/` · **Units:** 4 · **Phase:** 3

Contains: 1 service, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `BusinessPartnerAnalyticsService` | Service | `Application/Services/BusinessPartnerAnalytics/BusinessPartnerAnalyticsService.cs` | `Services/BusinessPartnerAnalyticsServiceTests.cs` |  |
| ☐ | `AnalyticsQueryParametersValidator` | Validator | `Application/Validators/BusinessPartnerAnalytics/AnalyticsQueryParametersValidator.cs` | `Validators/AnalyticsQueryParametersValidatorTests.cs` |  |
| ☐ | `BusinessPartnerAnalyticsRepository` | Repository | `Data/Repositories/BusinessPartnerAnalyticsRepository.cs` | `Repositories/BusinessPartnerAnalyticsRepositoryTests.cs` | needs the test database |
| ☐ | `BusinessPartnerAnalyticsController` | Controller | `Api/Controllers/BusinessPartnerAnalyticsController.cs` | `Controllers/BusinessPartnerAnalyticsControllerTests.cs` |  |

<a id="be-certificate"></a>

### Certificate

**Folder:** `Features/Certificate/` · **Units:** 5 · **Phase:** 2

Contains: 1 service, 2 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `CertificateService` | Service | `Application/Services/Certificate/CertificateService.cs` | `Services/CertificateServiceTests.cs` |  |
| ✅ | `GenerateCertificateRequestValidator` | Validator | `Application/Validators/Certificate/GenerateCertificateRequestValidator.cs` | `Validators/GenerateCertificateRequestValidatorTests.cs` |  |
| ✅ | `VerifyCertificateRequestValidator` | Validator | `Application/Validators/Certificate/VerifyCertificateRequestValidator.cs` | `Validators/VerifyCertificateRequestValidatorTests.cs` |  |
| ✅ | `CertificateRepository` | Repository | `Data/Repositories/CertificateRepository.cs` | `Repositories/CertificateRepositoryTests.cs` | needs the test database |
| ✅ | `CertificatesController` | Controller | `Api/Controllers/CertificatesController.cs` | `Controllers/CertificatesControllerTests.cs` |  |

<a id="be-certificates"></a>

### Certificates

**Folder:** `Features/Certificates/` · **Units:** 3 · **Phase:** 2

Contains: 1 service, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `ExpertiseCertificateService` | Service | `Application/Services/Certificates/ExpertiseCertificateService.cs` | `Services/ExpertiseCertificateServiceTests.cs` |  |
| ✅ | `ExpertiseCertificateRepository` | Repository | `Data/Repositories/ExpertiseCertificateRepository.cs` | `Repositories/ExpertiseCertificateRepositoryTests.cs` | needs the test database |
| ✅ | `ExpertiseCertificatesController` | Controller | `Api/Controllers/ExpertiseCertificatesController.cs` | `Controllers/ExpertiseCertificatesControllerTests.cs` |  |

<a id="be-cms"></a>

### Cms

**Folder:** `Features/Cms/` · **Units:** 30 · **Phase:** 5

Contains: 6 services, 11 validators, 6 repositories, 1 seeder, 6 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AnnouncementService` | Service | `Application/Services/Cms/AnnouncementService.cs` | `Services/AnnouncementServiceTests.cs` |  |
| ☐ | `BlogPostService` | Service | `Application/Services/Cms/BlogPostService.cs` | `Services/BlogPostServiceTests.cs` |  |
| ☐ | `CmsEventService` | Service | `Application/Services/Cms/CmsEventService.cs` | `Services/CmsEventServiceTests.cs` |  |
| ☐ | `HomepageSectionService` | Service | `Application/Services/Cms/HomepageSectionService.cs` | `Services/HomepageSectionServiceTests.cs` |  |
| ☐ | `NewsItemService` | Service | `Application/Services/Cms/NewsItemService.cs` | `Services/NewsItemServiceTests.cs` |  |
| ☐ | `SiteContentService` | Service | `Application/Services/Cms/SiteContentService.cs` | `Services/SiteContentServiceTests.cs` |  |
| ☐ | `CreateAnnouncementRequestValidator` | Validator | `Application/Validators/Cms/CreateAnnouncementRequestValidator.cs` | `Validators/CreateAnnouncementRequestValidatorTests.cs` |  |
| ☐ | `CreateBlogPostRequestValidator` | Validator | `Application/Validators/Cms/CreateBlogPostRequestValidator.cs` | `Validators/CreateBlogPostRequestValidatorTests.cs` |  |
| ☐ | `CreateCmsEventRequestValidator` | Validator | `Application/Validators/Cms/CreateCmsEventRequestValidator.cs` | `Validators/CreateCmsEventRequestValidatorTests.cs` |  |
| ☐ | `CreateHomepageSectionRequestValidator` | Validator | `Application/Validators/Cms/CreateHomepageSectionRequestValidator.cs` | `Validators/CreateHomepageSectionRequestValidatorTests.cs` |  |
| ☐ | `CreateNewsItemRequestValidator` | Validator | `Application/Validators/Cms/CreateNewsItemRequestValidator.cs` | `Validators/CreateNewsItemRequestValidatorTests.cs` |  |
| ☐ | `SaveSiteContentItemRequestValidator` | Validator | `Application/Validators/Cms/SiteContentValidators.cs` | `Validators/SaveSiteContentItemRequestValidatorTests.cs` |  |
| ☐ | `UpdateAnnouncementRequestValidator` | Validator | `Application/Validators/Cms/UpdateAnnouncementRequestValidator.cs` | `Validators/UpdateAnnouncementRequestValidatorTests.cs` |  |
| ☐ | `UpdateBlogPostRequestValidator` | Validator | `Application/Validators/Cms/UpdateBlogPostRequestValidator.cs` | `Validators/UpdateBlogPostRequestValidatorTests.cs` |  |
| ☐ | `UpdateCmsEventRequestValidator` | Validator | `Application/Validators/Cms/UpdateCmsEventRequestValidator.cs` | `Validators/UpdateCmsEventRequestValidatorTests.cs` |  |
| ☐ | `UpdateHomepageSectionRequestValidator` | Validator | `Application/Validators/Cms/UpdateHomepageSectionRequestValidator.cs` | `Validators/UpdateHomepageSectionRequestValidatorTests.cs` |  |
| ☐ | `UpdateNewsItemRequestValidator` | Validator | `Application/Validators/Cms/UpdateNewsItemRequestValidator.cs` | `Validators/UpdateNewsItemRequestValidatorTests.cs` |  |
| ☐ | `AnnouncementRepository` | Repository | `Data/Repositories/AnnouncementRepository.cs` | `Repositories/AnnouncementRepositoryTests.cs` | needs the test database |
| ☐ | `BlogPostRepository` | Repository | `Data/Repositories/BlogPostRepository.cs` | `Repositories/BlogPostRepositoryTests.cs` | needs the test database |
| ☐ | `CmsEventRepository` | Repository | `Data/Repositories/CmsEventRepository.cs` | `Repositories/CmsEventRepositoryTests.cs` | needs the test database |
| ☐ | `HomepageSectionRepository` | Repository | `Data/Repositories/HomepageSectionRepository.cs` | `Repositories/HomepageSectionRepositoryTests.cs` | needs the test database |
| ☐ | `NewsItemRepository` | Repository | `Data/Repositories/NewsItemRepository.cs` | `Repositories/NewsItemRepositoryTests.cs` | needs the test database |
| ☐ | `SiteContentRepository` | Repository | `Data/Repositories/SiteContentRepository.cs` | `Repositories/SiteContentRepositoryTests.cs` | needs the test database |
| ☐ | `SiteContentSeeder` | Seeder | `Data/Seed/SiteContentSeeder.cs` | `Seed/SiteContentSeederTests.cs` |  |
| ☐ | `CmsAnnouncementsController` | Controller | `Api/Controllers/CmsAnnouncementsController.cs` | `Controllers/CmsAnnouncementsControllerTests.cs` |  |
| ☐ | `CmsBlogsController` | Controller | `Api/Controllers/CmsBlogsController.cs` | `Controllers/CmsBlogsControllerTests.cs` |  |
| ☐ | `CmsEventsController` | Controller | `Api/Controllers/CmsEventsController.cs` | `Controllers/CmsEventsControllerTests.cs` |  |
| ☐ | `CmsHomepageController` | Controller | `Api/Controllers/CmsHomepageController.cs` | `Controllers/CmsHomepageControllerTests.cs` |  |
| ☐ | `CmsNewsController` | Controller | `Api/Controllers/CmsNewsController.cs` | `Controllers/CmsNewsControllerTests.cs` |  |
| ☐ | `SiteContentController` | Controller | `Api/Controllers/SiteContentController.cs` | `Controllers/SiteContentControllerTests.cs` |  |

<a id="be-commerce"></a>

### Commerce

**Folder:** `Features/Commerce/` · **Units:** 26 · **Phase:** 2

Contains: 4 services, 13 validators, 1 infrastructure class, 4 repositories, 4 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `CartService` | Service | `Application/Services/Commerce/CartService.cs` | `Services/CartServiceTests.cs` |  |
| ☐ | `OrderService` | Service | `Application/Services/Commerce/OrderService.cs` | `Services/OrderServiceTests.cs` |  |
| ☐ | `PaymentService` | Service | `Application/Services/Commerce/PaymentService.cs` | `Services/PaymentServiceTests.cs` |  |
| ☐ | `WishlistService` | Service | `Application/Services/Commerce/WishlistService.cs` | `Services/WishlistServiceTests.cs` |  |
| ☐ | `AddToCartRequestValidator` | Validator | `Application/Validators/Commerce/AddToCartRequestValidator.cs` | `Validators/AddToCartRequestValidatorTests.cs` |  |
| ☐ | `AddToWishlistRequestValidator` | Validator | `Application/Validators/Commerce/AddToWishlistRequestValidator.cs` | `Validators/AddToWishlistRequestValidatorTests.cs` |  |
| ☐ | `CancelOrderRequestValidator` | Validator | `Application/Validators/Commerce/CancelOrderRequestValidator.cs` | `Validators/CancelOrderRequestValidatorTests.cs` |  |
| ☐ | `CheckoutRequestValidator` | Validator | `Application/Validators/Commerce/CheckoutRequestValidator.cs` | `Validators/CheckoutRequestValidatorTests.cs` |  |
| ☐ | `InitiatePaymentRequestValidator` | Validator | `Application/Validators/Commerce/InitiatePaymentRequestValidator.cs` | `Validators/InitiatePaymentRequestValidatorTests.cs` |  |
| ☐ | `MoveToCartRequestValidator` | Validator | `Application/Validators/Commerce/MoveToCartRequestValidator.cs` | `Validators/MoveToCartRequestValidatorTests.cs` |  |
| ☐ | `OrderQueryParametersValidator` | Validator | `Application/Validators/Commerce/OrderQueryParametersValidator.cs` | `Validators/OrderQueryParametersValidatorTests.cs` |  |
| ☐ | `RefundOrderRequestValidator` | Validator | `Application/Validators/Commerce/RefundOrderRequestValidator.cs` | `Validators/RefundOrderRequestValidatorTests.cs` |  |
| ☐ | `RefundPaymentRequestValidator` | Validator | `Application/Validators/Commerce/RefundPaymentRequestValidator.cs` | `Validators/RefundPaymentRequestValidatorTests.cs` |  |
| ☐ | `RejectReturnRequestValidator` | Validator | `Application/Validators/Commerce/RejectReturnRequestValidator.cs` | `Validators/RejectReturnRequestValidatorTests.cs` |  |
| ☐ | `ReturnOrderRequestValidator` | Validator | `Application/Validators/Commerce/ReturnOrderRequestValidator.cs` | `Validators/ReturnOrderRequestValidatorTests.cs` |  |
| ☐ | `ShipOrderRequestValidator` | Validator | `Application/Validators/Commerce/ShipOrderRequestValidator.cs` | `Validators/ShipOrderRequestValidatorTests.cs` |  |
| ☐ | `UpdateCartItemRequestValidator` | Validator | `Application/Validators/Commerce/UpdateCartItemRequestValidator.cs` | `Validators/UpdateCartItemRequestValidatorTests.cs` |  |
| ☐ | `CashOnDeliveryPaymentProvider` | Infrastructure | `Infrastructure/Payments/CashOnDeliveryPaymentProvider.cs` | `Infrastructure/CashOnDeliveryPaymentProviderTests.cs` |  |
| ☐ | `CartRepository` | Repository | `Data/Repositories/CartRepository.cs` | `Repositories/CartRepositoryTests.cs` | needs the test database |
| ☐ | `OrderRepository` | Repository | `Data/Repositories/OrderRepository.cs` | `Repositories/OrderRepositoryTests.cs` | needs the test database |
| ☐ | `PaymentRepository` | Repository | `Data/Repositories/PaymentRepository.cs` | `Repositories/PaymentRepositoryTests.cs` | needs the test database |
| ☐ | `WishlistRepository` | Repository | `Data/Repositories/WishlistRepository.cs` | `Repositories/WishlistRepositoryTests.cs` | needs the test database |
| ☐ | `CartController` | Controller | `Api/Controllers/CartController.cs` | `Controllers/CartControllerTests.cs` |  |
| ☐ | `OrdersController` | Controller | `Api/Controllers/OrdersController.cs` | `Controllers/OrdersControllerTests.cs` |  |
| ☐ | `PaymentsController` | Controller | `Api/Controllers/PaymentsController.cs` | `Controllers/PaymentsControllerTests.cs` |  |
| ☐ | `WishlistController` | Controller | `Api/Controllers/WishlistController.cs` | `Controllers/WishlistControllerTests.cs` |  |

<a id="be-community"></a>

### Community

**Folder:** `Features/Community/` · **Units:** 19 · **Phase:** 5

Contains: 4 services, 7 validators, 4 repositories, 4 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `DiscussionService` | Service | `Application/Services/Community/DiscussionService.cs` | `Services/DiscussionServiceTests.cs` |  |
| ☐ | `ProducerFollowService` | Service | `Application/Services/Community/ProducerFollowService.cs` | `Services/ProducerFollowServiceTests.cs` |  |
| ☐ | `QuestionService` | Service | `Application/Services/Community/QuestionService.cs` | `Services/QuestionServiceTests.cs` |  |
| ☐ | `VillageService` | Service | `Application/Services/Community/VillageService.cs` | `Services/VillageServiceTests.cs` |  |
| ☐ | `CreateAnswerRequestValidator` | Validator | `Application/Validators/Community/CreateAnswerRequestValidator.cs` | `Validators/CreateAnswerRequestValidatorTests.cs` |  |
| ☐ | `CreateDiscussionReplyRequestValidator` | Validator | `Application/Validators/Community/CreateDiscussionReplyRequestValidator.cs` | `Validators/CreateDiscussionReplyRequestValidatorTests.cs` |  |
| ☐ | `CreateDiscussionThreadRequestValidator` | Validator | `Application/Validators/Community/CreateDiscussionThreadRequestValidator.cs` | `Validators/CreateDiscussionThreadRequestValidatorTests.cs` |  |
| ☐ | `CreateQuestionRequestValidator` | Validator | `Application/Validators/Community/CreateQuestionRequestValidator.cs` | `Validators/CreateQuestionRequestValidatorTests.cs` |  |
| ☐ | `CreateVillageRequestValidator` | Validator | `Application/Validators/Community/CreateVillageRequestValidator.cs` | `Validators/CreateVillageRequestValidatorTests.cs` |  |
| ☐ | `DiscussionQueryParametersValidator` | Validator | `Application/Validators/Community/DiscussionQueryParametersValidator.cs` | `Validators/DiscussionQueryParametersValidatorTests.cs` |  |
| ☐ | `UpdateVillageRequestValidator` | Validator | `Application/Validators/Community/UpdateVillageRequestValidator.cs` | `Validators/UpdateVillageRequestValidatorTests.cs` |  |
| ☐ | `DiscussionRepository` | Repository | `Data/Repositories/DiscussionRepository.cs` | `Repositories/DiscussionRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerFollowRepository` | Repository | `Data/Repositories/ProducerFollowRepository.cs` | `Repositories/ProducerFollowRepositoryTests.cs` | needs the test database |
| ☐ | `QuestionRepository` | Repository | `Data/Repositories/QuestionRepository.cs` | `Repositories/QuestionRepositoryTests.cs` | needs the test database |
| ☐ | `VillageRepository` | Repository | `Data/Repositories/VillageRepository.cs` | `Repositories/VillageRepositoryTests.cs` | needs the test database |
| ☐ | `DiscussionsController` | Controller | `Api/Controllers/DiscussionsController.cs` | `Controllers/DiscussionsControllerTests.cs` |  |
| ☐ | `ProducerFollowsController` | Controller | `Api/Controllers/ProducerFollowsController.cs` | `Controllers/ProducerFollowsControllerTests.cs` |  |
| ☐ | `QuestionsController` | Controller | `Api/Controllers/QuestionsController.cs` | `Controllers/QuestionsControllerTests.cs` |  |
| ☐ | `VillagesController` | Controller | `Api/Controllers/VillagesController.cs` | `Controllers/VillagesControllerTests.cs` |  |

<a id="be-complaints"></a>

### Complaints

**Folder:** `Features/Complaints/` · **Units:** 6 · **Phase:** 2

Contains: 1 service, 3 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `OrderComplaintService` | Service | `Application/Services/Complaints/OrderComplaintService.cs` | `Services/OrderComplaintServiceTests.cs` |  |
| ✅ | `CreateOrderComplaintRequestValidator` | Validator | `Application/Validators/Complaints/OrderComplaintValidators.cs` | `Validators/CreateOrderComplaintRequestValidatorTests.cs` |  |
| ✅ | `CustomerComplaintNoteRequestValidator` | Validator | `Application/Validators/Complaints/OrderComplaintValidators.cs` | `Validators/CustomerComplaintNoteRequestValidatorTests.cs` |  |
| ✅ | `RespondToOrderComplaintRequestValidator` | Validator | `Application/Validators/Complaints/OrderComplaintValidators.cs` | `Validators/RespondToOrderComplaintRequestValidatorTests.cs` |  |
| ✅ | `OrderComplaintRepository` | Repository | `Data/Repositories/OrderComplaintRepository.cs` | `Repositories/OrderComplaintRepositoryTests.cs` | needs the test database |
| ✅ | `OrderComplaintsController` | Controller | `Api/Controllers/OrderComplaintsController.cs` | `Controllers/OrderComplaintsControllerTests.cs` |  |

<a id="be-contracts"></a>

### Contracts

**Folder:** `Features/Contracts/` · **Units:** 11 · **Phase:** 3

Contains: 1 service, 8 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ContractService` | Service | `Application/Services/Contracts/ContractService.cs` | `Services/ContractServiceTests.cs` |  |
| ☐ | `AddContractDocumentRequestValidator` | Validator | `Application/Validators/Contracts/AddContractDocumentRequestValidator.cs` | `Validators/AddContractDocumentRequestValidatorTests.cs` |  |
| ☐ | `ContractDecisionRequestValidator` | Validator | `Application/Validators/Contracts/ContractDecisionRequestValidator.cs` | `Validators/ContractDecisionRequestValidatorTests.cs` |  |
| ☐ | `ContractDeliveryScheduleInputValidator` | Validator | `Application/Validators/Contracts/ContractDeliveryScheduleInputValidator.cs` | `Validators/ContractDeliveryScheduleInputValidatorTests.cs` |  |
| ☐ | `ContractItemInputValidator` | Validator | `Application/Validators/Contracts/ContractItemInputValidator.cs` | `Validators/ContractItemInputValidatorTests.cs` |  |
| ☐ | `ContractQueryParametersValidator` | Validator | `Application/Validators/Contracts/ContractQueryParametersValidator.cs` | `Validators/ContractQueryParametersValidatorTests.cs` |  |
| ☐ | `CreateContractRequestValidator` | Validator | `Application/Validators/Contracts/CreateContractRequestValidator.cs` | `Validators/CreateContractRequestValidatorTests.cs` |  |
| ☐ | `RenewContractRequestValidator` | Validator | `Application/Validators/Contracts/RenewContractRequestValidator.cs` | `Validators/RenewContractRequestValidatorTests.cs` |  |
| ☐ | `UpdateDeliveryStatusRequestValidator` | Validator | `Application/Validators/Contracts/UpdateDeliveryStatusRequestValidator.cs` | `Validators/UpdateDeliveryStatusRequestValidatorTests.cs` |  |
| ☐ | `ContractRepository` | Repository | `Data/Repositories/ContractRepository.cs` | `Repositories/ContractRepositoryTests.cs` | needs the test database |
| ☐ | `ContractsController` | Controller | `Api/Controllers/ContractsController.cs` | `Controllers/ContractsControllerTests.cs` |  |

<a id="be-counterfeitdetection"></a>

### CounterfeitDetection

**Folder:** `Features/CounterfeitDetection/` · **Units:** 3 · **Phase:** 5

Contains: 1 service, 1 infrastructure class, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `CounterfeitDetectionService` | Service | `Application/Services/CounterfeitDetection/CounterfeitDetectionService.cs` | `Services/CounterfeitDetectionServiceTests.cs` |  |
| ☐ | `RuleBasedCounterfeitDetectionProvider` | Infrastructure | `Infrastructure/CounterfeitDetection/RuleBasedCounterfeitDetectionProvider.cs` | `Infrastructure/RuleBasedCounterfeitDetectionProviderTests.cs` |  |
| ☐ | `CounterfeitDetectionController` | Controller | `Api/Controllers/CounterfeitDetectionController.cs` | `Controllers/CounterfeitDetectionControllerTests.cs` |  |

<a id="be-csrsponsorship"></a>

### CSRSponsorship

**Folder:** `Features/CSRSponsorship/` · **Units:** 12 · **Phase:** 3

Contains: 1 service, 9 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `CSRSponsorshipService` | Service | `Application/Services/CSRSponsorship/CSRSponsorshipService.cs` | `Services/CSRSponsorshipServiceTests.cs` |  |
| ☐ | `AddImpactRecordRequestValidator` | Validator | `Application/Validators/CSRSponsorship/AddImpactRecordRequestValidator.cs` | `Validators/AddImpactRecordRequestValidatorTests.cs` |  |
| ☐ | `AddProgressUpdateRequestValidator` | Validator | `Application/Validators/CSRSponsorship/AddProgressUpdateRequestValidator.cs` | `Validators/AddProgressUpdateRequestValidatorTests.cs` |  |
| ☐ | `CreateOpportunityRequestValidator` | Validator | `Application/Validators/CSRSponsorship/CreateOpportunityRequestValidator.cs` | `Validators/CreateOpportunityRequestValidatorTests.cs` |  |
| ☐ | `OpportunityQueryParametersValidator` | Validator | `Application/Validators/CSRSponsorship/OpportunityQueryParametersValidator.cs` | `Validators/OpportunityQueryParametersValidatorTests.cs` |  |
| ☐ | `ProposalDecisionRequestValidator` | Validator | `Application/Validators/CSRSponsorship/ProposalDecisionRequestValidator.cs` | `Validators/ProposalDecisionRequestValidatorTests.cs` |  |
| ☐ | `ProposalQueryParametersValidator` | Validator | `Application/Validators/CSRSponsorship/ProposalQueryParametersValidator.cs` | `Validators/ProposalQueryParametersValidatorTests.cs` |  |
| ☐ | `SponsorshipMilestoneInputValidator` | Validator | `Application/Validators/CSRSponsorship/SponsorshipMilestoneInputValidator.cs` | `Validators/SponsorshipMilestoneInputValidatorTests.cs` |  |
| ☐ | `SubmitProposalRequestValidator` | Validator | `Application/Validators/CSRSponsorship/SubmitProposalRequestValidator.cs` | `Validators/SubmitProposalRequestValidatorTests.cs` |  |
| ☐ | `UpdateMilestoneStatusRequestValidator` | Validator | `Application/Validators/CSRSponsorship/UpdateMilestoneStatusRequestValidator.cs` | `Validators/UpdateMilestoneStatusRequestValidatorTests.cs` |  |
| ☐ | `CSRSponsorshipRepository` | Repository | `Data/Repositories/CSRSponsorshipRepository.cs` | `Repositories/CSRSponsorshipRepositoryTests.cs` | needs the test database |
| ☐ | `CSRSponsorshipController` | Controller | `Api/Controllers/CSRSponsorshipController.cs` | `Controllers/CSRSponsorshipControllerTests.cs` |  |

<a id="be-customorders"></a>

### CustomOrders

**Folder:** `Features/CustomOrders/` · **Units:** 8 · **Phase:** 2

Contains: 1 service, 1 provider/handler, 4 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `CustomOrderService` | Service | `Application/Services/CustomOrders/CustomOrderService.cs` | `Services/CustomOrderServiceTests.cs` |  |
| ☐ | `CustomOrderDeliveryHandler` | Provider/Handler | `Application/Services/CustomOrders/CustomOrderDeliveryHandler.cs` | `Services/CustomOrderDeliveryHandlerTests.cs` |  |
| ☐ | `CreateCustomOrderRequestValidator` | Validator | `Application/Validators/CustomOrders/CreateCustomOrderRequestValidator.cs` | `Validators/CreateCustomOrderRequestValidatorTests.cs` |  |
| ☐ | `RespondToCustomOrderRequestValidator` | Validator | `Application/Validators/CustomOrders/RespondToCustomOrderRequestValidator.cs` | `Validators/RespondToCustomOrderRequestValidatorTests.cs` |  |
| ☐ | `ShipCustomOrderRequestValidator` | Validator | `Application/Validators/CustomOrders/ShipCustomOrderRequestValidator.cs` | `Validators/ShipCustomOrderRequestValidatorTests.cs` |  |
| ☐ | `UpdateCustomOrderDeliveryRequestValidator` | Validator | `Application/Validators/CustomOrders/UpdateCustomOrderDeliveryRequestValidator.cs` | `Validators/UpdateCustomOrderDeliveryRequestValidatorTests.cs` |  |
| ☐ | `CustomOrderRepository` | Repository | `Data/Repositories/CustomOrderRepository.cs` | `Repositories/CustomOrderRepositoryTests.cs` | needs the test database |
| ☐ | `CustomOrdersController` | Controller | `Api/Controllers/CustomOrdersController.cs` | `Controllers/CustomOrdersControllerTests.cs` |  |

<a id="be-designcollaboration"></a>

### DesignCollaboration

**Folder:** `Features/DesignCollaboration/` · **Units:** 10 · **Phase:** 3

Contains: 1 service, 7 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `DesignCollaborationService` | Service | `Application/Services/DesignCollaboration/DesignCollaborationService.cs` | `Services/DesignCollaborationServiceTests.cs` |  |
| ☐ | `AddCommentRequestValidator` | Validator | `Application/Validators/DesignCollaboration/AddCommentRequestValidator.cs` | `Validators/AddCommentRequestValidatorTests.cs` |  |
| ☐ | `CollaborationResponseRequestValidator` | Validator | `Application/Validators/DesignCollaboration/CollaborationResponseRequestValidator.cs` | `Validators/CollaborationResponseRequestValidatorTests.cs` |  |
| ☐ | `CreateProjectRequestValidator` | Validator | `Application/Validators/DesignCollaboration/CreateProjectRequestValidator.cs` | `Validators/CreateProjectRequestValidatorTests.cs` |  |
| ☐ | `DesignFileInputValidator` | Validator | `Application/Validators/DesignCollaboration/DesignFileInputValidator.cs` | `Validators/DesignFileInputValidatorTests.cs` |  |
| ☐ | `ProjectQueryParametersValidator` | Validator | `Application/Validators/DesignCollaboration/ProjectQueryParametersValidator.cs` | `Validators/ProjectQueryParametersValidatorTests.cs` |  |
| ☐ | `RevisionDecisionRequestValidator` | Validator | `Application/Validators/DesignCollaboration/RevisionDecisionRequestValidator.cs` | `Validators/RevisionDecisionRequestValidatorTests.cs` |  |
| ☐ | `SubmitRevisionRequestValidator` | Validator | `Application/Validators/DesignCollaboration/SubmitRevisionRequestValidator.cs` | `Validators/SubmitRevisionRequestValidatorTests.cs` |  |
| ☐ | `DesignCollaborationRepository` | Repository | `Data/Repositories/DesignCollaborationRepository.cs` | `Repositories/DesignCollaborationRepositoryTests.cs` | needs the test database |
| ☐ | `DesignCollaborationsController` | Controller | `Api/Controllers/DesignCollaborationsController.cs` | `Controllers/DesignCollaborationsControllerTests.cs` |  |

<a id="be-employment"></a>

### Employment

**Folder:** `Features/Employment/` · **Units:** 14 · **Phase:** 4

Contains: 3 services, 6 validators, 2 repositories, 3 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `JobApplicationService` | Service | `Application/Services/Employment/JobApplicationService.cs` | `Services/JobApplicationServiceTests.cs` |  |
| ☐ | `JobListingService` | Service | `Application/Services/Employment/JobListingService.cs` | `Services/JobListingServiceTests.cs` |  |
| ☐ | `JobMatchingService` | Service | `Application/Services/Employment/JobMatchingService.cs` | `Services/JobMatchingServiceTests.cs` |  |
| ☐ | `AddJobSkillRequirementRequestValidator` | Validator | `Application/Validators/Employment/AddJobSkillRequirementRequestValidator.cs` | `Validators/AddJobSkillRequirementRequestValidatorTests.cs` |  |
| ☐ | `CreateJobApplicationRequestValidator` | Validator | `Application/Validators/Employment/CreateJobApplicationRequestValidator.cs` | `Validators/CreateJobApplicationRequestValidatorTests.cs` |  |
| ☐ | `CreateJobListingRequestValidator` | Validator | `Application/Validators/Employment/CreateJobListingRequestValidator.cs` | `Validators/CreateJobListingRequestValidatorTests.cs` |  |
| ☐ | `JobMatchRequestValidator` | Validator | `Application/Validators/Employment/JobMatchRequestValidator.cs` | `Validators/JobMatchRequestValidatorTests.cs` |  |
| ☐ | `RespondJobApplicationRequestValidator` | Validator | `Application/Validators/Employment/RespondJobApplicationRequestValidator.cs` | `Validators/RespondJobApplicationRequestValidatorTests.cs` |  |
| ☐ | `UpdateJobListingRequestValidator` | Validator | `Application/Validators/Employment/UpdateJobListingRequestValidator.cs` | `Validators/UpdateJobListingRequestValidatorTests.cs` |  |
| ☐ | `JobApplicationRepository` | Repository | `Data/Repositories/JobApplicationRepository.cs` | `Repositories/JobApplicationRepositoryTests.cs` | needs the test database |
| ☐ | `JobListingRepository` | Repository | `Data/Repositories/JobListingRepository.cs` | `Repositories/JobListingRepositoryTests.cs` | needs the test database |
| ☐ | `JobApplicationsController` | Controller | `Api/Controllers/JobApplicationsController.cs` | `Controllers/JobApplicationsControllerTests.cs` |  |
| ☐ | `JobListingsController` | Controller | `Api/Controllers/JobListingsController.cs` | `Controllers/JobListingsControllerTests.cs` |  |
| ☐ | `JobMatchingController` | Controller | `Api/Controllers/JobMatchingController.cs` | `Controllers/JobMatchingControllerTests.cs` |  |

<a id="be-fieldresearch"></a>

### FieldResearch

**Folder:** `Features/FieldResearch/` · **Units:** 19 · **Phase:** 4

Contains: 3 services, 12 validators, 1 repository, 3 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `FieldEvidenceService` | Service | `Application/Services/FieldResearch/FieldEvidenceService.cs` | `Services/FieldEvidenceServiceTests.cs` |  |
| ☐ | `SurveyResponseService` | Service | `Application/Services/FieldResearch/SurveyResponseService.cs` | `Services/SurveyResponseServiceTests.cs` |  |
| ☐ | `SurveyService` | Service | `Application/Services/FieldResearch/SurveyService.cs` | `Services/SurveyServiceTests.cs` |  |
| ☐ | `AssignFieldResearcherRequestValidator` | Validator | `Application/Validators/FieldResearch/AssignFieldResearcherRequestValidator.cs` | `Validators/AssignFieldResearcherRequestValidatorTests.cs` |  |
| ☐ | `CreateFieldEvidenceRequestValidator` | Validator | `Application/Validators/FieldResearch/CreateFieldEvidenceRequestValidator.cs` | `Validators/CreateFieldEvidenceRequestValidatorTests.cs` |  |
| ☐ | `CreateSurveyQuestionRequestValidator` | Validator | `Application/Validators/FieldResearch/CreateSurveyQuestionRequestValidator.cs` | `Validators/CreateSurveyQuestionRequestValidatorTests.cs` |  |
| ☐ | `CreateSurveyRequestValidator` | Validator | `Application/Validators/FieldResearch/CreateSurveyRequestValidator.cs` | `Validators/CreateSurveyRequestValidatorTests.cs` |  |
| ☐ | `CreateSurveyResponseRequestValidator` | Validator | `Application/Validators/FieldResearch/CreateSurveyResponseRequestValidator.cs` | `Validators/CreateSurveyResponseRequestValidatorTests.cs` |  |
| ☐ | `ReviewSurveyResponseRequestValidator` | Validator | `Application/Validators/FieldResearch/ReviewSurveyResponseRequestValidator.cs` | `Validators/ReviewSurveyResponseRequestValidatorTests.cs` |  |
| ☐ | `UpdateFieldAssignmentRequestValidator` | Validator | `Application/Validators/FieldResearch/UpdateFieldAssignmentRequestValidator.cs` | `Validators/UpdateFieldAssignmentRequestValidatorTests.cs` |  |
| ☐ | `UpdateFieldEvidenceRequestValidator` | Validator | `Application/Validators/FieldResearch/UpdateFieldEvidenceRequestValidator.cs` | `Validators/UpdateFieldEvidenceRequestValidatorTests.cs` |  |
| ☐ | `UpdateSurveyQuestionRequestValidator` | Validator | `Application/Validators/FieldResearch/UpdateSurveyQuestionRequestValidator.cs` | `Validators/UpdateSurveyQuestionRequestValidatorTests.cs` |  |
| ☐ | `UpdateSurveyRequestValidator` | Validator | `Application/Validators/FieldResearch/UpdateSurveyRequestValidator.cs` | `Validators/UpdateSurveyRequestValidatorTests.cs` |  |
| ☐ | `UpdateSurveyResponseRequestValidator` | Validator | `Application/Validators/FieldResearch/UpdateSurveyResponseRequestValidator.cs` | `Validators/UpdateSurveyResponseRequestValidatorTests.cs` |  |
| ☐ | `UpdateSurveyStatusRequestValidator` | Validator | `Application/Validators/FieldResearch/UpdateSurveyStatusRequestValidator.cs` | `Validators/UpdateSurveyStatusRequestValidatorTests.cs` |  |
| ☐ | `SurveyRepository` | Repository | `Data/Repositories/SurveyRepository.cs` | `Repositories/SurveyRepositoryTests.cs` | needs the test database |
| ☐ | `FieldEvidenceController` | Controller | `Api/Controllers/FieldEvidenceController.cs` | `Controllers/FieldEvidenceControllerTests.cs` |  |
| ☐ | `SurveyResponsesController` | Controller | `Api/Controllers/SurveyResponsesController.cs` | `Controllers/SurveyResponsesControllerTests.cs` |  |
| ☐ | `SurveysController` | Controller | `Api/Controllers/SurveysController.cs` | `Controllers/SurveysControllerTests.cs` |  |

<a id="be-governance"></a>

### Governance

**Folder:** `Features/Governance/` · **Units:** 69 · **Phase:** 4

Contains: 11 services, 32 validators, 5 infrastructure classes, 9 repositories, 12 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ArtisanSupportImpactService` | Service | `Application/Services/Governance/ArtisanSupportImpactService.cs` | `Services/ArtisanSupportImpactServiceTests.cs` |  |
| ☐ | `ArtisanSupportService` | Service | `Application/Services/Governance/ArtisanSupportService.cs` | `Services/ArtisanSupportServiceTests.cs` |  |
| ☐ | `ComplaintService` | Service | `Application/Services/Governance/ComplaintService.cs` | `Services/ComplaintServiceTests.cs` |  |
| ☐ | `ComplianceService` | Service | `Application/Services/Governance/ComplianceService.cs` | `Services/ComplianceServiceTests.cs` |  |
| ☐ | `FundingService` | Service | `Application/Services/Governance/FundingService.cs` | `Services/FundingServiceTests.cs` |  |
| ☐ | `GovForecastService` | Service | `Application/Services/Governance/GovForecastService.cs` | `Services/GovForecastServiceTests.cs` |  |
| ☐ | `GovReportService` | Service | `Application/Services/Governance/GovReportService.cs` | `Services/GovReportServiceTests.cs` |  |
| ☐ | `HeritageIntelligenceService` | Service | `Application/Services/Governance/HeritageIntelligenceService.cs` | `Services/HeritageIntelligenceServiceTests.cs` |  |
| ☐ | `MonitoringService` | Service | `Application/Services/Governance/MonitoringService.cs` | `Services/MonitoringServiceTests.cs` |  |
| ☐ | `NationalDashboardService` | Service | `Application/Services/Governance/NationalDashboardService.cs` | `Services/NationalDashboardServiceTests.cs` |  |
| ☐ | `PolicySimulationService` | Service | `Application/Services/Governance/PolicySimulationService.cs` | `Services/PolicySimulationServiceTests.cs` |  |
| ☐ | `AddComplaintUpdateRequestValidator` | Validator | `Application/Validators/Governance/ComplaintValidators.cs` | `Validators/AddComplaintUpdateRequestValidatorTests.cs` |  |
| ☐ | `AddFundingApplicationNoteRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/AddFundingApplicationNoteRequestValidatorTests.cs` |  |
| ☐ | `AddMonitoringFlagNoteRequestValidator` | Validator | `Application/Validators/Governance/MonitoringValidators.cs` | `Validators/AddMonitoringFlagNoteRequestValidatorTests.cs` |  |
| ☐ | `AssignComplaintRequestValidator` | Validator | `Application/Validators/Governance/ComplaintValidators.cs` | `Validators/AssignComplaintRequestValidatorTests.cs` |  |
| ☐ | `AssignMonitoringFlagRequestValidator` | Validator | `Application/Validators/Governance/MonitoringValidators.cs` | `Validators/AssignMonitoringFlagRequestValidatorTests.cs` |  |
| ☐ | `CompleteAnalyticsExportRequestValidator` | Validator | `Application/Validators/Governance/GovReportingValidators.cs` | `Validators/CompleteAnalyticsExportRequestValidatorTests.cs` |  |
| ☐ | `ComputeHeritageIndexRequestValidator` | Validator | `Application/Validators/Governance/ComputeHeritageIndexRequestValidator.cs` | `Validators/ComputeHeritageIndexRequestValidatorTests.cs` |  |
| ☐ | `CreateAnalyticsExportRequestValidator` | Validator | `Application/Validators/Governance/GovReportingValidators.cs` | `Validators/CreateAnalyticsExportRequestValidatorTests.cs` |  |
| ☐ | `CreateComplaintRequestValidator` | Validator | `Application/Validators/Governance/ComplaintValidators.cs` | `Validators/CreateComplaintRequestValidatorTests.cs` |  |
| ☐ | `CreateComplianceRecordRequestValidator` | Validator | `Application/Validators/Governance/ComplianceValidators.cs` | `Validators/CreateComplianceRecordRequestValidatorTests.cs` |  |
| ☐ | `CreateFundingApplicationRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/CreateFundingApplicationRequestValidatorTests.cs` |  |
| ☐ | `CreateFundingProgramRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/CreateFundingProgramRequestValidatorTests.cs` |  |
| ☐ | `CreateMonitoringFlagRequestValidator` | Validator | `Application/Validators/Governance/MonitoringValidators.cs` | `Validators/CreateMonitoringFlagRequestValidatorTests.cs` |  |
| ☐ | `CreateNationalDashboardSnapshotRequestValidator` | Validator | `Application/Validators/Governance/CreateNationalDashboardSnapshotRequestValidator.cs` | `Validators/CreateNationalDashboardSnapshotRequestValidatorTests.cs` |  |
| ☐ | `DecideFundingApplicationRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/DecideFundingApplicationRequestValidatorTests.cs` |  |
| ☐ | `GenerateGovForecastRequestValidator` | Validator | `Application/Validators/Governance/GovReportingValidators.cs` | `Validators/GenerateGovForecastRequestValidatorTests.cs` |  |
| ☐ | `GenerateGovReportRequestValidator` | Validator | `Application/Validators/Governance/GovReportingValidators.cs` | `Validators/GenerateGovReportRequestValidatorTests.cs` |  |
| ☐ | `LinkComplaintFlagRequestValidator` | Validator | `Application/Validators/Governance/ComplaintValidators.cs` | `Validators/LinkComplaintFlagRequestValidatorTests.cs` |  |
| ☐ | `RecordLoanRepaymentRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/RecordLoanRepaymentRequestValidatorTests.cs` |  |
| ☐ | `ResolveComplaintRequestValidator` | Validator | `Application/Validators/Governance/ComplaintValidators.cs` | `Validators/ResolveComplaintRequestValidatorTests.cs` |  |
| ☐ | `RunMonitoringScanRequestValidator` | Validator | `Application/Validators/Governance/MonitoringValidators.cs` | `Validators/RunMonitoringScanRequestValidatorTests.cs` |  |
| ☐ | `RunPolicySimulationRequestValidator` | Validator | `Application/Validators/Governance/RunPolicySimulationRequestValidator.cs` | `Validators/RunPolicySimulationRequestValidatorTests.cs` |  |
| ☐ | `ScheduleFundingDisbursementRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/ScheduleFundingDisbursementRequestValidatorTests.cs` |  |
| ☐ | `SubmitFundingReviewRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/SubmitFundingReviewRequestValidatorTests.cs` |  |
| ☐ | `UpdateComplaintRequestValidator` | Validator | `Application/Validators/Governance/ComplaintValidators.cs` | `Validators/UpdateComplaintRequestValidatorTests.cs` |  |
| ☐ | `UpdateComplianceRecordRequestValidator` | Validator | `Application/Validators/Governance/ComplianceValidators.cs` | `Validators/UpdateComplianceRecordRequestValidatorTests.cs` |  |
| ☐ | `UpdateFundingDisbursementStatusRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/UpdateFundingDisbursementStatusRequestValidatorTests.cs` |  |
| ☐ | `UpdateFundingProgramRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/UpdateFundingProgramRequestValidatorTests.cs` |  |
| ☐ | `UpdateGovReportRequestValidator` | Validator | `Application/Validators/Governance/GovReportingValidators.cs` | `Validators/UpdateGovReportRequestValidatorTests.cs` |  |
| ☐ | `UpdateMonitoringFlagStatusRequestValidator` | Validator | `Application/Validators/Governance/MonitoringValidators.cs` | `Validators/UpdateMonitoringFlagStatusRequestValidatorTests.cs` |  |
| ☐ | `UpsertComplianceRequirementRequestValidator` | Validator | `Application/Validators/Governance/ComplianceValidators.cs` | `Validators/UpsertComplianceRequirementRequestValidatorTests.cs` |  |
| ☐ | `WithdrawFundingApplicationRequestValidator` | Validator | `Application/Validators/Governance/FundingValidators.cs` | `Validators/WithdrawFundingApplicationRequestValidatorTests.cs` |  |
| ☐ | `GeminiProducerImpactProvider` | Infrastructure | `Infrastructure/ProducerImpact/GeminiProducerImpactProvider.cs` | `Infrastructure/GeminiProducerImpactProviderTests.cs` |  |
| ☐ | `RuleBasedGovForecastProvider` | Infrastructure | `Infrastructure/GovForecasting/RuleBasedGovForecastProvider.cs` | `Infrastructure/RuleBasedGovForecastProviderTests.cs` |  |
| ☐ | `RuleBasedHeritageIntelligenceProvider` | Infrastructure | `Infrastructure/HeritageIntelligence/RuleBasedHeritageIntelligenceProvider.cs` | `Infrastructure/RuleBasedHeritageIntelligenceProviderTests.cs` |  |
| ☐ | `RuleBasedPolicySimulationProvider` | Infrastructure | `Infrastructure/PolicySimulation/RuleBasedPolicySimulationProvider.cs` | `Infrastructure/RuleBasedPolicySimulationProviderTests.cs` |  |
| ☐ | `RuleBasedProducerImpactProvider` | Infrastructure | `Infrastructure/ProducerImpact/RuleBasedProducerImpactProvider.cs` | `Infrastructure/RuleBasedProducerImpactProviderTests.cs` |  |
| ☐ | `ArtisanSupportRepository` | Repository | `Data/Repositories/ArtisanSupportRepository.cs` | `Repositories/ArtisanSupportRepositoryTests.cs` | needs the test database |
| ☐ | `ComplaintRepository` | Repository | `Data/Repositories/ComplaintRepository.cs` | `Repositories/ComplaintRepositoryTests.cs` | needs the test database |
| ☐ | `ComplianceRepository` | Repository | `Data/Repositories/ComplianceRepository.cs` | `Repositories/ComplianceRepositoryTests.cs` | needs the test database |
| ☐ | `FundingRepository` | Repository | `Data/Repositories/FundingRepository.cs` | `Repositories/FundingRepositoryTests.cs` | needs the test database |
| ☐ | `GovAnalyticsRepository` | Repository | `Data/Repositories/GovAnalyticsRepository.cs` | `Repositories/GovAnalyticsRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageIntelligenceRepository` | Repository | `Data/Repositories/HeritageIntelligenceRepository.cs` | `Repositories/HeritageIntelligenceRepositoryTests.cs` | needs the test database |
| ☐ | `MonitoringRepository` | Repository | `Data/Repositories/MonitoringRepository.cs` | `Repositories/MonitoringRepositoryTests.cs` | needs the test database |
| ☐ | `NationalDashboardRepository` | Repository | `Data/Repositories/NationalDashboardRepository.cs` | `Repositories/NationalDashboardRepositoryTests.cs` | needs the test database |
| ☐ | `PolicySimulationRepository` | Repository | `Data/Repositories/PolicySimulationRepository.cs` | `Repositories/PolicySimulationRepositoryTests.cs` | needs the test database |
| ☐ | `ArtisanSupportController` | Controller | `Api/Controllers/ArtisanSupportController.cs` | `Controllers/ArtisanSupportControllerTests.cs` |  |
| ☐ | `ComplaintsController` | Controller | `Api/Controllers/ComplaintsController.cs` | `Controllers/ComplaintsControllerTests.cs` |  |
| ☐ | `ComplianceController` | Controller | `Api/Controllers/ComplianceController.cs` | `Controllers/ComplianceControllerTests.cs` |  |
| ☐ | `FundingApplicationsController` | Controller | `Api/Controllers/FundingApplicationsController.cs` | `Controllers/FundingApplicationsControllerTests.cs` |  |
| ☐ | `FundingProgramsController` | Controller | `Api/Controllers/FundingProgramsController.cs` | `Controllers/FundingProgramsControllerTests.cs` |  |
| ☐ | `GovAnalyticsController` | Controller | `Api/Controllers/GovAnalyticsController.cs` | `Controllers/GovAnalyticsControllerTests.cs` |  |
| ☐ | `GovForecastsController` | Controller | `Api/Controllers/GovForecastsController.cs` | `Controllers/GovForecastsControllerTests.cs` |  |
| ☐ | `GovReportsController` | Controller | `Api/Controllers/GovReportsController.cs` | `Controllers/GovReportsControllerTests.cs` |  |
| ☐ | `HeritageIntelligenceController` | Controller | `Api/Controllers/HeritageIntelligenceController.cs` | `Controllers/HeritageIntelligenceControllerTests.cs` |  |
| ☐ | `MonitoringController` | Controller | `Api/Controllers/MonitoringController.cs` | `Controllers/MonitoringControllerTests.cs` |  |
| ☐ | `NationalDashboardController` | Controller | `Api/Controllers/NationalDashboardController.cs` | `Controllers/NationalDashboardControllerTests.cs` |  |
| ☐ | `PolicySimulatorController` | Controller | `Api/Controllers/PolicySimulatorController.cs` | `Controllers/PolicySimulatorControllerTests.cs` |  |

<a id="be-heritageassistant"></a>

### HeritageAssistant

**Folder:** `Features/HeritageAssistant/` · **Units:** 5 · **Phase:** 5

Contains: 1 service, 1 validator, 2 infrastructure classes, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `HeritageAssistantService` | Service | `Application/Services/HeritageAssistant/HeritageAssistantService.cs` | `Services/HeritageAssistantServiceTests.cs` |  |
| ☐ | `AskHeritageAssistantRequestValidator` | Validator | `Application/Validators/HeritageAssistant/AskHeritageAssistantRequestValidator.cs` | `Validators/AskHeritageAssistantRequestValidatorTests.cs` |  |
| ☐ | `RagHeritageAssistantProvider` | Infrastructure | `Infrastructure/HeritageAssistant/RagHeritageAssistantProvider.cs` | `Infrastructure/RagHeritageAssistantProviderTests.cs` |  |
| ☐ | `RuleBasedHeritageAssistantProvider` | Infrastructure | `Infrastructure/HeritageAssistant/RuleBasedHeritageAssistantProvider.cs` | `Infrastructure/RuleBasedHeritageAssistantProviderTests.cs` |  |
| ☐ | `HeritageAssistantController` | Controller | `Api/Controllers/HeritageAssistantController.cs` | `Controllers/HeritageAssistantControllerTests.cs` |  |

<a id="be-heritagedatabase"></a>

### HeritageDatabase

**Folder:** `Features/HeritageDatabase/` · **Units:** 16 · **Phase:** 4

Contains: 3 services, 7 validators, 3 repositories, 3 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `HeritageDataService` | Service | `Application/Services/HeritageDatabase/HeritageDataService.cs` | `Services/HeritageDataServiceTests.cs` |  |
| ☐ | `HeritageDatasetService` | Service | `Application/Services/HeritageDatabase/HeritageDatasetService.cs` | `Services/HeritageDatasetServiceTests.cs` |  |
| ☐ | `HeritageRiskService` | Service | `Application/Services/HeritageDatabase/HeritageRiskService.cs` | `Services/HeritageRiskServiceTests.cs` |  |
| ☐ | `CreateHeritageDatasetExportRequestValidator` | Validator | `Application/Validators/HeritageDatabase/CreateHeritageDatasetExportRequestValidator.cs` | `Validators/CreateHeritageDatasetExportRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritageDatasetRequestValidator` | Validator | `Application/Validators/HeritageDatabase/CreateHeritageDatasetRequestValidator.cs` | `Validators/CreateHeritageDatasetRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritageDatasetVersionRequestValidator` | Validator | `Application/Validators/HeritageDatabase/CreateHeritageDatasetVersionRequestValidator.cs` | `Validators/CreateHeritageDatasetVersionRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritageRiskRecordRequestValidator` | Validator | `Application/Validators/HeritageDatabase/CreateHeritageRiskRecordRequestValidator.cs` | `Validators/CreateHeritageRiskRecordRequestValidatorTests.cs` |  |
| ☐ | `GrantHeritageDatasetAccessRequestValidator` | Validator | `Application/Validators/HeritageDatabase/GrantHeritageDatasetAccessRequestValidator.cs` | `Validators/GrantHeritageDatasetAccessRequestValidatorTests.cs` |  |
| ☐ | `UpdateHeritageDatasetRequestValidator` | Validator | `Application/Validators/HeritageDatabase/UpdateHeritageDatasetRequestValidator.cs` | `Validators/UpdateHeritageDatasetRequestValidatorTests.cs` |  |
| ☐ | `UpdateHeritageRiskRecordRequestValidator` | Validator | `Application/Validators/HeritageDatabase/UpdateHeritageRiskRecordRequestValidator.cs` | `Validators/UpdateHeritageRiskRecordRequestValidatorTests.cs` |  |
| ☐ | `HeritageDataRepository` | Repository | `Data/Repositories/HeritageDataRepository.cs` | `Repositories/HeritageDataRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageDatasetRepository` | Repository | `Data/Repositories/HeritageDatasetRepository.cs` | `Repositories/HeritageDatasetRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageRiskRepository` | Repository | `Data/Repositories/HeritageRiskRepository.cs` | `Repositories/HeritageRiskRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageDataController` | Controller | `Api/Controllers/HeritageDataController.cs` | `Controllers/HeritageDataControllerTests.cs` |  |
| ☐ | `HeritageDatasetsController` | Controller | `Api/Controllers/HeritageDatasetsController.cs` | `Controllers/HeritageDatasetsControllerTests.cs` |  |
| ☐ | `HeritageRiskController` | Controller | `Api/Controllers/HeritageRiskController.cs` | `Controllers/HeritageRiskControllerTests.cs` |  |

<a id="be-heritagediscovery"></a>

### HeritageDiscovery

**Folder:** `Features/HeritageDiscovery/` · **Units:** 37 · **Phase:** 5

Contains: 7 services, 15 validators, 7 repositories, 1 seeder, 7 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `CraftHeritageService` | Service | `Application/Services/HeritageDiscovery/CraftHeritageService.cs` | `Services/CraftHeritageServiceTests.cs` |  |
| ☐ | `CulturalEventService` | Service | `Application/Services/HeritageDiscovery/CulturalEventService.cs` | `Services/CulturalEventServiceTests.cs` |  |
| ☐ | `HeritageFestivalService` | Service | `Application/Services/HeritageDiscovery/HeritageFestivalService.cs` | `Services/HeritageFestivalServiceTests.cs` |  |
| ☐ | `HeritagePlaceService` | Service | `Application/Services/HeritageDiscovery/HeritagePlaceService.cs` | `Services/HeritagePlaceServiceTests.cs` |  |
| ☐ | `HeritageRouteService` | Service | `Application/Services/HeritageDiscovery/HeritageRouteService.cs` | `Services/HeritageRouteServiceTests.cs` |  |
| ☐ | `LocalCuisineService` | Service | `Application/Services/HeritageDiscovery/LocalCuisineService.cs` | `Services/LocalCuisineServiceTests.cs` |  |
| ☐ | `UnescoRecordService` | Service | `Application/Services/HeritageDiscovery/UnescoRecordService.cs` | `Services/UnescoRecordServiceTests.cs` |  |
| ☐ | `CreateCulturalEventRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/CreateCulturalEventRequestValidator.cs` | `Validators/CreateCulturalEventRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritageFestivalRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/CreateHeritageFestivalRequestValidator.cs` | `Validators/CreateHeritageFestivalRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritagePlaceRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/CreateHeritagePlaceRequestValidator.cs` | `Validators/CreateHeritagePlaceRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritageRouteRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/CreateHeritageRouteRequestValidator.cs` | `Validators/CreateHeritageRouteRequestValidatorTests.cs` |  |
| ☐ | `CreateLocalCuisineRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/CreateLocalCuisineRequestValidator.cs` | `Validators/CreateLocalCuisineRequestValidatorTests.cs` |  |
| ☐ | `CreateRouteStopRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/CreateRouteStopRequestValidator.cs` | `Validators/CreateRouteStopRequestValidatorTests.cs` |  |
| ☐ | `CreateUnescoRecordRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/UnescoRecordValidators.cs` | `Validators/CreateUnescoRecordRequestValidatorTests.cs` |  |
| ☐ | `ReorderStopsRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/ReorderStopsRequestValidator.cs` | `Validators/ReorderStopsRequestValidatorTests.cs` |  |
| ☐ | `SaveCraftHeritageEntryRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/CraftHeritageValidators.cs` | `Validators/SaveCraftHeritageEntryRequestValidatorTests.cs` |  |
| ☐ | `UpdateCulturalEventRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/UpdateCulturalEventRequestValidator.cs` | `Validators/UpdateCulturalEventRequestValidatorTests.cs` |  |
| ☐ | `UpdateHeritageFestivalRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/UpdateHeritageFestivalRequestValidator.cs` | `Validators/UpdateHeritageFestivalRequestValidatorTests.cs` |  |
| ☐ | `UpdateHeritagePlaceRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/UpdateHeritagePlaceRequestValidator.cs` | `Validators/UpdateHeritagePlaceRequestValidatorTests.cs` |  |
| ☐ | `UpdateHeritageRouteRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/UpdateHeritageRouteRequestValidator.cs` | `Validators/UpdateHeritageRouteRequestValidatorTests.cs` |  |
| ☐ | `UpdateLocalCuisineRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/UpdateLocalCuisineRequestValidator.cs` | `Validators/UpdateLocalCuisineRequestValidatorTests.cs` |  |
| ☐ | `UpdateUnescoRecordRequestValidator` | Validator | `Application/Validators/HeritageDiscovery/UnescoRecordValidators.cs` | `Validators/UpdateUnescoRecordRequestValidatorTests.cs` |  |
| ☐ | `CraftHeritageRepository` | Repository | `Data/Repositories/CraftHeritageRepository.cs` | `Repositories/CraftHeritageRepositoryTests.cs` | needs the test database |
| ☐ | `CulturalEventRepository` | Repository | `Data/Repositories/CulturalEventRepository.cs` | `Repositories/CulturalEventRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageFestivalRepository` | Repository | `Data/Repositories/HeritageFestivalRepository.cs` | `Repositories/HeritageFestivalRepositoryTests.cs` | needs the test database |
| ☐ | `HeritagePlaceRepository` | Repository | `Data/Repositories/HeritagePlaceRepository.cs` | `Repositories/HeritagePlaceRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageRouteRepository` | Repository | `Data/Repositories/HeritageRouteRepository.cs` | `Repositories/HeritageRouteRepositoryTests.cs` | needs the test database |
| ☐ | `LocalCuisineRepository` | Repository | `Data/Repositories/LocalCuisineRepository.cs` | `Repositories/LocalCuisineRepositoryTests.cs` | needs the test database |
| ☐ | `UnescoRecordRepository` | Repository | `Data/Repositories/UnescoRecordRepository.cs` | `Repositories/UnescoRecordRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageDiscoverySeeder` | Seeder | `Data/Seed/HeritageDiscoverySeeder.cs` | `Seed/HeritageDiscoverySeederTests.cs` |  |
| ☐ | `CraftHeritageController` | Controller | `Api/Controllers/CraftHeritageController.cs` | `Controllers/CraftHeritageControllerTests.cs` |  |
| ☐ | `CulturalEventsController` | Controller | `Api/Controllers/CulturalEventsController.cs` | `Controllers/CulturalEventsControllerTests.cs` |  |
| ☐ | `HeritageFestivalsController` | Controller | `Api/Controllers/HeritageFestivalsController.cs` | `Controllers/HeritageFestivalsControllerTests.cs` |  |
| ☐ | `HeritagePlacesController` | Controller | `Api/Controllers/HeritagePlacesController.cs` | `Controllers/HeritagePlacesControllerTests.cs` |  |
| ☐ | `HeritageRoutesController` | Controller | `Api/Controllers/HeritageRoutesController.cs` | `Controllers/HeritageRoutesControllerTests.cs` |  |
| ☐ | `LocalCuisinesController` | Controller | `Api/Controllers/LocalCuisinesController.cs` | `Controllers/LocalCuisinesControllerTests.cs` |  |
| ☐ | `UnescoRecordsController` | Controller | `Api/Controllers/UnescoRecordsController.cs` | `Controllers/UnescoRecordsControllerTests.cs` |  |

<a id="be-heritageidentity"></a>

### HeritageIdentity

**Folder:** `Features/HeritageIdentity/` · **Units:** 10 · **Phase:** 5

Contains: 1 service, 7 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `HeritageIdentityService` | Service | `Application/Services/HeritageIdentity/HeritageIdentityService.cs` | `Services/HeritageIdentityServiceTests.cs` |  |
| ☐ | `FamilyHeritageMemberInputValidator` | Validator | `Application/Validators/HeritageIdentity/FamilyHeritageMemberInputValidator.cs` | `Validators/FamilyHeritageMemberInputValidatorTests.cs` |  |
| ☐ | `HeritageAwardInputValidator` | Validator | `Application/Validators/HeritageIdentity/HeritageAwardInputValidator.cs` | `Validators/HeritageAwardInputValidatorTests.cs` |  |
| ☐ | `HeritageCertificationInputValidator` | Validator | `Application/Validators/HeritageIdentity/HeritageCertificationInputValidator.cs` | `Validators/HeritageCertificationInputValidatorTests.cs` |  |
| ☐ | `SkillTimelineEntryInputValidator` | Validator | `Application/Validators/HeritageIdentity/SkillTimelineEntryInputValidator.cs` | `Validators/SkillTimelineEntryInputValidatorTests.cs` |  |
| ☐ | `StoryArchiveEntryInputValidator` | Validator | `Application/Validators/HeritageIdentity/StoryArchiveEntryInputValidator.cs` | `Validators/StoryArchiveEntryInputValidatorTests.cs` |  |
| ☐ | `UpsertHeritageIdentityRequestValidator` | Validator | `Application/Validators/HeritageIdentity/UpsertHeritageIdentityRequestValidator.cs` | `Validators/UpsertHeritageIdentityRequestValidatorTests.cs` |  |
| ☐ | `VerifyHeritageIdentityRequestValidator` | Validator | `Application/Validators/HeritageIdentity/VerifyHeritageIdentityRequestValidator.cs` | `Validators/VerifyHeritageIdentityRequestValidatorTests.cs` |  |
| ☐ | `HeritageIdentityRepository` | Repository | `Data/Repositories/HeritageIdentityRepository.cs` | `Repositories/HeritageIdentityRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageIdentityController` | Controller | `Api/Controllers/HeritageIdentityController.cs` | `Controllers/HeritageIdentityControllerTests.cs` |  |

<a id="be-impact"></a>

### Impact

**Folder:** `Features/Impact/` · **Units:** 3 · **Phase:** 5

Contains: 1 service, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `ImpactService` | Service | `Application/Services/Impact/ImpactService.cs` | `Services/ImpactServiceTests.cs` |  |
| ✅ | `ImpactRepository` | Repository | `Data/Repositories/ImpactRepository.cs` | `Repositories/ImpactRepositoryTests.cs` | needs the test database |
| ✅ | `ImpactController` | Controller | `Api/Controllers/ImpactController.cs` | `Controllers/ImpactControllerTests.cs` |  |

<a id="be-innovation"></a>

### Innovation

**Folder:** `Features/Innovation/` · **Units:** 36 · **Phase:** 4

Contains: 4 services, 24 validators, 4 repositories, 4 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `HeritageInnovationSubmissionService` | Service | `Application/Services/Innovation/HeritageInnovationSubmissionService.cs` | `Services/HeritageInnovationSubmissionServiceTests.cs` |  |
| ☐ | `InnovationExperimentService` | Service | `Application/Services/Innovation/InnovationExperimentService.cs` | `Services/InnovationExperimentServiceTests.cs` |  |
| ☐ | `InnovationPrototypeService` | Service | `Application/Services/Innovation/InnovationPrototypeService.cs` | `Services/InnovationPrototypeServiceTests.cs` |  |
| ☐ | `PreservationStrategyService` | Service | `Application/Services/Innovation/PreservationStrategyService.cs` | `Services/PreservationStrategyServiceTests.cs` |  |
| ☐ | `AddSubmissionTeamMemberRequestValidator` | Validator | `Application/Validators/Innovation/AddSubmissionTeamMemberRequestValidator.cs` | `Validators/AddSubmissionTeamMemberRequestValidatorTests.cs` |  |
| ☐ | `CreateExperimentVersionRequestValidator` | Validator | `Application/Validators/Innovation/CreateExperimentVersionRequestValidator.cs` | `Validators/CreateExperimentVersionRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritageInnovationSubmissionRequestValidator` | Validator | `Application/Validators/Innovation/CreateHeritageInnovationSubmissionRequestValidator.cs` | `Validators/CreateHeritageInnovationSubmissionRequestValidatorTests.cs` |  |
| ☐ | `CreateInnovationExperimentRequestValidator` | Validator | `Application/Validators/Innovation/CreateInnovationExperimentRequestValidator.cs` | `Validators/CreateInnovationExperimentRequestValidatorTests.cs` |  |
| ☐ | `CreateInnovationPrototypeRequestValidator` | Validator | `Application/Validators/Innovation/CreateInnovationPrototypeRequestValidator.cs` | `Validators/CreateInnovationPrototypeRequestValidatorTests.cs` |  |
| ☐ | `CreatePreservationStrategyRequestValidator` | Validator | `Application/Validators/Innovation/CreatePreservationStrategyRequestValidator.cs` | `Validators/CreatePreservationStrategyRequestValidatorTests.cs` |  |
| ☐ | `CreatePrototypeIssueRequestValidator` | Validator | `Application/Validators/Innovation/CreatePrototypeIssueRequestValidator.cs` | `Validators/CreatePrototypeIssueRequestValidatorTests.cs` |  |
| ☐ | `CreatePrototypeIterationRequestValidator` | Validator | `Application/Validators/Innovation/CreatePrototypeIterationRequestValidator.cs` | `Validators/CreatePrototypeIterationRequestValidatorTests.cs` |  |
| ☐ | `CreatePrototypeTestCaseRequestValidator` | Validator | `Application/Validators/Innovation/CreatePrototypeTestCaseRequestValidator.cs` | `Validators/CreatePrototypeTestCaseRequestValidatorTests.cs` |  |
| ☐ | `CreatePrototypeTestRunRequestValidator` | Validator | `Application/Validators/Innovation/CreatePrototypeTestRunRequestValidator.cs` | `Validators/CreatePrototypeTestRunRequestValidatorTests.cs` |  |
| ☐ | `CreateStrategyActionRequestValidator` | Validator | `Application/Validators/Innovation/CreateStrategyActionRequestValidator.cs` | `Validators/CreateStrategyActionRequestValidatorTests.cs` |  |
| ☐ | `CreateStrategyObjectiveRequestValidator` | Validator | `Application/Validators/Innovation/CreateStrategyObjectiveRequestValidator.cs` | `Validators/CreateStrategyObjectiveRequestValidatorTests.cs` |  |
| ☐ | `CreateSubmissionReviewRequestValidator` | Validator | `Application/Validators/Innovation/CreateSubmissionReviewRequestValidator.cs` | `Validators/CreateSubmissionReviewRequestValidatorTests.cs` |  |
| ☐ | `CreateTrainingRunRequestValidator` | Validator | `Application/Validators/Innovation/CreateTrainingRunRequestValidator.cs` | `Validators/CreateTrainingRunRequestValidatorTests.cs` |  |
| ☐ | `UpdateHeritageInnovationSubmissionRequestValidator` | Validator | `Application/Validators/Innovation/UpdateHeritageInnovationSubmissionRequestValidator.cs` | `Validators/UpdateHeritageInnovationSubmissionRequestValidatorTests.cs` |  |
| ☐ | `UpdateInnovationExperimentRequestValidator` | Validator | `Application/Validators/Innovation/UpdateInnovationExperimentRequestValidator.cs` | `Validators/UpdateInnovationExperimentRequestValidatorTests.cs` |  |
| ☐ | `UpdateInnovationPrototypeRequestValidator` | Validator | `Application/Validators/Innovation/UpdateInnovationPrototypeRequestValidator.cs` | `Validators/UpdateInnovationPrototypeRequestValidatorTests.cs` |  |
| ☐ | `UpdatePreservationStrategyRequestValidator` | Validator | `Application/Validators/Innovation/UpdatePreservationStrategyRequestValidator.cs` | `Validators/UpdatePreservationStrategyRequestValidatorTests.cs` |  |
| ☐ | `UpdatePrototypeIssueRequestValidator` | Validator | `Application/Validators/Innovation/UpdatePrototypeIssueRequestValidator.cs` | `Validators/UpdatePrototypeIssueRequestValidatorTests.cs` |  |
| ☐ | `UpdatePrototypeTestCaseRequestValidator` | Validator | `Application/Validators/Innovation/UpdatePrototypeTestCaseRequestValidator.cs` | `Validators/UpdatePrototypeTestCaseRequestValidatorTests.cs` |  |
| ☐ | `UpdatePrototypeTestRunRequestValidator` | Validator | `Application/Validators/Innovation/UpdatePrototypeTestRunRequestValidator.cs` | `Validators/UpdatePrototypeTestRunRequestValidatorTests.cs` |  |
| ☐ | `UpdateStrategyActionRequestValidator` | Validator | `Application/Validators/Innovation/UpdateStrategyActionRequestValidator.cs` | `Validators/UpdateStrategyActionRequestValidatorTests.cs` |  |
| ☐ | `UpdateStrategyObjectiveRequestValidator` | Validator | `Application/Validators/Innovation/UpdateStrategyObjectiveRequestValidator.cs` | `Validators/UpdateStrategyObjectiveRequestValidatorTests.cs` |  |
| ☐ | `UpdateTrainingRunRequestValidator` | Validator | `Application/Validators/Innovation/UpdateTrainingRunRequestValidator.cs` | `Validators/UpdateTrainingRunRequestValidatorTests.cs` |  |
| ☐ | `HeritageInnovationSubmissionRepository` | Repository | `Data/Repositories/HeritageInnovationSubmissionRepository.cs` | `Repositories/HeritageInnovationSubmissionRepositoryTests.cs` | needs the test database |
| ☐ | `InnovationExperimentRepository` | Repository | `Data/Repositories/InnovationExperimentRepository.cs` | `Repositories/InnovationExperimentRepositoryTests.cs` | needs the test database |
| ☐ | `InnovationPrototypeRepository` | Repository | `Data/Repositories/InnovationPrototypeRepository.cs` | `Repositories/InnovationPrototypeRepositoryTests.cs` | needs the test database |
| ☐ | `PreservationStrategyRepository` | Repository | `Data/Repositories/PreservationStrategyRepository.cs` | `Repositories/PreservationStrategyRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageInnovationSubmissionsController` | Controller | `Api/Controllers/HeritageInnovationSubmissionsController.cs` | `Controllers/HeritageInnovationSubmissionsControllerTests.cs` |  |
| ☐ | `InnovationExperimentsController` | Controller | `Api/Controllers/InnovationExperimentsController.cs` | `Controllers/InnovationExperimentsControllerTests.cs` |  |
| ☐ | `InnovationPrototypesController` | Controller | `Api/Controllers/InnovationPrototypesController.cs` | `Controllers/InnovationPrototypesControllerTests.cs` |  |
| ☐ | `PreservationStrategiesController` | Controller | `Api/Controllers/PreservationStrategiesController.cs` | `Controllers/PreservationStrategiesControllerTests.cs` |  |

<a id="be-inventory"></a>

### Inventory

**Folder:** `Features/Inventory/` · **Units:** 4 · **Phase:** 2

Contains: 1 service, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `InventoryService` | Service | `Application/Services/Inventory/InventoryService.cs` | `Services/InventoryServiceTests.cs` |  |
| ✅ | `AdjustStockRequestValidator` | Validator | `Application/Validators/Inventory/AdjustStockRequestValidator.cs` | `Validators/AdjustStockRequestValidatorTests.cs` |  |
| ✅ | `InventoryRepository` | Repository | `Data/Repositories/InventoryRepository.cs` | `Repositories/InventoryRepositoryTests.cs` | needs the test database |
| ✅ | `InventoryController` | Controller | `Api/Controllers/InventoryController.cs` | `Controllers/InventoryControllerTests.cs` |  |

<a id="be-investment"></a>

### Investment

**Folder:** `Features/Investment/` · **Units:** 11 · **Phase:** 3

Contains: 1 service, 8 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `InvestmentService` | Service | `Application/Services/Investment/InvestmentService.cs` | `Services/InvestmentServiceTests.cs` |  |
| ☐ | `AddInvestmentDocumentRequestValidator` | Validator | `Application/Validators/Investment/AddInvestmentDocumentRequestValidator.cs` | `Validators/AddInvestmentDocumentRequestValidatorTests.cs` |  |
| ☐ | `CreateInvestmentOpportunityRequestValidator` | Validator | `Application/Validators/Investment/CreateInvestmentOpportunityRequestValidator.cs` | `Validators/CreateInvestmentOpportunityRequestValidatorTests.cs` |  |
| ☐ | `InvestmentMilestoneInputValidator` | Validator | `Application/Validators/Investment/InvestmentMilestoneInputValidator.cs` | `Validators/InvestmentMilestoneInputValidatorTests.cs` |  |
| ☐ | `InvestmentOpportunityQueryParametersValidator` | Validator | `Application/Validators/Investment/InvestmentOpportunityQueryParametersValidator.cs` | `Validators/InvestmentOpportunityQueryParametersValidatorTests.cs` |  |
| ☐ | `InvestmentProposalDecisionRequestValidator` | Validator | `Application/Validators/Investment/InvestmentProposalDecisionRequestValidator.cs` | `Validators/InvestmentProposalDecisionRequestValidatorTests.cs` |  |
| ☐ | `InvestmentProposalQueryParametersValidator` | Validator | `Application/Validators/Investment/InvestmentProposalQueryParametersValidator.cs` | `Validators/InvestmentProposalQueryParametersValidatorTests.cs` |  |
| ☐ | `SubmitInvestmentProposalRequestValidator` | Validator | `Application/Validators/Investment/SubmitInvestmentProposalRequestValidator.cs` | `Validators/SubmitInvestmentProposalRequestValidatorTests.cs` |  |
| ☐ | `UpdateInvestmentMilestoneStatusRequestValidator` | Validator | `Application/Validators/Investment/UpdateInvestmentMilestoneStatusRequestValidator.cs` | `Validators/UpdateInvestmentMilestoneStatusRequestValidatorTests.cs` |  |
| ☐ | `InvestmentRepository` | Repository | `Data/Repositories/InvestmentRepository.cs` | `Repositories/InvestmentRepositoryTests.cs` | needs the test database |
| ☐ | `InvestmentOpportunitiesController` | Controller | `Api/Controllers/InvestmentOpportunitiesController.cs` | `Controllers/InvestmentOpportunitiesControllerTests.cs` |  |

<a id="be-knowledgegraph"></a>

### KnowledgeGraph

**Folder:** `Features/KnowledgeGraph/` · **Units:** 8 · **Phase:** 4

Contains: 1 service, 5 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `KnowledgeGraphService` | Service | `Application/Services/KnowledgeGraph/KnowledgeGraphService.cs` | `Services/KnowledgeGraphServiceTests.cs` |  |
| ☐ | `CreateKnowledgeNodeRequestValidator` | Validator | `Application/Validators/KnowledgeGraph/CreateKnowledgeNodeRequestValidator.cs` | `Validators/CreateKnowledgeNodeRequestValidatorTests.cs` |  |
| ☐ | `CreateKnowledgeRelationshipRequestValidator` | Validator | `Application/Validators/KnowledgeGraph/CreateKnowledgeRelationshipRequestValidator.cs` | `Validators/CreateKnowledgeRelationshipRequestValidatorTests.cs` |  |
| ☐ | `ImportKnowledgeNodeRequestValidator` | Validator | `Application/Validators/KnowledgeGraph/ImportKnowledgeNodeRequestValidator.cs` | `Validators/ImportKnowledgeNodeRequestValidatorTests.cs` |  |
| ☐ | `UpdateKnowledgeNodeRequestValidator` | Validator | `Application/Validators/KnowledgeGraph/UpdateKnowledgeNodeRequestValidator.cs` | `Validators/UpdateKnowledgeNodeRequestValidatorTests.cs` |  |
| ☐ | `UpdateKnowledgeRelationshipRequestValidator` | Validator | `Application/Validators/KnowledgeGraph/UpdateKnowledgeRelationshipRequestValidator.cs` | `Validators/UpdateKnowledgeRelationshipRequestValidatorTests.cs` |  |
| ☐ | `KnowledgeGraphRepository` | Repository | `Data/Repositories/KnowledgeGraphRepository.cs` | `Repositories/KnowledgeGraphRepositoryTests.cs` | needs the test database |
| ☐ | `KnowledgeGraphController` | Controller | `Api/Controllers/KnowledgeGraphController.cs` | `Controllers/KnowledgeGraphControllerTests.cs` |  |

<a id="be-learning"></a>

### Learning

**Folder:** `Features/Learning/` · **Units:** 74 · **Phase:** 4

Contains: 11 services, 39 validators, 11 repositories, 11 controllers, 1 SignalR hub, 1 realtime notifier.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AcademyMemberProfileService` | Service | `Application/Services/Learning/AcademyMemberProfileService.cs` | `Services/AcademyMemberProfileServiceTests.cs` |  |
| ☐ | `AssignmentService` | Service | `Application/Services/Learning/AssignmentService.cs` | `Services/AssignmentServiceTests.cs` |  |
| ☐ | `CourseCategoryService` | Service | `Application/Services/Learning/CourseCategoryService.cs` | `Services/CourseCategoryServiceTests.cs` |  |
| ☐ | `CourseService` | Service | `Application/Services/Learning/CourseService.cs` | `Services/CourseServiceTests.cs` |  |
| ☐ | `EnrollmentService` | Service | `Application/Services/Learning/EnrollmentService.cs` | `Services/EnrollmentServiceTests.cs` |  |
| ☐ | `ExamService` | Service | `Application/Services/Learning/ExamService.cs` | `Services/ExamServiceTests.cs` |  |
| ☐ | `HeritageSkillService` | Service | `Application/Services/Learning/HeritageSkillService.cs` | `Services/HeritageSkillServiceTests.cs` |  |
| ☐ | `LiveClassService` | Service | `Application/Services/Learning/LiveClassService.cs` | `Services/LiveClassServiceTests.cs` |  |
| ☐ | `MentorService` | Service | `Application/Services/Learning/MentorService.cs` | `Services/MentorServiceTests.cs` |  |
| ☐ | `QuizService` | Service | `Application/Services/Learning/QuizService.cs` | `Services/QuizServiceTests.cs` |  |
| ☐ | `TrainingCertificateService` | Service | `Application/Services/Learning/TrainingCertificateService.cs` | `Services/TrainingCertificateServiceTests.cs` |  |
| ☐ | `AddMemberSkillRequestValidator` | Validator | `Application/Validators/Learning/AddMemberSkillRequestValidator.cs` | `Validators/AddMemberSkillRequestValidatorTests.cs` |  |
| ☐ | `AddMentorSkillRequestValidator` | Validator | `Application/Validators/Learning/AddMentorSkillRequestValidator.cs` | `Validators/AddMentorSkillRequestValidatorTests.cs` |  |
| ☐ | `AnswerQuestionRequestValidator` | Validator | `Application/Validators/LiveClass/AnswerQuestionRequestValidator.cs` | `Validators/AnswerQuestionRequestValidatorTests.cs` |  |
| ☐ | `AskQuestionRequestValidator` | Validator | `Application/Validators/LiveClass/AskQuestionRequestValidator.cs` | `Validators/AskQuestionRequestValidatorTests.cs` |  |
| ☐ | `BecomeMentorRequestValidator` | Validator | `Application/Validators/Learning/BecomeMentorRequestValidator.cs` | `Validators/BecomeMentorRequestValidatorTests.cs` |  |
| ☐ | `CreateAcademyMemberProfileRequestValidator` | Validator | `Application/Validators/Learning/CreateAcademyMemberProfileRequestValidator.cs` | `Validators/CreateAcademyMemberProfileRequestValidatorTests.cs` |  |
| ☐ | `CreateAssignmentRequestValidator` | Validator | `Application/Validators/Assessment/CreateAssignmentRequestValidator.cs` | `Validators/CreateAssignmentRequestValidatorTests.cs` |  |
| ☐ | `CreateCourseCategoryRequestValidator` | Validator | `Application/Validators/Learning/CreateCourseCategoryRequestValidator.cs` | `Validators/CreateCourseCategoryRequestValidatorTests.cs` |  |
| ☐ | `CreateCourseMaterialRequestValidator` | Validator | `Application/Validators/Learning/CreateCourseMaterialRequestValidator.cs` | `Validators/CreateCourseMaterialRequestValidatorTests.cs` |  |
| ☐ | `CreateCourseModuleRequestValidator` | Validator | `Application/Validators/Learning/CreateCourseModuleRequestValidator.cs` | `Validators/CreateCourseModuleRequestValidatorTests.cs` |  |
| ☐ | `CreateCourseRequestValidator` | Validator | `Application/Validators/Learning/CreateCourseRequestValidator.cs` | `Validators/CreateCourseRequestValidatorTests.cs` |  |
| ☐ | `CreateExamQuestionOptionRequestValidator` | Validator | `Application/Validators/Assessment/CreateExamQuestionOptionRequestValidator.cs` | `Validators/CreateExamQuestionOptionRequestValidatorTests.cs` |  |
| ☐ | `CreateExamQuestionRequestValidator` | Validator | `Application/Validators/Assessment/CreateExamQuestionRequestValidator.cs` | `Validators/CreateExamQuestionRequestValidatorTests.cs` |  |
| ☐ | `CreateExamRequestValidator` | Validator | `Application/Validators/Assessment/CreateExamRequestValidator.cs` | `Validators/CreateExamRequestValidatorTests.cs` |  |
| ☐ | `CreateHeritageSkillRequestValidator` | Validator | `Application/Validators/Learning/CreateHeritageSkillRequestValidator.cs` | `Validators/CreateHeritageSkillRequestValidatorTests.cs` |  |
| ☐ | `CreateLessonRequestValidator` | Validator | `Application/Validators/Learning/CreateLessonRequestValidator.cs` | `Validators/CreateLessonRequestValidatorTests.cs` |  |
| ☐ | `CreateLiveClassRequestValidator` | Validator | `Application/Validators/LiveClass/CreateLiveClassRequestValidator.cs` | `Validators/CreateLiveClassRequestValidatorTests.cs` |  |
| ☐ | `CreateQuizQuestionOptionRequestValidator` | Validator | `Application/Validators/Assessment/CreateQuizQuestionOptionRequestValidator.cs` | `Validators/CreateQuizQuestionOptionRequestValidatorTests.cs` |  |
| ☐ | `CreateQuizQuestionRequestValidator` | Validator | `Application/Validators/Assessment/CreateQuizQuestionRequestValidator.cs` | `Validators/CreateQuizQuestionRequestValidatorTests.cs` |  |
| ☐ | `CreateQuizRequestValidator` | Validator | `Application/Validators/Assessment/CreateQuizRequestValidator.cs` | `Validators/CreateQuizRequestValidatorTests.cs` |  |
| ☐ | `EvaluateExamAnswerRequestValidator` | Validator | `Application/Validators/Assessment/EvaluateExamAnswerRequestValidator.cs` | `Validators/EvaluateExamAnswerRequestValidatorTests.cs` |  |
| ☐ | `GradeAssignmentSubmissionRequestValidator` | Validator | `Application/Validators/Assessment/GradeAssignmentSubmissionRequestValidator.cs` | `Validators/GradeAssignmentSubmissionRequestValidatorTests.cs` |  |
| ☐ | `IssueSkillCertificateRequestValidator` | Validator | `Application/Validators/Learning/IssueSkillCertificateRequestValidator.cs` | `Validators/IssueSkillCertificateRequestValidatorTests.cs` |  |
| ☐ | `MarkLessonProgressRequestValidator` | Validator | `Application/Validators/Learning/MarkLessonProgressRequestValidator.cs` | `Validators/MarkLessonProgressRequestValidatorTests.cs` |  |
| ☐ | `SubmitAssignmentRequestValidator` | Validator | `Application/Validators/Assessment/SubmitAssignmentRequestValidator.cs` | `Validators/SubmitAssignmentRequestValidatorTests.cs` |  |
| ☐ | `SubmitExamAttemptRequestValidator` | Validator | `Application/Validators/Assessment/SubmitExamAttemptRequestValidator.cs` | `Validators/SubmitExamAttemptRequestValidatorTests.cs` |  |
| ☐ | `SubmitQuizAttemptRequestValidator` | Validator | `Application/Validators/Assessment/SubmitQuizAttemptRequestValidator.cs` | `Validators/SubmitQuizAttemptRequestValidatorTests.cs` |  |
| ☐ | `UpdateAcademyMemberProfileRequestValidator` | Validator | `Application/Validators/Learning/UpdateAcademyMemberProfileRequestValidator.cs` | `Validators/UpdateAcademyMemberProfileRequestValidatorTests.cs` |  |
| ☐ | `UpdateAssignmentRequestValidator` | Validator | `Application/Validators/Assessment/UpdateAssignmentRequestValidator.cs` | `Validators/UpdateAssignmentRequestValidatorTests.cs` |  |
| ☐ | `UpdateCourseModuleRequestValidator` | Validator | `Application/Validators/Learning/UpdateCourseModuleRequestValidator.cs` | `Validators/UpdateCourseModuleRequestValidatorTests.cs` |  |
| ☐ | `UpdateCourseRequestValidator` | Validator | `Application/Validators/Learning/UpdateCourseRequestValidator.cs` | `Validators/UpdateCourseRequestValidatorTests.cs` |  |
| ☐ | `UpdateExamQuestionRequestValidator` | Validator | `Application/Validators/Assessment/UpdateExamQuestionRequestValidator.cs` | `Validators/UpdateExamQuestionRequestValidatorTests.cs` |  |
| ☐ | `UpdateExamRequestValidator` | Validator | `Application/Validators/Assessment/UpdateExamRequestValidator.cs` | `Validators/UpdateExamRequestValidatorTests.cs` |  |
| ☐ | `UpdateLessonRequestValidator` | Validator | `Application/Validators/Learning/UpdateLessonRequestValidator.cs` | `Validators/UpdateLessonRequestValidatorTests.cs` |  |
| ☐ | `UpdateLiveClassRequestValidator` | Validator | `Application/Validators/LiveClass/UpdateLiveClassRequestValidator.cs` | `Validators/UpdateLiveClassRequestValidatorTests.cs` |  |
| ☐ | `UpdateMentorProfileRequestValidator` | Validator | `Application/Validators/Learning/UpdateMentorProfileRequestValidator.cs` | `Validators/UpdateMentorProfileRequestValidatorTests.cs` |  |
| ☐ | `UpdateQuizQuestionRequestValidator` | Validator | `Application/Validators/Assessment/UpdateQuizQuestionRequestValidator.cs` | `Validators/UpdateQuizQuestionRequestValidatorTests.cs` |  |
| ☐ | `UpdateQuizRequestValidator` | Validator | `Application/Validators/Assessment/UpdateQuizRequestValidator.cs` | `Validators/UpdateQuizRequestValidatorTests.cs` |  |
| ☐ | `VerifyTrainingCertificateRequestValidator` | Validator | `Application/Validators/Learning/VerifyTrainingCertificateRequestValidator.cs` | `Validators/VerifyTrainingCertificateRequestValidatorTests.cs` |  |
| ☐ | `AcademyMemberProfileRepository` | Repository | `Data/Repositories/AcademyMemberProfileRepository.cs` | `Repositories/AcademyMemberProfileRepositoryTests.cs` | needs the test database |
| ☐ | `AssignmentRepository` | Repository | `Data/Repositories/AssignmentRepository.cs` | `Repositories/AssignmentRepositoryTests.cs` | needs the test database |
| ☐ | `CourseCategoryRepository` | Repository | `Data/Repositories/CourseCategoryRepository.cs` | `Repositories/CourseCategoryRepositoryTests.cs` | needs the test database |
| ☐ | `CourseRepository` | Repository | `Data/Repositories/CourseRepository.cs` | `Repositories/CourseRepositoryTests.cs` | needs the test database |
| ☐ | `EnrollmentRepository` | Repository | `Data/Repositories/EnrollmentRepository.cs` | `Repositories/EnrollmentRepositoryTests.cs` | needs the test database |
| ☐ | `ExamRepository` | Repository | `Data/Repositories/ExamRepository.cs` | `Repositories/ExamRepositoryTests.cs` | needs the test database |
| ☐ | `HeritageSkillRepository` | Repository | `Data/Repositories/HeritageSkillRepository.cs` | `Repositories/HeritageSkillRepositoryTests.cs` | needs the test database |
| ☐ | `LiveClassRepository` | Repository | `Data/Repositories/LiveClassRepository.cs` | `Repositories/LiveClassRepositoryTests.cs` | needs the test database |
| ☐ | `MentorRepository` | Repository | `Data/Repositories/MentorRepository.cs` | `Repositories/MentorRepositoryTests.cs` | needs the test database |
| ☐ | `QuizRepository` | Repository | `Data/Repositories/QuizRepository.cs` | `Repositories/QuizRepositoryTests.cs` | needs the test database |
| ☐ | `TrainingCertificateRepository` | Repository | `Data/Repositories/TrainingCertificateRepository.cs` | `Repositories/TrainingCertificateRepositoryTests.cs` | needs the test database |
| ☐ | `AcademyMemberProfilesController` | Controller | `Api/Controllers/AcademyMemberProfilesController.cs` | `Controllers/AcademyMemberProfilesControllerTests.cs` |  |
| ☐ | `AssignmentsController` | Controller | `Api/Controllers/AssignmentsController.cs` | `Controllers/AssignmentsControllerTests.cs` |  |
| ☐ | `CourseCategoriesController` | Controller | `Api/Controllers/CourseCategoriesController.cs` | `Controllers/CourseCategoriesControllerTests.cs` |  |
| ☐ | `CoursesController` | Controller | `Api/Controllers/CoursesController.cs` | `Controllers/CoursesControllerTests.cs` |  |
| ☐ | `EnrollmentsController` | Controller | `Api/Controllers/EnrollmentsController.cs` | `Controllers/EnrollmentsControllerTests.cs` |  |
| ☐ | `ExamsController` | Controller | `Api/Controllers/ExamsController.cs` | `Controllers/ExamsControllerTests.cs` |  |
| ☐ | `HeritageSkillsController` | Controller | `Api/Controllers/HeritageSkillsController.cs` | `Controllers/HeritageSkillsControllerTests.cs` |  |
| ☐ | `LiveClassesController` | Controller | `Api/Controllers/LiveClassesController.cs` | `Controllers/LiveClassesControllerTests.cs` |  |
| ☐ | `MentorsController` | Controller | `Api/Controllers/MentorsController.cs` | `Controllers/MentorsControllerTests.cs` |  |
| ☐ | `QuizzesController` | Controller | `Api/Controllers/QuizzesController.cs` | `Controllers/QuizzesControllerTests.cs` |  |
| ☐ | `TrainingCertificatesController` | Controller | `Api/Controllers/TrainingCertificatesController.cs` | `Controllers/TrainingCertificatesControllerTests.cs` |  |
| ☐ | `LiveClassHub` | SignalR hub | `Api/Hubs/LiveClassHub.cs` | `Hubs/LiveClassHubTests.cs` |  |
| ☐ | `SignalRLiveClassNotifier` | Realtime notifier | `Api/Realtime/SignalRLiveClassNotifier.cs` | `Realtime/SignalRLiveClassNotifierTests.cs` |  |

<a id="be-liveshopping"></a>

### LiveShopping

**Folder:** `Features/LiveShopping/` · **Units:** 10 · **Phase:** 2

Contains: 1 service, 5 validators, 1 repository, 1 controller, 1 SignalR hub, 1 realtime notifier.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `LiveShoppingService` | Service | `Application/Services/LiveShopping/LiveShoppingService.cs` | `Services/LiveShoppingServiceTests.cs` |  |
| ☐ | `AddLiveCommentRequestValidator` | Validator | `Application/Validators/LiveShopping/AddLiveCommentRequestValidator.cs` | `Validators/AddLiveCommentRequestValidatorTests.cs` |  |
| ☐ | `AddLiveReactionRequestValidator` | Validator | `Application/Validators/LiveShopping/AddLiveReactionRequestValidator.cs` | `Validators/AddLiveReactionRequestValidatorTests.cs` |  |
| ☐ | `BuyDuringLiveRequestValidator` | Validator | `Application/Validators/LiveShopping/BuyDuringLiveRequestValidator.cs` | `Validators/BuyDuringLiveRequestValidatorTests.cs` |  |
| ☐ | `CreateLiveEventRequestValidator` | Validator | `Application/Validators/LiveShopping/CreateLiveEventRequestValidator.cs` | `Validators/CreateLiveEventRequestValidatorTests.cs` |  |
| ☐ | `LiveEventQueryParametersValidator` | Validator | `Application/Validators/LiveShopping/LiveEventQueryParametersValidator.cs` | `Validators/LiveEventQueryParametersValidatorTests.cs` |  |
| ☐ | `LiveShoppingRepository` | Repository | `Data/Repositories/LiveShoppingRepository.cs` | `Repositories/LiveShoppingRepositoryTests.cs` | needs the test database |
| ☐ | `LiveShoppingController` | Controller | `Api/Controllers/LiveShoppingController.cs` | `Controllers/LiveShoppingControllerTests.cs` |  |
| ☐ | `LiveEventHub` | SignalR hub | `Api/Hubs/LiveEventHub.cs` | `Hubs/LiveEventHubTests.cs` |  |
| ☐ | `SignalRLiveEventNotifier` | Realtime notifier | `Api/Realtime/SignalRLiveEventNotifier.cs` | `Realtime/SignalRLiveEventNotifierTests.cs` |  |

<a id="be-logistics"></a>

### Logistics

**Folder:** `Features/Logistics/` · **Units:** 88 · **Phase:** 3

Contains: 8 services, 59 validators, 4 infrastructure classes, 8 repositories, 9 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AiLogisticsService` | Service | `Application/Services/Logistics/AiLogisticsService.cs` | `Services/AiLogisticsServiceTests.cs` |  |
| ☐ | `DeliveryTrackingService` | Service | `Application/Services/Logistics/DeliveryTrackingService.cs` | `Services/DeliveryTrackingServiceTests.cs` |  |
| ☐ | `LogisticsPartnerService` | Service | `Application/Services/Logistics/LogisticsPartnerService.cs` | `Services/LogisticsPartnerServiceTests.cs` |  |
| ☐ | `PickupSchedulingService` | Service | `Application/Services/Logistics/PickupSchedulingService.cs` | `Services/PickupSchedulingServiceTests.cs` |  |
| ☐ | `ReturnHandlingService` | Service | `Application/Services/Logistics/ReturnHandlingService.cs` | `Services/ReturnHandlingServiceTests.cs` |  |
| ☐ | `RouteOptimizationService` | Service | `Application/Services/Logistics/RouteOptimizationService.cs` | `Services/RouteOptimizationServiceTests.cs` |  |
| ☐ | `WarehouseService` | Service | `Application/Services/Logistics/WarehouseService.cs` | `Services/WarehouseServiceTests.cs` |  |
| ☐ | `WarehouseStockService` | Service | `Application/Services/Logistics/WarehouseStockService.cs` | `Services/WarehouseStockServiceTests.cs` |  |
| ☐ | `AddPickupNoteRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/AddPickupNoteRequestValidatorTests.cs` |  |
| ☐ | `AddReturnNoteRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/AddReturnNoteRequestValidatorTests.cs` |  |
| ☐ | `AddRouteNoteRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/AddRouteNoteRequestValidatorTests.cs` |  |
| ☐ | `AddShipmentNoteRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/AddShipmentNoteRequestValidatorTests.cs` |  |
| ☐ | `AddShipmentTrackingEventRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/AddShipmentTrackingEventRequestValidatorTests.cs` |  |
| ☐ | `AdjustStockRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseStockValidators.cs` | `Validators/AdjustStockRequestValidatorTests.cs` |  |
| ☐ | `ApproveReturnRequestRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/ApproveReturnRequestRequestValidatorTests.cs` |  |
| ☐ | `AssignPickupRequestRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/AssignPickupRequestRequestValidatorTests.cs` |  |
| ☐ | `AssignRouteRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/AssignRouteRequestValidatorTests.cs` |  |
| ☐ | `CancelPickupRequestRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/CancelPickupRequestRequestValidatorTests.cs` |  |
| ☐ | `CancelReturnRequestRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/CancelReturnRequestRequestValidatorTests.cs` |  |
| ☐ | `CancelRouteRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/CancelRouteRequestValidatorTests.cs` |  |
| ☐ | `CancelShipmentRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/CancelShipmentRequestValidatorTests.cs` |  |
| ☐ | `CompleteRouteStopRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/CompleteRouteStopRequestValidatorTests.cs` |  |
| ☐ | `CreateDeliveryRouteRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/CreateDeliveryRouteRequestValidatorTests.cs` |  |
| ☐ | `CreatePickupRequestRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/CreatePickupRequestRequestValidatorTests.cs` |  |
| ☐ | `CreateReturnRequestRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/CreateReturnRequestRequestValidatorTests.cs` |  |
| ☐ | `CreateShipmentRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/CreateShipmentRequestValidatorTests.cs` |  |
| ☐ | `CreateWarehouseRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseValidators.cs` | `Validators/CreateWarehouseRequestValidatorTests.cs` |  |
| ☐ | `FailRouteStopRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/FailRouteStopRequestValidatorTests.cs` |  |
| ☐ | `ForecastDemandRequestValidator` | Validator | `Application/Validators/Logistics/AiLogisticsValidators.cs` | `Validators/ForecastDemandRequestValidatorTests.cs` |  |
| ☐ | `IssueStockRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseStockValidators.cs` | `Validators/IssueStockRequestValidatorTests.cs` |  |
| ☐ | `MarkShipmentDeliveredRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/MarkShipmentDeliveredRequestValidatorTests.cs` |  |
| ☐ | `OptimizeRouteAiRequestValidator` | Validator | `Application/Validators/Logistics/AiLogisticsValidators.cs` | `Validators/OptimizeRouteAiRequestValidatorTests.cs` |  |
| ☐ | `OptimizeRouteRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/OptimizeRouteRequestValidatorTests.cs` |  |
| ☐ | `PickupItemRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/PickupItemRequestValidatorTests.cs` |  |
| ☐ | `PredictDeliveryRequestValidator` | Validator | `Application/Validators/Logistics/AiLogisticsValidators.cs` | `Validators/PredictDeliveryRequestValidatorTests.cs` |  |
| ☐ | `ReceiveStockRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseStockValidators.cs` | `Validators/ReceiveStockRequestValidatorTests.cs` |  |
| ☐ | `RecommendWarehouseRequestValidator` | Validator | `Application/Validators/Logistics/AiLogisticsValidators.cs` | `Validators/RecommendWarehouseRequestValidatorTests.cs` |  |
| ☐ | `RecordDeliveryAttemptRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/RecordDeliveryAttemptRequestValidatorTests.cs` |  |
| ☐ | `RecordReturnInspectionRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/RecordReturnInspectionRequestValidatorTests.cs` |  |
| ☐ | `RecordReturnRefundRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/RecordReturnRefundRequestValidatorTests.cs` |  |
| ☐ | `RejectReturnRequestRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/RejectReturnRequestRequestValidatorTests.cs` |  |
| ☐ | `ResequenceRouteRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/ResequenceRouteRequestValidatorTests.cs` |  |
| ☐ | `ReserveStockRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseStockValidators.cs` | `Validators/ReserveStockRequestValidatorTests.cs` |  |
| ☐ | `RestockReturnItemInputValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/RestockReturnItemInputValidatorTests.cs` |  |
| ☐ | `RestockReturnRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/RestockReturnRequestValidatorTests.cs` |  |
| ☐ | `ReturnItemAssessmentInputValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/ReturnItemAssessmentInputValidatorTests.cs` |  |
| ☐ | `ReturnItemInputValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/ReturnItemInputValidatorTests.cs` |  |
| ☐ | `RouteStopInputValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/RouteStopInputValidatorTests.cs` |  |
| ☐ | `RouteTransitionRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/RouteTransitionRequestValidatorTests.cs` |  |
| ☐ | `SchedulePickupRequestRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/SchedulePickupRequestRequestValidatorTests.cs` |  |
| ☐ | `ScheduleReturnPickupRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/ScheduleReturnPickupRequestValidatorTests.cs` |  |
| ☐ | `TransferStockRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseStockValidators.cs` | `Validators/TransferStockRequestValidatorTests.cs` |  |
| ☐ | `UpdateDeliveryRouteRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/UpdateDeliveryRouteRequestValidatorTests.cs` |  |
| ☐ | `UpdatePickupRequestRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/UpdatePickupRequestRequestValidatorTests.cs` |  |
| ☐ | `UpdatePickupStatusRequestValidator` | Validator | `Application/Validators/Logistics/PickupSchedulingValidators.cs` | `Validators/UpdatePickupStatusRequestValidatorTests.cs` |  |
| ☐ | `UpdateReturnRequestRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/UpdateReturnRequestRequestValidatorTests.cs` |  |
| ☐ | `UpdateReturnStatusRequestValidator` | Validator | `Application/Validators/Logistics/ReturnHandlingValidators.cs` | `Validators/UpdateReturnStatusRequestValidatorTests.cs` |  |
| ☐ | `UpdateRouteStopRequestValidator` | Validator | `Application/Validators/Logistics/RouteOptimizationValidators.cs` | `Validators/UpdateRouteStopRequestValidatorTests.cs` |  |
| ☐ | `UpdateShipmentLocationRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/UpdateShipmentLocationRequestValidatorTests.cs` |  |
| ☐ | `UpdateShipmentRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/UpdateShipmentRequestValidatorTests.cs` |  |
| ☐ | `UpdateShipmentStatusRequestValidator` | Validator | `Application/Validators/Logistics/DeliveryTrackingValidators.cs` | `Validators/UpdateShipmentStatusRequestValidatorTests.cs` |  |
| ☐ | `UpdateWarehouseRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseValidators.cs` | `Validators/UpdateWarehouseRequestValidatorTests.cs` |  |
| ☐ | `UpsertLogisticsPartnerProfileRequestValidator` | Validator | `Application/Validators/Logistics/LogisticsPartnerValidators.cs` | `Validators/UpsertLogisticsPartnerProfileRequestValidatorTests.cs` |  |
| ☐ | `UpsertLogisticsServiceAreaRequestValidator` | Validator | `Application/Validators/Logistics/LogisticsPartnerValidators.cs` | `Validators/UpsertLogisticsServiceAreaRequestValidatorTests.cs` |  |
| ☐ | `UpsertWarehouseBinRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseValidators.cs` | `Validators/UpsertWarehouseBinRequestValidatorTests.cs` |  |
| ☐ | `UpsertWarehouseZoneRequestValidator` | Validator | `Application/Validators/Logistics/WarehouseValidators.cs` | `Validators/UpsertWarehouseZoneRequestValidatorTests.cs` |  |
| ☐ | `VerifyLogisticsPartnerRequestValidator` | Validator | `Application/Validators/Logistics/LogisticsPartnerValidators.cs` | `Validators/VerifyLogisticsPartnerRequestValidatorTests.cs` |  |
| ☐ | `RuleBasedAiRouteOptimizationProvider` | Infrastructure | `Infrastructure/AILogistics/RuleBasedAiRouteOptimizationProvider.cs` | `Infrastructure/RuleBasedAiRouteOptimizationProviderTests.cs` |  |
| ☐ | `RuleBasedDeliveryPredictionProvider` | Infrastructure | `Infrastructure/AILogistics/RuleBasedDeliveryPredictionProvider.cs` | `Infrastructure/RuleBasedDeliveryPredictionProviderTests.cs` |  |
| ☐ | `RuleBasedDemandForecastProvider` | Infrastructure | `Infrastructure/AILogistics/RuleBasedDemandForecastProvider.cs` | `Infrastructure/RuleBasedDemandForecastProviderTests.cs` |  |
| ☐ | `RuleBasedWarehouseAllocationProvider` | Infrastructure | `Infrastructure/AILogistics/RuleBasedWarehouseAllocationProvider.cs` | `Infrastructure/RuleBasedWarehouseAllocationProviderTests.cs` |  |
| ☐ | `AiLogisticsRepository` | Repository | `Data/Repositories/AiLogisticsRepository.cs` | `Repositories/AiLogisticsRepositoryTests.cs` | needs the test database |
| ☐ | `DeliveryTrackingRepository` | Repository | `Data/Repositories/DeliveryTrackingRepository.cs` | `Repositories/DeliveryTrackingRepositoryTests.cs` | needs the test database |
| ☐ | `LogisticsPartnerRepository` | Repository | `Data/Repositories/LogisticsPartnerRepository.cs` | `Repositories/LogisticsPartnerRepositoryTests.cs` | needs the test database |
| ☐ | `PickupRequestRepository` | Repository | `Data/Repositories/PickupRequestRepository.cs` | `Repositories/PickupRequestRepositoryTests.cs` | needs the test database |
| ☐ | `ReturnHandlingRepository` | Repository | `Data/Repositories/ReturnHandlingRepository.cs` | `Repositories/ReturnHandlingRepositoryTests.cs` | needs the test database |
| ☐ | `RouteOptimizationRepository` | Repository | `Data/Repositories/RouteOptimizationRepository.cs` | `Repositories/RouteOptimizationRepositoryTests.cs` | needs the test database |
| ☐ | `WarehouseRepository` | Repository | `Data/Repositories/WarehouseRepository.cs` | `Repositories/WarehouseRepositoryTests.cs` | needs the test database |
| ☐ | `WarehouseStockRepository` | Repository | `Data/Repositories/WarehouseStockRepository.cs` | `Repositories/WarehouseStockRepositoryTests.cs` | needs the test database |
| ☐ | `AiLogisticsController` | Controller | `Api/Controllers/AiLogisticsController.cs` | `Controllers/AiLogisticsControllerTests.cs` |  |
| ☐ | `DeliveryRoutesController` | Controller | `Api/Controllers/DeliveryRoutesController.cs` | `Controllers/DeliveryRoutesControllerTests.cs` |  |
| ☐ | `LogisticsDirectoryController` | Controller | `Api/Controllers/LogisticsDirectoryController.cs` | `Controllers/LogisticsDirectoryControllerTests.cs` |  |
| ☐ | `LogisticsPartnersController` | Controller | `Api/Controllers/LogisticsPartnersController.cs` | `Controllers/LogisticsPartnersControllerTests.cs` |  |
| ☐ | `PickupRequestsController` | Controller | `Api/Controllers/PickupRequestsController.cs` | `Controllers/PickupRequestsControllerTests.cs` |  |
| ☐ | `ReturnsController` | Controller | `Api/Controllers/ReturnsController.cs` | `Controllers/ReturnsControllerTests.cs` |  |
| ☐ | `ShipmentsController` | Controller | `Api/Controllers/ShipmentsController.cs` | `Controllers/ShipmentsControllerTests.cs` |  |
| ☐ | `WarehousesController` | Controller | `Api/Controllers/WarehousesController.cs` | `Controllers/WarehousesControllerTests.cs` |  |
| ☐ | `WarehouseStockController` | Controller | `Api/Controllers/WarehouseStockController.cs` | `Controllers/WarehouseStockControllerTests.cs` |  |

<a id="be-manufacturingpartnership"></a>

### ManufacturingPartnership

**Folder:** `Features/ManufacturingPartnership/` · **Units:** 8 · **Phase:** 3

Contains: 1 service, 5 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `PartnershipService` | Service | `Application/Services/ManufacturingPartnership/PartnershipService.cs` | `Services/PartnershipServiceTests.cs` |  |
| ☐ | `CreatePartnershipRequestValidator` | Validator | `Application/Validators/ManufacturingPartnership/CreatePartnershipRequestValidator.cs` | `Validators/CreatePartnershipRequestValidatorTests.cs` |  |
| ☐ | `MilestoneInputValidator` | Validator | `Application/Validators/ManufacturingPartnership/MilestoneInputValidator.cs` | `Validators/MilestoneInputValidatorTests.cs` |  |
| ☐ | `PartnershipQueryParametersValidator` | Validator | `Application/Validators/ManufacturingPartnership/PartnershipQueryParametersValidator.cs` | `Validators/PartnershipQueryParametersValidatorTests.cs` |  |
| ☐ | `PartnershipResponseRequestValidator` | Validator | `Application/Validators/ManufacturingPartnership/PartnershipResponseRequestValidator.cs` | `Validators/PartnershipResponseRequestValidatorTests.cs` |  |
| ☐ | `UpdateMilestoneStatusRequestValidator` | Validator | `Application/Validators/ManufacturingPartnership/UpdateMilestoneStatusRequestValidator.cs` | `Validators/UpdateMilestoneStatusRequestValidatorTests.cs` |  |
| ☐ | `PartnershipRepository` | Repository | `Data/Repositories/PartnershipRepository.cs` | `Repositories/PartnershipRepositoryTests.cs` | needs the test database |
| ☐ | `ManufacturingPartnershipsController` | Controller | `Api/Controllers/ManufacturingPartnershipsController.cs` | `Controllers/ManufacturingPartnershipsControllerTests.cs` |  |

<a id="be-marketplace"></a>

### Marketplace

**Folder:** `Features/Marketplace/` · **Units:** 36 · **Phase:** 2

Contains: 6 services, 17 validators, 6 repositories, 1 seeder, 6 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `CategoryService` | Service | `Application/Services/Marketplace/CategoryService.cs` | `Services/CategoryServiceTests.cs` |  |
| ☐ | `CraftStoryService` | Service | `Application/Services/Marketplace/CraftStoryService.cs` | `Services/CraftStoryServiceTests.cs` |  |
| ☐ | `DistrictService` | Service | `Application/Services/Marketplace/DistrictService.cs` | `Services/DistrictServiceTests.cs` |  |
| ☐ | `ProducerStoryService` | Service | `Application/Services/Marketplace/ProducerStoryService.cs` | `Services/ProducerStoryServiceTests.cs` |  |
| ☐ | `ProductService` | Service | `Application/Services/Marketplace/ProductService.cs` | `Services/ProductServiceTests.cs` |  |
| ☐ | `WorkshopGalleryService` | Service | `Application/Services/Marketplace/WorkshopGalleryService.cs` | `Services/WorkshopGalleryServiceTests.cs` |  |
| ☐ | `BulkCreateProductsRequestValidator` | Validator | `Application/Validators/Marketplace/BulkCreateProductsRequestValidator.cs` | `Validators/BulkCreateProductsRequestValidatorTests.cs` |  |
| ☐ | `CreateCategoryRequestValidator` | Validator | `Application/Validators/Marketplace/CreateCategoryRequestValidator.cs` | `Validators/CreateCategoryRequestValidatorTests.cs` |  |
| ☐ | `CreateCraftStoryRequestValidator` | Validator | `Application/Validators/Marketplace/CreateCraftStoryRequestValidator.cs` | `Validators/CreateCraftStoryRequestValidatorTests.cs` |  |
| ☐ | `CreateProductRequestValidator` | Validator | `Application/Validators/Marketplace/CreateProductRequestValidator.cs` | `Validators/CreateProductRequestValidatorTests.cs` |  |
| ☐ | `CreateProductVariantRequestValidator` | Validator | `Application/Validators/Marketplace/CreateProductVariantRequestValidator.cs` | `Validators/CreateProductVariantRequestValidatorTests.cs` |  |
| ☐ | `CreateProductVideoRequestValidator` | Validator | `Application/Validators/Marketplace/CreateProductVideoRequestValidator.cs` | `Validators/CreateProductVideoRequestValidatorTests.cs` |  |
| ☐ | `CreateWorkshopGalleryItemRequestValidator` | Validator | `Application/Validators/Marketplace/CreateWorkshopGalleryItemRequestValidator.cs` | `Validators/CreateWorkshopGalleryItemRequestValidatorTests.cs` |  |
| ☐ | `ProductQueryParametersValidator` | Validator | `Application/Validators/Marketplace/ProductQueryParametersValidator.cs` | `Validators/ProductQueryParametersValidatorTests.cs` |  |
| ☐ | `SetHandmadeVerificationRequestValidator` | Validator | `Application/Validators/Marketplace/SetHandmadeVerificationRequestValidator.cs` | `Validators/SetHandmadeVerificationRequestValidatorTests.cs` |  |
| ☐ | `StoryChapterInputValidator` | Validator | `Application/Validators/Marketplace/StoryChapterInputValidator.cs` | `Validators/StoryChapterInputValidatorTests.cs` |  |
| ☐ | `UpdateCategoryRequestValidator` | Validator | `Application/Validators/Marketplace/UpdateCategoryRequestValidator.cs` | `Validators/UpdateCategoryRequestValidatorTests.cs` |  |
| ☐ | `UpdateCraftStoryRequestValidator` | Validator | `Application/Validators/Marketplace/UpdateCraftStoryRequestValidator.cs` | `Validators/UpdateCraftStoryRequestValidatorTests.cs` |  |
| ☐ | `UpdateDistrictRequestValidator` | Validator | `Application/Validators/Marketplace/UpdateDistrictRequestValidator.cs` | `Validators/UpdateDistrictRequestValidatorTests.cs` |  |
| ☐ | `UpdateProductRequestValidator` | Validator | `Application/Validators/Marketplace/UpdateProductRequestValidator.cs` | `Validators/UpdateProductRequestValidatorTests.cs` |  |
| ☐ | `UpdateProductVariantRequestValidator` | Validator | `Application/Validators/Marketplace/UpdateProductVariantRequestValidator.cs` | `Validators/UpdateProductVariantRequestValidatorTests.cs` |  |
| ☐ | `UpdateProductVideoRequestValidator` | Validator | `Application/Validators/Marketplace/UpdateProductVideoRequestValidator.cs` | `Validators/UpdateProductVideoRequestValidatorTests.cs` |  |
| ☐ | `UpsertProducerStoryRequestValidator` | Validator | `Application/Validators/Marketplace/UpsertProducerStoryRequestValidator.cs` | `Validators/UpsertProducerStoryRequestValidatorTests.cs` |  |
| ☐ | `CategoryRepository` | Repository | `Data/Repositories/CategoryRepository.cs` | `Repositories/CategoryRepositoryTests.cs` | needs the test database |
| ☐ | `CraftStoryRepository` | Repository | `Data/Repositories/CraftStoryRepository.cs` | `Repositories/CraftStoryRepositoryTests.cs` | needs the test database |
| ☐ | `DistrictRepository` | Repository | `Data/Repositories/DistrictRepository.cs` | `Repositories/DistrictRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerStoryRepository` | Repository | `Data/Repositories/ProducerStoryRepository.cs` | `Repositories/ProducerStoryRepositoryTests.cs` | needs the test database |
| ☐ | `ProductRepository` | Repository | `Data/Repositories/ProductRepository.cs` | `Repositories/ProductRepositoryTests.cs` | needs the test database |
| ☐ | `WorkshopGalleryRepository` | Repository | `Data/Repositories/WorkshopGalleryRepository.cs` | `Repositories/WorkshopGalleryRepositoryTests.cs` | needs the test database |
| ☐ | `MarketplaceReferenceDataSeeder` | Seeder | `Data/Seed/MarketplaceReferenceDataSeeder.cs` | `Seed/MarketplaceReferenceDataSeederTests.cs` |  |
| ☐ | `CategoriesController` | Controller | `Api/Controllers/CategoriesController.cs` | `Controllers/CategoriesControllerTests.cs` |  |
| ☐ | `CraftStoriesController` | Controller | `Api/Controllers/CraftStoriesController.cs` | `Controllers/CraftStoriesControllerTests.cs` |  |
| ☐ | `DistrictsController` | Controller | `Api/Controllers/DistrictsController.cs` | `Controllers/DistrictsControllerTests.cs` |  |
| ☐ | `ProducerStoriesController` | Controller | `Api/Controllers/ProducerStoriesController.cs` | `Controllers/ProducerStoriesControllerTests.cs` |  |
| ☐ | `ProductsController` | Controller | `Api/Controllers/ProductsController.cs` | `Controllers/ProductsControllerTests.cs` |  |
| ☐ | `WorkshopGalleryController` | Controller | `Api/Controllers/WorkshopGalleryController.cs` | `Controllers/WorkshopGalleryControllerTests.cs` |  |

<a id="be-mentormatching"></a>

### MentorMatching

**Folder:** `Features/MentorMatching/` · **Units:** 4 · **Phase:** 4

Contains: 1 service, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `MentorMatchingService` | Service | `Application/Services/MentorMatching/MentorMatchingService.cs` | `Services/MentorMatchingServiceTests.cs` |  |
| ☐ | `MentorMatchRequestValidator` | Validator | `Application/Validators/MentorMatching/MentorMatchRequestValidator.cs` | `Validators/MentorMatchRequestValidatorTests.cs` |  |
| ☐ | `MentorMatchingRepository` | Repository | `Data/Repositories/MentorMatchingRepository.cs` | `Repositories/MentorMatchingRepositoryTests.cs` | needs the test database |
| ☐ | `MentorMatchingController` | Controller | `Api/Controllers/MentorMatchingController.cs` | `Controllers/MentorMatchingControllerTests.cs` |  |

<a id="be-mentorship"></a>

### Mentorship

**Folder:** `Features/Mentorship/` · **Units:** 5 · **Phase:** 4

Contains: 1 service, 2 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `MentorshipService` | Service | `Application/Services/Mentorship/MentorshipService.cs` | `Services/MentorshipServiceTests.cs` |  |
| ☐ | `CreateMentorshipRequestRequestValidator` | Validator | `Application/Validators/Mentorship/CreateMentorshipRequestRequestValidator.cs` | `Validators/CreateMentorshipRequestRequestValidatorTests.cs` |  |
| ☐ | `RespondMentorshipRequestRequestValidator` | Validator | `Application/Validators/Mentorship/RespondMentorshipRequestRequestValidator.cs` | `Validators/RespondMentorshipRequestRequestValidatorTests.cs` |  |
| ☐ | `MentorshipRequestRepository` | Repository | `Data/Repositories/MentorshipRequestRepository.cs` | `Repositories/MentorshipRequestRepositoryTests.cs` | needs the test database |
| ☐ | `MentorshipRequestsController` | Controller | `Api/Controllers/MentorshipRequestsController.cs` | `Controllers/MentorshipRequestsControllerTests.cs` |  |

<a id="be-messaging"></a>

### Messaging

**Folder:** `Features/Messaging/` · **Units:** 9 · **Phase:** 5

Contains: 1 service, 3 validators, 1 repository, 2 controllers, 1 SignalR hub, 1 realtime notifier.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `MessagingService` | Service | `Application/Services/Messaging/MessagingService.cs` | `Services/MessagingServiceTests.cs` |  |
| ☐ | `ConversationQueryParametersValidator` | Validator | `Application/Validators/Messaging/ConversationQueryParametersValidator.cs` | `Validators/ConversationQueryParametersValidatorTests.cs` |  |
| ☐ | `SendMessageRequestValidator` | Validator | `Application/Validators/Messaging/SendMessageRequestValidator.cs` | `Validators/SendMessageRequestValidatorTests.cs` |  |
| ☐ | `StartConversationRequestValidator` | Validator | `Application/Validators/Messaging/StartConversationRequestValidator.cs` | `Validators/StartConversationRequestValidatorTests.cs` |  |
| ☐ | `MessagingRepository` | Repository | `Data/Repositories/MessagingRepository.cs` | `Repositories/MessagingRepositoryTests.cs` | needs the test database |
| ☐ | `ChatMediaController` | Controller | `Api/Controllers/ChatMediaController.cs` | `Controllers/ChatMediaControllerTests.cs` |  |
| ☐ | `MessagingController` | Controller | `Api/Controllers/MessagingController.cs` | `Controllers/MessagingControllerTests.cs` |  |
| ☐ | `MessagingHub` | SignalR hub | `Api/Hubs/MessagingHub.cs` | `Hubs/MessagingHubTests.cs` |  |
| ☐ | `SignalRMessageNotifier` | Realtime notifier | `Api/Realtime/SignalRMessageNotifier.cs` | `Realtime/SignalRMessageNotifierTests.cs` |  |

<a id="be-notifications"></a>

### Notifications

**Folder:** `Features/Notifications/` · **Units:** 2 · **Phase:** 1 · **Partly tested already:** 1

Contains: 1 DbContext (notifications), 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `ShilpoHubDbContext.Notifications` | DbContext (notifications) | `Data/ShilpoHubDbContext.Notifications.cs` | `Data/ShilpoHubDbContextNotificationsTests.cs` | partly tested by `backend/tests/NotificationsRegression`; notification rows created on SaveChanges |
| ✅ | `NotificationsController` | Controller | `Api/Controllers/NotificationsController.cs` | `Controllers/NotificationsControllerTests.cs` | queries ShilpoHubDbContext directly (Postgres ILike): needs the test database |

<a id="be-passport"></a>

### Passport

**Folder:** `Features/Passport/` · **Units:** 11 · **Phase:** 4

Contains: 1 service, 6 validators, 3 repositories, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `PassportService` | Service | `Application/Services/Passport/PassportService.cs` | `Services/PassportServiceTests.cs` |  |
| ☐ | `ClaimDistrictBadgeRequestValidator` | Validator | `Application/Validators/Passport/ClaimDistrictBadgeRequestValidator.cs` | `Validators/ClaimDistrictBadgeRequestValidatorTests.cs` |  |
| ☐ | `ClaimFestivalBadgeRequestValidator` | Validator | `Application/Validators/Passport/ClaimFestivalBadgeRequestValidator.cs` | `Validators/ClaimFestivalBadgeRequestValidatorTests.cs` |  |
| ☐ | `CreateBadgeRequestValidator` | Validator | `Application/Validators/Passport/CreateBadgeRequestValidator.cs` | `Validators/CreateBadgeRequestValidatorTests.cs` |  |
| ☐ | `CreateCheckInRequestValidator` | Validator | `Application/Validators/Passport/CreateCheckInRequestValidator.cs` | `Validators/CreateCheckInRequestValidatorTests.cs` |  |
| ☐ | `CreateJournalEntryRequestValidator` | Validator | `Application/Validators/Passport/CreateJournalEntryRequestValidator.cs` | `Validators/CreateJournalEntryRequestValidatorTests.cs` |  |
| ☐ | `UpdateJournalEntryRequestValidator` | Validator | `Application/Validators/Passport/UpdateJournalEntryRequestValidator.cs` | `Validators/UpdateJournalEntryRequestValidatorTests.cs` |  |
| ☐ | `HeritageCheckInRepository` | Repository | `Data/Repositories/HeritageCheckInRepository.cs` | `Repositories/HeritageCheckInRepositoryTests.cs` | needs the test database |
| ☐ | `PassportRepository` | Repository | `Data/Repositories/PassportRepository.cs` | `Repositories/PassportRepositoryTests.cs` | needs the test database |
| ☐ | `TravelJournalRepository` | Repository | `Data/Repositories/TravelJournalRepository.cs` | `Repositories/TravelJournalRepositoryTests.cs` | needs the test database |
| ☐ | `PassportController` | Controller | `Api/Controllers/PassportController.cs` | `Controllers/PassportControllerTests.cs` |  |

<a id="be-platform"></a>

### Platform

**Folder:** `Features/Platform/` · **Units:** 10 · **Phase:** 1 · **Partly tested already:** 1

Contains: 1 DbContext, 1 controller, 2 middleware, 2 JSON converters, 1 helper, 3 DI registrations.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `ShilpoHubDbContext` | DbContext | `Data/ShilpoHubDbContext.cs` | `Data/ShilpoHubDbContextTests.cs` | partly tested by `backend/tests/ProductImageRegression (image replacement only)` |
| ✅ | `MediaController` | Controller | `Api/Controllers/MediaController.cs` | `Controllers/MediaControllerTests.cs` |  |
| ✅ | `GlobalExceptionHandler` | Middleware | `Api/Middlewares/GlobalExceptionHandler.cs` | `Middlewares/GlobalExceptionHandlerTests.cs` |  |
| ✅ | `ValidationFilter` | Middleware | `Api/Middlewares/ValidationFilter.cs` | `Middlewares/ValidationFilterTests.cs` |  |
| ✅ | `UtcDateTimeJsonConverter` | JSON converter | `Api/Helpers/UtcDateTimeJsonConverters.cs` | `Json/UtcDateTimeJsonConverterTests.cs` |  |
| ✅ | `UtcNullableDateTimeJsonConverter` | JSON converter | `Api/Helpers/UtcDateTimeJsonConverters.cs` | `Json/UtcNullableDateTimeJsonConverterTests.cs` |  |
| ✅ | `SlugGenerator` | Helper | `Application/Common/SlugGenerator.cs` | `Common/SlugGeneratorTests.cs` |  |
| ✅ | `Application DependencyInjection` | DI registration | `Application/DependencyInjection.cs` | `DependencyInjection/ApplicationDependencyInjectionTests.cs` | every registered interface resolves |
| ✅ | `Data DependencyInjection` | DI registration | `Data/DependencyInjection.cs` | `DependencyInjection/DataDependencyInjectionTests.cs` | every registered interface resolves |
| ✅ | `Infrastructure DependencyInjection` | DI registration | `Infrastructure/DependencyInjection.cs` | `DependencyInjection/InfrastructureDependencyInjectionTests.cs` | every registered interface resolves |

<a id="be-portfolio"></a>

### Portfolio

**Folder:** `Features/Portfolio/` · **Units:** 11 · **Phase:** 4

Contains: 2 services, 5 validators, 2 repositories, 2 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `MentorFeedbackService` | Service | `Application/Services/Portfolio/MentorFeedbackService.cs` | `Services/MentorFeedbackServiceTests.cs` |  |
| ☐ | `PortfolioService` | Service | `Application/Services/Portfolio/PortfolioService.cs` | `Services/PortfolioServiceTests.cs` |  |
| ☐ | `CreatePortfolioProjectRequestValidator` | Validator | `Application/Validators/Portfolio/CreatePortfolioProjectRequestValidator.cs` | `Validators/CreatePortfolioProjectRequestValidatorTests.cs` |  |
| ☐ | `SubmitMentorFeedbackRequestValidator` | Validator | `Application/Validators/Portfolio/SubmitMentorFeedbackRequestValidator.cs` | `Validators/SubmitMentorFeedbackRequestValidatorTests.cs` |  |
| ☐ | `UpdatePortfolioProjectRequestValidator` | Validator | `Application/Validators/Portfolio/UpdatePortfolioProjectRequestValidator.cs` | `Validators/UpdatePortfolioProjectRequestValidatorTests.cs` |  |
| ☐ | `UpdatePortfolioRequestValidator` | Validator | `Application/Validators/Portfolio/UpdatePortfolioRequestValidator.cs` | `Validators/UpdatePortfolioRequestValidatorTests.cs` |  |
| ☐ | `UpdatePortfolioVisibilityRequestValidator` | Validator | `Application/Validators/Portfolio/UpdatePortfolioVisibilityRequestValidator.cs` | `Validators/UpdatePortfolioVisibilityRequestValidatorTests.cs` |  |
| ☐ | `MentorFeedbackRepository` | Repository | `Data/Repositories/MentorFeedbackRepository.cs` | `Repositories/MentorFeedbackRepositoryTests.cs` | needs the test database |
| ☐ | `PortfolioRepository` | Repository | `Data/Repositories/PortfolioRepository.cs` | `Repositories/PortfolioRepositoryTests.cs` | needs the test database |
| ☐ | `MentorFeedbackController` | Controller | `Api/Controllers/MentorFeedbackController.cs` | `Controllers/MentorFeedbackControllerTests.cs` |  |
| ☐ | `PortfoliosController` | Controller | `Api/Controllers/PortfoliosController.cs` | `Controllers/PortfoliosControllerTests.cs` |  |

<a id="be-procurement"></a>

### Procurement

**Folder:** `Features/Procurement/` · **Units:** 11 · **Phase:** 3

Contains: 1 service, 6 validators, 1 repository, 3 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ProcurementService` | Service | `Application/Services/Procurement/ProcurementService.cs` | `Services/ProcurementServiceTests.cs` |  |
| ☐ | `CreateProcurementFromQuotationRequestValidator` | Validator | `Application/Validators/Procurement/CreateProcurementFromQuotationRequestValidator.cs` | `Validators/CreateProcurementFromQuotationRequestValidatorTests.cs` |  |
| ☐ | `CreateProcurementRequestValidator` | Validator | `Application/Validators/Procurement/CreateProcurementRequestValidator.cs` | `Validators/CreateProcurementRequestValidatorTests.cs` |  |
| ☐ | `PayProcurementAdvanceRequestValidator` | Validator | `Application/Validators/Procurement/PayProcurementAdvanceRequestValidator.cs` | `Validators/PayProcurementAdvanceRequestValidatorTests.cs` |  |
| ☐ | `ProcurementDecisionRequestValidator` | Validator | `Application/Validators/Procurement/ProcurementDecisionRequestValidator.cs` | `Validators/ProcurementDecisionRequestValidatorTests.cs` |  |
| ☐ | `ProcurementItemInputValidator` | Validator | `Application/Validators/Procurement/ProcurementItemInputValidator.cs` | `Validators/ProcurementItemInputValidatorTests.cs` |  |
| ☐ | `ProcurementQueryParametersValidator` | Validator | `Application/Validators/Procurement/ProcurementQueryParametersValidator.cs` | `Validators/ProcurementQueryParametersValidatorTests.cs` |  |
| ☐ | `ProcurementRepository` | Repository | `Data/Repositories/ProcurementRepository.cs` | `Repositories/ProcurementRepositoryTests.cs` | needs the test database |
| ☐ | `AdminProcurementInspectionsController` | Controller | `Api/Controllers/AdminProcurementInspectionsController.cs` | `Controllers/AdminProcurementInspectionsControllerTests.cs` |  |
| ☐ | `ProcurementsController` | Controller | `Api/Controllers/ProcurementsController.cs` | `Controllers/ProcurementsControllerTests.cs` |  |
| ☐ | `ProducerProcurementsController` | Controller | `Api/Controllers/ProducerProcurementsController.cs` | `Controllers/ProducerProcurementsControllerTests.cs` |  |

<a id="be-producerbusiness"></a>

### ProducerBusiness

**Folder:** `Features/ProducerBusiness/` · **Units:** 12 · **Phase:** 2

Contains: 3 services, 2 validators, 2 repositories, 4 controllers, 1 background service.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ProducerMonthlyReportService` | Service | `Application/Services/ProducerBusiness/ProducerMonthlyReportService.cs` | `Services/ProducerMonthlyReportServiceTests.cs` |  |
| ☐ | `ProducerOrderService` | Service | `Application/Services/ProducerBusiness/ProducerOrderService.cs` | `Services/ProducerOrderServiceTests.cs` |  |
| ☐ | `ProducerReturnService` | Service | `Application/Services/ProducerBusiness/ProducerReturnService.cs` | `Services/ProducerReturnServiceTests.cs` |  |
| ☐ | `RejectOrderItemRequestValidator` | Validator | `Application/Validators/ProducerBusiness/RejectOrderItemRequestValidator.cs` | `Validators/RejectOrderItemRequestValidatorTests.cs` |  |
| ☐ | `ShipOrderItemRequestValidator` | Validator | `Application/Validators/ProducerBusiness/ShipOrderItemRequestValidator.cs` | `Validators/ShipOrderItemRequestValidatorTests.cs` |  |
| ☐ | `ProducerMonthlyReportRepository` | Repository | `Data/Repositories/ProducerMonthlyReportRepository.cs` | `Repositories/ProducerMonthlyReportRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerOrderRepository` | Repository | `Data/Repositories/ProducerOrderRepository.cs` | `Repositories/ProducerOrderRepositoryTests.cs` | needs the test database |
| ☐ | `AdminProducerMonthlyReportsController` | Controller | `Api/Controllers/AdminProducerMonthlyReportsController.cs` | `Controllers/AdminProducerMonthlyReportsControllerTests.cs` |  |
| ☐ | `GovProducerMonthlyReportsController` | Controller | `Api/Controllers/GovProducerMonthlyReportsController.cs` | `Controllers/GovProducerMonthlyReportsControllerTests.cs` |  |
| ☐ | `ProducerOrdersController` | Controller | `Api/Controllers/ProducerOrdersController.cs` | `Controllers/ProducerOrdersControllerTests.cs` |  |
| ☐ | `ProducerReturnsController` | Controller | `Api/Controllers/ProducerReturnsController.cs` | `Controllers/ProducerReturnsControllerTests.cs` |  |
| ☐ | `ProducerMonthlyReportGenerationHostedService` | Background service | `Api/BackgroundServices/ProducerMonthlyReportGenerationHostedService.cs` | `BackgroundServices/ProducerMonthlyReportGenerationHostedServiceTests.cs` |  |

<a id="be-producercomparison"></a>

### ProducerComparison

**Folder:** `Features/ProducerComparison/` · **Units:** 4 · **Phase:** 3

Contains: 1 service, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ProducerComparisonService` | Service | `Application/Services/ProducerComparison/ProducerComparisonService.cs` | `Services/ProducerComparisonServiceTests.cs` |  |
| ☐ | `ProducerComparisonRequestValidator` | Validator | `Application/Validators/ProducerComparison/ProducerComparisonRequestValidator.cs` | `Validators/ProducerComparisonRequestValidatorTests.cs` |  |
| ☐ | `ProducerComparisonRepository` | Repository | `Data/Repositories/ProducerComparisonRepository.cs` | `Repositories/ProducerComparisonRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerComparisonController` | Controller | `Api/Controllers/ProducerComparisonController.cs` | `Controllers/ProducerComparisonControllerTests.cs` |  |

<a id="be-producerpartnership"></a>

### ProducerPartnership

**Folder:** `Features/ProducerPartnership/` · **Units:** 25 · **Phase:** 3 · **Partly tested already:** 6

Contains: 6 services, 8 validators, 5 repositories, 6 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ProducerPartnershipAgreementService` | Service | `Application/Services/ProducerPartnership/ProducerPartnershipAgreementService.cs` | `Services/ProducerPartnershipAgreementServiceTests.cs` | partly tested by `backend/tests/ProducerPartnershipAgreementRegression`, `backend/tests/BusinessPartnershipEndToEndRegression` |
| ☐ | `ProducerPartnershipAuctionBidService` | Service | `Application/Services/ProducerPartnership/ProducerPartnershipAuctionBidService.cs` | `Services/ProducerPartnershipAuctionBidServiceTests.cs` | partly tested by `backend/tests/ProducerPartnershipAuctionRegression` |
| ☐ | `ProducerPartnershipAuctionLotService` | Service | `Application/Services/ProducerPartnership/ProducerPartnershipAuctionLotService.cs` | `Services/ProducerPartnershipAuctionLotServiceTests.cs` | partly tested by `backend/tests/ProducerPartnershipAuctionRegression` |
| ☐ | `ProducerPartnershipAuctionParticipantService` | Service | `Application/Services/ProducerPartnership/ProducerPartnershipAuctionParticipantService.cs` | `Services/ProducerPartnershipAuctionParticipantServiceTests.cs` | partly tested by `backend/tests/ProducerPartnershipAuctionRegression` |
| ☐ | `ProducerPartnershipAuctionService` | Service | `Application/Services/ProducerPartnership/ProducerPartnershipAuctionService.cs` | `Services/ProducerPartnershipAuctionServiceTests.cs` | partly tested by `backend/tests/ProducerPartnershipAuctionRegression`, `backend/tests/BusinessPartnershipEndToEndRegression` |
| ☐ | `ProducerPartnershipSettlementService` | Service | `Application/Services/ProducerPartnership/ProducerPartnershipSettlementService.cs` | `Services/ProducerPartnershipSettlementServiceTests.cs` | partly tested by `backend/tests/ProducerPartnershipSettlementRegression`, `backend/tests/BusinessPartnershipEndToEndRegression` |
| ☐ | `AddProducerPartnershipAuctionLotRequestValidator` | Validator | `Application/Validators/ProducerPartnership/AddProducerPartnershipAuctionLotRequestValidator.cs` | `Validators/AddProducerPartnershipAuctionLotRequestValidatorTests.cs` |  |
| ☐ | `CreateProducerPartnershipAgreementRequestValidator` | Validator | `Application/Validators/ProducerPartnership/CreateProducerPartnershipAgreementRequestValidator.cs` | `Validators/CreateProducerPartnershipAgreementRequestValidatorTests.cs` |  |
| ☐ | `CreateProducerPartnershipAuctionRequestValidator` | Validator | `Application/Validators/ProducerPartnership/CreateProducerPartnershipAuctionRequestValidator.cs` | `Validators/CreateProducerPartnershipAuctionRequestValidatorTests.cs` |  |
| ☐ | `DecideProducerPartnershipAuctionParticipantRequestValidator` | Validator | `Application/Validators/ProducerPartnership/DecideProducerPartnershipAuctionParticipantRequestValidator.cs` | `Validators/DecideProducerPartnershipAuctionParticipantRequestValidatorTests.cs` |  |
| ☐ | `GenerateProducerPartnershipSettlementRequestValidator` | Validator | `Application/Validators/ProducerPartnership/GenerateProducerPartnershipSettlementRequestValidator.cs` | `Validators/GenerateProducerPartnershipSettlementRequestValidatorTests.cs` |  |
| ☐ | `PlaceProducerPartnershipAuctionBidRequestValidator` | Validator | `Application/Validators/ProducerPartnership/PlaceProducerPartnershipAuctionBidRequestValidator.cs` | `Validators/PlaceProducerPartnershipAuctionBidRequestValidatorTests.cs` |  |
| ☐ | `UpdateProducerPartnershipAgreementTermsRequestValidator` | Validator | `Application/Validators/ProducerPartnership/UpdateProducerPartnershipAgreementTermsRequestValidator.cs` | `Validators/UpdateProducerPartnershipAgreementTermsRequestValidatorTests.cs` |  |
| ☐ | `UpdateProducerPartnershipAuctionRequestValidator` | Validator | `Application/Validators/ProducerPartnership/UpdateProducerPartnershipAuctionRequestValidator.cs` | `Validators/UpdateProducerPartnershipAuctionRequestValidatorTests.cs` |  |
| ☐ | `ProducerPartnershipAgreementRepository` | Repository | `Data/Repositories/ProducerPartnershipAgreementRepository.cs` | `Repositories/ProducerPartnershipAgreementRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerPartnershipAuctionLotRepository` | Repository | `Data/Repositories/ProducerPartnershipAuctionLotRepository.cs` | `Repositories/ProducerPartnershipAuctionLotRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerPartnershipAuctionParticipantRepository` | Repository | `Data/Repositories/ProducerPartnershipAuctionParticipantRepository.cs` | `Repositories/ProducerPartnershipAuctionParticipantRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerPartnershipAuctionRepository` | Repository | `Data/Repositories/ProducerPartnershipAuctionRepository.cs` | `Repositories/ProducerPartnershipAuctionRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerPartnershipSettlementRepository` | Repository | `Data/Repositories/ProducerPartnershipSettlementRepository.cs` | `Repositories/ProducerPartnershipSettlementRepositoryTests.cs` | needs the test database |
| ☐ | `ProducerPartnershipAgreementsController` | Controller | `Api/Controllers/ProducerPartnershipAgreementsController.cs` | `Controllers/ProducerPartnershipAgreementsControllerTests.cs` |  |
| ☐ | `ProducerPartnershipAuctionBidsController` | Controller | `Api/Controllers/ProducerPartnershipAuctionBidsController.cs` | `Controllers/ProducerPartnershipAuctionBidsControllerTests.cs` |  |
| ☐ | `ProducerPartnershipAuctionLotsController` | Controller | `Api/Controllers/ProducerPartnershipAuctionLotsController.cs` | `Controllers/ProducerPartnershipAuctionLotsControllerTests.cs` |  |
| ☐ | `ProducerPartnershipAuctionParticipantsController` | Controller | `Api/Controllers/ProducerPartnershipAuctionParticipantsController.cs` | `Controllers/ProducerPartnershipAuctionParticipantsControllerTests.cs` |  |
| ☐ | `ProducerPartnershipAuctionsController` | Controller | `Api/Controllers/ProducerPartnershipAuctionsController.cs` | `Controllers/ProducerPartnershipAuctionsControllerTests.cs` |  |
| ☐ | `ProducerPartnershipSettlementsController` | Controller | `Api/Controllers/ProducerPartnershipSettlementsController.cs` | `Controllers/ProducerPartnershipSettlementsControllerTests.cs` |  |

<a id="be-productdevelopment"></a>

### ProductDevelopment

**Folder:** `Features/ProductDevelopment/` · **Units:** 13 · **Phase:** 3

Contains: 1 service, 10 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ProductDevelopmentService` | Service | `Application/Services/ProductDevelopment/ProductDevelopmentService.cs` | `Services/ProductDevelopmentServiceTests.cs` |  |
| ☐ | `AddDevelopmentCommentRequestValidator` | Validator | `Application/Validators/ProductDevelopment/AddDevelopmentCommentRequestValidator.cs` | `Validators/AddDevelopmentCommentRequestValidatorTests.cs` |  |
| ☐ | `ConvertToProductRequestValidator` | Validator | `Application/Validators/ProductDevelopment/ConvertToProductRequestValidator.cs` | `Validators/ConvertToProductRequestValidatorTests.cs` |  |
| ☐ | `CreateDevelopmentProjectRequestValidator` | Validator | `Application/Validators/ProductDevelopment/CreateDevelopmentProjectRequestValidator.cs` | `Validators/CreateDevelopmentProjectRequestValidatorTests.cs` |  |
| ☐ | `DevelopmentMilestoneInputValidator` | Validator | `Application/Validators/ProductDevelopment/DevelopmentMilestoneInputValidator.cs` | `Validators/DevelopmentMilestoneInputValidatorTests.cs` |  |
| ☐ | `DevelopmentProjectQueryParametersValidator` | Validator | `Application/Validators/ProductDevelopment/DevelopmentProjectQueryParametersValidator.cs` | `Validators/DevelopmentProjectQueryParametersValidatorTests.cs` |  |
| ☐ | `DevelopmentResponseRequestValidator` | Validator | `Application/Validators/ProductDevelopment/DevelopmentResponseRequestValidator.cs` | `Validators/DevelopmentResponseRequestValidatorTests.cs` |  |
| ☐ | `PrototypeDecisionRequestValidator` | Validator | `Application/Validators/ProductDevelopment/PrototypeDecisionRequestValidator.cs` | `Validators/PrototypeDecisionRequestValidatorTests.cs` |  |
| ☐ | `PrototypeFileInputValidator` | Validator | `Application/Validators/ProductDevelopment/PrototypeFileInputValidator.cs` | `Validators/PrototypeFileInputValidatorTests.cs` |  |
| ☐ | `SubmitPrototypeRequestValidator` | Validator | `Application/Validators/ProductDevelopment/SubmitPrototypeRequestValidator.cs` | `Validators/SubmitPrototypeRequestValidatorTests.cs` |  |
| ☐ | `UpdateDevelopmentMilestoneStatusRequestValidator` | Validator | `Application/Validators/ProductDevelopment/UpdateDevelopmentMilestoneStatusRequestValidator.cs` | `Validators/UpdateDevelopmentMilestoneStatusRequestValidatorTests.cs` |  |
| ☐ | `ProductDevelopmentRepository` | Repository | `Data/Repositories/ProductDevelopmentRepository.cs` | `Repositories/ProductDevelopmentRepositoryTests.cs` | needs the test database |
| ☐ | `ProductDevelopmentController` | Controller | `Api/Controllers/ProductDevelopmentController.cs` | `Controllers/ProductDevelopmentControllerTests.cs` |  |

<a id="be-productintelligence"></a>

### ProductIntelligence

**Folder:** `Features/ProductIntelligence/` · **Units:** 4 · **Phase:** 3 · **Partly tested already:** 1

Contains: 1 service, 2 infrastructure classes, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ProductIntelligenceService` | Service | `Application/Services/ProductIntelligence/ProductIntelligenceService.cs` | `Services/ProductIntelligenceServiceTests.cs` | partly tested by `backend/tests/ProductIntelligenceRegression`, `backend/tests/BusinessPartnershipEndToEndRegression` |
| ☐ | `DummyProductIntelligenceAIProvider` | Infrastructure | `Infrastructure/ProductIntelligence/DummyProductIntelligenceAIProvider.cs` | `Infrastructure/DummyProductIntelligenceAIProviderTests.cs` |  |
| ☐ | `GeminiProductIntelligenceProvider` | Infrastructure | `Infrastructure/ProductIntelligence/GeminiProductIntelligenceProvider.cs` | `Infrastructure/GeminiProductIntelligenceProviderTests.cs` |  |
| ☐ | `ProductIntelligenceController` | Controller | `Api/Controllers/ProductIntelligenceController.cs` | `Controllers/ProductIntelligenceControllerTests.cs` |  |

<a id="be-productsearch"></a>

### ProductSearch

**Folder:** `Features/ProductSearch/` · **Units:** 22 · **Phase:** 2 · **Partly tested already:** 1

Contains: 4 services, 5 validators, 2 infrastructure classes, 4 repositories, 1 data component, 1 seeder, 5 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ProductAttributesService` | Service | `Application/Services/ProductSearch/ProductAttributesService.cs` | `Services/ProductAttributesServiceTests.cs` |  |
| ☐ | `ProductIndexService` | Service | `Application/Services/ProductSearch/ProductIndexService.cs` | `Services/ProductIndexServiceTests.cs` |  |
| ☐ | `ProductLookupService` | Service | `Application/Services/ProductSearch/ProductLookupService.cs` | `Services/ProductLookupServiceTests.cs` |  |
| ☐ | `ProductSearchService` | Service | `Application/Services/ProductSearch/ProductSearchService.cs` | `Services/ProductSearchServiceTests.cs` | partly tested by `backend/tests/ProductSearchRegression` |
| ☐ | `ConfirmAttributeSuggestionRequestValidator` | Validator | `Application/Validators/ProductSearch/ProductSearchValidators.cs` | `Validators/ConfirmAttributeSuggestionRequestValidatorTests.cs` |  |
| ☐ | `ProductSearchQueryValidator` | Validator | `Application/Validators/ProductSearch/ProductSearchQueryValidator.cs` | `Validators/ProductSearchQueryValidatorTests.cs` |  |
| ☐ | `SaveLookupItemRequestValidator` | Validator | `Application/Validators/ProductSearch/ProductSearchValidators.cs` | `Validators/SaveLookupItemRequestValidatorTests.cs` |  |
| ☐ | `SaveProductAttributesRequestValidator` | Validator | `Application/Validators/ProductSearch/ProductSearchValidators.cs` | `Validators/SaveProductAttributesRequestValidatorTests.cs` |  |
| ☐ | `SubmitAttributeSuggestionRequestValidator` | Validator | `Application/Validators/ProductSearch/ProductSearchValidators.cs` | `Validators/SubmitAttributeSuggestionRequestValidatorTests.cs` |  |
| ☐ | `ProductSearchServiceOptions` | Infrastructure | `Infrastructure/ProductSearch/PythonProductSearchProvider.cs` | `Infrastructure/ProductSearchServiceOptionsTests.cs` |  |
| ☐ | `PythonAttributeSuggester` | Infrastructure | `Infrastructure/ProductSearch/PythonAttributeSuggester.cs` | `Infrastructure/PythonAttributeSuggesterTests.cs` |  |
| ☐ | `ProductAttributesRepository` | Repository | `Data/Repositories/ProductSearchRepositories.cs` | `Repositories/ProductAttributesRepositoryTests.cs` | needs the test database |
| ☐ | `ProductIndexRepository` | Repository | `Data/Repositories/ProductSearchRepositories.cs` | `Repositories/ProductIndexRepositoryTests.cs` | needs the test database |
| ☐ | `ProductLookupRepository` | Repository | `Data/Repositories/ProductSearchRepositories.cs` | `Repositories/ProductLookupRepositoryTests.cs` | needs the test database |
| ☐ | `ProductSearchQueryRepository` | Repository | `Data/Repositories/ProductSearchQueryRepository.cs` | `Repositories/ProductSearchQueryRepositoryTests.cs` | needs the test database |
| ☐ | `ProductIndexDirtyInterceptor` | Data component | `Data/Interceptors/ProductIndexDirtyInterceptor.cs` | `Data/ProductIndexDirtyInterceptorTests.cs` |  |
| ☐ | `ProductSearchSeeder` | Seeder | `Data/Seed/ProductSearchSeeder.cs` | `Seed/ProductSearchSeederTests.cs` |  |
| ☐ | `MaterialsController` | Controller | `Api/Controllers/ProductSearchControllers.cs` | `Controllers/MaterialsControllerTests.cs` |  |
| ☐ | `ProductAttributesController` | Controller | `Api/Controllers/ProductSearchControllers.cs` | `Controllers/ProductAttributesControllerTests.cs` |  |
| ☐ | `ProductIndexController` | Controller | `Api/Controllers/ProductSearchControllers.cs` | `Controllers/ProductIndexControllerTests.cs` |  |
| ☐ | `ProductSearchController` | Controller | `Api/Controllers/ProductSearchControllers.cs` | `Controllers/ProductSearchControllerTests.cs` |  |
| ☐ | `ProductTypesController` | Controller | `Api/Controllers/ProductSearchControllers.cs` | `Controllers/ProductTypesControllerTests.cs` |  |

<a id="be-profiles"></a>

### Profiles

**Folder:** `Features/Profiles/` · **Units:** 4 · **Phase:** 1

Contains: 1 service, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `UserProfileService` | Service | `Application/Services/Profiles/UserProfileService.cs` | `Services/UserProfileServiceTests.cs` |  |
| ✅ | `UpsertUserProfileRequestValidator` | Validator | `Application/Validators/Profiles/UpsertUserProfileRequestValidator.cs` | `Validators/UpsertUserProfileRequestValidatorTests.cs` |  |
| ✅ | `UserProfileRepository` | Repository | `Data/Repositories/UserProfileRepository.cs` | `Repositories/UserProfileRepositoryTests.cs` | needs the test database |
| ✅ | `ProfileController` | Controller | `Api/Controllers/ProfileController.cs` | `Controllers/ProfileControllerTests.cs` |  |

<a id="be-qrverification"></a>

### QRVerification

**Folder:** `Features/QRVerification/` · **Units:** 6 · **Phase:** 2

Contains: 1 service, 3 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `QRVerificationService` | Service | `Application/Services/QRVerification/QRVerificationService.cs` | `Services/QRVerificationServiceTests.cs` |  |
| ✅ | `GenerateQRCodeRequestValidator` | Validator | `Application/Validators/QRVerification/GenerateQRCodeRequestValidator.cs` | `Validators/GenerateQRCodeRequestValidatorTests.cs` |  |
| ✅ | `QRVerificationQueryParametersValidator` | Validator | `Application/Validators/QRVerification/QRVerificationQueryParametersValidator.cs` | `Validators/QRVerificationQueryParametersValidatorTests.cs` |  |
| ✅ | `VerifyQRRequestValidator` | Validator | `Application/Validators/QRVerification/VerifyQRRequestValidator.cs` | `Validators/VerifyQRRequestValidatorTests.cs` |  |
| ✅ | `QRVerificationRepository` | Repository | `Data/Repositories/QRVerificationRepository.cs` | `Repositories/QRVerificationRepositoryTests.cs` | needs the test database |
| ✅ | `QRVerificationController` | Controller | `Api/Controllers/QRVerificationController.cs` | `Controllers/QRVerificationControllerTests.cs` |  |

<a id="be-quotations"></a>

### Quotations

**Folder:** `Features/Quotations/` · **Units:** 9 · **Phase:** 3

Contains: 1 service, 6 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `QuotationService` | Service | `Application/Services/Quotations/QuotationService.cs` | `Services/QuotationServiceTests.cs` |  |
| ☐ | `CreateQuotationRequestValidator` | Validator | `Application/Validators/Quotations/CreateQuotationRequestValidator.cs` | `Validators/CreateQuotationRequestValidatorTests.cs` |  |
| ☐ | `QuotationQueryParametersValidator` | Validator | `Application/Validators/Quotations/QuotationQueryParametersValidator.cs` | `Validators/QuotationQueryParametersValidatorTests.cs` |  |
| ☐ | `QuotationRequestItemInputValidator` | Validator | `Application/Validators/Quotations/QuotationRequestItemInputValidator.cs` | `Validators/QuotationRequestItemInputValidatorTests.cs` |  |
| ☐ | `QuotationResponseDecisionRequestValidator` | Validator | `Application/Validators/Quotations/QuotationResponseDecisionRequestValidator.cs` | `Validators/QuotationResponseDecisionRequestValidatorTests.cs` |  |
| ☐ | `SubmitQuotationResponseItemInputValidator` | Validator | `Application/Validators/Quotations/SubmitQuotationResponseItemInputValidator.cs` | `Validators/SubmitQuotationResponseItemInputValidatorTests.cs` |  |
| ☐ | `SubmitQuotationResponseRequestValidator` | Validator | `Application/Validators/Quotations/SubmitQuotationResponseRequestValidator.cs` | `Validators/SubmitQuotationResponseRequestValidatorTests.cs` |  |
| ☐ | `QuotationRepository` | Repository | `Data/Repositories/QuotationRepository.cs` | `Repositories/QuotationRepositoryTests.cs` | needs the test database |
| ☐ | `QuotationsController` | Controller | `Api/Controllers/QuotationsController.cs` | `Controllers/QuotationsControllerTests.cs` |  |

<a id="be-recommendation"></a>

### Recommendation

**Folder:** `Features/Recommendation/` · **Units:** 3 · **Phase:** 5

Contains: 1 service, 1 infrastructure class, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `RecommendationService` | Service | `Application/Services/Recommendation/RecommendationService.cs` | `Services/RecommendationServiceTests.cs` |  |
| ☐ | `DummyRecommendationProvider` | Infrastructure | `Infrastructure/Recommendations/DummyRecommendationProvider.cs` | `Infrastructure/DummyRecommendationProviderTests.cs` |  |
| ☐ | `RecommendationsController` | Controller | `Api/Controllers/RecommendationsController.cs` | `Controllers/RecommendationsControllerTests.cs` |  |

<a id="be-research"></a>

### Research

**Folder:** `Features/Research/` · **Units:** 37 · **Phase:** 4

Contains: 7 services, 18 validators, 1 infrastructure class, 2 repositories, 9 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ResearchAIService` | Service | `Application/Services/Research/ResearchAIService.cs` | `Services/ResearchAIServiceTests.cs` |  |
| ☐ | `ResearchMilestoneService` | Service | `Application/Services/Research/ResearchMilestoneService.cs` | `Services/ResearchMilestoneServiceTests.cs` |  |
| ☐ | `ResearchNoteService` | Service | `Application/Services/Research/ResearchNoteService.cs` | `Services/ResearchNoteServiceTests.cs` |  |
| ☐ | `ResearchPaperService` | Service | `Application/Services/Research/ResearchPaperService.cs` | `Services/ResearchPaperServiceTests.cs` |  |
| ☐ | `ResearchProjectService` | Service | `Application/Services/Research/ResearchProjectService.cs` | `Services/ResearchProjectServiceTests.cs` |  |
| ☐ | `ResearchPublicationService` | Service | `Application/Services/Research/ResearchPublicationService.cs` | `Services/ResearchPublicationServiceTests.cs` |  |
| ☐ | `ResearchTaskService` | Service | `Application/Services/Research/ResearchTaskService.cs` | `Services/ResearchTaskServiceTests.cs` |  |
| ☐ | `AddResearchProjectMemberRequestValidator` | Validator | `Application/Validators/Research/AddResearchProjectMemberRequestValidator.cs` | `Validators/AddResearchProjectMemberRequestValidatorTests.cs` |  |
| ☐ | `CreateResearchMilestoneRequestValidator` | Validator | `Application/Validators/Research/CreateResearchMilestoneRequestValidator.cs` | `Validators/CreateResearchMilestoneRequestValidatorTests.cs` |  |
| ☐ | `CreateResearchNoteRequestValidator` | Validator | `Application/Validators/Research/CreateResearchNoteRequestValidator.cs` | `Validators/CreateResearchNoteRequestValidatorTests.cs` |  |
| ☐ | `CreateResearchPaperRequestValidator` | Validator | `Application/Validators/Research/CreateResearchPaperRequestValidator.cs` | `Validators/CreateResearchPaperRequestValidatorTests.cs` |  |
| ☐ | `CreateResearchProjectRequestValidator` | Validator | `Application/Validators/Research/CreateResearchProjectRequestValidator.cs` | `Validators/CreateResearchProjectRequestValidatorTests.cs` |  |
| ☐ | `CreateResearchPublicationRequestValidator` | Validator | `Application/Validators/Research/CreateResearchPublicationRequestValidator.cs` | `Validators/CreateResearchPublicationRequestValidatorTests.cs` |  |
| ☐ | `CreateResearchTaskRequestValidator` | Validator | `Application/Validators/Research/CreateResearchTaskRequestValidator.cs` | `Validators/CreateResearchTaskRequestValidatorTests.cs` |  |
| ☐ | `GenerateResearchCitationsRequestValidator` | Validator | `Application/Validators/Research/GenerateResearchCitationsRequestValidator.cs` | `Validators/GenerateResearchCitationsRequestValidatorTests.cs` |  |
| ☐ | `RunResearchAnalysisRequestValidator` | Validator | `Application/Validators/Research/RunResearchAnalysisRequestValidator.cs` | `Validators/RunResearchAnalysisRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchMemberRoleRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchMemberRoleRequestValidator.cs` | `Validators/UpdateResearchMemberRoleRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchMilestoneRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchMilestoneRequestValidator.cs` | `Validators/UpdateResearchMilestoneRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchNoteRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchNoteRequestValidator.cs` | `Validators/UpdateResearchNoteRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchPaperRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchPaperRequestValidator.cs` | `Validators/UpdateResearchPaperRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchProjectRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchProjectRequestValidator.cs` | `Validators/UpdateResearchProjectRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchProjectStatusRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchProjectStatusRequestValidator.cs` | `Validators/UpdateResearchProjectStatusRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchPublicationRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchPublicationRequestValidator.cs` | `Validators/UpdateResearchPublicationRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchTaskRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchTaskRequestValidator.cs` | `Validators/UpdateResearchTaskRequestValidatorTests.cs` |  |
| ☐ | `UpdateResearchTaskStatusRequestValidator` | Validator | `Application/Validators/Research/UpdateResearchTaskStatusRequestValidator.cs` | `Validators/UpdateResearchTaskStatusRequestValidatorTests.cs` |  |
| ☐ | `DummyResearchAIProvider` | Infrastructure | `Infrastructure/ResearchAI/DummyResearchAIProvider.cs` | `Infrastructure/DummyResearchAIProviderTests.cs` |  |
| ☐ | `ResearchAIAnalysisRepository` | Repository | `Data/Repositories/ResearchAIAnalysisRepository.cs` | `Repositories/ResearchAIAnalysisRepositoryTests.cs` | needs the test database |
| ☐ | `ResearchProjectRepository` | Repository | `Data/Repositories/ResearchProjectRepository.cs` | `Repositories/ResearchProjectRepositoryTests.cs` | needs the test database |
| ☐ | `PublicationRepositoryController` | Controller | `Api/Controllers/PublicationRepositoryController.cs` | `Controllers/PublicationRepositoryControllerTests.cs` |  |
| ☐ | `ResearchAiAssistantController` | Controller | `Api/Controllers/ResearchAiAssistantController.cs` | `Controllers/ResearchAiAssistantControllerTests.cs` |  |
| ☐ | `ResearchMilestonesController` | Controller | `Api/Controllers/ResearchMilestonesController.cs` | `Controllers/ResearchMilestonesControllerTests.cs` |  |
| ☐ | `ResearchNotesController` | Controller | `Api/Controllers/ResearchNotesController.cs` | `Controllers/ResearchNotesControllerTests.cs` |  |
| ☐ | `ResearchPapersController` | Controller | `Api/Controllers/ResearchPapersController.cs` | `Controllers/ResearchPapersControllerTests.cs` |  |
| ☐ | `ResearchProjectsController` | Controller | `Api/Controllers/ResearchProjectsController.cs` | `Controllers/ResearchProjectsControllerTests.cs` |  |
| ☐ | `ResearchPublicationsController` | Controller | `Api/Controllers/ResearchPublicationsController.cs` | `Controllers/ResearchPublicationsControllerTests.cs` |  |
| ☐ | `ResearchTasksController` | Controller | `Api/Controllers/ResearchTasksController.cs` | `Controllers/ResearchTasksControllerTests.cs` |  |
| ☐ | `UserLookupController` | Controller | `Api/Controllers/UserLookupController.cs` | `Controllers/UserLookupControllerTests.cs` | queries ShilpoHubDbContext directly (Postgres ILike): needs the test database |

<a id="be-reviews"></a>

### Reviews

**Folder:** `Features/Reviews/` · **Units:** 6 · **Phase:** 2

Contains: 1 service, 3 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `ReviewService` | Service | `Application/Services/Reviews/ReviewService.cs` | `Services/ReviewServiceTests.cs` |  |
| ✅ | `CreateReviewRequestValidator` | Validator | `Application/Validators/Reviews/CreateReviewRequestValidator.cs` | `Validators/CreateReviewRequestValidatorTests.cs` |  |
| ✅ | `ReviewQueryParametersValidator` | Validator | `Application/Validators/Reviews/ReviewQueryParametersValidator.cs` | `Validators/ReviewQueryParametersValidatorTests.cs` |  |
| ✅ | `UpdateReviewRequestValidator` | Validator | `Application/Validators/Reviews/UpdateReviewRequestValidator.cs` | `Validators/UpdateReviewRequestValidatorTests.cs` |  |
| ✅ | `ReviewRepository` | Repository | `Data/Repositories/ReviewRepository.cs` | `Repositories/ReviewRepositoryTests.cs` | needs the test database |
| ✅ | `ReviewsController` | Controller | `Api/Controllers/ReviewsController.cs` | `Controllers/ReviewsControllerTests.cs` |  |

<a id="be-roadmap"></a>

### Roadmap

**Folder:** `Features/Roadmap/` · **Units:** 5 · **Phase:** 4

Contains: 1 service, 1 provider/handler, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `LearningRoadmapService` | Service | `Application/Services/Roadmap/LearningRoadmapService.cs` | `Services/LearningRoadmapServiceTests.cs` |  |
| ☐ | `RuleBasedLearningRoadmapProvider` | Provider/Handler | `Application/Services/Roadmap/RuleBasedLearningRoadmapProvider.cs` | `Services/RuleBasedLearningRoadmapProviderTests.cs` |  |
| ☐ | `CreateRoadmapRequestValidator` | Validator | `Application/Validators/Roadmap/CreateRoadmapRequestValidator.cs` | `Validators/CreateRoadmapRequestValidatorTests.cs` |  |
| ☐ | `LearningRoadmapRepository` | Repository | `Data/Repositories/LearningRoadmapRepository.cs` | `Repositories/LearningRoadmapRepositoryTests.cs` | needs the test database |
| ☐ | `LearningRoadmapsController` | Controller | `Api/Controllers/LearningRoadmapsController.cs` | `Controllers/LearningRoadmapsControllerTests.cs` |  |

<a id="be-search"></a>

### Search

**Folder:** `Features/Search/` · **Units:** 3 · **Phase:** 2

Contains: 1 service, 1 data component, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `SearchService` | Service | `Application/Services/Search/SearchService.cs` | `Services/SearchServiceTests.cs` |  |
| ✅ | `PostgresProductSearchProvider` | Data component | `Data/Search/PostgresProductSearchProvider.cs` | `Data/PostgresProductSearchProviderTests.cs` |  |
| ✅ | `SearchController` | Controller | `Api/Controllers/SearchController.cs` | `Controllers/SearchControllerTests.cs` |  |

<a id="be-security"></a>

### Security

**Folder:** `Features/Security/` · **Units:** 18 · **Phase:** 1

Contains: 5 services, 2 validators, 1 infrastructure class, 5 repositories, 5 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `ApiKeyService` | Service | `Application/Services/Security/ApiKeyService.cs` | `Services/ApiKeyServiceTests.cs` |  |
| ✅ | `AuditLogService` | Service | `Application/Services/Security/AuditLogService.cs` | `Services/AuditLogServiceTests.cs` |  |
| ✅ | `BackupService` | Service | `Application/Services/Security/BackupService.cs` | `Services/BackupServiceTests.cs` |  |
| ✅ | `SystemHealthService` | Service | `Application/Services/Security/SystemHealthService.cs` | `Services/SystemHealthServiceTests.cs` |  |
| ✅ | `ThreatDetectionService` | Service | `Application/Services/Security/ThreatDetectionService.cs` | `Services/ThreatDetectionServiceTests.cs` |  |
| ✅ | `BlockIpRequestValidator` | Validator | `Application/Validators/Security/BlockIpRequestValidator.cs` | `Validators/BlockIpRequestValidatorTests.cs` |  |
| ✅ | `CreateApiKeyRequestValidator` | Validator | `Application/Validators/Security/CreateApiKeyRequestValidator.cs` | `Validators/CreateApiKeyRequestValidatorTests.cs` |  |
| ✅ | `PgDumpBackupRunner` | Infrastructure | `Infrastructure/Security/PgDumpBackupRunner.cs` | `Infrastructure/PgDumpBackupRunnerTests.cs` |  |
| ✅ | `ApiKeyRepository` | Repository | `Data/Repositories/ApiKeyRepository.cs` | `Repositories/ApiKeyRepositoryTests.cs` | needs the test database |
| ✅ | `AuditLogRepository` | Repository | `Data/Repositories/AuditLogRepository.cs` | `Repositories/AuditLogRepositoryTests.cs` | needs the test database |
| ✅ | `BackupRepository` | Repository | `Data/Repositories/BackupRepository.cs` | `Repositories/BackupRepositoryTests.cs` | needs the test database |
| ✅ | `SystemHealthRepository` | Repository | `Data/Repositories/SystemHealthRepository.cs` | `Repositories/SystemHealthRepositoryTests.cs` | needs the test database |
| ✅ | `ThreatDetectionRepository` | Repository | `Data/Repositories/ThreatDetectionRepository.cs` | `Repositories/ThreatDetectionRepositoryTests.cs` | needs the test database |
| ✅ | `ApiKeysController` | Controller | `Api/Controllers/ApiKeysController.cs` | `Controllers/ApiKeysControllerTests.cs` |  |
| ✅ | `AuditLogsController` | Controller | `Api/Controllers/AuditLogsController.cs` | `Controllers/AuditLogsControllerTests.cs` |  |
| ✅ | `BackupsController` | Controller | `Api/Controllers/BackupsController.cs` | `Controllers/BackupsControllerTests.cs` |  |
| ✅ | `SystemHealthController` | Controller | `Api/Controllers/SystemHealthController.cs` | `Controllers/SystemHealthControllerTests.cs` |  |
| ✅ | `ThreatDetectionController` | Controller | `Api/Controllers/ThreatDetectionController.cs` | `Controllers/ThreatDetectionControllerTests.cs` |  |

<a id="be-sentimentanalysis"></a>

### SentimentAnalysis

**Folder:** `Features/SentimentAnalysis/` · **Units:** 4 · **Phase:** 5

Contains: 1 service, 1 validator, 1 infrastructure class, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `SentimentAnalysisService` | Service | `Application/Services/SentimentAnalysis/SentimentAnalysisService.cs` | `Services/SentimentAnalysisServiceTests.cs` |  |
| ☐ | `AnalyzeSentimentRequestValidator` | Validator | `Application/Validators/SentimentAnalysis/AnalyzeSentimentRequestValidator.cs` | `Validators/AnalyzeSentimentRequestValidatorTests.cs` |  |
| ☐ | `RuleBasedSentimentAnalysisProvider` | Infrastructure | `Infrastructure/SentimentAnalysis/RuleBasedSentimentAnalysisProvider.cs` | `Infrastructure/RuleBasedSentimentAnalysisProviderTests.cs` |  |
| ☐ | `SentimentAnalysisController` | Controller | `Api/Controllers/SentimentAnalysisController.cs` | `Controllers/SentimentAnalysisControllerTests.cs` |  |

<a id="be-skillassessment"></a>

### SkillAssessment

**Folder:** `Features/SkillAssessment/` · **Units:** 4 · **Phase:** 4

Contains: 1 service, 1 provider/handler, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `SkillAssessmentService` | Service | `Application/Services/SkillAssessment/SkillAssessmentService.cs` | `Services/SkillAssessmentServiceTests.cs` |  |
| ☐ | `DummySkillAssessmentProvider` | Provider/Handler | `Application/Services/SkillAssessment/DummySkillAssessmentProvider.cs` | `Services/DummySkillAssessmentProviderTests.cs` |  |
| ☐ | `SkillAssessmentRepository` | Repository | `Data/Repositories/SkillAssessmentRepository.cs` | `Repositories/SkillAssessmentRepositoryTests.cs` | needs the test database |
| ☐ | `SkillAssessmentsController` | Controller | `Api/Controllers/SkillAssessmentsController.cs` | `Controllers/SkillAssessmentsControllerTests.cs` |  |

<a id="be-storygenerator"></a>

### StoryGenerator

**Folder:** `Features/StoryGenerator/` · **Units:** 4 · **Phase:** 5

Contains: 1 service, 1 validator, 1 infrastructure class, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `StoryGeneratorService` | Service | `Application/Services/StoryGenerator/StoryGeneratorService.cs` | `Services/StoryGeneratorServiceTests.cs` |  |
| ☐ | `GenerateCraftStoryRequestValidator` | Validator | `Application/Validators/StoryGenerator/GenerateCraftStoryRequestValidator.cs` | `Validators/GenerateCraftStoryRequestValidatorTests.cs` |  |
| ☐ | `RuleBasedStoryGeneratorProvider` | Infrastructure | `Infrastructure/StoryGenerator/RuleBasedStoryGeneratorProvider.cs` | `Infrastructure/RuleBasedStoryGeneratorProviderTests.cs` |  |
| ☐ | `StoryGeneratorController` | Controller | `Api/Controllers/StoryGeneratorController.cs` | `Controllers/StoryGeneratorControllerTests.cs` |  |

<a id="be-supplierdiscovery"></a>

### SupplierDiscovery

**Folder:** `Features/SupplierDiscovery/` · **Units:** 4 · **Phase:** 3

Contains: 1 service, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `SupplierDiscoveryService` | Service | `Application/Services/SupplierDiscovery/SupplierDiscoveryService.cs` | `Services/SupplierDiscoveryServiceTests.cs` |  |
| ☐ | `SupplierSearchParametersValidator` | Validator | `Application/Validators/SupplierDiscovery/SupplierSearchParametersValidator.cs` | `Validators/SupplierSearchParametersValidatorTests.cs` |  |
| ☐ | `SupplierDiscoveryRepository` | Repository | `Data/Repositories/SupplierDiscoveryRepository.cs` | `Repositories/SupplierDiscoveryRepositoryTests.cs` | needs the test database |
| ☐ | `SupplierDiscoveryController` | Controller | `Api/Controllers/SupplierDiscoveryController.cs` | `Controllers/SupplierDiscoveryControllerTests.cs` |  |

<a id="be-suppliermatching"></a>

### SupplierMatching

**Folder:** `Features/SupplierMatching/` · **Units:** 4 · **Phase:** 3

Contains: 1 service, 1 validator, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `SupplierMatchingService` | Service | `Application/Services/SupplierMatching/SupplierMatchingService.cs` | `Services/SupplierMatchingServiceTests.cs` |  |
| ☐ | `SupplierMatchRequestValidator` | Validator | `Application/Validators/SupplierMatching/SupplierMatchRequestValidator.cs` | `Validators/SupplierMatchRequestValidatorTests.cs` |  |
| ☐ | `SupplierMatchingRepository` | Repository | `Data/Repositories/SupplierMatchingRepository.cs` | `Repositories/SupplierMatchingRepositoryTests.cs` | needs the test database |
| ☐ | `SupplierMatchingController` | Controller | `Api/Controllers/SupplierMatchingController.cs` | `Controllers/SupplierMatchingControllerTests.cs` |  |

<a id="be-sustainability"></a>

### Sustainability

**Folder:** `Features/Sustainability/` · **Units:** 5 · **Phase:** 5

Contains: 1 service, 2 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `SustainabilityService` | Service | `Application/Services/Sustainability/SustainabilityService.cs` | `Services/SustainabilityServiceTests.cs` |  |
| ☐ | `CreateMaterialCertificationRequestValidator` | Validator | `Application/Validators/Sustainability/CreateMaterialCertificationRequestValidator.cs` | `Validators/CreateMaterialCertificationRequestValidatorTests.cs` |  |
| ☐ | `CreateMaterialRecordRequestValidator` | Validator | `Application/Validators/Sustainability/CreateMaterialRecordRequestValidator.cs` | `Validators/CreateMaterialRecordRequestValidatorTests.cs` |  |
| ☐ | `SustainabilityRepository` | Repository | `Data/Repositories/SustainabilityRepository.cs` | `Repositories/SustainabilityRepositoryTests.cs` | needs the test database |
| ☐ | `SustainabilityController` | Controller | `Api/Controllers/SustainabilityController.cs` | `Controllers/SustainabilityControllerTests.cs` |  |

<a id="be-tourism"></a>

### Tourism

**Folder:** `Features/Tourism/` · **Units:** 11 · **Phase:** 4

Contains: 1 service, 2 validators, 4 infrastructure classes, 1 repository, 1 seeder, 2 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `TourismLocationService` | Service | `Application/Services/Tourism/TourismLocationService.cs` | `Services/TourismLocationServiceTests.cs` |  |
| ☐ | `CreateTourismLocationRequestValidator` | Validator | `Application/Validators/Tourism/CreateTourismLocationRequestValidator.cs` | `Validators/CreateTourismLocationRequestValidatorTests.cs` |  |
| ☐ | `UpdateTourismLocationRequestValidator` | Validator | `Application/Validators/Tourism/UpdateTourismLocationRequestValidator.cs` | `Validators/UpdateTourismLocationRequestValidatorTests.cs` |  |
| ☐ | `AccommodationService` | Infrastructure | `Infrastructure/Tourism/TourismLocationQueries.cs` | `Infrastructure/AccommodationServiceTests.cs` |  |
| ☐ | `OverpassPoiClient` | Infrastructure | `Infrastructure/Tourism/OverpassPoiClient.cs` | `Infrastructure/OverpassPoiClientTests.cs` |  |
| ☐ | `TourismExternalSyncService` | Infrastructure | `Infrastructure/Tourism/TourismExternalSyncService.cs` | `Infrastructure/TourismExternalSyncServiceTests.cs` |  |
| ☐ | `TourismPoiService` | Infrastructure | `Infrastructure/Tourism/TourismLocationQueries.cs` | `Infrastructure/TourismPoiServiceTests.cs` |  |
| ☐ | `TourismLocationRepository` | Repository | `Data/Repositories/TourismLocationRepository.cs` | `Repositories/TourismLocationRepositoryTests.cs` | needs the test database |
| ☐ | `TourismLocationSeeder` | Seeder | `Data/Seed/TourismLocationSeeder.cs` | `Seed/TourismLocationSeederTests.cs` |  |
| ☐ | `TourismExternalController` | Controller | `Api/Controllers/TourismExternalController.cs` | `Controllers/TourismExternalControllerTests.cs` |  |
| ☐ | `TourismLocationsController` | Controller | `Api/Controllers/TourismLocationsController.cs` | `Controllers/TourismLocationsControllerTests.cs` |  |

<a id="be-touristbooking"></a>

### TouristBooking

**Folder:** `Features/TouristBooking/` · **Units:** 14 · **Phase:** 4

Contains: 3 services, 6 validators, 3 repositories, 2 controllers.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `BookingService` | Service | `Application/Services/TouristBooking/BookingService.cs` | `Services/BookingServiceTests.cs` |  |
| ☐ | `ServiceAvailabilityService` | Service | `Application/Services/TouristBooking/ServiceAvailabilityService.cs` | `Services/ServiceAvailabilityServiceTests.cs` |  |
| ☐ | `TouristServiceService` | Service | `Application/Services/TouristBooking/TouristServiceService.cs` | `Services/TouristServiceServiceTests.cs` |  |
| ☐ | `CancelBookingRequestValidator` | Validator | `Application/Validators/TouristBooking/CancelBookingRequestValidator.cs` | `Validators/CancelBookingRequestValidatorTests.cs` |  |
| ☐ | `CreateBookingRequestValidator` | Validator | `Application/Validators/TouristBooking/CreateBookingRequestValidator.cs` | `Validators/CreateBookingRequestValidatorTests.cs` |  |
| ☐ | `CreateServiceAvailabilitySlotRequestValidator` | Validator | `Application/Validators/TouristBooking/CreateServiceAvailabilitySlotRequestValidator.cs` | `Validators/CreateServiceAvailabilitySlotRequestValidatorTests.cs` |  |
| ☐ | `CreateTouristServiceRequestValidator` | Validator | `Application/Validators/TouristBooking/CreateTouristServiceRequestValidator.cs` | `Validators/CreateTouristServiceRequestValidatorTests.cs` |  |
| ☐ | `UpdateServiceAvailabilitySlotRequestValidator` | Validator | `Application/Validators/TouristBooking/UpdateServiceAvailabilitySlotRequestValidator.cs` | `Validators/UpdateServiceAvailabilitySlotRequestValidatorTests.cs` |  |
| ☐ | `UpdateTouristServiceRequestValidator` | Validator | `Application/Validators/TouristBooking/UpdateTouristServiceRequestValidator.cs` | `Validators/UpdateTouristServiceRequestValidatorTests.cs` |  |
| ☐ | `BookingRepository` | Repository | `Data/Repositories/BookingRepository.cs` | `Repositories/BookingRepositoryTests.cs` | needs the test database |
| ☐ | `ServiceAvailabilitySlotRepository` | Repository | `Data/Repositories/ServiceAvailabilitySlotRepository.cs` | `Repositories/ServiceAvailabilitySlotRepositoryTests.cs` | needs the test database |
| ☐ | `TouristServiceRepository` | Repository | `Data/Repositories/TouristServiceRepository.cs` | `Repositories/TouristServiceRepositoryTests.cs` | needs the test database |
| ☐ | `BookingsController` | Controller | `Api/Controllers/BookingsController.cs` | `Controllers/BookingsControllerTests.cs` |  |
| ☐ | `TouristServicesController` | Controller | `Api/Controllers/TouristServicesController.cs` | `Controllers/TouristServicesControllerTests.cs` |  |

<a id="be-traceability"></a>

### Traceability

**Folder:** `Features/Traceability/` · **Units:** 7 · **Phase:** 2

Contains: 1 service, 4 validators, 1 repository, 1 controller.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ✅ | `TraceabilityService` | Service | `Application/Services/Traceability/TraceabilityService.cs` | `Services/TraceabilityServiceTests.cs` |  |
| ✅ | `CreateProductTraceabilityRequestValidator` | Validator | `Application/Validators/Traceability/CreateProductTraceabilityRequestValidator.cs` | `Validators/CreateProductTraceabilityRequestValidatorTests.cs` |  |
| ✅ | `MaterialSourceInputValidator` | Validator | `Application/Validators/Traceability/MaterialSourceInputValidator.cs` | `Validators/MaterialSourceInputValidatorTests.cs` |  |
| ✅ | `TimelineEventInputValidator` | Validator | `Application/Validators/Traceability/TimelineEventInputValidator.cs` | `Validators/TimelineEventInputValidatorTests.cs` |  |
| ✅ | `UpdateProductTraceabilityRequestValidator` | Validator | `Application/Validators/Traceability/UpdateProductTraceabilityRequestValidator.cs` | `Validators/UpdateProductTraceabilityRequestValidatorTests.cs` |  |
| ✅ | `TraceabilityRepository` | Repository | `Data/Repositories/TraceabilityRepository.cs` | `Repositories/TraceabilityRepositoryTests.cs` | needs the test database |
| ✅ | `TraceabilityController` | Controller | `Api/Controllers/TraceabilityController.cs` | `Controllers/TraceabilityControllerTests.cs` |  |

---

## 13. Frontend features and units

Root folder: `frontend/tests/features/`. Source paths are relative to `frontend/src/`, and test paths are relative to the feature folder.

Pages belong to the feature of their `pages/` folder. Hooks and API service files belong to the feature whose pages import them most. Those imported by 4 or more features are in **SharedData**.

| Feature | Units | Pages | Components | Hooks | API services | Other | Phase |
|---|---|---|---|---|---|---|---|
| [About](#fe-about) | 1 | 1 | · | · | · | · | 5 |
| [Academy](#fe-academy) | 50 | 16 | · | 17 | 17 | · | 4 |
| [Admin](#fe-admin) | 30 | 23 | · | 3 | 4 | · | 1 |
| [ApprenticeStudent](#fe-apprenticestudent) | 8 | 3 | · | 2 | 3 | · | 4 |
| [AppShell](#fe-appshell) | 49 | 3 | 14 | 2 | 2 | 28 | 1 |
| [Auth](#fe-auth) | 8 | 5 | 1 | 1 | 1 | · | 1 |
| [BusinessPartner](#fe-businesspartner) | 44 | 18 | 4 | 11 | 11 | · | 3 |
| [Customer](#fe-customer) | 85 | 38 | 1 | 23 | 23 | · | 2 |
| [Dashboard](#fe-dashboard) | 19 | 12 | 4 | 2 | 1 | · | 4 |
| [Explore](#fe-explore) | 19 | 11 | 2 | 3 | 3 | · | 5 |
| [Government](#fe-government) | 24 | 8 | · | 8 | 8 | · | 4 |
| [Home](#fe-home) | 4 | 1 | 2 | 1 | · | · | 5 |
| [LogisticsPartner](#fe-logisticspartner) | 27 | 9 | 1 | 9 | 8 | · | 3 |
| [Marketplace](#fe-marketplace) | 15 | 8 | 1 | 3 | 3 | · | 2 |
| [Messaging](#fe-messaging) | 5 | · | 3 | 1 | 1 | · | 5 |
| [News](#fe-news) | 3 | 3 | · | · | · | · | 5 |
| [NGO](#fe-ngo) | 1 | 1 | · | · | · | · | 4 |
| [Notifications](#fe-notifications) | 3 | · | 2 | 1 | · | · | 1 |
| [Producer](#fe-producer) | 50 | 26 | 1 | 12 | 11 | · | 2 |
| [Research](#fe-research) | 32 | 11 | 1 | 10 | 10 | · | 4 |
| [Researcher](#fe-researcher) | 1 | 1 | · | · | · | · | 4 |
| [SharedData](#fe-shareddata) | 28 | · | · | 14 | 14 | · | 2 |
| [SharedUI](#fe-sharedui) | 62 | · | 62 | · | · | · | 2 |
| [Tourism](#fe-tourism) | 39 | 15 | 4 | 10 | 10 | · | 4 |
| [Tourist](#fe-tourist) | 1 | 1 | · | · | · | · | 4 |
| [TrainerMasterArtisan](#fe-trainermasterartisan) | 3 | 2 | · | 1 | · | · | 4 |
| **Total (26 features)** | **611** | **216** | **103** | **134** | **130** | **28** | |

<a id="fe-about"></a>

### About

The public About page.

**Folder:** `features/About/` · **Units:** 1 · **Phase:** 5

Contains: 1 page.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AboutPage` | Page | `pages/About/AboutPage.jsx` | `pages/AboutPage.test.jsx` |  |

<a id="fe-academy"></a>

### Academy

Heritage Academy: public course catalogue, live classes and mentors, plus the signed-in learning pages (roadmap, exams, quizzes, assignments, certificates).

**Folder:** `features/Academy/` · **Units:** 50 · **Phase:** 4

Contains: 16 pages, 17 hooks, 17 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AssignmentDetails` | Page | `pages/Academy/AssignmentDetails.jsx` | `pages/AssignmentDetails.test.jsx` |  |
| ☐ | `Certificates` | Page | `pages/Academy/Certificates.jsx` | `pages/Certificates.test.jsx` |  |
| ☐ | `Certifications` | Page | `pages/Academy/Certifications.jsx` | `pages/Certifications.test.jsx` |  |
| ☐ | `CourseCatalog` | Page | `pages/Academy/CourseCatalog.jsx` | `pages/CourseCatalog.test.jsx` |  |
| ☐ | `CourseDetails` | Page | `pages/Academy/CourseDetails.jsx` | `pages/CourseDetails.test.jsx` |  |
| ☐ | `ExamDetails` | Page | `pages/Academy/ExamDetails.jsx` | `pages/ExamDetails.test.jsx` |  |
| ☐ | `LearningDashboard` | Page | `pages/Academy/LearningDashboard.jsx` | `pages/LearningDashboard.test.jsx` |  |
| ☐ | `LearningRoadmap` | Page | `pages/Academy/LearningRoadmap.jsx` | `pages/LearningRoadmap.test.jsx` |  |
| ☐ | `LiveClassDetails` | Page | `pages/Academy/LiveClassDetails.jsx` | `pages/LiveClassDetails.test.jsx` |  |
| ☐ | `LiveClasses` | Page | `pages/Academy/LiveClasses.jsx` | `pages/LiveClasses.test.jsx` |  |
| ☐ | `MentorMatching` | Page | `pages/Academy/MentorMatching.jsx` | `pages/MentorMatching.test.jsx` |  |
| ☐ | `Mentors` | Page | `pages/Academy/Mentors.jsx` | `pages/Mentors.test.jsx` |  |
| ☐ | `MentorshipRequests` | Page | `pages/Academy/MentorshipRequests.jsx` | `pages/MentorshipRequests.test.jsx` |  |
| ☐ | `Portfolio` | Page | `pages/Academy/Portfolio.jsx` | `pages/Portfolio.test.jsx` |  |
| ☐ | `QuizDetails` | Page | `pages/Academy/QuizDetails.jsx` | `pages/QuizDetails.test.jsx` |  |
| ☐ | `SkillAssessments` | Page | `pages/Academy/SkillAssessments.jsx` | `pages/SkillAssessments.test.jsx` |  |
| ☐ | `useAcademyProfile` | Hook | `hooks/useAcademyProfile.js` | `hooks/useAcademyProfile.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useAssignments` | Hook | `hooks/useAssignments.js` | `hooks/useAssignments.test.js` |  |
| ☐ | `useCourseCategories` | Hook | `hooks/useCourseCategories.js` | `hooks/useCourseCategories.test.js` |  |
| ☐ | `useCourses` | Hook | `hooks/useCourses.js` | `hooks/useCourses.test.js` |  |
| ☐ | `useEnrollments` | Hook | `hooks/useEnrollments.js` | `hooks/useEnrollments.test.js` |  |
| ☐ | `useExams` | Hook | `hooks/useExams.js` | `hooks/useExams.test.js` |  |
| ☐ | `useHeritageSkills` | Hook | `hooks/useHeritageSkills.js` | `hooks/useHeritageSkills.test.js` |  |
| ☐ | `useLearningRoadmaps` | Hook | `hooks/useLearningRoadmaps.js` | `hooks/useLearningRoadmaps.test.js` |  |
| ☐ | `useLiveClasses` | Hook | `hooks/useLiveClasses.js` | `hooks/useLiveClasses.test.js` |  |
| ☐ | `useMentorFeedback` | Hook | `hooks/useMentorFeedback.js` | `hooks/useMentorFeedback.test.js` |  |
| ☐ | `useMentorMatching` | Hook | `hooks/useMentorMatching.js` | `hooks/useMentorMatching.test.js` |  |
| ☐ | `useMentors` | Hook | `hooks/useMentors.js` | `hooks/useMentors.test.js` |  |
| ☐ | `useMentorshipRequests` | Hook | `hooks/useMentorshipRequests.js` | `hooks/useMentorshipRequests.test.js` |  |
| ☐ | `usePortfolio` | Hook | `hooks/usePortfolio.js` | `hooks/usePortfolio.test.js` |  |
| ☐ | `useQuizzes` | Hook | `hooks/useQuizzes.js` | `hooks/useQuizzes.test.js` |  |
| ☐ | `useSkillAssessments` | Hook | `hooks/useSkillAssessments.js` | `hooks/useSkillAssessments.test.js` |  |
| ☐ | `useTrainingCertificates` | Hook | `hooks/useTrainingCertificates.js` | `hooks/useTrainingCertificates.test.js` |  |
| ☐ | `academyMemberProfilesService` | API service | `services/academyMemberProfilesService.js` | `services/academyMemberProfilesService.test.js` |  |
| ☐ | `assignmentsService` | API service | `services/assignmentsService.js` | `services/assignmentsService.test.js` |  |
| ☐ | `courseCategoriesService` | API service | `services/courseCategoriesService.js` | `services/courseCategoriesService.test.js` |  |
| ☐ | `coursesService` | API service | `services/coursesService.js` | `services/coursesService.test.js` |  |
| ☐ | `enrollmentsService` | API service | `services/enrollmentsService.js` | `services/enrollmentsService.test.js` |  |
| ☐ | `examsService` | API service | `services/examsService.js` | `services/examsService.test.js` |  |
| ☐ | `heritageSkillsService` | API service | `services/heritageSkillsService.js` | `services/heritageSkillsService.test.js` |  |
| ☐ | `learningRoadmapsService` | API service | `services/learningRoadmapsService.js` | `services/learningRoadmapsService.test.js` |  |
| ☐ | `liveClassesService` | API service | `services/liveClassesService.js` | `services/liveClassesService.test.js` |  |
| ☐ | `mentorFeedbackService` | API service | `services/mentorFeedbackService.js` | `services/mentorFeedbackService.test.js` |  |
| ☐ | `mentorMatchingService` | API service | `services/mentorMatchingService.js` | `services/mentorMatchingService.test.js` |  |
| ☐ | `mentorshipRequestsService` | API service | `services/mentorshipRequestsService.js` | `services/mentorshipRequestsService.test.js` |  |
| ☐ | `mentorsService` | API service | `services/mentorsService.js` | `services/mentorsService.test.js` |  |
| ☐ | `portfolioService` | API service | `services/portfolioService.js` | `services/portfolioService.test.js` |  |
| ☐ | `quizzesService` | API service | `services/quizzesService.js` | `services/quizzesService.test.js` |  |
| ☐ | `skillAssessmentsService` | API service | `services/skillAssessmentsService.js` | `services/skillAssessmentsService.test.js` |  |
| ☐ | `trainingCertificatesService` | API service | `services/trainingCertificatesService.js` | `services/trainingCertificatesService.test.js` |  |

<a id="fe-admin"></a>

### Admin

SuperAdmin workspace: users, heritage and marketplace records, approvals, inspections, partnership oversight, producer intelligence.

**Folder:** `features/Admin/` · **Units:** 30 · **Phase:** 1 · **Partly tested already:** 1

Contains: 22 pages, 1 page config, 3 hooks, 4 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AdminDashboard` | Page | `pages/Admin/AdminDashboard.jsx` | `pages/AdminDashboard.test.jsx` |  |
| ☐ | `AdminMarketplace` | Page | `pages/Admin/AdminMarketplace.jsx` | `pages/AdminMarketplace.test.jsx` |  |
| ☐ | `AdminModeration` | Page | `pages/Admin/AdminModeration.jsx` | `pages/AdminModeration.test.jsx` |  |
| ☐ | `AdminResources` | Page | `pages/Admin/AdminResources.jsx` | `pages/AdminResources.test.jsx` |  |
| ☐ | `AdminSecurity` | Page | `pages/Admin/AdminSecurity.jsx` | `pages/AdminSecurity.test.jsx` |  |
| ☐ | `AdminUI` | Page | `pages/Admin/AdminUI.jsx` | `pages/AdminUI.test.jsx` |  |
| ☐ | `AdminUsers` | Page | `pages/Admin/AdminUsers.jsx` | `pages/AdminUsers.test.jsx` |  |
| ☐ | `AdminWorkspace` | Page | `pages/Admin/AdminWorkspace.jsx` | `pages/AdminWorkspace.test.jsx` |  |
| ☐ | `CMS` | Page | `pages/Admin/CMS.jsx` | `pages/CMS.test.jsx` |  |
| ☐ | `ExpertiseCertificates` | Page | `pages/Admin/ExpertiseCertificates.jsx` | `pages/ExpertiseCertificates.test.jsx` |  |
| ☐ | `HeritageManagement` | Page | `pages/Admin/HeritageManagement.jsx` | `pages/HeritageManagement.test.jsx` |  |
| ☐ | `MarketplaceMonitoring` | Page | `pages/Admin/MarketplaceMonitoring.jsx` | `pages/MarketplaceMonitoring.test.jsx` |  |
| ☐ | `ProcurementInspections` | Page | `pages/Admin/ProcurementInspections.jsx` | `pages/ProcurementInspections.test.jsx` |  |
| ☐ | `ProducerIntelligenceDashboard` | Page | `pages/Admin/ProducerIntelligenceDashboard.jsx` | `pages/ProducerIntelligenceDashboard.test.jsx` |  |
| ☐ | `ProducerIntelligenceDetail` | Page | `pages/Admin/ProducerIntelligenceDetail.jsx` | `pages/ProducerIntelligenceDetail.test.jsx` |  |
| ☐ | `ProducerPartnershipAgreements` | Page | `pages/Admin/ProducerPartnershipAgreements.jsx` | `pages/ProducerPartnershipAgreements.test.jsx` |  |
| ☐ | `ProducerPartnershipAuctions` | Page | `pages/Admin/ProducerPartnershipAuctions.jsx` | `pages/ProducerPartnershipAuctions.test.jsx` |  |
| ☐ | `ProducerPartnershipSettlements` | Page | `pages/Admin/ProducerPartnershipSettlements.jsx` | `pages/ProducerPartnershipSettlements.test.jsx` |  |
| ☐ | `ProfileApprovals` | Page | `pages/Admin/ProfileApprovals.jsx` | `pages/ProfileApprovals.test.jsx` |  |
| ☐ | `SecurityCenter` | Page | `pages/Admin/SecurityCenter.jsx` | `pages/SecurityCenter.test.jsx` |  |
| ☐ | `SupportOversight` | Page | `pages/Admin/SupportOversight.jsx` | `pages/SupportOversight.test.jsx` |  |
| ☐ | `UserManagement` | Page | `pages/Admin/UserManagement.jsx` | `pages/UserManagement.test.jsx` |  |
| ☐ | `adminConfig` | Page config | `pages/Admin/adminConfig.js` | `pages/adminConfig.test.js` | partly tested by `frontend/scripts/test-admin-contracts.mjs` |
| ☐ | `useAdminUsers` | Hook | `hooks/useAdminUsers.js` | `hooks/useAdminUsers.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useProducerIntelligence` | Hook | `hooks/useProducerIntelligence.js` | `hooks/useProducerIntelligence.test.js` |  |
| ☐ | `useProducerPartnershipAgreements` | Hook | `hooks/useProducerPartnershipAgreements.js` | `hooks/useProducerPartnershipAgreements.test.js` |  |
| ☐ | `adminUsersService` | API service | `services/adminUsersService.js` | `services/adminUsersService.test.js` |  |
| ☐ | `producerMonthlyReportsService` | API service | `services/producerMonthlyReportsService.js` | `services/producerMonthlyReportsService.test.js` |  |
| ☐ | `producerPartnershipAgreementService` | API service | `services/producerPartnershipAgreementService.js` | `services/producerPartnershipAgreementService.test.js` |  |
| ☐ | `superAdminService` | API service | `services/superAdminService.js` | `services/superAdminService.test.js` |  |

<a id="fe-apprenticestudent"></a>

### ApprenticeStudent

Apprentice / Student pages: browse programmes and my apprenticeships.

**Folder:** `features/ApprenticeStudent/` · **Units:** 8 · **Phase:** 4

Contains: 3 pages, 2 hooks, 3 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ApprenticeStudentPage` | Page | `pages/ApprenticeStudent/ApprenticeStudentPage.jsx` | `pages/ApprenticeStudentPage.test.jsx` |  |
| ☐ | `BrowsePrograms` | Page | `pages/ApprenticeStudent/BrowsePrograms.jsx` | `pages/BrowsePrograms.test.jsx` |  |
| ☐ | `MyApprenticeships` | Page | `pages/ApprenticeStudent/MyApprenticeships.jsx` | `pages/MyApprenticeships.test.jsx` |  |
| ☐ | `useApprenticeEnrollments` | Hook | `hooks/useApprenticeEnrollments.js` | `hooks/useApprenticeEnrollments.test.js` |  |
| ☐ | `useProgramApplications` | Hook | `hooks/useProgramApplications.js` | `hooks/useProgramApplications.test.js` |  |
| ☐ | `apprenticeEnrollmentsService` | API service | `services/apprenticeEnrollmentsService.js` | `services/apprenticeEnrollmentsService.test.js` |  |
| ☐ | `apprenticeshipProgramsService` | API service | `services/apprenticeshipProgramsService.js` | `services/apprenticeshipProgramsService.test.js` |  |
| ☐ | `programApplicationsService` | API service | `services/programApplicationsService.js` | `services/programApplicationsService.test.js` |  |

<a id="fe-appshell"></a>

### AppShell

What every page runs inside: app root, router and route guards, layouts, the sidebar and header, auth and theme stores, API client and interceptors, utils, config and navigation data.

**Folder:** `features/AppShell/` · **Units:** 49 · **Phase:** 1 · **Partly tested already:** 3

Contains: 3 pages, 14 components, 2 hooks, 2 API services, 3 stores, 1 context, 3 layouts, 4 routes, 11 utils, 2 lib, 2 config, 1 navigation data, 1 app root.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `NotFoundPage` | Page | `pages/NotFoundPage.jsx` | `pages/NotFoundPage.test.jsx` |  |
| ☐ | `RouteErrorPage` | Page | `pages/RouteErrorPage.jsx` | `pages/RouteErrorPage.test.jsx` |  |
| ☐ | `UnauthorizedPage` | Page | `pages/UnauthorizedPage.jsx` | `pages/UnauthorizedPage.test.jsx` |  |
| ☐ | `Breadcrumbs` | Component | `components/layout/Breadcrumbs.jsx` | `components/Breadcrumbs.test.jsx` |  |
| ☐ | `DashboardFooter` | Component | `components/layout/DashboardFooter.jsx` | `components/DashboardFooter.test.jsx` |  |
| ☐ | `Footer` | Component | `components/layout/Footer.jsx` | `components/Footer.test.jsx` |  |
| ☐ | `GlobalSearch` | Component | `components/layout/GlobalSearch.jsx` | `components/GlobalSearch.test.jsx` |  |
| ☐ | `HelplineChip` | Component | `components/layout/HelplineChip.jsx` | `components/HelplineChip.test.jsx` |  |
| ☐ | `LanguageMenu` | Component | `components/layout/LanguageMenu.jsx` | `components/LanguageMenu.test.jsx` |  |
| ☐ | `MegaMenu` | Component | `components/layout/MegaMenu.jsx` | `components/MegaMenu.test.jsx` |  |
| ☐ | `Navbar` | Component | `components/layout/Navbar.jsx` | `components/Navbar.test.jsx` |  |
| ☐ | `NavigationIcon` | Component | `components/layout/NavigationIcon.jsx` | `components/NavigationIcon.test.jsx` |  |
| ☐ | `NotificationPanel` | Component | `components/layout/NotificationPanel.jsx` | `components/NotificationPanel.test.jsx` | not imported anywhere (check if still used before testing) |
| ☐ | `ProfileDropdown` | Component | `components/layout/ProfileDropdown.jsx` | `components/ProfileDropdown.test.jsx` |  |
| ☐ | `Sidebar` | Component | `components/layout/Sidebar.jsx` | `components/Sidebar.test.jsx` |  |
| ☐ | `WorkspaceIntro` | Component | `components/layout/WorkspaceIntro.jsx` | `components/WorkspaceIntro.test.jsx` |  |
| ☐ | `workspaceNavigation` | Component | `components/layout/workspaceNavigation.js` | `components/workspaceNavigation.test.js` | partly tested by `frontend/scripts/test-design-navigation.mjs` |
| ☐ | `useBackLogoutGuard` | Hook | `hooks/useBackLogoutGuard.js` | `hooks/useBackLogoutGuard.test.js` |  |
| ☐ | `useLogoutFlow` | Hook | `hooks/useLogoutFlow.js` | `hooks/useLogoutFlow.test.js` |  |
| ☐ | `apiClient` | API service | `services/apiClient.js` | `services/apiClient.test.js` |  |
| ☐ | `axiosInterceptor` | API service | `services/interceptors/axiosInterceptor.js` | `services/axiosInterceptor.test.js` |  |
| ☐ | `appStore` | Store | `stores/appStore.js` | `stores/appStore.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useAuthStore` | Store | `stores/useAuthStore.js` | `stores/useAuthStore.test.js` |  |
| ☐ | `useThemeStore` | Store | `stores/useThemeStore.js` | `stores/useThemeStore.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `ThemeContext` | Context | `contexts/ThemeContext.jsx` | `contexts/ThemeContext.test.jsx` |  |
| ☐ | `AuthLayout` | Layout | `layouts/AuthLayout.jsx` | `layouts/AuthLayout.test.jsx` |  |
| ☐ | `DashboardLayout` | Layout | `layouts/DashboardLayout.jsx` | `layouts/DashboardLayout.test.jsx` |  |
| ☐ | `RootLayout` | Layout | `layouts/RootLayout.jsx` | `layouts/RootLayout.test.jsx` |  |
| ☐ | `ProtectedRoute` | Route | `routes/ProtectedRoute.jsx` | `routes/ProtectedRoute.test.jsx` |  |
| ☐ | `RoleBasedRoute` | Route | `routes/RoleBasedRoute.jsx` | `routes/RoleBasedRoute.test.jsx` |  |
| ☐ | `routePaths` | Route | `routes/routePaths.js` | `routes/routePaths.test.js` | partly tested by `frontend/scripts/test-launch-contracts.mjs (partial)` |
| ☐ | `router` | Route | `routes/router.jsx` | `routes/router.test.jsx` |  |
| ☐ | `apiError` | Util | `utils/apiError.js` | `utils/apiError.test.js` |  |
| ☐ | `constants` | Util | `utils/constants.js` | `utils/constants.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `jwt` | Util | `utils/jwt.js` | `utils/jwt.test.js` |  |
| ☐ | `mappers` | Util | `utils/mappers.js` | `utils/mappers.test.js` |  |
| ☐ | `productAdapters` | Util | `utils/productAdapters.js` | `utils/productAdapters.test.js` |  |
| ☐ | `roles` | Util | `utils/roles.js` | `utils/roles.test.js` |  |
| ☐ | `storage` | Util | `utils/storage.js` | `utils/storage.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `tourismAdapters` | Util | `utils/tourismAdapters.js` | `utils/tourismAdapters.test.js` |  |
| ☐ | `tourismLocation` | Util | `utils/tourismLocation.js` | `utils/tourismLocation.test.js` |  |
| ☐ | `validation` | Util | `utils/validation.js` | `utils/validation.test.js` |  |
| ☐ | `villageAdapters` | Util | `utils/villageAdapters.js` | `utils/villageAdapters.test.js` |  |
| ☐ | `confirm` | Lib | `lib/confirm.js` | `lib/confirm.test.js` |  |
| ☐ | `queryClient` | Lib | `lib/queryClient.js` | `lib/queryClient.test.js` |  |
| ☐ | `runtime` | Config | `config/runtime.js` | `config/runtime.test.js` |  |
| ☐ | `support` | Config | `config/support.js` | `config/support.test.js` |  |
| ☐ | `navigation` | Navigation data | `data/navigation.js` | `data/navigation.test.js` | partly tested by `frontend/scripts/test-design-navigation.mjs` |
| ☐ | `App` | App root | `App.jsx` | `app/App.test.jsx` |  |

<a id="fe-auth"></a>

### Auth

Login, register, forgot password and reset password.

**Folder:** `features/Auth/` · **Units:** 8 · **Phase:** 1

Contains: 5 pages, 1 component, 1 hook, 1 API service.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `accountTypes` | Page | `pages/Auth/accountTypes.jsx` | `pages/accountTypes.test.jsx` |  |
| ☐ | `ForgotPasswordPage` | Page | `pages/Auth/ForgotPasswordPage.jsx` | `pages/ForgotPasswordPage.test.jsx` |  |
| ☐ | `LoginPage` | Page | `pages/Auth/LoginPage.jsx` | `pages/LoginPage.test.jsx` |  |
| ☐ | `RegisterPage` | Page | `pages/Auth/RegisterPage.jsx` | `pages/RegisterPage.test.jsx` |  |
| ☐ | `ResetPasswordPage` | Page | `pages/Auth/ResetPasswordPage.jsx` | `pages/ResetPasswordPage.test.jsx` |  |
| ☐ | `AuthBootstrap` | Component | `components/auth/AuthBootstrap.jsx` | `components/AuthBootstrap.test.jsx` |  |
| ☐ | `useAuth` | Hook | `hooks/useAuth.js` | `hooks/useAuth.test.js` |  |
| ☐ | `authService` | API service | `services/authService.js` | `services/authService.test.js` |  |

<a id="fe-businesspartner"></a>

### BusinessPartner

Business Partner dashboard: contracts, quotations, procurement, partnerships, sponsorship and investment, supplier discovery and matching, analytics.

**Folder:** `features/BusinessPartner/` · **Units:** 44 · **Phase:** 3

Contains: 18 pages, 4 components, 11 hooks, 11 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AiIntelligence` | Page | `pages/BusinessPartner/AiIntelligence.jsx` | `pages/AiIntelligence.test.jsx` |  |
| ☐ | `Analytics` | Page | `pages/BusinessPartner/Analytics.jsx` | `pages/Analytics.test.jsx` |  |
| ☐ | `BusinessPartnerDashboard` | Page | `pages/BusinessPartner/BusinessPartnerDashboard.jsx` | `pages/BusinessPartnerDashboard.test.jsx` |  |
| ☐ | `Contracts` | Page | `pages/BusinessPartner/Contracts.jsx` | `pages/Contracts.test.jsx` |  |
| ☐ | `DesignCollaborations` | Page | `pages/BusinessPartner/DesignCollaborations.jsx` | `pages/DesignCollaborations.test.jsx` |  |
| ☐ | `InvestmentMarketplace` | Page | `pages/BusinessPartner/InvestmentMarketplace.jsx` | `pages/InvestmentMarketplace.test.jsx` |  |
| ☐ | `ManufacturingPartnerships` | Page | `pages/BusinessPartner/ManufacturingPartnerships.jsx` | `pages/ManufacturingPartnerships.test.jsx` |  |
| ☐ | `PartnershipAgreements` | Page | `pages/BusinessPartner/PartnershipAgreements.jsx` | `pages/PartnershipAgreements.test.jsx` |  |
| ☐ | `PartnershipAuctions` | Page | `pages/BusinessPartner/PartnershipAuctions.jsx` | `pages/PartnershipAuctions.test.jsx` |  |
| ☐ | `Procurements` | Page | `pages/BusinessPartner/Procurements.jsx` | `pages/Procurements.test.jsx` |  |
| ☐ | `ProducerComparison` | Page | `pages/BusinessPartner/ProducerComparison.jsx` | `pages/ProducerComparison.test.jsx` |  |
| ☐ | `ProductDevelopment` | Page | `pages/BusinessPartner/ProductDevelopment.jsx` | `pages/ProductDevelopment.test.jsx` |  |
| ☐ | `ProductIntelligence` | Page | `pages/BusinessPartner/ProductIntelligence.jsx` | `pages/ProductIntelligence.test.jsx` |  |
| ☐ | `Profile` | Page | `pages/BusinessPartner/Profile.jsx` | `pages/Profile.test.jsx` |  |
| ☐ | `Quotations` | Page | `pages/BusinessPartner/Quotations.jsx` | `pages/Quotations.test.jsx` |  |
| ☐ | `SponsorshipMarketplace` | Page | `pages/BusinessPartner/SponsorshipMarketplace.jsx` | `pages/SponsorshipMarketplace.test.jsx` |  |
| ☐ | `SupplierDiscovery` | Page | `pages/BusinessPartner/SupplierDiscovery.jsx` | `pages/SupplierDiscovery.test.jsx` |  |
| ☐ | `SupplierMatching` | Page | `pages/BusinessPartner/SupplierMatching.jsx` | `pages/SupplierMatching.test.jsx` |  |
| ☐ | `PartnershipAgreementCard` | Component | `components/business/PartnershipAgreementCard.jsx` | `components/PartnershipAgreementCard.test.jsx` |  |
| ☐ | `ProcurementAdvanceInfo` | Component | `components/business/ProcurementAdvanceInfo.jsx` | `components/ProcurementAdvanceInfo.test.jsx` |  |
| ☐ | `ProducerBusinessProfilePanel` | Component | `components/business/ProducerBusinessProfilePanel.jsx` | `components/ProducerBusinessProfilePanel.test.jsx` |  |
| ☐ | `SupplierProfilePanel` | Component | `components/business/SupplierProfilePanel.jsx` | `components/SupplierProfilePanel.test.jsx` |  |
| ☐ | `useAiIntelligence` | Hook | `hooks/useAiIntelligence.js` | `hooks/useAiIntelligence.test.js` |  |
| ☐ | `useBusinessPartnerAnalytics` | Hook | `hooks/useBusinessPartnerAnalytics.js` | `hooks/useBusinessPartnerAnalytics.test.js` |  |
| ☐ | `useBusinessPartners` | Hook | `hooks/useBusinessPartners.js` | `hooks/useBusinessPartners.test.js` |  |
| ☐ | `useCsrSponsorship` | Hook | `hooks/useCsrSponsorship.js` | `hooks/useCsrSponsorship.test.js` |  |
| ☐ | `useProcurements` | Hook | `hooks/useProcurements.js` | `hooks/useProcurements.test.js` |  |
| ☐ | `useProducerComparison` | Hook | `hooks/useProducerComparison.js` | `hooks/useProducerComparison.test.js` |  |
| ☐ | `useProducerPartnershipAuctions` | Hook | `hooks/useProducerPartnershipAuctions.js` | `hooks/useProducerPartnershipAuctions.test.js` |  |
| ☐ | `useProducerPartnershipSettlements` | Hook | `hooks/useProducerPartnershipSettlements.js` | `hooks/useProducerPartnershipSettlements.test.js` |  |
| ☐ | `useProductIntelligence` | Hook | `hooks/useProductIntelligence.js` | `hooks/useProductIntelligence.test.js` |  |
| ☐ | `useQuotations` | Hook | `hooks/useQuotations.js` | `hooks/useQuotations.test.js` |  |
| ☐ | `useSupplierDiscovery` | Hook | `hooks/useSupplierDiscovery.js` | `hooks/useSupplierDiscovery.test.js` |  |
| ☐ | `aiIntelligenceService` | API service | `services/aiIntelligenceService.js` | `services/aiIntelligenceService.test.js` |  |
| ☐ | `businessPartnerAnalyticsService` | API service | `services/businessPartnerAnalyticsService.js` | `services/businessPartnerAnalyticsService.test.js` |  |
| ☐ | `businessPartnersService` | API service | `services/businessPartnersService.js` | `services/businessPartnersService.test.js` |  |
| ☐ | `csrSponsorshipService` | API service | `services/csrSponsorshipService.js` | `services/csrSponsorshipService.test.js` |  |
| ☐ | `procurementsService` | API service | `services/procurementsService.js` | `services/procurementsService.test.js` |  |
| ☐ | `producerComparisonService` | API service | `services/producerComparisonService.js` | `services/producerComparisonService.test.js` |  |
| ☐ | `producerPartnershipAuctionService` | API service | `services/producerPartnershipAuctionService.js` | `services/producerPartnershipAuctionService.test.js` |  |
| ☐ | `producerPartnershipSettlementService` | API service | `services/producerPartnershipSettlementService.js` | `services/producerPartnershipSettlementService.test.js` |  |
| ☐ | `productIntelligenceService` | API service | `services/productIntelligenceService.js` | `services/productIntelligenceService.test.js` |  |
| ☐ | `quotationsService` | API service | `services/quotationsService.js` | `services/quotationsService.test.js` |  |
| ☐ | `supplierDiscoveryService` | API service | `services/supplierDiscoveryService.js` | `services/supplierDiscoveryService.test.js` |  |

<a id="fe-customer"></a>

### Customer

Customer dashboard: shopping, orders, returns, refunds, community, messages, heritage passport, achievements and AI shopping tools.

**Folder:** `features/Customer/` · **Units:** 85 · **Phase:** 2

Contains: 38 pages, 1 component, 23 hooks, 23 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `Achievements` | Page | `pages/Customer/Achievements.jsx` | `pages/Achievements.test.jsx` |  |
| ☐ | `AIFashionMatching` | Page | `pages/Customer/AIFashionMatching.jsx` | `pages/AIFashionMatching.test.jsx` |  |
| ☐ | `AIGiftRecommendation` | Page | `pages/Customer/AIGiftRecommendation.jsx` | `pages/AIGiftRecommendation.test.jsx` |  |
| ☐ | `AIInteriorPreview` | Page | `pages/Customer/AIInteriorPreview.jsx` | `pages/AIInteriorPreview.test.jsx` |  |
| ☐ | `AISimilarProducts` | Page | `pages/Customer/AISimilarProducts.jsx` | `pages/AISimilarProducts.test.jsx` |  |
| ☐ | `AuctionDetails` | Page | `pages/Customer/AuctionDetails.jsx` | `pages/AuctionDetails.test.jsx` |  |
| ☐ | `AuctionMarketplace` | Page | `pages/Customer/AuctionMarketplace.jsx` | `pages/AuctionMarketplace.test.jsx` |  |
| ☐ | `BadgeCollection` | Page | `pages/Customer/BadgeCollection.jsx` | `pages/BadgeCollection.test.jsx` |  |
| ☐ | `Checkout` | Page | `pages/Customer/Checkout.jsx` | `pages/Checkout.test.jsx` |  |
| ☐ | `CommunityFeed` | Page | `pages/Customer/CommunityFeed.jsx` | `pages/CommunityFeed.test.jsx` |  |
| ☐ | `Complaints` | Page | `pages/Customer/Complaints.jsx` | `pages/Complaints.test.jsx` |  |
| ☐ | `CraftStory` | Page | `pages/Customer/CraftStory.jsx` | `pages/CraftStory.test.jsx` |  |
| ☐ | `CustomerDashboard` | Page | `pages/Customer/CustomerDashboard.jsx` | `pages/CustomerDashboard.test.jsx` |  |
| ☐ | `CustomerNotifications` | Page | `pages/Customer/CustomerNotifications.jsx` | `pages/CustomerNotifications.test.jsx` |  |
| ☐ | `CustomOrder` | Page | `pages/Customer/CustomOrder.jsx` | `pages/CustomOrder.test.jsx` |  |
| ☐ | `DiscussionForum` | Page | `pages/Customer/DiscussionForum.jsx` | `pages/DiscussionForum.test.jsx` |  |
| ☐ | `FavoriteVillages` | Page | `pages/Customer/FavoriteVillages.jsx` | `pages/FavoriteVillages.test.jsx` |  |
| ☐ | `FollowingProducers` | Page | `pages/Customer/FollowingProducers.jsx` | `pages/FollowingProducers.test.jsx` |  |
| ☐ | `HeritageCollection` | Page | `pages/Customer/HeritageCollection.jsx` | `pages/HeritageCollection.test.jsx` |  |
| ☐ | `HeritagePassport` | Page | `pages/Customer/HeritagePassport.jsx` | `pages/HeritagePassport.test.jsx` |  |
| ☐ | `ImpactDashboard` | Page | `pages/Customer/ImpactDashboard.jsx` | `pages/ImpactDashboard.test.jsx` |  |
| ☐ | `LiveShopping` | Page | `pages/Customer/LiveShopping.jsx` | `pages/LiveShopping.test.jsx` |  |
| ☐ | `Marketplace` | Page | `pages/Customer/Marketplace.jsx` | `pages/Marketplace.test.jsx` |  |
| ☐ | `Messages` | Page | `pages/Customer/Messages.jsx` | `pages/Messages.test.jsx` |  |
| ☐ | `OrderDetails` | Page | `pages/Customer/OrderDetails.jsx` | `pages/OrderDetails.test.jsx` |  |
| ☐ | `OrderHistory` | Page | `pages/Customer/OrderHistory.jsx` | `pages/OrderHistory.test.jsx` |  |
| ☐ | `OrderSuccess` | Page | `pages/Customer/OrderSuccess.jsx` | `pages/OrderSuccess.test.jsx` |  |
| ☐ | `ProducerProfile` | Page | `pages/Customer/ProducerProfile.jsx` | `pages/ProducerProfile.test.jsx` |  |
| ☐ | `ProducerStory` | Page | `pages/Customer/ProducerStory.jsx` | `pages/ProducerStory.test.jsx` |  |
| ☐ | `ProductDetails` | Page | `pages/Customer/ProductDetails.jsx` | `pages/ProductDetails.test.jsx` |  |
| ☐ | `PurchaseAnalytics` | Page | `pages/Customer/PurchaseAnalytics.jsx` | `pages/PurchaseAnalytics.test.jsx` |  |
| ☐ | `QuestionsAnswers` | Page | `pages/Customer/QuestionsAnswers.jsx` | `pages/QuestionsAnswers.test.jsx` |  |
| ☐ | `Refunds` | Page | `pages/Customer/Refunds.jsx` | `pages/Refunds.test.jsx` |  |
| ☐ | `Returns` | Page | `pages/Customer/Returns.jsx` | `pages/Returns.test.jsx` |  |
| ☐ | `SavedAddresses` | Page | `pages/Customer/SavedAddresses.jsx` | `pages/SavedAddresses.test.jsx` |  |
| ☐ | `ShoppingCart` | Page | `pages/Customer/ShoppingCart.jsx` | `pages/ShoppingCart.test.jsx` |  |
| ☐ | `Wishlist` | Page | `pages/Customer/Wishlist.jsx` | `pages/Wishlist.test.jsx` |  |
| ☐ | `WorkshopGallery` | Page | `pages/Customer/WorkshopGallery.jsx` | `pages/WorkshopGallery.test.jsx` |  |
| ☐ | `ReportProblemForm` | Component | `components/orders/ReportProblemForm.jsx` | `components/ReportProblemForm.test.jsx` |  |
| ☐ | `useAchievements` | Hook | `hooks/useAchievements.js` | `hooks/useAchievements.test.js` |  |
| ☐ | `useAiShopping` | Hook | `hooks/useAiShopping.js` | `hooks/useAiShopping.test.js` |  |
| ☐ | `useAnalytics` | Hook | `hooks/useAnalytics.js` | `hooks/useAnalytics.test.js` |  |
| ☐ | `useArCraftScan` | Hook | `hooks/useArCraftScan.js` | `hooks/useArCraftScan.test.js` |  |
| ☐ | `useAuctions` | Hook | `hooks/useAuctions.js` | `hooks/useAuctions.test.js` |  |
| ☐ | `useCraftStories` | Hook | `hooks/useCraftStories.js` | `hooks/useCraftStories.test.js` |  |
| ☐ | `useCustomOrders` | Hook | `hooks/useCustomOrders.js` | `hooks/useCustomOrders.test.js` |  |
| ☐ | `useDiscussions` | Hook | `hooks/useDiscussions.js` | `hooks/useDiscussions.test.js` |  |
| ☐ | `useHeritageIdentity` | Hook | `hooks/useHeritageIdentity.js` | `hooks/useHeritageIdentity.test.js` |  |
| ☐ | `useImpact` | Hook | `hooks/useImpact.js` | `hooks/useImpact.test.js` |  |
| ☐ | `useLiveEvents` | Hook | `hooks/useLiveEvents.js` | `hooks/useLiveEvents.test.js` |  |
| ☐ | `useOrderComplaints` | Hook | `hooks/useOrderComplaints.js` | `hooks/useOrderComplaints.test.js` |  |
| ☐ | `useOrders` | Hook | `hooks/useOrders.js` | `hooks/useOrders.test.js` |  |
| ☐ | `usePassport` | Hook | `hooks/usePassport.js` | `hooks/usePassport.test.js` |  |
| ☐ | `useProducerFollows` | Hook | `hooks/useProducerFollows.js` | `hooks/useProducerFollows.test.js` |  |
| ☐ | `useProducerStories` | Hook | `hooks/useProducerStories.js` | `hooks/useProducerStories.test.js` |  |
| ☐ | `useQRVerification` | Hook | `hooks/useQRVerification.js` | `hooks/useQRVerification.test.js` |  |
| ☐ | `useQuestions` | Hook | `hooks/useQuestions.js` | `hooks/useQuestions.test.js` |  |
| ☐ | `useRecommendations` | Hook | `hooks/useRecommendations.js` | `hooks/useRecommendations.test.js` |  |
| ☐ | `useReviews` | Hook | `hooks/useReviews.js` | `hooks/useReviews.test.js` |  |
| ☐ | `useTraceability` | Hook | `hooks/useTraceability.js` | `hooks/useTraceability.test.js` |  |
| ☐ | `useWishlist` | Hook | `hooks/useWishlist.js` | `hooks/useWishlist.test.js` |  |
| ☐ | `useWorkshopGallery` | Hook | `hooks/useWorkshopGallery.js` | `hooks/useWorkshopGallery.test.js` |  |
| ☐ | `achievementsService` | API service | `services/achievementsService.js` | `services/achievementsService.test.js` |  |
| ☐ | `aiShoppingService` | API service | `services/aiShoppingService.js` | `services/aiShoppingService.test.js` |  |
| ☐ | `analyticsService` | API service | `services/analyticsService.js` | `services/analyticsService.test.js` |  |
| ☐ | `arCraftScanService` | API service | `services/arCraftScanService.js` | `services/arCraftScanService.test.js` |  |
| ☐ | `auctionsService` | API service | `services/auctionsService.js` | `services/auctionsService.test.js` |  |
| ☐ | `craftStoriesService` | API service | `services/craftStoriesService.js` | `services/craftStoriesService.test.js` |  |
| ☐ | `customOrdersService` | API service | `services/customOrdersService.js` | `services/customOrdersService.test.js` |  |
| ☐ | `discussionsService` | API service | `services/discussionsService.js` | `services/discussionsService.test.js` |  |
| ☐ | `heritageIdentityService` | API service | `services/heritageIdentityService.js` | `services/heritageIdentityService.test.js` |  |
| ☐ | `impactService` | API service | `services/impactService.js` | `services/impactService.test.js` |  |
| ☐ | `liveEventsService` | API service | `services/liveEventsService.js` | `services/liveEventsService.test.js` |  |
| ☐ | `orderComplaintsService` | API service | `services/orderComplaintsService.js` | `services/orderComplaintsService.test.js` |  |
| ☐ | `ordersService` | API service | `services/ordersService.js` | `services/ordersService.test.js` |  |
| ☐ | `passportService` | API service | `services/passportService.js` | `services/passportService.test.js` |  |
| ☐ | `producerFollowsService` | API service | `services/producerFollowsService.js` | `services/producerFollowsService.test.js` |  |
| ☐ | `producerStoriesService` | API service | `services/producerStoriesService.js` | `services/producerStoriesService.test.js` |  |
| ☐ | `qrVerificationService` | API service | `services/qrVerificationService.js` | `services/qrVerificationService.test.js` |  |
| ☐ | `questionsService` | API service | `services/questionsService.js` | `services/questionsService.test.js` |  |
| ☐ | `recommendationsService` | API service | `services/recommendationsService.js` | `services/recommendationsService.test.js` |  |
| ☐ | `reviewsService` | API service | `services/reviewsService.js` | `services/reviewsService.test.js` |  |
| ☐ | `traceabilityService` | API service | `services/traceabilityService.js` | `services/traceabilityService.test.js` |  |
| ☐ | `wishlistService` | API service | `services/wishlistService.js` | `services/wishlistService.test.js` |  |
| ☐ | `workshopGalleryService` | API service | `services/workshopGalleryService.js` | `services/workshopGalleryService.test.js` |  |

<a id="fe-dashboard"></a>

### Dashboard

The shared `/dashboard` pages every signed-in user can open: home, profile, settings, messages, notifications, jobs.

**Folder:** `features/Dashboard/` · **Units:** 19 · **Phase:** 4

Contains: 12 pages, 4 components, 2 hooks, 1 API service.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `DashboardAcademy` | Page | `pages/Dashboard/DashboardAcademy.jsx` | `pages/DashboardAcademy.test.jsx` |  |
| ☐ | `DashboardAnalytics` | Page | `pages/Dashboard/DashboardAnalytics.jsx` | `pages/DashboardAnalytics.test.jsx` |  |
| ☐ | `DashboardCommunity` | Page | `pages/Dashboard/DashboardCommunity.jsx` | `pages/DashboardCommunity.test.jsx` |  |
| ☐ | `DashboardExplore` | Page | `pages/Dashboard/DashboardExplore.jsx` | `pages/DashboardExplore.test.jsx` |  |
| ☐ | `DashboardHome` | Page | `pages/Dashboard/DashboardHome.jsx` | `pages/DashboardHome.test.jsx` |  |
| ☐ | `DashboardMarketplace` | Page | `pages/Dashboard/DashboardMarketplace.jsx` | `pages/DashboardMarketplace.test.jsx` |  |
| ☐ | `DashboardMessages` | Page | `pages/Dashboard/DashboardMessages.jsx` | `pages/DashboardMessages.test.jsx` |  |
| ☐ | `DashboardNotifications` | Page | `pages/Dashboard/DashboardNotifications.jsx` | `pages/DashboardNotifications.test.jsx` |  |
| ☐ | `DashboardProfile` | Page | `pages/Dashboard/DashboardProfile.jsx` | `pages/DashboardProfile.test.jsx` |  |
| ☐ | `DashboardSettings` | Page | `pages/Dashboard/DashboardSettings.jsx` | `pages/DashboardSettings.test.jsx` |  |
| ☐ | `DashboardTourism` | Page | `pages/Dashboard/DashboardTourism.jsx` | `pages/DashboardTourism.test.jsx` |  |
| ☐ | `JobBoard` | Page | `pages/Dashboard/JobBoard.jsx` | `pages/JobBoard.test.jsx` |  |
| ☐ | `ProfileForm` | Component | `components/profile/ProfileForm.jsx` | `components/ProfileForm.test.jsx` |  |
| ☐ | `ProfilePhotoField` | Component | `components/profile/ProfilePhotoField.jsx` | `components/ProfilePhotoField.test.jsx` |  |
| ☐ | `ProfileStatusBanner` | Component | `components/profile/ProfileStatusBanner.jsx` | `components/ProfileStatusBanner.test.jsx` |  |
| ☐ | `UserAvatar` | Component | `components/profile/UserAvatar.jsx` | `components/UserAvatar.test.jsx` |  |
| ☐ | `useJobBoard` | Hook | `hooks/useJobBoard.js` | `hooks/useJobBoard.test.js` |  |
| ☐ | `useTheme` | Hook | `hooks/useTheme.js` | `hooks/useTheme.test.js` |  |
| ☐ | `jobBoardService` | API service | `services/jobBoardService.js` | `services/jobBoardService.test.js` |  |

<a id="fe-explore"></a>

### Explore

Public heritage explorer: districts, villages, crafts, producers, UNESCO records, digital museum.

**Folder:** `features/Explore/` · **Units:** 19 · **Phase:** 5

Contains: 11 pages, 2 components, 3 hooks, 3 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `CraftDetails` | Page | `pages/Explore/CraftDetails.jsx` | `pages/CraftDetails.test.jsx` |  |
| ☐ | `Crafts` | Page | `pages/Explore/Crafts.jsx` | `pages/Crafts.test.jsx` |  |
| ☐ | `DigitalMuseum` | Page | `pages/Explore/DigitalMuseum.jsx` | `pages/DigitalMuseum.test.jsx` |  |
| ☐ | `DistrictDetails` | Page | `pages/Explore/DistrictDetails.jsx` | `pages/DistrictDetails.test.jsx` |  |
| ☐ | `Districts` | Page | `pages/Explore/Districts.jsx` | `pages/Districts.test.jsx` |  |
| ☐ | `ExploreHome` | Page | `pages/Explore/ExploreHome.jsx` | `pages/ExploreHome.test.jsx` |  |
| ☐ | `ProducerDetails` | Page | `pages/Explore/ProducerDetails.jsx` | `pages/ProducerDetails.test.jsx` |  |
| ☐ | `Producers` | Page | `pages/Explore/Producers.jsx` | `pages/Producers.test.jsx` |  |
| ☐ | `Unesco` | Page | `pages/Explore/Unesco.jsx` | `pages/Unesco.test.jsx` |  |
| ☐ | `VillageDetails` | Page | `pages/Explore/VillageDetails.jsx` | `pages/VillageDetails.test.jsx` |  |
| ☐ | `Villages` | Page | `pages/Explore/Villages.jsx` | `pages/Villages.test.jsx` |  |
| ☐ | `CraftHeritageCatalog` | Component | `components/heritage/CraftHeritageCatalog.jsx` | `components/CraftHeritageCatalog.test.jsx` |  |
| ☐ | `CraftHeritageDetails` | Component | `components/heritage/CraftHeritageDetails.jsx` | `components/CraftHeritageDetails.test.jsx` |  |
| ☐ | `useCatalog` | Hook | `hooks/queries/useCatalog.js` | `hooks/useCatalog.test.js` |  |
| ☐ | `useLocalCuisines` | Hook | `hooks/useLocalCuisines.js` | `hooks/useLocalCuisines.test.js` |  |
| ☐ | `useUnescoRecords` | Hook | `hooks/useUnescoRecords.js` | `hooks/useUnescoRecords.test.js` |  |
| ☐ | `catalogService` | API service | `services/catalogService.js` | `services/catalogService.test.js` |  |
| ☐ | `localCuisinesService` | API service | `services/localCuisinesService.js` | `services/localCuisinesService.test.js` |  |
| ☐ | `unescoRecordsService` | API service | `services/unescoRecordsService.js` | `services/unescoRecordsService.test.js` |  |

<a id="fe-government"></a>

### Government

Government & NGO dashboard: organisation profile, artisan support, reports and forecasts, policy compliance, complaints, funding.

**Folder:** `features/Government/` · **Units:** 24 · **Phase:** 4

Contains: 8 pages, 8 hooks, 8 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ArtisanSupportCases` | Page | `pages/Government/ArtisanSupportCases.jsx` | `pages/ArtisanSupportCases.test.jsx` |  |
| ☐ | `ComplaintsMonitoring` | Page | `pages/Government/ComplaintsMonitoring.jsx` | `pages/ComplaintsMonitoring.test.jsx` |  |
| ☐ | `Funding` | Page | `pages/Government/Funding.jsx` | `pages/Funding.test.jsx` |  |
| ☐ | `GovernmentPage` | Page | `pages/Government/GovernmentPage.jsx` | `pages/GovernmentPage.test.jsx` |  |
| ☐ | `GovProducerDashboard` | Page | `pages/Government/GovProducerDashboard.jsx` | `pages/GovProducerDashboard.test.jsx` |  |
| ☐ | `GovReportsForecasts` | Page | `pages/Government/GovReportsForecasts.jsx` | `pages/GovReportsForecasts.test.jsx` |  |
| ☐ | `OrganizationProfile` | Page | `pages/Government/OrganizationProfile.jsx` | `pages/OrganizationProfile.test.jsx` |  |
| ☐ | `PolicyCompliance` | Page | `pages/Government/PolicyCompliance.jsx` | `pages/PolicyCompliance.test.jsx` |  |
| ☐ | `useArtisanSupport` | Hook | `hooks/useArtisanSupport.js` | `hooks/useArtisanSupport.test.js` |  |
| ☐ | `useComplaintsMonitoring` | Hook | `hooks/useComplaintsMonitoring.js` | `hooks/useComplaintsMonitoring.test.js` |  |
| ☐ | `useFunding` | Hook | `hooks/useFunding.js` | `hooks/useFunding.test.js` |  |
| ☐ | `useGovProducerReports` | Hook | `hooks/useGovProducerReports.js` | `hooks/useGovProducerReports.test.js` |  |
| ☐ | `useGovReports` | Hook | `hooks/useGovReports.js` | `hooks/useGovReports.test.js` |  |
| ☐ | `useHeritageIntelligence` | Hook | `hooks/useHeritageIntelligence.js` | `hooks/useHeritageIntelligence.test.js` |  |
| ☐ | `useNationalDashboard` | Hook | `hooks/useNationalDashboard.js` | `hooks/useNationalDashboard.test.js` |  |
| ☐ | `usePolicyCompliance` | Hook | `hooks/usePolicyCompliance.js` | `hooks/usePolicyCompliance.test.js` |  |
| ☐ | `artisanSupportService` | API service | `services/artisanSupportService.js` | `services/artisanSupportService.test.js` |  |
| ☐ | `complaintsMonitoringService` | API service | `services/complaintsMonitoringService.js` | `services/complaintsMonitoringService.test.js` |  |
| ☐ | `fundingService` | API service | `services/fundingService.js` | `services/fundingService.test.js` |  |
| ☐ | `govProducerReportsService` | API service | `services/govProducerReportsService.js` | `services/govProducerReportsService.test.js` |  |
| ☐ | `govReportsService` | API service | `services/govReportsService.js` | `services/govReportsService.test.js` |  |
| ☐ | `heritageIntelligenceService` | API service | `services/heritageIntelligenceService.js` | `services/heritageIntelligenceService.test.js` |  |
| ☐ | `nationalDashboardService` | API service | `services/nationalDashboardService.js` | `services/nationalDashboardService.test.js` |  |
| ☐ | `policyComplianceService` | API service | `services/policyComplianceService.js` | `services/policyComplianceService.test.js` |  |

<a id="fe-home"></a>

### Home

The public home page.

**Folder:** `features/Home/` · **Units:** 4 · **Phase:** 5

Contains: 1 page, 2 components, 1 hook.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `HomePage` | Page | `pages/Home/HomePage.jsx` | `pages/HomePage.test.jsx` |  |
| ☐ | `HeritageGallery` | Component | `components/home/HeritageGallery.jsx` | `components/HeritageGallery.test.jsx` |  |
| ☐ | `PublishedContent` | Component | `components/home/PublishedContent.jsx` | `components/PublishedContent.test.jsx` |  |
| ☐ | `useHeritageGallery` | Hook | `hooks/useHeritageGallery.js` | `hooks/useHeritageGallery.test.js` |  |

<a id="fe-logisticspartner"></a>

### LogisticsPartner

Logistics dashboard: warehouses, stock, shipments, pickups, returns, delivery routes, AI logistics tools.

**Folder:** `features/LogisticsPartner/` · **Units:** 27 · **Phase:** 3

Contains: 9 pages, 1 component, 9 hooks, 8 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AiLogisticsTools` | Page | `pages/LogisticsPartner/AiLogisticsTools.jsx` | `pages/AiLogisticsTools.test.jsx` |  |
| ☐ | `DeliveryRoutes` | Page | `pages/LogisticsPartner/DeliveryRoutes.jsx` | `pages/DeliveryRoutes.test.jsx` |  |
| ☐ | `LogisticsPartnerPage` | Page | `pages/LogisticsPartner/LogisticsPartnerPage.jsx` | `pages/LogisticsPartnerPage.test.jsx` |  |
| ☐ | `PickupRequests` | Page | `pages/LogisticsPartner/PickupRequests.jsx` | `pages/PickupRequests.test.jsx` |  |
| ☐ | `Profile` | Page | `pages/LogisticsPartner/Profile.jsx` | `pages/Profile.test.jsx` |  |
| ☐ | `Returns` | Page | `pages/LogisticsPartner/Returns.jsx` | `pages/Returns.test.jsx` |  |
| ☐ | `Shipments` | Page | `pages/LogisticsPartner/Shipments.jsx` | `pages/Shipments.test.jsx` |  |
| ☐ | `Warehouses` | Page | `pages/LogisticsPartner/Warehouses.jsx` | `pages/Warehouses.test.jsx` |  |
| ☐ | `WarehouseStock` | Page | `pages/LogisticsPartner/WarehouseStock.jsx` | `pages/WarehouseStock.test.jsx` |  |
| ☐ | `LogisticsWorkspaceGuard` | Component | `components/logistics/LogisticsWorkspaceGuard.jsx` | `components/LogisticsWorkspaceGuard.test.jsx` |  |
| ☐ | `useAiLogistics` | Hook | `hooks/useAiLogistics.js` | `hooks/useAiLogistics.test.js` |  |
| ☐ | `useDeliveryRoutes` | Hook | `hooks/useDeliveryRoutes.js` | `hooks/useDeliveryRoutes.test.js` |  |
| ☐ | `useLogisticsDashboardStats` | Hook | `hooks/useLogisticsDashboardStats.js` | `hooks/useLogisticsDashboardStats.test.js` |  |
| ☐ | `useLogisticsPartners` | Hook | `hooks/useLogisticsPartners.js` | `hooks/useLogisticsPartners.test.js` |  |
| ☐ | `useLogisticsReturns` | Hook | `hooks/useLogisticsReturns.js` | `hooks/useLogisticsReturns.test.js` |  |
| ☐ | `usePickupRequests` | Hook | `hooks/usePickupRequests.js` | `hooks/usePickupRequests.test.js` |  |
| ☐ | `useShipments` | Hook | `hooks/useShipments.js` | `hooks/useShipments.test.js` |  |
| ☐ | `useWarehouses` | Hook | `hooks/useWarehouses.js` | `hooks/useWarehouses.test.js` |  |
| ☐ | `useWarehouseStock` | Hook | `hooks/useWarehouseStock.js` | `hooks/useWarehouseStock.test.js` |  |
| ☐ | `aiLogisticsService` | API service | `services/aiLogisticsService.js` | `services/aiLogisticsService.test.js` |  |
| ☐ | `deliveryRoutesService` | API service | `services/deliveryRoutesService.js` | `services/deliveryRoutesService.test.js` |  |
| ☐ | `logisticsPartnersService` | API service | `services/logisticsPartnersService.js` | `services/logisticsPartnersService.test.js` |  |
| ☐ | `logisticsReturnsService` | API service | `services/logisticsReturnsService.js` | `services/logisticsReturnsService.test.js` |  |
| ☐ | `pickupRequestsService` | API service | `services/pickupRequestsService.js` | `services/pickupRequestsService.test.js` |  |
| ☐ | `shipmentsService` | API service | `services/shipmentsService.js` | `services/shipmentsService.test.js` |  |
| ☐ | `warehousesService` | API service | `services/warehousesService.js` | `services/warehousesService.test.js` |  |
| ☐ | `warehouseStockService` | API service | `services/warehouseStockService.js` | `services/warehouseStockService.test.js` |  |

<a id="fe-marketplace"></a>

### Marketplace

Public marketplace: listings, product details, categories, auctions, wishlist, cart, checkout.

**Folder:** `features/Marketplace/` · **Units:** 15 · **Phase:** 2

Contains: 8 pages, 1 component, 3 hooks, 3 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `Auctions` | Page | `pages/Marketplace/Auctions.jsx` | `pages/Auctions.test.jsx` |  |
| ☐ | `Cart` | Page | `pages/Marketplace/Cart.jsx` | `pages/Cart.test.jsx` |  |
| ☐ | `Categories` | Page | `pages/Marketplace/Categories.jsx` | `pages/Categories.test.jsx` |  |
| ☐ | `Checkout` | Page | `pages/Marketplace/Checkout.jsx` | `pages/Checkout.test.jsx` |  |
| ☐ | `MarketplaceHome` | Page | `pages/Marketplace/MarketplaceHome.jsx` | `pages/MarketplaceHome.test.jsx` |  |
| ☐ | `ProductDetails` | Page | `pages/Marketplace/ProductDetails.jsx` | `pages/ProductDetails.test.jsx` |  |
| ☐ | `ProductListing` | Page | `pages/Marketplace/ProductListing.jsx` | `pages/ProductListing.test.jsx` |  |
| ☐ | `Wishlist` | Page | `pages/Marketplace/Wishlist.jsx` | `pages/Wishlist.test.jsx` |  |
| ☐ | `AiProductSearch` | Component | `components/marketplace/AiProductSearch.jsx` | `components/AiProductSearch.test.jsx` |  |
| ☐ | `useCart` | Hook | `hooks/useCart.js` | `hooks/useCart.test.js` |  |
| ☐ | `useProducerDirectory` | Hook | `hooks/useProducerDirectory.js` | `hooks/useProducerDirectory.test.js` |  |
| ☐ | `useProductSearch` | Hook | `hooks/useProductSearch.js` | `hooks/useProductSearch.test.js` |  |
| ☐ | `cartService` | API service | `services/cartService.js` | `services/cartService.test.js` |  |
| ☐ | `productSearchService` | API service | `services/productSearchService.js` | `services/productSearchService.test.js` |  |
| ☐ | `productsService` | API service | `services/productsService.js` | `services/productsService.test.js` |  |

<a id="fe-messaging"></a>

### Messaging

Chat components.

**Folder:** `features/Messaging/` · **Units:** 5 · **Phase:** 5

Contains: 3 components, 1 hook, 1 API service.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `DirectMessages` | Component | `components/messaging/DirectMessages.jsx` | `components/DirectMessages.test.jsx` |  |
| ☐ | `ImageAttachButton` | Component | `components/messaging/ImageAttachButton.jsx` | `components/ImageAttachButton.test.jsx` |  |
| ☐ | `QuickMessageDialog` | Component | `components/messaging/QuickMessageDialog.jsx` | `components/QuickMessageDialog.test.jsx` |  |
| ☐ | `useMessaging` | Hook | `hooks/useMessaging.js` | `hooks/useMessaging.test.js` |  |
| ☐ | `messagingService` | API service | `services/messagingService.js` | `services/messagingService.test.js` |  |

<a id="fe-news"></a>

### News

Published news and update pages.

**Folder:** `features/News/` · **Units:** 3 · **Phase:** 5

Contains: 3 pages.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `NewsDetails` | Page | `pages/News/NewsDetails.jsx` | `pages/NewsDetails.test.jsx` |  |
| ☐ | `NewsList` | Page | `pages/News/NewsList.jsx` | `pages/NewsList.test.jsx` |  |
| ☐ | `PublishedContentDetails` | Page | `pages/News/PublishedContentDetails.jsx` | `pages/PublishedContentDetails.test.jsx` |  |

<a id="fe-ngo"></a>

### NGO

NGO programmes page inside the Government & NGO dashboard.

**Folder:** `features/NGO/` · **Units:** 1 · **Phase:** 4

Contains: 1 page.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `NGOPage` | Page | `pages/NGO/NGOPage.jsx` | `pages/NGOPage.test.jsx` |  |

<a id="fe-notifications"></a>

### Notifications

Notification bell and inbox components.

**Folder:** `features/Notifications/` · **Units:** 3 · **Phase:** 1

Contains: 2 components, 1 hook.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `NotificationBell` | Component | `components/notifications/NotificationBell.jsx` | `components/NotificationBell.test.jsx` |  |
| ☐ | `NotificationCenter` | Component | `components/notifications/NotificationCenter.jsx` | `components/NotificationCenter.test.jsx` |  |
| ☐ | `useNotifications` | Hook | `hooks/useNotifications.js` | `hooks/useNotifications.test.js` |  |

<a id="fe-producer"></a>

### Producer

Producer (artisan) dashboard: products, inventory, orders, custom orders, auctions, partnerships, sustainability, live shopping.

**Folder:** `features/Producer/` · **Units:** 50 · **Phase:** 2

Contains: 26 pages, 1 component, 12 hooks, 11 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `Auctions` | Page | `pages/Producer/Auctions.jsx` | `pages/Auctions.test.jsx` |  |
| ☐ | `Complaints` | Page | `pages/Producer/Complaints.jsx` | `pages/Complaints.test.jsx` |  |
| ☐ | `Contracts` | Page | `pages/Producer/Contracts.jsx` | `pages/Contracts.test.jsx` |  |
| ☐ | `CsrSponsorship` | Page | `pages/Producer/CsrSponsorship.jsx` | `pages/CsrSponsorship.test.jsx` |  |
| ☐ | `CustomOrders` | Page | `pages/Producer/CustomOrders.jsx` | `pages/CustomOrders.test.jsx` |  |
| ☐ | `DesignCollaborations` | Page | `pages/Producer/DesignCollaborations.jsx` | `pages/DesignCollaborations.test.jsx` |  |
| ☐ | `Expertise` | Page | `pages/Producer/Expertise.jsx` | `pages/Expertise.test.jsx` |  |
| ☐ | `Inventory` | Page | `pages/Producer/Inventory.jsx` | `pages/Inventory.test.jsx` |  |
| ☐ | `InvestmentOpportunities` | Page | `pages/Producer/InvestmentOpportunities.jsx` | `pages/InvestmentOpportunities.test.jsx` |  |
| ☐ | `LiveShoppingManager` | Page | `pages/Producer/LiveShoppingManager.jsx` | `pages/LiveShoppingManager.test.jsx` |  |
| ☐ | `ManufacturingPartnerships` | Page | `pages/Producer/ManufacturingPartnerships.jsx` | `pages/ManufacturingPartnerships.test.jsx` |  |
| ☐ | `NewProductForm` | Page | `pages/Producer/NewProductForm.jsx` | `pages/NewProductForm.test.jsx` |  |
| ☐ | `Orders` | Page | `pages/Producer/Orders.jsx` | `pages/Orders.test.jsx` |  |
| ☐ | `PartnershipAgreements` | Page | `pages/Producer/PartnershipAgreements.jsx` | `pages/PartnershipAgreements.test.jsx` |  |
| ☐ | `Procurements` | Page | `pages/Producer/Procurements.jsx` | `pages/Procurements.test.jsx` |  |
| ☐ | `ProducerDashboard` | Page | `pages/Producer/ProducerDashboard.jsx` | `pages/ProducerDashboard.test.jsx` |  |
| ☐ | `ProducerInsights` | Page | `pages/Producer/ProducerInsights.jsx` | `pages/ProducerInsights.test.jsx` |  |
| ☐ | `ProductAttributes` | Page | `pages/Producer/ProductAttributes.jsx` | `pages/ProductAttributes.test.jsx` |  |
| ☐ | `ProductDevelopment` | Page | `pages/Producer/ProductDevelopment.jsx` | `pages/ProductDevelopment.test.jsx` |  |
| ☐ | `ProductOverview` | Page | `pages/Producer/ProductOverview.jsx` | `pages/ProductOverview.test.jsx` |  |
| ☐ | `Products` | Page | `pages/Producer/Products.jsx` | `pages/Products.test.jsx` |  |
| ☐ | `Questions` | Page | `pages/Producer/Questions.jsx` | `pages/Questions.test.jsx` |  |
| ☐ | `Quotations` | Page | `pages/Producer/Quotations.jsx` | `pages/Quotations.test.jsx` |  |
| ☐ | `Returns` | Page | `pages/Producer/Returns.jsx` | `pages/Returns.test.jsx` |  |
| ☐ | `SupportCases` | Page | `pages/Producer/SupportCases.jsx` | `pages/SupportCases.test.jsx` |  |
| ☐ | `Sustainability` | Page | `pages/Producer/Sustainability.jsx` | `pages/Sustainability.test.jsx` |  |
| ☐ | `LogisticsHandoffControls` | Component | `components/producer/LogisticsHandoffControls.jsx` | `components/LogisticsHandoffControls.test.jsx` |  |
| ☐ | `useContracts` | Hook | `hooks/useContracts.js` | `hooks/useContracts.test.js` |  |
| ☐ | `useDesignCollaborations` | Hook | `hooks/useDesignCollaborations.js` | `hooks/useDesignCollaborations.test.js` |  |
| ☐ | `useExpertiseCertificates` | Hook | `hooks/useExpertiseCertificates.js` | `hooks/useExpertiseCertificates.test.js` |  |
| ☐ | `useInventory` | Hook | `hooks/useInventory.js` | `hooks/useInventory.test.js` |  |
| ☐ | `useInvestmentOpportunities` | Hook | `hooks/useInvestmentOpportunities.js` | `hooks/useInvestmentOpportunities.test.js` |  |
| ☐ | `useLogisticsDirectory` | Hook | `hooks/useLogisticsDirectory.js` | `hooks/useLogisticsDirectory.test.js` |  |
| ☐ | `useManufacturingPartnerships` | Hook | `hooks/useManufacturingPartnerships.js` | `hooks/useManufacturingPartnerships.test.js` |  |
| ☐ | `useProducerOrders` | Hook | `hooks/useProducerOrders.js` | `hooks/useProducerOrders.test.js` |  |
| ☐ | `useProducerReturns` | Hook | `hooks/useProducerReturns.js` | `hooks/useProducerReturns.test.js` |  |
| ☐ | `useProductAttributes` | Hook | `hooks/useProductAttributes.js` | `hooks/useProductAttributes.test.js` |  |
| ☐ | `useProductDevelopment` | Hook | `hooks/useProductDevelopment.js` | `hooks/useProductDevelopment.test.js` |  |
| ☐ | `useSustainability` | Hook | `hooks/useSustainability.js` | `hooks/useSustainability.test.js` |  |
| ☐ | `contractsService` | API service | `services/contractsService.js` | `services/contractsService.test.js` |  |
| ☐ | `designCollaborationsService` | API service | `services/designCollaborationsService.js` | `services/designCollaborationsService.test.js` |  |
| ☐ | `expertiseCertificatesService` | API service | `services/expertiseCertificatesService.js` | `services/expertiseCertificatesService.test.js` |  |
| ☐ | `inventoryService` | API service | `services/inventoryService.js` | `services/inventoryService.test.js` |  |
| ☐ | `investmentOpportunitiesService` | API service | `services/investmentOpportunitiesService.js` | `services/investmentOpportunitiesService.test.js` |  |
| ☐ | `manufacturingPartnershipsService` | API service | `services/manufacturingPartnershipsService.js` | `services/manufacturingPartnershipsService.test.js` |  |
| ☐ | `producerOrdersService` | API service | `services/producerOrdersService.js` | `services/producerOrdersService.test.js` |  |
| ☐ | `producerReturnsService` | API service | `services/producerReturnsService.js` | `services/producerReturnsService.test.js` |  |
| ☐ | `productAttributesService` | API service | `services/productAttributesService.js` | `services/productAttributesService.test.js` |  |
| ☐ | `productDevelopmentService` | API service | `services/productDevelopmentService.js` | `services/productDevelopmentService.test.js` |  |
| ☐ | `sustainabilityService` | API service | `services/sustainabilityService.js` | `services/sustainabilityService.test.js` |  |

<a id="fe-research"></a>

### Research

Innovation Hub: research workspace, AI assistant, field research, knowledge graph, heritage database, publications, experiments.

**Folder:** `features/Research/` · **Units:** 32 · **Phase:** 4

Contains: 11 pages, 1 component, 10 hooks, 10 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `FieldResearch` | Page | `pages/Research/FieldResearch.jsx` | `pages/FieldResearch.test.jsx` |  |
| ☐ | `HeritageDatabase` | Page | `pages/Research/HeritageDatabase.jsx` | `pages/HeritageDatabase.test.jsx` |  |
| ☐ | `HeritageInnovationSubmissions` | Page | `pages/Research/HeritageInnovationSubmissions.jsx` | `pages/HeritageInnovationSubmissions.test.jsx` |  |
| ☐ | `InnovationExperiments` | Page | `pages/Research/InnovationExperiments.jsx` | `pages/InnovationExperiments.test.jsx` |  |
| ☐ | `InnovationHubHome` | Page | `pages/Research/InnovationHubHome.jsx` | `pages/InnovationHubHome.test.jsx` |  |
| ☐ | `InnovationPrototypes` | Page | `pages/Research/InnovationPrototypes.jsx` | `pages/InnovationPrototypes.test.jsx` |  |
| ☐ | `KnowledgeGraph` | Page | `pages/Research/KnowledgeGraph.jsx` | `pages/KnowledgeGraph.test.jsx` |  |
| ☐ | `PreservationStrategies` | Page | `pages/Research/PreservationStrategies.jsx` | `pages/PreservationStrategies.test.jsx` |  |
| ☐ | `Publications` | Page | `pages/Research/Publications.jsx` | `pages/Publications.test.jsx` |  |
| ☐ | `ResearchAiAssistant` | Page | `pages/Research/ResearchAiAssistant.jsx` | `pages/ResearchAiAssistant.test.jsx` |  |
| ☐ | `ResearchWorkspace` | Page | `pages/Research/ResearchWorkspace.jsx` | `pages/ResearchWorkspace.test.jsx` |  |
| ☐ | `KnowledgeGraphCanvas` | Component | `components/knowledge/KnowledgeGraphCanvas.jsx` | `components/KnowledgeGraphCanvas.test.jsx` |  |
| ☐ | `useFieldResearch` | Hook | `hooks/useFieldResearch.js` | `hooks/useFieldResearch.test.js` |  |
| ☐ | `useHeritageDatabase` | Hook | `hooks/useHeritageDatabase.js` | `hooks/useHeritageDatabase.test.js` |  |
| ☐ | `useHeritageInnovationSubmissions` | Hook | `hooks/useHeritageInnovationSubmissions.js` | `hooks/useHeritageInnovationSubmissions.test.js` |  |
| ☐ | `useInnovationExperiments` | Hook | `hooks/useInnovationExperiments.js` | `hooks/useInnovationExperiments.test.js` |  |
| ☐ | `useInnovationPrototypes` | Hook | `hooks/useInnovationPrototypes.js` | `hooks/useInnovationPrototypes.test.js` |  |
| ☐ | `useKnowledgeGraph` | Hook | `hooks/useKnowledgeGraph.js` | `hooks/useKnowledgeGraph.test.js` |  |
| ☐ | `usePreservationStrategies` | Hook | `hooks/usePreservationStrategies.js` | `hooks/usePreservationStrategies.test.js` |  |
| ☐ | `useResearchAiAssistant` | Hook | `hooks/useResearchAiAssistant.js` | `hooks/useResearchAiAssistant.test.js` |  |
| ☐ | `useResearchPublications` | Hook | `hooks/useResearchPublications.js` | `hooks/useResearchPublications.test.js` |  |
| ☐ | `useResearchWorkspace` | Hook | `hooks/useResearchWorkspace.js` | `hooks/useResearchWorkspace.test.js` |  |
| ☐ | `fieldResearchService` | API service | `services/fieldResearchService.js` | `services/fieldResearchService.test.js` |  |
| ☐ | `heritageDatabaseService` | API service | `services/heritageDatabaseService.js` | `services/heritageDatabaseService.test.js` |  |
| ☐ | `heritageInnovationSubmissionsService` | API service | `services/heritageInnovationSubmissionsService.js` | `services/heritageInnovationSubmissionsService.test.js` |  |
| ☐ | `innovationExperimentsService` | API service | `services/innovationExperimentsService.js` | `services/innovationExperimentsService.test.js` |  |
| ☐ | `innovationPrototypesService` | API service | `services/innovationPrototypesService.js` | `services/innovationPrototypesService.test.js` |  |
| ☐ | `knowledgeGraphService` | API service | `services/knowledgeGraphService.js` | `services/knowledgeGraphService.test.js` |  |
| ☐ | `preservationStrategiesService` | API service | `services/preservationStrategiesService.js` | `services/preservationStrategiesService.test.js` |  |
| ☐ | `researchAiAssistantService` | API service | `services/researchAiAssistantService.js` | `services/researchAiAssistantService.test.js` |  |
| ☐ | `researchPublicationsService` | API service | `services/researchPublicationsService.js` | `services/researchPublicationsService.test.js` |  |
| ☐ | `researchWorkspaceService` | API service | `services/researchWorkspaceService.js` | `services/researchWorkspaceService.test.js` |  |

<a id="fe-researcher"></a>

### Researcher

Innovation Hub workspace home.

**Folder:** `features/Researcher/` · **Units:** 1 · **Phase:** 4

Contains: 1 page.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ResearcherPage` | Page | `pages/Researcher/ResearcherPage.jsx` | `pages/ResearcherPage.test.jsx` |  |

<a id="fe-shareddata"></a>

### SharedData

Hooks and API service files imported by 4 or more features.

**Folder:** `features/SharedData/` · **Units:** 28 · **Phase:** 2

Contains: 14 hooks, 14 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `useAiBusiness` | Hook | `hooks/useAiBusiness.js` | `hooks/useAiBusiness.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useCategories` | Hook | `hooks/useCategories.js` | `hooks/useCategories.test.js` |  |
| ☐ | `useCulturalStories` | Hook | `hooks/useCulturalStories.js` | `hooks/useCulturalStories.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useDistricts` | Hook | `hooks/useDistricts.js` | `hooks/useDistricts.test.js` |  |
| ☐ | `useIdentityVerification` | Hook | `hooks/useIdentityVerification.js` | `hooks/useIdentityVerification.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useMuseumItems` | Hook | `hooks/useMuseumItems.js` | `hooks/useMuseumItems.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `usePermissions` | Hook | `hooks/usePermissions.js` | `hooks/usePermissions.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useProducts` | Hook | `hooks/useProducts.js` | `hooks/useProducts.test.js` |  |
| ☐ | `useProfile` | Hook | `hooks/useProfile.js` | `hooks/useProfile.test.js` |  |
| ☐ | `useRoles` | Hook | `hooks/useRoles.js` | `hooks/useRoles.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useSearch` | Hook | `hooks/useSearch.js` | `hooks/useSearch.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useSiteContent` | Hook | `hooks/useSiteContent.js` | `hooks/useSiteContent.test.js` |  |
| ☐ | `useSupplierMatching` | Hook | `hooks/useSupplierMatching.js` | `hooks/useSupplierMatching.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `useVillages` | Hook | `hooks/useVillages.js` | `hooks/useVillages.test.js` |  |
| ☐ | `aiBusinessService` | API service | `services/aiBusinessService.js` | `services/aiBusinessService.test.js` |  |
| ☐ | `categoriesService` | API service | `services/categoriesService.js` | `services/categoriesService.test.js` |  |
| ☐ | `culturalStoriesService` | API service | `services/culturalStoriesService.js` | `services/culturalStoriesService.test.js` |  |
| ☐ | `districtsService` | API service | `services/districtsService.js` | `services/districtsService.test.js` |  |
| ☐ | `identityVerificationService` | API service | `services/identityVerificationService.js` | `services/identityVerificationService.test.js` |  |
| ☐ | `museumItemsService` | API service | `services/museumItemsService.js` | `services/museumItemsService.test.js` |  |
| ☐ | `paymentsService` | API service | `services/paymentsService.js` | `services/paymentsService.test.js` | not imported anywhere (check if still used before testing) |
| ☐ | `permissionsService` | API service | `services/permissionsService.js` | `services/permissionsService.test.js` |  |
| ☐ | `profileService` | API service | `services/profileService.js` | `services/profileService.test.js` |  |
| ☐ | `rolesService` | API service | `services/rolesService.js` | `services/rolesService.test.js` |  |
| ☐ | `searchService` | API service | `services/searchService.js` | `services/searchService.test.js` |  |
| ☐ | `siteContentService` | API service | `services/siteContentService.js` | `services/siteContentService.test.js` |  |
| ☐ | `supplierMatchingService` | API service | `services/supplierMatchingService.js` | `services/supplierMatchingService.test.js` |  |
| ☐ | `villagesService` | API service | `services/villagesService.js` | `services/villagesService.test.js` |  |

<a id="fe-sharedui"></a>

### SharedUI

Reusable UI from `components/ui`, `cards`, `forms`, `media`, `brand` and `shared`.

**Folder:** `features/SharedUI/` · **Units:** 62 · **Phase:** 2

Contains: 62 components.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AIAssistantWidget` | Component | `components/ui/AIAssistantWidget.jsx` | `components/AIAssistantWidget.test.jsx` |  |
| ☐ | `AnalyticsChart` | Component | `components/ui/AnalyticsChart.jsx` | `components/AnalyticsChart.test.jsx` |  |
| ☐ | `AsyncState` | Component | `components/ui/AsyncState.jsx` | `components/AsyncState.test.jsx` |  |
| ☐ | `AuctionCard` | Component | `components/cards/AuctionCard.jsx` | `components/AuctionCard.test.jsx` |  |
| ☐ | `Badge` | Component | `components/ui/Badge.jsx` | `components/Badge.test.jsx` |  |
| ☐ | `BadgeCard` | Component | `components/cards/BadgeCard.jsx` | `components/BadgeCard.test.jsx` |  |
| ☐ | `BangladeshMap` | Component | `components/media/BangladeshMap.jsx` | `components/BangladeshMap.test.jsx` |  |
| ☐ | `BidForm` | Component | `components/ui/BidForm.jsx` | `components/BidForm.test.jsx` |  |
| ☐ | `BrandLogo` | Component | `components/brand/BrandLogo.jsx` | `components/BrandLogo.test.jsx` |  |
| ☐ | `Button` | Component | `components/ui/Button.jsx` | `components/Button.test.jsx` |  |
| ☐ | `CardMedia` | Component | `components/media/CardMedia.jsx` | `components/CardMedia.test.jsx` |  |
| ☐ | `CartItem` | Component | `components/ui/CartItem.jsx` | `components/CartItem.test.jsx` |  |
| ☐ | `CategoryFilter` | Component | `components/ui/CategoryFilter.jsx` | `components/CategoryFilter.test.jsx` |  |
| ☐ | `CertificateViewer` | Component | `components/media/CertificateViewer.jsx` | `components/CertificateViewer.test.jsx` |  |
| ☐ | `ChartPlaceholder` | Component | `components/ui/ChartPlaceholder.jsx` | `components/ChartPlaceholder.test.jsx` | not imported anywhere (check if still used before testing) |
| ☐ | `ChatBox` | Component | `components/ui/ChatBox.jsx` | `components/ChatBox.test.jsx` |  |
| ☐ | `CheckoutForm` | Component | `components/ui/CheckoutForm.jsx` | `components/CheckoutForm.test.jsx` |  |
| ☐ | `ChipInput` | Component | `components/ui/ChipInput.jsx` | `components/ChipInput.test.jsx` |  |
| ☐ | `ConfirmDialog` | Component | `components/ui/ConfirmDialog.jsx` | `components/ConfirmDialog.test.jsx` |  |
| ☐ | `ConfirmHost` | Component | `components/ui/ConfirmHost.jsx` | `components/ConfirmHost.test.jsx` |  |
| ☐ | `CourseCard` | Component | `components/cards/CourseCard.jsx` | `components/CourseCard.test.jsx` |  |
| ☐ | `DashboardCard` | Component | `components/cards/DashboardCard.jsx` | `components/DashboardCard.test.jsx` |  |
| ☐ | `DiscussionCard` | Component | `components/cards/DiscussionCard.jsx` | `components/DiscussionCard.test.jsx` |  |
| ☐ | `EntityCard` | Component | `components/cards/EntityCard.jsx` | `components/EntityCard.test.jsx` |  |
| ☐ | `EntityPickers` | Component | `components/forms/EntityPickers.jsx` | `components/EntityPickers.test.jsx` |  |
| ☐ | `FestivalCard` | Component | `components/cards/FestivalCard.jsx` | `components/FestivalCard.test.jsx` |  |
| ☐ | `FilterPanel` | Component | `components/ui/FilterPanel.jsx` | `components/FilterPanel.test.jsx` |  |
| ☐ | `FullPageLoader` | Component | `components/ui/FullPageLoader.jsx` | `components/FullPageLoader.test.jsx` |  |
| ☐ | `GlobalFeedback` | Component | `components/ui/GlobalFeedback.jsx` | `components/GlobalFeedback.test.jsx` |  |
| ☐ | `ImpactCard` | Component | `components/cards/ImpactCard.jsx` | `components/ImpactCard.test.jsx` |  |
| ☐ | `LiveShoppingPlayer` | Component | `components/media/LiveShoppingPlayer.jsx` | `components/LiveShoppingPlayer.test.jsx` |  |
| ☐ | `MarketplaceFilter` | Component | `components/ui/MarketplaceFilter.jsx` | `components/MarketplaceFilter.test.jsx` |  |
| ☐ | `MaterialTraceabilityCard` | Component | `components/cards/MaterialTraceabilityCard.jsx` | `components/MaterialTraceabilityCard.test.jsx` |  |
| ☐ | `MilestoneList` | Component | `components/ui/MilestoneList.jsx` | `components/MilestoneList.test.jsx` |  |
| ☐ | `MutationFeedback` | Component | `components/ui/MutationFeedback.jsx` | `components/MutationFeedback.test.jsx` |  |
| ☐ | `OptionalCardLink` | Component | `components/cards/OptionalCardLink.jsx` | `components/OptionalCardLink.test.jsx` |  |
| ☐ | `PageHeader` | Component | `components/ui/PageHeader.jsx` | `components/PageHeader.test.jsx` |  |
| ☐ | `Pagination` | Component | `components/ui/Pagination.jsx` | `components/Pagination.test.jsx` |  |
| ☐ | `ProducerCard` | Component | `components/cards/ProducerCard.jsx` | `components/ProducerCard.test.jsx` |  |
| ☐ | `Product360Viewer` | Component | `components/media/Product360Viewer.jsx` | `components/Product360Viewer.test.jsx` |  |
| ☐ | `ProductCard` | Component | `components/cards/ProductCard.jsx` | `components/ProductCard.test.jsx` |  |
| ☐ | `ProductGallery` | Component | `components/media/ProductGallery.jsx` | `components/ProductGallery.test.jsx` |  |
| ☐ | `QnASection` | Component | `components/ui/QnASection.jsx` | `components/QnASection.test.jsx` |  |
| ☐ | `QRCodeViewer` | Component | `components/media/QRCodeViewer.jsx` | `components/QRCodeViewer.test.jsx` |  |
| ☐ | `QueryState` | Component | `components/ui/QueryState.jsx` | `components/QueryState.test.jsx` |  |
| ☐ | `QueryStatusBanner` | Component | `components/ui/QueryStatusBanner.jsx` | `components/QueryStatusBanner.test.jsx` |  |
| ☐ | `RecommendationCard` | Component | `components/cards/RecommendationCard.jsx` | `components/RecommendationCard.test.jsx` |  |
| ☐ | `ReviewCard` | Component | `components/cards/ReviewCard.jsx` | `components/ReviewCard.test.jsx` |  |
| ☐ | `RoleOverview` | Component | `components/shared/RoleOverview.jsx` | `components/RoleOverview.test.jsx` |  |
| ☐ | `SafeImage` | Component | `components/media/SafeImage.jsx` | `components/SafeImage.test.jsx` |  |
| ☐ | `SearchBar` | Component | `components/ui/SearchBar.jsx` | `components/SearchBar.test.jsx` |  |
| ☐ | `SectionHeader` | Component | `components/ui/SectionHeader.jsx` | `components/SectionHeader.test.jsx` |  |
| ☐ | `ShoppingCartLink` | Component | `components/ui/ShoppingCartLink.jsx` | `components/ShoppingCartLink.test.jsx` |  |
| ☐ | `StatCard` | Component | `components/cards/StatCard.jsx` | `components/StatCard.test.jsx` |  |
| ☐ | `StatusTimeline` | Component | `components/ui/StatusTimeline.jsx` | `components/StatusTimeline.test.jsx` |  |
| ☐ | `Table` | Component | `components/ui/Table.jsx` | `components/Table.test.jsx` |  |
| ☐ | `TimelineViewer` | Component | `components/media/TimelineViewer.jsx` | `components/TimelineViewer.test.jsx` |  |
| ☐ | `TravelEmptyState` | Component | `components/ui/TravelEmptyState.jsx` | `components/TravelEmptyState.test.jsx` |  |
| ☐ | `UserSelect` | Component | `components/forms/UserSelect.jsx` | `components/UserSelect.test.jsx` |  |
| ☐ | `VideoPlayer` | Component | `components/media/VideoPlayer.jsx` | `components/VideoPlayer.test.jsx` |  |
| ☐ | `VillageCard` | Component | `components/cards/VillageCard.jsx` | `components/VillageCard.test.jsx` |  |
| ☐ | `WishlistButton` | Component | `components/ui/WishlistButton.jsx` | `components/WishlistButton.test.jsx` |  |

<a id="fe-tourism"></a>

### Tourism

Public tourism pages (map, festivals, events, routes, cuisines, services) and the tourist trip tools (bookings, AI planner, passport).

**Folder:** `features/Tourism/` · **Units:** 39 · **Phase:** 4

Contains: 15 pages, 4 components, 10 hooks, 10 API services.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `AiTourismPlanner` | Page | `pages/Tourism/AiTourismPlanner.jsx` | `pages/AiTourismPlanner.test.jsx` |  |
| ☐ | `CulturalEvents` | Page | `pages/Tourism/CulturalEvents.jsx` | `pages/CulturalEvents.test.jsx` |  |
| ☐ | `FestivalDirectory` | Page | `pages/Tourism/FestivalDirectory.jsx` | `pages/FestivalDirectory.test.jsx` |  |
| ☐ | `HeritageMap` | Page | `pages/Tourism/HeritageMap.jsx` | `pages/HeritageMap.test.jsx` |  |
| ☐ | `HeritagePlaceDetails` | Page | `pages/Tourism/HeritagePlaceDetails.jsx` | `pages/HeritagePlaceDetails.test.jsx` |  |
| ☐ | `LocalCuisines` | Page | `pages/Tourism/LocalCuisines.jsx` | `pages/LocalCuisines.test.jsx` |  |
| ☐ | `MyBookings` | Page | `pages/Tourism/MyBookings.jsx` | `pages/MyBookings.test.jsx` |  |
| ☐ | `MyTripPlans` | Page | `pages/Tourism/MyTripPlans.jsx` | `pages/MyTripPlans.test.jsx` |  |
| ☐ | `TourismHome` | Page | `pages/Tourism/TourismHome.jsx` | `pages/TourismHome.test.jsx` |  |
| ☐ | `TourismLocationDetails` | Page | `pages/Tourism/TourismLocationDetails.jsx` | `pages/TourismLocationDetails.test.jsx` |  |
| ☐ | `TouristServiceDetails` | Page | `pages/Tourism/TouristServiceDetails.jsx` | `pages/TouristServiceDetails.test.jsx` |  |
| ☐ | `TouristServices` | Page | `pages/Tourism/TouristServices.jsx` | `pages/TouristServices.test.jsx` |  |
| ☐ | `TourRoutes` | Page | `pages/Tourism/TourRoutes.jsx` | `pages/TourRoutes.test.jsx` |  |
| ☐ | `TravelPassport` | Page | `pages/Tourism/TravelPassport.jsx` | `pages/TravelPassport.test.jsx` |  |
| ☐ | `VillageExplorer` | Page | `pages/Tourism/VillageExplorer.jsx` | `pages/VillageExplorer.test.jsx` |  |
| ☐ | `BudgetBreakdown` | Component | `components/tourism/BudgetBreakdown.jsx` | `components/BudgetBreakdown.test.jsx` |  |
| ☐ | `HeritageLeafletMap` | Component | `components/tourism/HeritageLeafletMap.jsx` | `components/HeritageLeafletMap.test.jsx` |  |
| ☐ | `LocationMedia` | Component | `components/tourism/LocationMedia.jsx` | `components/LocationMedia.test.jsx` |  |
| ☐ | `TravelUI` | Component | `components/tourism/TravelUI.jsx` | `components/TravelUI.test.jsx` |  |
| ☐ | `useAITourism` | Hook | `hooks/useAITourism.js` | `hooks/useAITourism.test.js` |  |
| ☐ | `useBookings` | Hook | `hooks/useBookings.js` | `hooks/useBookings.test.js` |  |
| ☐ | `useCulturalEvents` | Hook | `hooks/useCulturalEvents.js` | `hooks/useCulturalEvents.test.js` |  |
| ☐ | `useHeritageFestivals` | Hook | `hooks/useHeritageFestivals.js` | `hooks/useHeritageFestivals.test.js` |  |
| ☐ | `useHeritagePlaces` | Hook | `hooks/useHeritagePlaces.js` | `hooks/useHeritagePlaces.test.js` |  |
| ☐ | `useHeritageRoutes` | Hook | `hooks/useHeritageRoutes.js` | `hooks/useHeritageRoutes.test.js` |  |
| ☐ | `useTourismLocations` | Hook | `hooks/useTourismLocations.js` | `hooks/useTourismLocations.test.js` |  |
| ☐ | `useTouristAnalytics` | Hook | `hooks/useTouristAnalytics.js` | `hooks/useTouristAnalytics.test.js` |  |
| ☐ | `useTouristServices` | Hook | `hooks/useTouristServices.js` | `hooks/useTouristServices.test.js` |  |
| ☐ | `useVillageTour` | Hook | `hooks/useVillageTour.js` | `hooks/useVillageTour.test.js` |  |
| ☐ | `aiTourismService` | API service | `services/aiTourismService.js` | `services/aiTourismService.test.js` |  |
| ☐ | `bookingsService` | API service | `services/bookingsService.js` | `services/bookingsService.test.js` |  |
| ☐ | `culturalEventsService` | API service | `services/culturalEventsService.js` | `services/culturalEventsService.test.js` |  |
| ☐ | `heritageFestivalsService` | API service | `services/heritageFestivalsService.js` | `services/heritageFestivalsService.test.js` |  |
| ☐ | `heritagePlacesService` | API service | `services/heritagePlacesService.js` | `services/heritagePlacesService.test.js` |  |
| ☐ | `heritageRoutesService` | API service | `services/heritageRoutesService.js` | `services/heritageRoutesService.test.js` |  |
| ☐ | `tourismLocationsService` | API service | `services/tourismLocationsService.js` | `services/tourismLocationsService.test.js` |  |
| ☐ | `touristAnalyticsService` | API service | `services/touristAnalyticsService.js` | `services/touristAnalyticsService.test.js` |  |
| ☐ | `touristServicesService` | API service | `services/touristServicesService.js` | `services/touristServicesService.test.js` |  |
| ☐ | `villageTourService` | API service | `services/villageTourService.js` | `services/villageTourService.test.js` |  |

<a id="fe-tourist"></a>

### Tourist

Tourist workspace home.

**Folder:** `features/Tourist/` · **Units:** 1 · **Phase:** 4

Contains: 1 page.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `TouristPage` | Page | `pages/Tourist/TouristPage.jsx` | `pages/TouristPage.test.jsx` |  |

<a id="fe-trainermasterartisan"></a>

### TrainerMasterArtisan

Trainer / Master Artisan pages: apprenticeship programmes.

**Folder:** `features/TrainerMasterArtisan/` · **Units:** 3 · **Phase:** 4

Contains: 2 pages, 1 hook.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ApprenticeshipPrograms` | Page | `pages/TrainerMasterArtisan/ApprenticeshipPrograms.jsx` | `pages/ApprenticeshipPrograms.test.jsx` |  |
| ☐ | `TrainerMasterArtisanPage` | Page | `pages/TrainerMasterArtisan/TrainerMasterArtisanPage.jsx` | `pages/TrainerMasterArtisanPage.test.jsx` |  |
| ☐ | `useApprenticeshipPrograms` | Hook | `hooks/useApprenticeshipPrograms.js` | `hooks/useApprenticeshipPrograms.test.js` |  |

---

## 14. AI chat service features and units

Root folder: `rag/tests/unit/`. Source paths are relative to `rag/`, and test paths are relative to the feature folder.

| Feature | Units | Modules | Phase |
|---|---|---|---|
| [API endpoints](#ai-api-endpoints) | 3 | 3 | 2 |
| [Heritage Q&A pipeline](#ai-heritage-q-a-pipeline) | 16 | 16 | 4 |
| [Ingestion scripts](#ai-ingestion-scripts) | 5 | 5 | 5 |
| [Product search](#ai-product-search) | 9 | 9 | 2 |
| [Service entry points & config](#ai-service-entry-points-config) | 4 | 4 | 5 |
| [Travel planner](#ai-travel-planner) | 7 | 7 | 4 |
| **Total (6 features)** | **44** | **44** | |

<a id="ai-api-endpoints"></a>

### API endpoints

FastAPI endpoints: chat, health and ingest.

**Folder:** `unit/api/` · **Units:** 3 · **Phase:** 2

Contains: 3 modules.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `chat` | Module | `api/chat.py` | `test_chat.py` |  |
| ☐ | `health` | Module | `api/health.py` | `test_health.py` |  |
| ☐ | `ingest` | Module | `api/ingest.py` | `test_ingest.py` |  |

<a id="ai-heritage-q-a-pipeline"></a>

### Heritage Q&A pipeline

The 11-step heritage question-answering pipeline, with prompts, craft keys and index metadata.

**Folder:** `unit/heritage_rag/` · **Units:** 16 · **Phase:** 4 · **Partly tested already:** 9

Contains: 16 modules.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `craft_keys` | Module | `rag/craft_keys.py` | `test_craft_keys.py` |  |
| ☐ | `index_meta` | Module | `rag/index_meta.py` | `test_index_meta.py` |  |
| ☐ | `pdf_font_repair` | Module | `rag/pdf_font_repair.py` | `test_pdf_font_repair.py` |  |
| ☐ | `pipeline` | Module | `rag/pipeline.py` | `test_pipeline.py` |  |
| ☐ | `prompts` | Module | `rag/prompts.py` | `test_prompts.py` |  |
| ☐ | `step01_load_json` | Module | `rag/step01_load_json.py` | `test_step01_load_json.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step02_normalize_json` | Module | `rag/step02_normalize_json.py` | `test_step02_normalize_json.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step03_clean_data` | Module | `rag/step03_clean_data.py` | `test_step03_clean_data.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step04_chunking` | Module | `rag/step04_chunking.py` | `test_step04_chunking.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step05_embedding` | Module | `rag/step05_embedding.py` | `test_step05_embedding.py` |  |
| ☐ | `step06_vector_store` | Module | `rag/step06_vector_store.py` | `test_step06_vector_store.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step07_user_question` | Module | `rag/step07_user_question.py` | `test_step07_user_question.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step08_query_embedding` | Module | `rag/step08_query_embedding.py` | `test_step08_query_embedding.py` |  |
| ☐ | `step09_retrieve` | Module | `rag/step09_retrieve.py` | `test_step09_retrieve.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step10_generate` | Module | `rag/step10_generate.py` | `test_step10_generate.py` | partly tested by `rag/tests/test_json_pipeline.py` |
| ☐ | `step11_answer` | Module | `rag/step11_answer.py` | `test_step11_answer.py` | partly tested by `rag/tests/test_json_pipeline.py` |

<a id="ai-ingestion-scripts"></a>

### Ingestion scripts

Command-line scripts that load heritage, product and travel data.

**Folder:** `unit/ingestion/` · **Units:** 5 · **Phase:** 5

Contains: 5 modules.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `geocode_places` | Module | `geocode_places.py` | `test_geocode_places.py` |  |
| ☐ | `ingest` | Module | `ingest.py` | `test_ingest.py` |  |
| ☐ | `ingest_products` | Module | `ingest_products.py` | `test_ingest_products.py` |  |
| ☐ | `ingest_travel` | Module | `ingest_travel.py` | `test_ingest_travel.py` |  |
| ☐ | `sync_facilities` | Module | `sync_facilities.py` | `test_sync_facilities.py` |  |

<a id="ai-product-search"></a>

### Product search

Product query analysis, retrieval, vector store, sync worker and attribute suggestions.

**Folder:** `unit/product_search/` · **Units:** 9 · **Phase:** 2 · **Partly tested already:** 6

Contains: 9 modules.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `analysis` | Module | `products/analysis.py` | `test_analysis.py` | partly tested by `rag/tests/test_product_search.py` |
| ☐ | `api_client` | Module | `products/api_client.py` | `test_api_client.py` |  |
| ☐ | `embeddings` | Module | `products/embeddings.py` | `test_embeddings.py` |  |
| ☐ | `retrieve` | Module | `products/retrieve.py` | `test_retrieve.py` | partly tested by `rag/tests/test_product_search.py` |
| ☐ | `settings` | Module | `products/settings.py` | `test_settings.py` |  |
| ☐ | `store` | Module | `products/store.py` | `test_store.py` | partly tested by `rag/tests/test_product_search.py`, `rag/tests/test_product_sync.py` |
| ☐ | `suggest` | Module | `products/suggest.py` | `test_suggest.py` | partly tested by `rag/tests/test_product_suggest.py` |
| ☐ | `sync` | Module | `products/sync.py` | `test_sync.py` | partly tested by `rag/tests/test_product_sync.py` |
| ☐ | `vocab` | Module | `products/vocab.py` | `test_vocab.py` | partly tested by `rag/tests/test_product_search.py`, `rag/tests/test_product_suggest.py` |

<a id="ai-service-entry-points-config"></a>

### Service entry points & config

Service start-up (`main.py`, `product_main.py`), the `ask.py` CLI and shared config.

**Folder:** `unit/service/` · **Units:** 4 · **Phase:** 5

Contains: 4 modules.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `ask` | Module | `ask.py` | `test_ask.py` |  |
| ☐ | `config` | Module | `config.py` | `test_config.py` |  |
| ☐ | `main` | Module | `main.py` | `test_main.py` |  |
| ☐ | `product_main` | Module | `product_main.py` | `test_product_main.py` |  |

<a id="ai-travel-planner"></a>

### Travel planner

Travel data cleaning, normalising, ingest and retrieval.

**Folder:** `unit/travel_planner/` · **Units:** 7 · **Phase:** 4

Contains: 7 modules.

| Status | Unit | Type | Source file | Test file | Notes |
|---|---|---|---|---|---|
| ☐ | `clean` | Module | `rag/travel/clean.py` | `test_clean.py` |  |
| ☐ | `ingest` | Module | `rag/travel/ingest.py` | `test_ingest.py` |  |
| ☐ | `normalize` | Module | `rag/travel/normalize.py` | `test_normalize.py` |  |
| ☐ | `normalize_districts` | Module | `rag/travel/normalize_districts.py` | `test_normalize_districts.py` |  |
| ☐ | `normalize_facilities` | Module | `rag/travel/normalize_facilities.py` | `test_normalize_facilities.py` |  |
| ☐ | `normalize_heritage` | Module | `rag/travel/normalize_heritage.py` | `test_normalize_heritage.py` |  |
| ☐ | `retrieve` | Module | `rag/travel/retrieve.py` | `test_retrieve.py` |  |
