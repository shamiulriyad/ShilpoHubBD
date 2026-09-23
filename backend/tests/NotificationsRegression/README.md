# Notification checks and deployment

From the ShilpoHubBD root, run `dotnet run --project backend/tests/NotificationsRegression`.
The tests exercise EF change tracking and notification recipient selection without connecting to a database or sending real notifications.

## Activate the notification API

Stop the running backend before rebuilding, then run from the repository root:

```sh
dotnet ef database update --project backend/src/ShilpoHubBD.Data --startup-project backend/src/ShilpoHubBD.Api -- --environment Development
dotnet run --project backend/src/ShilpoHubBD.Api --launch-profile http
```

Apply `20260922112853_AddUserNotifications` before starting this version. The additive migration creates the inbox and indexes; it does not change existing business records. `backend/notifications-migration.sql` is the equivalent SQL for a database already at `20260917174004_AddSecurityModule` (run once). Supabase public API roles have no access to inbox rows; the trusted backend database account must own the table or have a suitable RLS policy.

## Behavior

- Every signed-in role uses `/dashboard/notifications`, with its existing workspace navigation.
- Bell and inbox refresh every 30 seconds while the page is active, and on focus/manual refresh.
- Notifications belong to the account across roles. Activity links are shown only for the current workspace or shared pages.
- Messages, role assignments, orders, producer fulfillment, product reviews, bookings, contracts, shipments, and status-bearing records with user relationships create persistent updates.
- The activity and its notifications save together. Unrelated edits and notification read changes do not generate more updates.
- Read/unread state is stored server-side; marking all read is bounded by the server's list timestamp, preserving newer arrivals.
- Existing history is not backfilled. Notifications begin with activity after deployment. Email, SMS, browser push, and scheduled reminders are outside this in-app feature.

## Browser fixture

Start Vite and visit `/scripts/notifications-fixture/index.html?role=Producer`. The fixture uses synthetic data and intercepts every API request. Other role names, `&empty=1`, and `&error=1` exercise role layouts, empty state, and retry state.
