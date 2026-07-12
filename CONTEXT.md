# Gym Management System - Domain Model & Glossary

This document outlines the ubiquitous language and domain model of the Gym Management System. It serves as the single source of truth for domain terms.

---

## Glossary of Terms

### 👤 Member
A registered client of the gym. Members can view their profile, manage memberships, book classes, track their health records, and log workouts.

### 🏋️ Trainer
A gym instructor who hosts scheduled training sessions. Each trainer has a designated set of fitness specialties (e.g., bodybuilding, cardio, powerlifting).

### 📅 Session
A scheduled class slot (e.g., CrossFit, Yoga, Spinning) led by a Trainer at a specific time and date, with a set participant capacity.

### 🎫 Booking
A reservation made by a Member to attend a scheduled Session. Bookings enforce capacity limits and track attendance status.

### 💳 Membership
An active subscription linking a Member to a specific Plan, defining their subscription status, start date, and expiration date.

### 📊 Health Record
Physical body metrics (height, weight, blood type) and notes recorded for a Member to track baseline body state.

### ⚖️ BMI (Body Mass Index)
A physical fitness score computed dynamically from a Member's height and weight. It is classified into standardized categories (Underweight, Normal, Overweight, Obese) to serve training tips.

### 📓 Workout Log
A personal journal entry logged by a Member representing a completed physical workout session on a specific date.

### 🏃 Exercise Log
A specific physical movement (e.g., Squats, Deadlifts) recorded within a Workout Log.

### 🔢 Set Log
A single recorded set of an Exercise Log, capturing the exact Weight (in kilograms) and Repetitions performed.

### 📈 Training Volume
The aggregate weight lifted during a set, exercise, or entire workout session, calculated as `Weight * Reps` summed across sets.

### 📣 Announcement
A global text alert managed by Admins and displayed site-wide to communicate gym closures, scheduling changes, or promotions.

### 🏆 Badge Definition
A template or definition of a badge (e.g. Bronze, Silver, Gold tier) specifying its earning category, description, icon artwork, and numeric threshold for automatic awards.

### 🏅 Member Badge
An instance of a Badge Definition earned by a specific Member, recording the date/time and whether it was awarded automatically or manually by an Admin.

### 📊 Leaderboard
A public list ranking gym members by their total count of earned badges, introducing social motivation and friendly competition.
