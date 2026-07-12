# Issue: Implement Member Workout Journal

## What happened

Members had no way to track their daily workouts, exercises, sets, weight lifted, and repetitions inside the gym system, making progress tracking impossible.

## What I expected

1. A Member dashboard action to access "My Workout Journal".
2. A workout logging form where members can name their workout, set the date, add multiple exercises, and add multiple sets (Weight and Reps) for each exercise.
3. A visual log history showing past workouts with collapsible detail views for exercises, sets, and notes.

## Resolution Implemented

1. **Database Schema & Models:** Added `WorkoutLog`, `WorkoutExerciseLog`, and `WorkoutSetLog` entities and configuration maps. Applied migrations.
2. **Repository Layer:** Registered `WorkoutLogs` repository under `UnitOfWork`.
3. **BLL Services:** Built `WorkoutService` to manage logging workouts, fetching member history, and validating input structures.
4. **PL Controller & Views:** Implemented `WorkoutsController` with `MyJournal`, `Create`, and `Details` actions along with sleek Bootstrap-styled frontends.
