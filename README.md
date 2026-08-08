# ShilpoHubBD

A marketplace connecting Bangladeshi heritage artisans and producers directly with customers. Monorepo with a React 19 + Vite frontend, an ASP.NET Core 8 Web API backend, a Supabase PostgreSQL database, and Supabase Storage integration.

## Repository Layout

- `frontend/` - React application
- `backend/` - ASP.NET Core solution ([backend/README.md](backend/README.md) for setup, architecture, and the full API module list)
- `database/` - schema, seeds, migrations, and scripts
- `docs/` - architecture, API, and deployment documentation

## Backend feature modules

The backend implements the full module set from `backendsetup.md`: authentication & roles, marketplace (products, categories, districts, craft/producer stories, workshop gallery), commerce (cart, wishlist, orders, provider-abstracted payments), reviews, community (Q&A, discussions, producer follows, villages), realtime messaging (SignalR), live shopping, auctions, QR verification, certificates of authenticity, product traceability, recommendations, full-text search, AI-shopping placeholders, a heritage passport (badges), achievements (XP/levels), purchase analytics, and social/environmental impact tracking. See [backend/README.md](backend/README.md) for the per-module route table.

## Conventions

- Clean Architecture on the backend
- Feature-based organization on the frontend
- Repository pattern for persistence abstractions
- Service layer for business logic
