# Sourced tourism data (Travel Planner)

One JSON file per district. `TourismLocationSeeder` imports every `*.json` here at API start-up into
`TourismLocations` (the table behind the Leaflet map, the planner's hotels/places, and its budget).
`rag/sync_facilities.py` feeds the same files to the Travel Planner RAG.

## Adding a district (Sylhet, Bandarban, Rangamati, Chattogram, Rajshahi, Khulna, Dhaka, ...)

1. Copy `coxsbazar.json` to `<district>.json`; set `district` (must match `Districts.Name`) and `retrievedOn`.
2. Research each place from public sources, in this order of trust: official operator/government page
   (`Verified`), then a reputable non-official source such as Wikivoyage/Wikipedia (`SecondarySource`).
3. `python database/tourism/geocode_tourism_data.py <district>.json --bbox <lat_min> <lat_max> <lng_min> <lng_max>`
   fills only the coordinates you did not source, via a `geocodeQuery` per record. Check the printed matches.
4. `python rag/sync_facilities.py && python rag/ingest_travel.py --recreate` (RAG side).
5. Restart the API; the seeder inserts new rows and never overwrites admin-edited ones.

## Field rules

| Field | Rule |
|---|---|
| `type` | Hotel, Resort, Hostel, Restaurant, TouristPlace, HeritageSite, Attraction |
| `price` | Lowest **listed** per-night rate from the source; full tariff goes in `description`. Omit if unpublished. |
| `entryFee`, `openingHours`, `contact` | Only if a source states it. Omit (= `null`) otherwise. Never estimate. |
| `latitude`/`longitude` | From the source when listed, with `coordinatesSource` and `coordinatesPrecision` (`official_listing`, `community_listing`); otherwise leave empty for the geocoder (`geocoded_approximate`). A record with no coordinates is skipped. |
| `sourceUrl` | The page the facts came from. |
| `verificationStatus` | `Verified` (official source), `SecondarySource`, or `Unverified`. Only `Verified` sets `IsVerified`. |
| `imageUrl`, `imageCredit`, `imageSourceUrl` | Optional. Use only a photo that really shows that place, with a licence that allows reuse (e.g. Wikimedia Commons: link `https://commons.wikimedia.org/wiki/Special:FilePath/<File_name>?width=900`). `imageCredit` must carry the author and licence ("Photo: Name, CC BY-SA 4.0, via Wikimedia Commons"); `imageSourceUrl` is the file's description page. No suitable photo? Leave all three out - the UI shows a placeholder, never a look-alike. |
| `unverifiedFields` | Every field that is missing or not confirmed, so the UI can say so. |
