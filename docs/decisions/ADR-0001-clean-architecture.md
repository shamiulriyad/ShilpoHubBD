# ADR 0001: Clean Architecture

## Status

Accepted

## Context

The application needs to remain maintainable as frontend and backend features grow.

## Decision

Use Clean Architecture with separate Domain, Application, Infrastructure, and Api layers.

## Consequences

- Business rules stay isolated from infrastructure concerns
- Tests can target the Application and Domain layers independently
- Dependency direction remains explicit and predictable
