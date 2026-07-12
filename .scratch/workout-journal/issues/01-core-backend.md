# 01 — Core Backend Engine (DAL + BLL + Unit Tests)

**What to build:**
The core database models, configurations, BLL ViewModels, and services required to save, query, and delete structured workout logs for logged-in gym members, verified by a complete suite of unit tests.

**Blocked by:**
None — can start immediately.

**Status:** ready-for-agent

## Acceptance Criteria
- [ ] Database models `WorkoutLog`, `WorkoutExerciseLog`, and `WorkoutSetLog` are created and linked correctly.
- [ ] EF Core configurations are set up (precision for weight decimals, cascading deletes on deletion of a parent workout).
- [ ] Database migrations are successfully generated and applied to the database.
- [ ] ViewModels `WorkoutLogViewModel` and `CreateWorkoutLogViewModel` are added in BLL.
- [ ] BLL `IWorkoutService` interface and `WorkoutService` implementation are added.
- [ ] BLL service contains transaction-safe methods: `SaveWorkoutAsync`, `GetMemberWorkoutsAsync`, and `DeleteWorkoutAsync`.
- [ ] Unit tests are written to verify that saving a workout log persists all nested sets/exercises, and deleting a workout cleanly removes all its nested children.
