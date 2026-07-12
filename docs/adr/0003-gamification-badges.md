# ADR 0003: Member Gamification (Achievements & Badges)

## Status
Accepted

## Context
Gym members lacked a sense of progression, social identity, and motivation to consistently use the platform. We need a gamification mechanism to incentivize daily behaviors (logging workouts, booking and attending sessions, progress logging) without introducing excessive operational overhead or complex external infrastructure.

## Decision
We implement a hybrid automatic-manual Gamification Engine based on the following architecture:

1. **Two-Table Schema**:
   - `BadgeDefinition` to define the badge metadata (Name, Description, Icon, Category, Tier, Threshold, Auto/Manual).
   - `MemberBadge` to record earned badges, containing foreign keys to `Member` and `BadgeDefinition`, with a nullable `AwardedByUserId` to attribute manually awarded achievements.

2. **On-Action Evaluation**:
   - Instead of running resource-intensive background workers or cron schedules (which would require Hangfire or similar dependencies), automatic badge evaluation is evaluated inline immediately following triggering events (e.g. saving a workout journal entry, booking a session).
   - Newly earned badges are stored in `TempData["NewBadges"]` as serialized JSON and rendered on the next page load using a customized Bootstrap toast component in `_Layout.cshtml`.

3. **Seeded Hybrid Definitions**:
   - We seed 21 automatic tiered badges (Bronze, Silver, Gold across 7 distinct categories) and 3 manual badges ("Competition Winner", "Member of the Month", "Special Recognition") to allow trainers/admins to manually award high-value badges directly from the Member Details page.

4. **Leaderboard Aggregation**:
   - Introduce a public Leaderboard view ranking members by total badge count, using a translatable dual-query database technique (retrieving top rankings first, then in-memory lookup for latest badges) to avoid Entity Framework translation exceptions on complex SQL aggregate functions.

## Consequences
- **Positive**: High motivation for gym members; fully automated real-time progression tracking; simple design requiring zero new database engine installations or scheduling packages.
- **Negative**: Increased load on database select queries when saving workout logs, mitigated by utilizing Entity Framework's index optimizations and native GroupBy count groupings.
