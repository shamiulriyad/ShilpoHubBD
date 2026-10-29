# ShilpoHubBD Frontend

The ShilpoHubBD frontend is the web client for a role-based digital heritage ecosystem serving artisans, customers, businesses, tourists, researchers, public organizations, logistics operators, and platform administrators. It combines a public heritage portal with authenticated workspaces for commerce, learning, tourism, research, governance, logistics, messaging, and administration.

The application is built with React 19 and Vite and consumes the ShilpoHubBD ASP.NET Core API. Persistent business data, authorization decisions, financial calculations, delivery validation, and workflow transitions are enforced by the backend.

## Technology stack

| Area | Technology |
| --- | --- |
| UI | React 19, JSX, Tailwind CSS |
| Build tooling | Vite 6, PostCSS, Autoprefixer |
| Routing | React Router 6 |
| Server state | TanStack React Query 5 |
| Client state | Zustand 5 |
| HTTP | Axios |
| Maps | Leaflet |

## Application areas

Public visitors can browse heritage districts, villages, crafts, producers, products, tourism content, academy content, research material, auctions, and platform information.

Authenticated users receive a workspace based on their backend role:

| Role | Primary workspace | Account creation |
| --- | --- | --- |
| Customer | Marketplace, orders, returns, community, heritage collections, and AI shopping | Self-registration |
| Producer | Products, inventory, orders, auctions, partnerships, support, sustainability, and live commerce | Self-registration |
| Business Partner | Procurement, contracts, supplier discovery, partnerships, investment, and analytics | Self-registration |
| Tourist | Heritage discovery, maps, routes, bookings, passport, and AI trip planning | Self-registration |
| Heritage Academy Member | Courses, mentors, live classes, assessments, and certificates | Self-registration |
| Heritage Innovation Hub | Research, heritage database, field work, publications, experiments, and knowledge graph | Self-registration |
| Government & NGO | Organization profile, artisan support, policy compliance, funding, monitoring, and reports | Super Admin provisioned |
| Logistics Partner | Assigned shipments, pickups, warehouses, stock, routes, returns, and tracking | Super Admin provisioned |
| Super Admin | Users, moderation, content, marketplace governance, logistics, security, and reporting | Existing Super Admin provisioned |

Government/NGO, Logistics Partner, and Super Admin roles are intentionally excluded from public registration. A Super Admin creates these accounts and provides their initial credentials.

## Prerequisites

- Node.js 20 or later
- npm 10 or later
- The ShilpoHubBD backend running on port `5065`
- Heritage RAG and Product Search RAG when using AI-backed features

Install dependencies on the same operating system that will run the project. Do not reuse a `node_modules` directory copied from another environment.

## Quick start

From the repository root, the recommended command starts the backend, frontend, and both RAG services:

```powershell
powershell -ExecutionPolicy Bypass -File .\run-all.ps1
```

The launcher checks environment files and dependencies, replaces previous project listeners, starts each service, writes logs to `.run-logs`, and waits for health checks.

| Service | Default URL |
| --- | --- |
| Frontend | `http://localhost:5173` |
| Backend Swagger | `http://localhost:5065/swagger` |
| Heritage RAG | `http://localhost:8000` |
| Product Search RAG | `http://localhost:8001` |

To run only the frontend:

```bash
cd frontend
npm ci
npm run dev
```

## Environment configuration

Copy the example when `frontend/.env` does not exist:

```powershell
Copy-Item .env.example .env
```

```env
VITE_API_BASE_URL=http://localhost:5065/api
```

The URL must include the backend `/api` base path. Runtime normalization and fallback behavior live in `src/config/runtime.js`.

Every `VITE_*` value is embedded in the browser bundle. Never place database credentials, JWT signing keys, or private API keys in frontend environment files.

## Commands

Run commands from `frontend/`:

