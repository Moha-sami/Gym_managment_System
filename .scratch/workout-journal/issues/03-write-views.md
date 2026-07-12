# 03 — Dynamic Workout Logging Form (Write Path UI)

**What to build:**
An interactive form page allowing members to log a new workout session by dynamically adding exercises and sets on a single page, and saving it to their journal.

**Blocked by:**
02 — Workout Journal Index & Details View (Read Path UI)

**Status:** ready-for-agent

## Acceptance Criteria
- [ ] `GET Create` action in `WorkoutsController` renders the workout creation form.
- [ ] `Create.cshtml` view features inputs for Workout Name/Title, Date, and a general Notes box.
- [ ] JavaScript handles the "Add Exercise" button, injecting a new exercise input block dynamically into the DOM.
- [ ] JavaScript handles the "Add Set" button inside each exercise block, dynamically injecting inputs for Weight (kg) and Repetitions.
- [ ] Dynamic inputs are properly indexed (e.g. `Exercises[0].ExerciseName`, `Exercises[0].Sets[0].Weight`) to bind directly to the BLL nested ViewModel.
- [ ] `POST Create` action binds the nested payload, validates inputs, saves the workout via `WorkoutService`, and redirects back to the journal dashboard showing a success banner.
- [ ] Members can delete previous workout entries from the journal index view.
