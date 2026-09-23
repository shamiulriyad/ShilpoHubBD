# Sourced transport options (Travel Planner)

One JSON file per route (origin city -> destination district). `TransportOptionSeeder` imports every
`*.json` here at API start-up into `TransportOptions`; the AI Travel Planner then lists the options for
the mode the traveller picked (Bus / Train / Plane), and adds a transport line to the budget when a
fare is reported.

This is a **directory of who serves the route**, not a live timetable. Bangladesh has no public
API for bus or train schedules, so nothing here is fetched live.

## Adding a route (e.g. Dhaka -> Sylhet)

1. Copy `dhaka-coxsbazar.json`; set `origin` (matched against the planner's starting-point text),
   `destinationDistrict` (must match `Districts.Name`) and `retrievedOn`.
2. One record per operator/service. Use official pages first (`Verified`), otherwise reputable
   secondary sources (`SecondarySource`).
3. Restart the API. Existing rows are refreshed only when `retrievedOn` is newer.

## Rules

| Field | Rule |
|---|---|
| `schedule`, `durationText`, `fareBdt` | Only if a source states it. If sources **disagree**, leave it `null` and say so in `notes`/`fareNote` (times for the Dhaka-Cox's Bazar trains are omitted for this reason). |
| `fareBdt` | The lowest reported fare; the per-class breakdown goes in `fareNote`. It feeds the trip budget, labelled "reported, unverified" unless the status is `Verified`. |
| `bookingUrl` | An official booking page only (e.g. the Bangladesh Railway e-ticket site). |
| `unverifiedFields` | Everything missing or unconfirmed, so the UI can say "not verified". |
