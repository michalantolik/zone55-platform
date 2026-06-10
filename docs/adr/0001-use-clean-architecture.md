# ADR 0001: Use Clean Architecture

## Status

Accepted

## Context

BlogPlatform is a cloud-native blog platform with multiple runtime projects:

- ASP.NET Core API
- Blazor WebAssembly frontend
- Umbraco CMS
- Application layer
- Domain layer
- Infrastructure layer
- Shared contracts

The project needs clear separation between business logic, infrastructure concerns, API delivery, CMS integration, and frontend contracts.

## Decision

Use Clean Architecture as the main application architecture.

The project is split into dedicated layers:

- `BlogPlatform.Domain`
- `BlogPlatform.Application`
- `BlogPlatform.Infrastructure`
- `BlogPlatform.Contracts`
- `BlogPlatform.Api`
- `BlogPlatform.Cms`
- `BlogPlatform.App`

Dependency direction should remain controlled:

- Domain has no dependencies on other project layers.
- Application depends on Domain.
- Infrastructure depends on Application and Domain.
- API and CMS act as composition roots.
- Contracts are shared with API consumers.
- Architecture tests enforce dependency rules.

## Consequences

Positive consequences:

- Business logic is separated from infrastructure.
- Infrastructure implementations can be replaced more easily.
- API, CMS, and frontend concerns stay separated.
- The codebase is easier to test and maintain.
- Architecture rules are visible and enforceable.

Trade-offs:

- More projects and folders are required.
- Simple features may need changes in multiple layers.
- Developers must understand dependency direction.
