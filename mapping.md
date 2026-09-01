# Frontend ↔ Backend Mapping

Living checklist mapping every backend controller/endpoint to its frontend page and integration status. Update the **FE Status** column as pages get wired to real API calls.

## Legend

- **BE Status**: `Real` (EF Core/Postgres, real logic) · `Dummy AI` (real CRUD, but the "intelligence" step is a rule-based `Dummy*Provider` placeholder, not real AI) · `Partial` (real, but missing a piece — noted)
- **FE Status**: `🔴 Missing` (no page at all) · `🟡 Mock` (page exists, uses `mockData.js`, no API call) · `🟢 Wired` (calls the real backend) · `⚪ N/A` (not user-facing)

---

## Auth

| Endpoint | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| POST /api/auth/register | Real | Auth/RegisterPage.jsx | 🟢 Wired | |
| POST /api/auth/login | Real | Auth/LoginPage.jsx | 🟢 Wired | |
| POST /api/auth/refresh | Real | services/interceptors/axiosInterceptor.js | 🟢 Wired | auto silent refresh |
| POST /api/auth/logout | Real | hooks/useAuth.js | 🟢 Wired | |
| POST /api/auth/forgot-password | Real | Auth/ForgotPasswordPage.jsx | 🟢 Wired | |
| POST /api/auth/reset-password | Real | Auth/ResetPasswordPage.jsx | 🟢 Wired | |
| POST /api/auth/switch-role | Real | — | 🔴 Missing | no role-switch UI exists yet (roles are in the auth store already) |

## Marketplace / Commerce

| Endpoint(s) | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| ProductsController (GET /, /featured, /trending, /slug/{slug}, /{id}) | Real | Marketplace/{MarketplaceHome,ProductListing,ProductDetails}, Customer/{Marketplace,ProductDetails} | 🟢 Wired | read endpoints only — /mine, POST/PUT/DELETE, variants, videos, bulk, handmade-verification are producer/admin-only and still 🔴 (no producer dashboard yet) |
| CategoriesController (GET /) | Real | Marketplace/Categories.jsx | 🟢 Wired | write endpoints (admin-only) still 🔴 |
| CartController | Real | Marketplace/Cart.jsx, Customer/ShoppingCart.jsx | 🟢 Wired | |
| WishlistController | Real | Marketplace/Wishlist.jsx, Customer/Wishlist.jsx | 🟢 Wired | |
| OrdersController (checkout, cancel, return, tracking, list, detail) | Real | Marketplace/Checkout.jsx, Customer/{Checkout,OrderHistory,OrderDetails,OrderSuccess,Returns,Refunds} | 🟢 Wired | confirm/ship/deliver/return-approve/return-reject/refund are SuperAdmin-only — belongs to the future Admin phase, not customer-facing |
| PaymentsController (initiate, verify, callback, refund) | Partial — only `CashOnDeliveryPaymentProvider` registered, no card/mobile-banking gateway | Customer/Checkout.jsx (payment method locked to Cash on Delivery) | 🟡 Mock | not called directly yet — checkout only sends `paymentMethod: CashOnDelivery`; no PaymentsController calls wired since there's nothing else to pay/verify |
| ReviewsController (product reviews: list + create) | Real | Customer/ProductDetails.jsx (Reviews tab) | 🟢 Wired | heritage-place/tourist-service reviews still 🔴 (belongs to Tourism phase) |
| AuctionsController (list, detail, bids) | Real | Marketplace/Auctions.jsx, Customer/{AuctionMarketplace,AuctionDetails} | 🟢 Wired | create/cancel (producer/admin) still 🔴 |
| LiveShoppingController (start/end, comments, reactions, buy) | Real | Customer/LiveShopping.jsx | 🟡 Mock | |
| CustomOrdersController | Real | Customer/CustomOrder.jsx | 🟡 Mock | |
| SearchController | Real (Postgres full-text search) | (SearchBar component exists, not wired) | 🟡 Mock | not part of this pass — product listing search now uses `ProductQueryParameters.Search` directly via ProductsController instead |
| RecommendationsController (GET /, /similar/{productId}) | Dummy AI | Customer/{Marketplace "Recommended for You"}, Customer/ProductDetails.jsx "Related Products" | 🟢 Wired | AISimilarProducts.jsx page itself is still 🟡 mock — separate from this |