| Command | Purpose |
| --- | --- |
| `npm run dev` | Start the Vite development server |
| `npm run build` | Create an optimized build in `dist/` |
| `npm run preview` | Serve the production build locally |
| `npm run lint` | Run deterministic source validation |
| `npm run check` | Run source validation and the production build |
| `npm run test:admin` | Run admin contract checks |
| `npm run test:launch` | Run launch-critical cross-layer contract checks |

`scripts/validate-source.mjs` checks syntax, imports, accessibility basics, broken route patterns, hard-coded development URLs, unsafe image handling, debugging statements, and unfinished-code markers. Contract scripts validate important assumptions against frontend and backend source. These checks complement browser and API integration testing; they do not replace it.

## Project structure

```text
frontend/
├── public/                 Static images and public assets
├── scripts/                Validation, contract tests, and UI fixtures
├── src/
│   ├── components/         Shared UI, layouts, navigation, and feature controls
│   ├── config/             Runtime configuration
│   ├── contexts/           Theme and other React contexts
│   ├── data/               Navigation and presentation configuration
│   ├── hooks/              React Query and reusable behavior hooks
│   ├── layouts/            Public, authentication, and dashboard shells
│   ├── lib/                Query client and shared infrastructure
│   ├── pages/              Route-level screens grouped by domain
│   ├── routes/             Routes, paths, and access guards
│   ├── services/           API clients and Axios interceptors
│   ├── stores/             Zustand state, including authentication
│   ├── styles/             Global styles and design tokens
│   └── utils/              Role, JWT, validation, storage, and error helpers
├── .env.example
├── package.json
├── tailwind.config.js
└── vite.config.js
```

## Architecture and data flow

Pages compose shared components and call feature hooks. Hooks use service modules, and services send requests through the shared Axios client.

```text
Page or component
      ↓
React Query hook
      ↓
Feature service
      ↓
Shared Axios client and interceptors
      ↓
ASP.NET Core API
```

Use this flow for API-backed features. Avoid page-specific Axios instances, hard-coded URLs, direct database access, or frontend-only copies of backend business rules.

TanStack React Query owns API data, loading states, caching, invalidation, and mutation refresh behavior. Query keys must identify the resource and parameters that affect it. Zustand stores only cross-component client state such as authentication; business records belong in the backend and query cache.

## Authentication and multi-tab role sessions

Authentication is stored in `sessionStorage`, so each browser tab has an independent login. Multiple ShilpoHubBD roles can remain open simultaneously—for example, Customer, Producer, Government/NGO, Logistics Partner, and Super Admin in separate tabs.

To open another workspace:

1. Open the profile menu in an authenticated workspace.
2. Select **Open another role tab**.
3. Sign in with the credentials for the other role.

The new tab uses `noopener` and fresh tab storage. Login, logout, token refresh, active-role switching, API authorization, and user-specific query data remain isolated. Theme and sidebar preferences remain browser-wide because they are not credentials.

The shared Axios interceptor:

- attaches the current tab's access token;
- coordinates concurrent refresh requests inside that tab;
- retries queued requests with the refreshed token;
- clears only the current tab when refresh fails; and
- redirects expired sessions to login.

Frontend role checks control navigation and presentation. Backend authorization remains the security boundary.

## Routing and workspaces

`src/routes/router.jsx` defines public, authenticated, and role-restricted branches. `src/routes/routePaths.js` is the canonical path registry. Role names and workspace landing routes live in `src/utils/roles.js`; sidebars live in `src/data/navigation.js`.

When adding a route:

1. Add or reuse its canonical path.
2. Register the page in the correct router branch.
3. Apply the existing authentication or role guard.
4. Add navigation only when the route and API action work.
5. Provide loading, error, empty, unauthorized, and not-found states.

## Key integrated workflows

### Marketplace and orders

The customer marketplace uses real product, cart, checkout, order, payment, return, refund, review, auction, and tracking APIs. The backend validates stock, ownership, order transitions, delivery availability, and money values.

