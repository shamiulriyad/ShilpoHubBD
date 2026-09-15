# ShilpoHubBD Frontend

ShilpoHubBD's frontend is a React 19 + Vite application that integrates with the existing ASP.NET Core backend. The application is organized by feature and uses a shared API/authentication layer so role-specific areas, marketplace flows, academy features, live commerce, messaging, heritage/research tools, and dashboards behave consistently.

## Technology

- React 19
- Vite 6
- React Router
- TanStack React Query
- Zustand
- Axios
- Tailwind CSS

## Requirements

- Node.js 20+ recommended
- npm 10+ recommended
- A running ShilpoHubBD backend for API-backed features

Do not reuse `node_modules` copied from another operating system. Run a clean install on the machine where the frontend will execute.

## Installation

```bash
npm ci
```

If you intentionally change dependencies, use `npm install` and commit the resulting `package-lock.json` change.

## Environment configuration

Create a local `.env` file when you need to override the API URL.

```env
VITE_API_BASE_URL=http://localhost:5065/api
```

`VITE_API_BASE_URL` must contain the backend API base path. Do not put backend secrets, database credentials, signing keys, or other private values in frontend environment variables.

The runtime configuration in `src/config/runtime.js` normalizes the API base URL and provides the development fallback. Production deployments should set `VITE_API_BASE_URL` explicitly.

## Run locally

```bash
npm run dev
```

Vite will print the local development URL in the terminal. Start the API in a separate terminal with `dotnet run --project backend/src/ShilpoHubBD.Api --launch-profile http` from the repository root. The default frontend API URL is `http://localhost:5065/api`. Restart Vite after changing `.env`.

## Production build

```bash
npm run build
```

Preview a completed production build with:

```bash
npm run preview
```

## Source validation

```bash
npm run lint
```

This repository uses `scripts/validate-source.mjs` as a deterministic source-level QA check. It validates frontend source files for syntax/import problems and common development leftovers such as inaccessible standard form controls, hard-coded localhost URLs in application source, raw broken-image handling, dead `#` links, `alert()`, `console.log`, `debugger`, and TODO/FIXME markers.

This command is not a substitute for browser/E2E testing, but it is designed to catch common regressions without requiring a project-wide ESLint migration.

## Frontend architecture

Important areas under `src/`:

- `components/` — reusable UI, layout, navigation, cards, async states, safe images, and shared controls
- `pages/` — route-level screens grouped by application domain
- `routes/` — route definitions, protected routes, role gates, and route error handling
- `services/` — API modules that map frontend features to backend endpoints
- `hooks/` — React Query hooks and feature-level reusable behavior
- `store/` — persisted/global client state such as authentication
- `config/` — runtime configuration
- `lib/` / `utils/` — query client, validation, API helpers, JWT helpers, and other shared utilities

## API integration

All normal API traffic should go through the shared Axios/API client rather than creating page-specific Axios instances or hard-coding backend URLs.

The shared layer handles:

- API base URL resolution
- request timeout behavior
- authentication headers
- token refresh handling
- consistent API error extraction
- invalid/expired session cleanup

When adding a feature, treat the ASP.NET backend controller/DTO contract as the source of truth. Do not create mock endpoints to make a UI appear functional.

## Authentication and authorization

Authentication state is managed centrally and protected routes wait for initial session validation before showing private content.

Role-specific UI should use the existing route/role guards. Client-side role checks are only a UX layer; the backend remains responsible for authorization enforcement.

Logout and invalid-session handling clear user-specific cached data so one account cannot see stale data from a previous session.

## Data fetching states

API-driven pages should explicitly handle:

1. loading
2. error
3. empty/unavailable
4. successful data

Use existing shared async-state components where appropriate instead of returning a blank page or leaving a permanent spinner.

## Forms

Forms should include:

- accessible labels or accessible names
- logical client-side validation
- backend validation/error feedback
- disabled/loading submit state
- duplicate-submit protection
- explicit successful completion behavior

Backend validation remains authoritative.

## Images

Use the shared safe-image behavior for remote/backend-provided images so missing or invalid URLs do not leave browser broken-image UI. Do not hard-code machine-specific asset paths.

## Routing

The router contains public, authenticated, and role-restricted areas. When adding a page:

- add the route in the appropriate authenticated/public branch
- use role protection when the backend endpoint requires a role
- avoid links to routes that do not exist
- provide meaningful invalid-resource and error states

## Live commerce integration

Live-shopping/workshop screens are backed by the real Live Events API. Producer management uses the authenticated producer-scoped `GET /api/live-events/mine` endpoint added to the backend while preserving the existing public API contracts.

The UI intentionally does not display a fake video player because the current backend live-event contract does not provide a stream URL/media stream contract.

## Theme and responsive layout

The application keeps the existing ShilpoHubBD design language and uses shared Tailwind/design-token behavior for light/dark appearance. Dashboard navigation collapses into a mobile-friendly drawer rather than forcing desktop sidebar columns onto narrow displays.

When adding new UI, verify at minimum around 320, 375, 425, 768, 1024 and 1366+ pixel widths.

## Important development rules

- Do not hard-code API URLs inside pages/components.
- Do not expose secrets in `VITE_*` variables.
- Do not replace backend data with mock data in production UI.
- Do not add controls without an implemented action.
- Do not silently swallow mutation errors.
- Do not trust client-side authorization as the security boundary.
- Prefer existing components/services/hooks before duplicating functionality.

## Verification before merging

Run:

```bash
npm ci
npm run lint
npm run build
```

Then manually verify the important flows relevant to the change, including authentication, protected navigation, backend failure states, mobile layout, and the affected create/update/delete or commerce flow.

If `npm ci` or the production build fails because `node_modules` was copied from a different OS, delete `node_modules` and perform a clean install on the current platform.
