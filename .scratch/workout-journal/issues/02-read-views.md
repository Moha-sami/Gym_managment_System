# 02 — Workout Journal Index & Details View (Read Path UI)

**What to build:**
A dedicated section in the Member Self-Service area where logged-in members can see a list of their past workout logs and click on any log to view the full details of exercises and sets completed during that session.

**Blocked by:**
01 — Core Backend Engine (DAL + BLL + Unit Tests)

**Status:** ready-for-agent

## Acceptance Criteria
- [ ] `WorkoutsController` is created in the PL layer with a `MyJournal` action.
- [ ] A navigation tab titled "My Workout Journal" is added to the header menu in `_Layout.cshtml`, visible only to logged-in users in the "Member" role.
- [ ] `MyJournal.cshtml` view displays all logged workouts for the member in a clean, chronological table or grid list.
- [ ] `Details.cshtml` view is implemented to display the exercises, sets, reps, weights, and notes of a selected past workout.
- [ ] Appropriate authorization rules are in place so members can only view their own workout logs.
