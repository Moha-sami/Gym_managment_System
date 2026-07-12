# Issue: Implement Rule-Based Workout Plan Generator

## What happened

Members had no access to personalized workout plans, requiring them to search outside the app or hire personal trainers.

## What I expected

1. A self-service generator where members select their Goal (Build Muscle, Lose Weight, Cardio), training Frequency (3 to 5 days), and Experience Level.
2. A tabbed daily workout schedule dashboard showing targets for sets, reps, and exercise names.
3. Integration with the Workout Journal so members can copy any day's plan straight into a logged workout session with a single click.

## Resolution Implemented

1. **Database Schema:** Added `MemberWorkoutPlan` with `PlanJson` storage to save generated programs. Applied migrations.
2. **Repository Layer:** Registered `MemberWorkoutPlans` repository under `UnitOfWork`.
3. **BLL Services:** Built `WorkoutPlanService` to select workout templates based on goal combinations, handle serialization, and automate the journal mapping.
4. **PL Controller & Views:** Implemented `WorkoutPlanController` with `MyPlan`, `Generate`, and `LogDay` actions.
