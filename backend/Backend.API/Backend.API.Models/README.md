# Backend Models

This project contains lightweight, shape-only types used to move data between layers and across process boundaries in a .NET Clean Architecture solution.

Purpose

- Hold request/response DTOs, API view models, and other surface-level data transfer types that represent the shape of data exposed by the API or consumed by clients.
- Provide stable contract types for the presentation/API layer so controllers/clients do not depend directly on domain entities or infrastructure details.

Typical contents

- Request and response models (CreateXRequest, UpdateXRequest, XResponse, PagedResult<T>).
- View models and input models used by the web/API layer.
- Simple mapping helpers or AutoMapper profile classes (preferably limited to mapping configuration only).
- Data annotations for validation if needed (keep validation rules simple and presentation-focused).

Guidelines

- Keep models immutable/simple and free of business logic. They are DTOs, not domain objects.
- Prefer mapping between domain entities (in the Domain/Application layers) and these DTOs in the Application layer or via mapping profiles — avoid embedding mapping logic directly in domain classes.
- Keep dependencies minimal: this project should not reference infrastructure or persistence projects. It may be referenced by API/Web and Application layers as needed.
- Version models carefully — these types form the public contract of the API.

What not to put here

- Domain entities, domain services, or business rules.
- Data access or persistence logic.
- Heavy validation or orchestration code; those belong in Application or Presentation where appropriate.

Rationale

Separating models into their own project clarifies boundaries between the public API surface and internal domain logic. It enables independent evolution and versioning of the API contract while keeping domain and infrastructure responsibilities isolated.