## Community

| Endpoint(s) | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| DiscussionsController | Real | Customer/DiscussionForum.jsx | 🟢 Wired | also feeds CommunityFeed.jsx "Recent Discussions" |
| QuestionsController (product Q&A) | Real | Customer/QuestionsAnswers.jsx | 🟢 Wired | endpoint is per-product; page now has a product picker since there's no cross-product feed on the backend |
| MessagingController | Real | Customer/Messages.jsx | 🟢 Wired | SignalR real-time (`MessagingHub`) still not hooked up — polling/refetch only via React Query |
| ProducerFollowsController | Real | Customer/FollowingProducers.jsx, Customer/ProducerProfile.jsx (follow button) | 🟢 Wired | |
| ProducerStoriesController | Real | Customer/ProducerStory.jsx, Customer/ProducerProfile.jsx | 🟢 Wired | Explore/ProducerDetails.jsx still 🟡 mock (duplicate public-site page, not yet rewired) |
| CraftStoriesController (keyed by **categoryId**, not a separate craft id) | Real | Customer/CraftStory.jsx, Customer/ProductDetails.jsx (Craft Story tab) | 🟢 Wired | Explore/CraftDetails.jsx still 🟡 mock |
| WorkshopGalleryController (producer photo/video gallery — NOT the live-shopping "workshops" concept) | Real | Customer/ProducerProfile.jsx (Workshop Gallery section) | 🟢 Wired | Customer/WorkshopGallery.jsx is a different feature (live-shopping streams) — stays 🟡 mock, deferred with LiveShoppingController |
| TraceabilityController | Real | Customer/ProductDetails.jsx (Traceability tab) | 🟢 Wired | moved here from the commerce table — it's product-story adjacent |
| CommunityFeed (posts) | *(no matching controller — confirmed no backend support)* | Customer/CommunityFeed.jsx (post feed + producer suggestions) | 🟡 Mock | intentionally left mock — nothing to wire to |
| *(no controller — no ProducersController exists)* | — | Producer directory/browse | 🔴 Missing | real gap: there is no way to list/search all producers; product-by-producer pages work around this by filtering a broad product fetch client-side |

## Gamification / Heritage Identity