### Logistics

Super Admin manages official logistics companies, operator credentials, service coverage, delivery methods, ETA, charges, activation, performance, and revenue. Logistics operators cannot self-register or change company coverage.

Checkout queries active coverage for the selected district and area. The backend validates the selected service again, creates a shipment, records tracking history, and persists the 30% ShilpoHub logistics share and partner remainder. Customers see status and history from stored shipment events.

### Government and artisan support

Government/NGO accounts are created by Super Admin. Their workspace includes organization data, artisan-support cases, inspections, evidence, monitoring, reports, policy tools, complaints, and funding.

### Heritage, tourism, academy, and research

Public and authenticated experiences share backend content. Role workspaces add bookings, heritage passports, learning progress, assessments, certificates, field research, datasets, knowledge graphs, and AI-assisted discovery.

### Messaging and notifications

Messaging and notifications use authenticated APIs and shared components. User-specific cached data is cleared when the current tab changes or ends its session.

## UI and accessibility conventions

- Reuse components from `src/components/ui` and existing domain components.
- Use shared design tokens instead of introducing isolated color systems.
- Give every form control a visible label or accessible name.
- Preserve keyboard focus, Escape behavior, and modal focus handling.
- Use shared async-state and error-feedback patterns.
- Use the safe-image component for backend or remote images.
- Verify layouts near 320, 375, 425, 768, 1024, and 1366+ pixel widths.
- Disable submissions while mutations are pending and show backend errors.

## Adding a frontend feature

1. Inspect the related backend entity, DTO, controller, service, and authorization.
2. Reuse existing services, hooks, components, and route patterns.
3. Add or extend the service method for the real endpoint.
4. Add a React Query hook with stable keys and focused invalidation.
5. Build complete loading, error, empty, form, and success states.
6. Add the route and role guard.
7. Add focused contract coverage for critical cross-layer assumptions.
8. Run verification and exercise the workflow against the real backend.

## Production deployment

```bash
npm ci
npm run check
```

Deploy `dist/` through a static host or reverse proxy with SPA fallback so client-side paths return `index.html`. Set `VITE_API_BASE_URL` at build time and configure backend CORS for the frontend origin. Do not deploy the Vite development server as the production server.

## Troubleshooting

### The same login appears in every tab

Refresh each tab once after upgrading from an older build. The app removes the former browser-wide auth entry and uses tab-scoped storage. Use **Open another role tab** for additional logins.

### API requests fail

Confirm that `VITE_API_BASE_URL` ends in `/api`, the backend is available, and the browser reports no CORS error. With the default environment, Swagger loads at `http://localhost:5065/swagger`.

### A protected page redirects to unauthorized

Confirm the account's backend role and the active role in that tab. Admin-provisioned workspaces also require the related organization or logistics company to be active and approved.

### The build cannot access `node_modules`

From `frontend/`, confirm the target and reinstall dependencies:

```powershell
Remove-Item -LiteralPath .\node_modules -Recurse -Force
npm ci
```

### AI features are unavailable

Check both RAG health endpoints and `.run-logs`. The rest of the application can remain available while an AI service shows an error state.

## Verification checklist

Before merging:

```bash
npm run check
npm run test:admin
npm run test:launch
```

Then verify the affected workflow against the real backend, including authorization failure, loading, empty, error, success, and responsive states. For authentication changes, verify two accounts in separate tabs.

## Development principles

- Reuse existing architecture before creating parallel abstractions.
- Treat backend DTOs and authorization policies as the contract source of truth.
- Keep secrets and sensitive business logic out of the browser bundle.
- Persist real workflows through APIs rather than mock state.
- Preserve historical financial and tracking records.
- Keep public registration limited to approved self-registerable roles.
- Add only controls whose actions are implemented and authorized.
- Validate critical workflows across frontend, API, and database boundaries.
