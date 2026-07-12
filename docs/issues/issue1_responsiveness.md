# Issue: Mobile and Tablet Responsiveness Enhancements for Tables and Headers

## What happened

Wide data tables (Users, Delete Requests, Trainers, Memberships, Bookings) and action-heavy headers (e.g. `Announcement Board` button in User Management, `+ Add Session` in Sessions) were breaking layout grids and causing horizontal scrollbars on mobile (320px) and tablet (768px) viewports. Form elements and role badges stretched to 100% card width, causing layout distortion.

## What I expected

1. All wide tables dynamically collapse into clean, styled vertical cards on viewports smaller than `992px`.
2. Action buttons in page headers wrap underneath title text on small viewports instead of overlapping or causing overflow.
3. Dropdowns and badges stay centered and sized appropriately inside responsive cards rather than taking up the entire width.

## Steps to reproduce

1. Open User Management `/Admin/Users` or Sessions `/Sessions` on a mobile device or simulation tool (width `320px` to `768px`).
2. Observe horizontal scrollbars on tables.
3. Observe heading text overlapping with action buttons.

## Resolution Implemented

1. **Global CSS utility:** Added `.responsive-card-table` media query breakpoints up to `991.98px` in `style.css` to transform standard tables into card layouts.
2. **Badge & Form Fixes:** Added overrides to preserve natural inline widths of `.badge` and `select` elements within cards.
3. **Header Stacking:** Applied `flex-column flex-sm-row` stacking properties on views to wrap buttons beneath text on narrow screens.
