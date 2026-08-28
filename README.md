# ShilpoHubBD

A marketplace connecting Bangladeshi heritage artisans and producers directly with customers. Monorepo with a React 19 + Vite frontend, an ASP.NET Core 8 Web API backend, a Supabase PostgreSQL database, and Supabase Storage integration.

## Repository Layout

- `frontend/` - React application
- `backend/` - ASP.NET Core solution ([backend/README.md](backend/README.md) for setup, architecture, and the full API module list)
- `database/` - schema, seeds, migrations, and scripts
- `docs/` - architecture, API, and deployment documentation

## Backend feature modules

The backend implements the full module set from `backendsetup.md`: authentication & roles, marketplace (products, categories, districts, craft/producer stories, workshop gallery), commerce (cart, wishlist, orders, provider-abstracted payments), reviews, community (Q&A, discussions, producer follows, villages), realtime messaging (SignalR), live shopping with an optional linked live auction, auctions, QR verification, certificates of authenticity, product traceability, recommendations, full-text search, AI-shopping placeholders, a heritage passport (badges), achievements (XP/levels), purchase analytics, and social/environmental impact tracking.

It also implements the Artisan/Producer business suite: **Producer Business & Orders** (order fulfillment, revenue dashboard, sales/visitor analytics, income reports, product performance), an **AI Business Assistant** (price suggestion, description generation, translation, demand forecasting, production planning, material forecasting, seasonal prediction, sales insights — rule-based today, provider-abstracted for a future real model), **Learning & Mentorship** (mentor profiles, courses & lessons, apprentice enrollment, progress tracking, auto-issued training certificates), **Sustainability** (Eco Score, Green Production Badge, carbon savings, sustainable material records/certifications), and the **Heritage Legacy Score** (configurable, rule-based, with score history and recalculation). See [backend/README.md](backend/README.md) for the per-module route table.

## Conventions

- Clean Architecture on the backend
- Feature-based organization on the frontend
- Repository pattern for persistence abstractions
- Service layer for business logic