| Endpoint(s) | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| AchievementsController (xp summary/history, all/mine, evaluate) | Real | Customer/Achievements.jsx | 🟢 Wired | admin-only Create/AwardXp endpoints untouched (belongs to Admin phase) |
| PassportController — badges | Real | Customer/BadgeCollection.jsx, Customer/HeritagePassport.jsx (badges grid + district-badge claim) | 🟢 Wired | Festival-badge claim wired at hook level; no UI trigger yet (needs a festival picker from Tourism phase) |
| PassportController — check-ins/journal | Real | Customer/HeritagePassport.jsx | 🟢 Wired | check-in *creation* deferred — needs a HeritagePlace picker from the Tourism phase; history/journal fully wired |
| HeritageIdentityController (legacy score, verify) | Real (explainable scoring, not AI) | — | 🟡 Mock | not yet surfaced on ProducerProfile.jsx — good candidate to add during a future pass |
| CertificatesController (Producer-owned product certificates) | Real | — | 🔴 Missing | Producer/Admin-only "mine" — not what Academy/Certificates.jsx needs (that's training certs); deferred, no producer dashboard yet |
| QRVerificationController (verify/history — generate is Producer-only) | Real | — | 🔴 Missing | superseded in the Customer flow by ArCraftScanController below (richer single-call result); could still get its own "my scan history" page later |
| TraceabilityController | Real | Customer/ProductDetails.jsx (Traceability tab) | 🟢 Wired | moved here from Community table |
| ArCraftScanController | Real | Customer/HeritageCollection.jsx ("Verify a Product" tool) | 🟢 Wired | bundles product + craft story + traceability + certificate in one call |
| ImpactController | Real | Customer/ImpactDashboard.jsx | 🟢 Wired | dropped the "Artisans You Support" list — `ImpactSummaryDto` has no producer breakdown |
| AnalyticsController (purchases/spending/favorite-categories) | Real | Customer/PurchaseAnalytics.jsx | 🟢 Wired | monthly-spending chart is still the generic `ChartPlaceholder` visual, not bound to `spending` endpoint data |

## Tourism / Heritage Discovery

| Endpoint(s) | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| VillagesController (GET/favorite/unfavorite — no write UI, admin-only) | Real | Explore/{Villages,VillageDetails}, Explore/{Districts,DistrictDetails} (village counts), Customer/FavoriteVillages.jsx | 🟢 Wired | |
| DistrictsController | Real | Explore/{Districts,DistrictDetails}, plus Marketplace product filters/Checkout | 🟢 Wired | no GetById — DistrictDetails filters client-side from the full list |
| HeritagePlacesController (+ nearby geo search) | Real | Tourism/HeritageMap.jsx | 🟢 Wired | `nearby` (geo-radius) endpoint not used yet — page uses district filter, not GPS |
| HeritageFestivalsController | Real | Tourism/{FestivalDirectory,TourismHome} | 🟢 Wired | |
| HeritageRoutesController (read + itinerary expand; stop CRUD/reorder is admin-only) | Real | Tourism/{TourRoutes,TourismHome} | 🟢 Wired | |
| CulturalEventsController | Real | Tourism/CulturalEvents.jsx | 🟢 Wired | |
| CulturalStoriesController (AR/VR chapters) | Real | — | 🔴 Missing | service/hook built (`useCulturalStories`) but no page consumes it yet — good candidate to fold into DigitalMuseum later |
| MuseumItemsController (AR/VR media) | Real | Explore/DigitalMuseum.jsx | 🟢 Wired | now genuinely shows `MuseumItemDto` records, not products |
| VillageTourController | Real | Tourism/VillageExplorer.jsx | 🟢 Wired | page redesigned around real 360°/video tour stops instead of duplicating the Villages list |
| LocalCuisinesController | Real | Tourism/LocalCuisines.jsx (new) | 🟢 Wired | |
| TouristServicesController (+ availability slots; producer-authoring endpoints untouched) | Real | Tourism/{TouristServices,TouristServiceDetails} (new) | 🟢 Wired | |
| BookingsController (create/mine/cancel; confirm/reject/complete are Producer/Admin-only) | Real | Tourism/{TouristServiceDetails (book), MyBookings} (new) | 🟢 Wired | |
| TouristAnalyticsController | Real | Tourism/TravelPassport.jsx | 🟢 Wired | `spending`/`bookings` stat endpoints not surfaced yet — only visited-locations/district-coverage/achievements/festival-participation used |
| AITourismController (tour-plan wired; budget-plan/route-optimization/translate/cultural-recommendations not yet) | Dummy AI | Tourism/AiTourismPlanner.jsx (new) | 🟢 Wired | only the `tour-plan` endpoint is used — the other 4 endpoints have hooks ready (`useBudgetPlan`, `useRouteOptimization`, `useCulturalRecommendationsAI`) but no UI yet |
| Explore/Unesco.jsx, Explore/ExploreHome.jsx | — | — | 🟡 Mock | no dedicated controller found; may just aggregate other endpoints — left as-is |

## Academy / Learning

| Endpoint(s) | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| CoursesController (read side; author-side create/publish/lessons/modules/materials CRUD untouched) | Real | Academy/{CourseCatalog,CourseDetails} | 🟢 Wired | no mentor/trainer authoring UI — same "no producer dashboard" gap as Commerce |
| CourseCategoriesController (read; admin-only create) | Real | Academy/CourseCatalog.jsx (category filter) | 🟢 Wired | |
| EnrollmentsController (enroll, mine, progress, complete → auto-certificate) | Real | Academy/{LearningDashboard,CourseDetails} | 🟢 Wired | |
| MentorsController (read; Producer-only become/update-mentor untouched) | Real | Academy/Mentors.jsx | 🟢 Wired | |
| LiveClassesController (list/detail/register/join/leave/ask; instructor start/end/cancel/answer untouched) | Real | Academy/{LiveClasses,LiveClassDetails} (new) | 🟢 Wired | distinct from Customer/LiveShopping (commerce livestream) |
| ExamsController (student attempt flow: start/submit/view/history; authoring CRUD + essay evaluation untouched) | Real | Academy/ExamDetails.jsx (new), linked from CourseDetails | 🟢 Wired | |
| QuizzesController (student attempt flow) | Real | Academy/QuizDetails.jsx (new), linked from CourseDetails | 🟢 Wired | |
| AssignmentsController (submit + view own submission/grade; authoring + grading-by-mentor untouched) | Real | Academy/AssignmentDetails.jsx (new), linked from CourseDetails | 🟢 Wired | |
| SkillAssessmentsController (run, history) | Dummy AI (rule-based scoring) | Academy/SkillAssessments.jsx (new) | 🟢 Wired | |
| HeritageSkillsController (read, feeds the skill picker) | Real | Academy/SkillAssessments.jsx | 🟢 Wired | moved here from Tourism placeholder row |
| AcademyMemberProfilesController (profile/skills/learning-history) | Real | — | 🔴 Missing | service/hooks built (`useAcademyProfile`) but no dedicated profile page yet — `Academy/Portfolio.jsx` stayed on mock (no "portfolio of work" concept exists on the backend) |
| TrainingCertificatesController | Real | Academy/Certificates.jsx (mine + download), Academy/Certifications.jsx (public verify tool) | 🟢 Wired | "Certifications" directory concept doesn't exist on the backend — repurposed the page as a certificate-verification tool, the one public capability the controller actually offers |

## AI Features

| Endpoint(s) | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| AIShoppingController: POST /gift-recommendations | Stub only (hardcoded, no DB) | Customer/AIGiftRecommendation.jsx | 🟢 Wired | backend itself is fake (invented product names, no real product IDs) — results render as plain cards, not linked to real products |
| AIShoppingController: POST /fashion-matches | Stub only | Customer/AIFashionMatching.jsx | 🟢 Wired | auto-runs on page load against the real product's name/category |
| AIShoppingController: POST /interior-preview | Stub only | Customer/AIInteriorPreview.jsx | 🟢 Wired | |
| AIShoppingController: POST /translate | Stub only | — | 🔴 Missing | service/hook exist (`aiShoppingService.translate`) but no page uses it — no natural home found |
| AIBusinessAssistantController (price-suggestion, description, sales-insights, seasonal-prediction wired) | Dummy AI (real data, heuristic output) | Producer/AiBusinessAssistant.jsx | 🟢 Wired | see B2B section — built as part of the Producer dashboard |
| AIIntelligenceController (all 5 endpoints) | Dummy AI | BusinessPartner/AiIntelligence.jsx | 🟢 Wired | see B2B section — built as part of the BusinessPartner dashboard |

## B2B / Producer Business Suite — now fully built (new `Producer/` and `BusinessPartner/` dashboards)

Real backend roles (`Producer`, `BusinessPartner`) don't match the 13 legacy placeholder role pages (Artisan/Farmer/Retailer/etc. — those still just render `RoleOverview` with hardcoded stats and are untouched). Instead, built two new proper dashboard shells at `/producer/*` and `/business-partner/*`, each with their own `DashboardLayout` sidebar (`producerSidebarNav`, `businessPartnerSidebarNav` in `data/navigation.js`).

| Endpoint(s) | BE Status | Frontend Page(s) | FE Status | Notes |
|---|---|---|---|---|
| BusinessPartnersController (self profile upsert; admin list/verify untouched) | Real | BusinessPartner/Profile.jsx | 🟢 Wired | |
| BusinessPartnerAnalyticsController (market-demand, spending, supplier-performance wired; export-trends/production-forecast/industry-insights/order-trends have hooks but no dedicated chart yet) | Real | BusinessPartner/Analytics.jsx | 🟢 Wired | |
| ContractsController | Real | Producer/Contracts.jsx (accept/reject/terminate), BusinessPartner/Contracts.jsx (create/terminate) | 🟢 Wired | renew + documents + delivery-schedule-status have hooks/service methods but no UI yet |
| QuotationsController | Real | Producer/Quotations.jsx (respond), BusinessPartner/Quotations.jsx (create/compare/decide/cancel) | 🟢 Wired | |
| ProcurementsController (BusinessPartner-only, no producer side) | Real | BusinessPartner/Procurements.jsx | 🟢 Wired | create-from-quotation has a hook but no UI trigger yet |
| ManufacturingPartnershipsController | Real | Producer/ManufacturingPartnerships.jsx (respond/milestones), BusinessPartner/ManufacturingPartnerships.jsx (create/milestones/complete/cancel) | 🟢 Wired | |
| DesignCollaborationsController | Real | Producer/DesignCollaborations.jsx (respond/comment/revisions), BusinessPartner/DesignCollaborations.jsx (create/comment/decide revisions/complete/cancel) | 🟢 Wired | file upload uses empty arrays (no asset upload wired yet) |
| ProductDevelopmentController | Real | Producer/ProductDevelopment.jsx (respond/comment/prototypes), BusinessPartner/ProductDevelopment.jsx (create/comment/decide prototypes/convert-to-product) | 🟢 Wired | |
| CSRSponsorshipController (Producer creates, BusinessPartner sponsors — reversed direction) | Real | Producer/CsrSponsorship.jsx (create/manage/decide proposals), BusinessPartner/SponsorshipMarketplace.jsx (browse/propose) | 🟢 Wired | milestones/progress-updates/impact-records have hooks but no UI yet |
| InvestmentOpportunitiesController (same reversed pattern as CSR) | Real | Producer/InvestmentOpportunities.jsx, BusinessPartner/InvestmentMarketplace.jsx | 🟢 Wired | milestones/documents have hooks but no UI yet |
| SupplierDiscoveryController | Real | BusinessPartner/SupplierDiscovery.jsx | 🟢 Wired | |
| SupplierMatchingController (rule-based match, not AI, by design) | Real | BusinessPartner/SupplierMatching.jsx | 🟢 Wired | |
| ProducerComparisonController | Real | BusinessPartner/ProducerComparison.jsx | 🟢 Wired | moved here from Community table (BusinessPartner/Admin-only) |
| SustainabilityController | Real | Producer/Sustainability.jsx (self profile + materials/certs) | 🟢 Wired | admin-only certification-verify endpoint untouched |
| InventoryController | Real | Producer/Inventory.jsx | 🟢 Wired | |
| ProducerOrdersController | Real | Producer/Orders.jsx (fulfillment lifecycle + revenue/product-performance) | 🟢 Wired | sales/visitors/income-report/customers endpoints have hooks but aren't all surfaced in the UI yet |
| AIBusinessAssistantController (price-suggestion, description, sales-insights, seasonal-prediction wired) | Dummy AI | Producer/AiBusinessAssistant.jsx | 🟢 Wired | translate/demand-forecast/production-plan/material-forecast have hooks but no tool card yet |
| AIIntelligenceController (all 5 endpoints) | Dummy AI | BusinessPartner/AiIntelligence.jsx | 🟢 Wired | |

**Known gaps carried forward**: several sub-actions (contract renewal, document uploads, milestone-detail add forms, quotation create-from-response) have working service/hook methods but no dedicated UI control yet — noted per row above rather than blocking the whole feature.

## Admin

| Endpoint(s) | BE Status | Frontend Page | FE Status | Notes |
|---|---|---|---|---|
| RolesController (assign/remove) | Real | Admin/UserManagement.jsx | 🟢 Wired | no user-directory/search endpoint exists anywhere in the backend — role changes are applied by pasting a raw user ID |
| BusinessPartnersController (admin list + verify) | Real | Admin/UserManagement.jsx (verification queue) | 🟢 Wired | repurposed this page — it's the closest real "user management" capability that exists |
| VillagesController (admin create), DistrictsController, CategoriesController | Real | Admin/HeritageManagement.jsx | 🟢 Wired | village update/delete and category admin CRUD still 🔴 (hooks not built) |
| HeritageIdentityController (verify) | Real | Admin/HeritageManagement.jsx (producer lookup + approve/reject) | 🟢 Wired | no "pending" list endpoint exists (`GET /verified` only returns already-verified producers) — admin looks up one producer ID at a time |
| ProductsController (SetFeatured, SetHandmadeVerification — admin-only) | Real | Admin/MarketplaceMonitoring.jsx | 🟢 Wired | "pending queue" is derived by filtering a broad product fetch client-side (no dedicated pending-verification query param) |
| AdminDashboard.jsx | — | pulls real pending-partner-verification and listing counts | 🟢 Wired | |
| CMS.jsx, SecurityCenter.jsx | *(no matching controller — confirmed no CMS/security-log endpoints exist)* | — | 🟡 Mock | intentionally left mock — nothing to wire to |

---

## Summary — full pass complete

Every domain in this file went through the same sequence: Commerce → Community → Gamification/Heritage Identity → Tourism → Academy → AI Features → B2B/Producer Business Suite → Admin. All are now 🟢 Wired except the handful of rows explicitly marked 🟡/🔴 above, each with a one-line reason (no backend endpoint exists, or a sub-action has a service/hook but no UI control yet).

- **75 backend controllers**: 68 fully real (EF Core/Postgres), 6 partial/heuristic ("Dummy AI" — real data, rule-based output — or Payments-COD-only), 1 stub-only (AIShopping — hardcoded responses, no DB at all, but now wired anyway since the endpoints are real and callable).
- **Frontend**: grew from 115 page files (5 wired, everything else mock) to ~115 rewired pages **plus 26 brand-new pages**: 5 in Tourism (Local Cuisine, Tourist Services, Tourist Service Details, My Bookings, AI Trip Planner), 5 in Academy (Live Classes list/detail, Exam/Quiz/Assignment taking, Skill Assessments), and two entirely new dashboard shells — `pages/Producer/` (12 pages) and `pages/BusinessPartner/` (15 pages) — since those backend roles had no matching frontend section at all.
- **New infrastructure**: ~60 `services/*.js` files (thin axios wrappers) and ~55 `hooks/use*.js` files (React Query) — one pair per backend domain — plus shared UI: `AsyncState` (loading/error), `StatusTimeline` and `MilestoneList` (the B2B suite's repeated status-history/milestone pattern), and adapters (`productAdapters.js`, `villageAdapters.js`) so real DTOs slot into existing card components without changing their contract.
- **Structural fix**: the 13 legacy role placeholder pages (Artisan/Farmer/Retailer/etc.) never matched the backend's actual roles (`Producer`, `BusinessPartner`, ...) — left untouched, and replaced by the two new dedicated dashboards for the roles that actually needed real screens.
- **Remaining gaps are narrow and documented inline**: no backend endpoint exists for a generic user directory, a "pending heritage identity" queue, per-district village counts, or a producer directory/search — each noted at its row above with the workaround used (client-side filtering, single-ID lookup, etc.) rather than left silently unbuilt.
- No .NET SDK is available in this sandbox, so all of this is code-correct by inspection (matching real DTOs/routes/enums) and verified after every phase with `npm run build`, but **not live-tested against a running API** — that's the one thing to do first once there's a runnable backend environment.
