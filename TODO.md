# TODO

Deferred work for the Job Application Tracker, roughly in priority order.

## Backend

- [ ] **Protect the Applications service with the shared JWT (make auth real).**
  Today only `GET /api/users/me` requires a token — the Applications and AI
  services accept unauthenticated requests, so the login/session in the UI is
  identity only, not enforcement. To close that:
  - Add JWT bearer validation to the Applications API using the *same* HS256
    signing key / issuer / audience as the Users service (share via
    configuration, not shared code — keep the services autonomous).
  - Put `[Authorize]` on `JobApplicationsController`.
  - Scope applications to their owner: read the `sub` claim (the user id),
    filter every query by it, and stamp it on create so users only ever see
    their own applications.
  - Add a `UserId` column + EF migration to the applications database.
  - Frontend: the interceptor already attaches the token; add 401 handling
    (redirect to `/login`) and put the `authGuard` on the applications routes.
  - Tests: unauthenticated request → 401; accessing another user's
    application → 404/403.

## Frontend

- [ ] Edit an existing application (currently list / create / delete only).
- [x] Salary-range fields in the create form. *(done alongside Smart Add)*
- [ ] Filter the applications list by status.
- [ ] Add the frontend to CI (build + lint; CI currently covers only the .NET services).

## Ops / infra

- [ ] Verify the Postgres volumes are **named** so data survives `docker compose down`
  (containers showed as "Recreated" on restarts — worth confirming the volumes persist).
