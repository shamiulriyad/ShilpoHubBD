-- Core schema placeholder
-- Add tables, indexes, constraints, and triggers here.
-- WARNING: This schema is for context only and is not meant to be run.
-- Table order and constraints may not be valid for execution.

CREATE TABLE public.__EFMigrationsHistory (
  MigrationId character varying NOT NULL,
  ProductVersion character varying NOT NULL,
  CONSTRAINT __EFMigrationsHistory_pkey PRIMARY KEY (MigrationId)
);
CREATE TABLE public.Roles (
  Id uuid NOT NULL,
  Name character varying NOT NULL,
  Description character varying,
  CONSTRAINT Roles_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.Users (
  Id uuid NOT NULL,
  Email character varying NOT NULL,
  PasswordHash text NOT NULL,
  FullName character varying NOT NULL,
  IsActive boolean NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Users_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.PasswordResetTokens (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  TokenHash character varying NOT NULL,
  ExpiresAt timestamp with time zone NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UsedAt timestamp with time zone,
  CONSTRAINT PasswordResetTokens_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_PasswordResetTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.RefreshTokens (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  TokenHash character varying NOT NULL,
  ExpiresAt timestamp with time zone NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CreatedByIp text,
  RevokedAt timestamp with time zone,
  RevokedByIp text,
  ReplacedByTokenHash text,
  ReasonRevoked text,
  CONSTRAINT RefreshTokens_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_RefreshTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.UserRoles (
  UserId uuid NOT NULL,
  RoleId uuid NOT NULL,
  AssignedAt timestamp with time zone NOT NULL,
  AssignedByUserId uuid,
  CONSTRAINT UserRoles_pkey PRIMARY KEY (UserId, RoleId),
  CONSTRAINT FK_UserRoles_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES public.Roles(Id),
  CONSTRAINT FK_UserRoles_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Categories (
  Id uuid NOT NULL,
  Name character varying NOT NULL,
  Slug character varying NOT NULL,
  Description character varying,
  ImageUrl character varying,
  DisplayOrder integer NOT NULL,
  IsActive boolean NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Categories_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.Districts (
  Id uuid NOT NULL,
  Name character varying NOT NULL,
  Division character varying NOT NULL,
  DisplayOrder integer NOT NULL,
  IsActive boolean NOT NULL,
  CONSTRAINT Districts_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.Products (
  Id uuid NOT NULL,
  Name character varying NOT NULL,
  Slug character varying NOT NULL,
  Description character varying NOT NULL,
  Price numeric NOT NULL,
  DiscountPrice numeric,
  Stock integer NOT NULL,
  IsFeatured boolean NOT NULL,
  IsActive boolean NOT NULL,
  ViewCount integer NOT NULL,
  SalesCount integer NOT NULL,
  AverageRating numeric NOT NULL,
  ReviewCount integer NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CategoryId uuid NOT NULL,
  DistrictId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  MakingProcessVideoUrl character varying,
  HandmadeVerificationNotes character varying,
  HandmadeVerificationStatus character varying NOT NULL DEFAULT 'Pending'::character varying,
  HandmadeVerifiedAt timestamp with time zone,
  HandmadeVerifiedByUserId uuid,
  LowStockThreshold integer,
  Story character varying,
  CONSTRAINT Products_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES public.Categories(Id),
  CONSTRAINT FK_Products_Districts_DistrictId FOREIGN KEY (DistrictId) REFERENCES public.Districts(Id),
  CONSTRAINT FK_Products_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_Products_Users_HandmadeVerifiedByUserId FOREIGN KEY (HandmadeVerifiedByUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ProductImages (
  Id uuid NOT NULL,
  ImageUrl character varying NOT NULL,
  DisplayOrder integer NOT NULL,
  IsPrimary boolean NOT NULL,
  ProductId uuid NOT NULL,
  ImageType character varying NOT NULL DEFAULT 'Standard'::character varying,
  CONSTRAINT ProductImages_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProductImages_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.CraftStories (
  Id uuid NOT NULL,
  Origin character varying NOT NULL,
  Since integer NOT NULL,
  Summary character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CategoryId uuid NOT NULL,
  CONSTRAINT CraftStories_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CraftStories_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES public.Categories(Id)
);
CREATE TABLE public.ProducerStories (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  HeritageId character varying NOT NULL,
  Generations integer NOT NULL,
  FoundingYear integer,
  Quote character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ProducerStories_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProducerStories_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ProductVariants (
  Id uuid NOT NULL,
  Name character varying NOT NULL,
  Sku character varying,
  Price numeric,
  Stock integer NOT NULL,
  DisplayOrder integer NOT NULL,
  IsActive boolean NOT NULL,
  ProductId uuid NOT NULL,
  CONSTRAINT ProductVariants_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProductVariants_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.WorkshopGalleryItems (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  MediaUrl character varying NOT NULL,
  MediaType character varying NOT NULL,
  Caption character varying,
  DisplayOrder integer NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT WorkshopGalleryItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_WorkshopGalleryItems_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.CraftStoryChapters (
  Id uuid NOT NULL,
  Heading character varying NOT NULL,
  Body character varying NOT NULL,
  DisplayOrder integer NOT NULL,
  CraftStoryId uuid NOT NULL,
  CONSTRAINT CraftStoryChapters_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CraftStoryChapters_CraftStories_CraftStoryId FOREIGN KEY (CraftStoryId) REFERENCES public.CraftStories(Id)
);
CREATE TABLE public.ProducerStoryChapters (
  Id uuid NOT NULL,
  Heading character varying NOT NULL,
  Body character varying NOT NULL,
  DisplayOrder integer NOT NULL,
  ProducerStoryId uuid NOT NULL,
  CONSTRAINT ProducerStoryChapters_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProducerStoryChapters_ProducerStories_ProducerStoryId FOREIGN KEY (ProducerStoryId) REFERENCES public.ProducerStories(Id)
);
CREATE TABLE public.CartItems (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  ProductId uuid NOT NULL,
  ProductVariantId uuid,
  Quantity integer NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT CartItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CartItems_ProductVariants_ProductVariantId FOREIGN KEY (ProductVariantId) REFERENCES public.ProductVariants(Id),
  CONSTRAINT FK_CartItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_CartItems_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.WishlistItems (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  ProductId uuid NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT WishlistItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_WishlistItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_WishlistItems_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Orders (
  Id uuid NOT NULL,
  OrderNumber character varying NOT NULL,
  UserId uuid NOT NULL,
  Status character varying NOT NULL,
  PaymentMethod character varying NOT NULL,
  Subtotal numeric NOT NULL,
  Total numeric NOT NULL,
  RecipientName character varying NOT NULL,
  RecipientPhone character varying NOT NULL,
  ShippingAddressLine character varying NOT NULL,
  ShippingDistrictId uuid NOT NULL,
  TrackingNumber character varying,
  Carrier character varying,
  CancelReason character varying,
  ReturnReason character varying,
  RefundAmount numeric,
  RefundReason character varying,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Orders_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Orders_Districts_ShippingDistrictId FOREIGN KEY (ShippingDistrictId) REFERENCES public.Districts(Id),
  CONSTRAINT FK_Orders_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.OrderItems (
  Id uuid NOT NULL,
  OrderId uuid NOT NULL,
  ProductId uuid NOT NULL,
  ProductName character varying NOT NULL,
  ProductImageUrl character varying,
  ProductVariantId uuid,
  VariantName character varying,
  UnitPrice numeric NOT NULL,
  Quantity integer NOT NULL,
  LineTotal numeric NOT NULL,
  Carrier character varying,
  DeliveredAt timestamp with time zone,
  ProducerNote character varying,
  ProducerRespondedAt timestamp with time zone,
  ProducerStatus character varying NOT NULL DEFAULT ''::character varying,
  ShippedAt timestamp with time zone,
  TrackingNumber character varying,
  CONSTRAINT OrderItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_OrderItems_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES public.Orders(Id),
  CONSTRAINT FK_OrderItems_ProductVariants_ProductVariantId FOREIGN KEY (ProductVariantId) REFERENCES public.ProductVariants(Id),
  CONSTRAINT FK_OrderItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.OrderStatusEvents (
  Id uuid NOT NULL,
  OrderId uuid NOT NULL,
  Status character varying NOT NULL,
  Note character varying,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT OrderStatusEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_OrderStatusEvents_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES public.Orders(Id)
);
CREATE TABLE public.Payments (
  Id uuid NOT NULL,
  OrderId uuid NOT NULL,
  Provider character varying NOT NULL,
  Amount numeric NOT NULL,
  RefundedAmount numeric NOT NULL,
  Status character varying NOT NULL,
  TransactionReference character varying,
  FailureReason character varying,
  RefundReason character varying,
  PaidAt timestamp with time zone,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Payments_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Payments_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES public.Orders(Id)
);
CREATE TABLE public.Reviews (
  Id uuid NOT NULL,
  ProductId uuid NOT NULL,
  UserId uuid NOT NULL,
  Rating integer NOT NULL,
  Comment character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Reviews_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Reviews_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_Reviews_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ReviewImages (
  Id uuid NOT NULL,
  ImageUrl character varying NOT NULL,
  DisplayOrder integer NOT NULL,
  ReviewId uuid NOT NULL,
  CONSTRAINT ReviewImages_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ReviewImages_Reviews_ReviewId FOREIGN KEY (ReviewId) REFERENCES public.Reviews(Id)
);
CREATE TABLE public.CommunityQuestions (
  Id uuid NOT NULL,
  ProductId uuid NOT NULL,
  UserId uuid NOT NULL,
  Body character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT CommunityQuestions_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CommunityQuestions_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_CommunityQuestions_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.DiscussionThreads (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  Title character varying NOT NULL,
  Category character varying NOT NULL,
  Body character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT DiscussionThreads_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_DiscussionThreads_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ProducerFollows (
  Id uuid NOT NULL,
  FollowerId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ProducerFollows_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProducerFollows_Users_FollowerId FOREIGN KEY (FollowerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_ProducerFollows_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Villages (
  Id uuid NOT NULL,
  Name character varying NOT NULL,
  Craft character varying NOT NULL,
  Description character varying,
  ImageUrl character varying,
  IsActive boolean NOT NULL,
  DistrictId uuid NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Villages_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Villages_Districts_DistrictId FOREIGN KEY (DistrictId) REFERENCES public.Districts(Id)
);
CREATE TABLE public.CommunityAnswers (
  Id uuid NOT NULL,
  QuestionId uuid NOT NULL,
  UserId uuid NOT NULL,
  Body character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT CommunityAnswers_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CommunityAnswers_CommunityQuestions_QuestionId FOREIGN KEY (QuestionId) REFERENCES public.CommunityQuestions(Id),
  CONSTRAINT FK_CommunityAnswers_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.DiscussionReplies (
  Id uuid NOT NULL,
  ThreadId uuid NOT NULL,
  UserId uuid NOT NULL,
  Body character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT DiscussionReplies_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_DiscussionReplies_DiscussionThreads_ThreadId FOREIGN KEY (ThreadId) REFERENCES public.DiscussionThreads(Id),
  CONSTRAINT FK_DiscussionReplies_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.VillageFavorites (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  VillageId uuid NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT VillageFavorites_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_VillageFavorites_Villages_VillageId FOREIGN KEY (VillageId) REFERENCES public.Villages(Id)
);
CREATE TABLE public.Conversations (
  Id uuid NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Conversations_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.ConversationParticipants (
  Id uuid NOT NULL,
  ConversationId uuid NOT NULL,
  UserId uuid NOT NULL,
  LastReadAt timestamp with time zone,
  CONSTRAINT ConversationParticipants_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ConversationParticipants_Conversations_ConversationId FOREIGN KEY (ConversationId) REFERENCES public.Conversations(Id),
  CONSTRAINT FK_ConversationParticipants_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Messages (
  Id uuid NOT NULL,
  ConversationId uuid NOT NULL,
  SenderId uuid NOT NULL,
  Body character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Messages_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Messages_Conversations_ConversationId FOREIGN KEY (ConversationId) REFERENCES public.Conversations(Id),
  CONSTRAINT FK_Messages_Users_SenderId FOREIGN KEY (SenderId) REFERENCES public.Users(Id)
);
CREATE TABLE public.LiveEvents (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  ProductId uuid NOT NULL,
  Title character varying NOT NULL,
  Description character varying NOT NULL,
  Status character varying NOT NULL,
  ScheduledStartAt timestamp with time zone NOT NULL,
  StartedAt timestamp with time zone,
  EndedAt timestamp with time zone,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  AuctionId uuid,
  CONSTRAINT LiveEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_LiveEvents_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_LiveEvents_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_LiveEvents_Auctions_AuctionId FOREIGN KEY (AuctionId) REFERENCES public.Auctions(Id)
);
CREATE TABLE public.LiveEventComments (
  Id uuid NOT NULL,
  LiveEventId uuid NOT NULL,
  UserId uuid NOT NULL,
  Body character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT LiveEventComments_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_LiveEventComments_LiveEvents_LiveEventId FOREIGN KEY (LiveEventId) REFERENCES public.LiveEvents(Id),
  CONSTRAINT FK_LiveEventComments_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.LiveEventPurchases (
  Id uuid NOT NULL,
  LiveEventId uuid NOT NULL,
  UserId uuid NOT NULL,
  ProductId uuid NOT NULL,
  Quantity integer NOT NULL,
  UnitPrice numeric NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT LiveEventPurchases_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_LiveEventPurchases_LiveEvents_LiveEventId FOREIGN KEY (LiveEventId) REFERENCES public.LiveEvents(Id),
  CONSTRAINT FK_LiveEventPurchases_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_LiveEventPurchases_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.LiveEventReactions (
  Id uuid NOT NULL,
  LiveEventId uuid NOT NULL,
  UserId uuid NOT NULL,
  Type character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT LiveEventReactions_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_LiveEventReactions_LiveEvents_LiveEventId FOREIGN KEY (LiveEventId) REFERENCES public.LiveEvents(Id),
  CONSTRAINT FK_LiveEventReactions_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Auctions (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  ProductId uuid NOT NULL,
  Title character varying NOT NULL,
  Description character varying NOT NULL,
  StartingPrice numeric NOT NULL,
  CurrentPrice numeric NOT NULL,
  MinBidIncrement numeric NOT NULL,
  Status character varying NOT NULL,
  StartAt timestamp with time zone NOT NULL,
  EndAt timestamp with time zone NOT NULL,
  WinnerId uuid,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Auctions_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Auctions_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_Auctions_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_Auctions_Users_WinnerId FOREIGN KEY (WinnerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.AuctionBids (
  Id uuid NOT NULL,
  AuctionId uuid NOT NULL,
  BidderId uuid NOT NULL,
  Amount numeric NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT AuctionBids_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_AuctionBids_Auctions_AuctionId FOREIGN KEY (AuctionId) REFERENCES public.Auctions(Id),
  CONSTRAINT FK_AuctionBids_Users_BidderId FOREIGN KEY (BidderId) REFERENCES public.Users(Id)
);
CREATE TABLE public.QRCodes (
  Id uuid NOT NULL,
  ProductId uuid NOT NULL,
  Code character varying NOT NULL,
  IsActive boolean NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT QRCodes_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QRCodes_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.QRVerificationRecords (
  Id uuid NOT NULL,
  ScannedCode character varying NOT NULL,
  QRCodeId uuid,
  VerifiedByUserId uuid,
  IsValid boolean NOT NULL,
  VerifiedAt timestamp with time zone NOT NULL,
  CONSTRAINT QRVerificationRecords_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QRVerificationRecords_QRCodes_QRCodeId FOREIGN KEY (QRCodeId) REFERENCES public.QRCodes(Id),
  CONSTRAINT FK_QRVerificationRecords_Users_VerifiedByUserId FOREIGN KEY (VerifiedByUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Certificates (
  Id uuid NOT NULL,
  ProductId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  CertificateNumber character varying NOT NULL,
  ProductName character varying NOT NULL,
  ProducerName character varying NOT NULL,
  District character varying NOT NULL,
  Category character varying NOT NULL,
  IsRevoked boolean NOT NULL,
  RevokedAt timestamp with time zone,
  IssuedAt timestamp with time zone NOT NULL,
  CONSTRAINT Certificates_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Certificates_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_Certificates_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ProductTraceabilities (
  Id uuid NOT NULL,
  ProductId uuid NOT NULL,
  Summary character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ProductTraceabilities_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProductTraceabilities_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.MaterialSources (
  Id uuid NOT NULL,
  ProductTraceabilityId uuid NOT NULL,
  MaterialName character varying NOT NULL,
  SourceLocation character varying NOT NULL,
  Description character varying NOT NULL,
  DisplayOrder integer NOT NULL,
  CONSTRAINT MaterialSources_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_MaterialSources_ProductTraceabilities_ProductTraceabilityId FOREIGN KEY (ProductTraceabilityId) REFERENCES public.ProductTraceabilities(Id)
);
CREATE TABLE public.TimelineEvents (
  Id uuid NOT NULL,
  ProductTraceabilityId uuid NOT NULL,
  Title character varying NOT NULL,
  Description character varying NOT NULL,
  Location character varying,
  EventDate timestamp with time zone NOT NULL,
  DisplayOrder integer NOT NULL,
  CONSTRAINT TimelineEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_TimelineEvents_ProductTraceabilities_ProductTraceabilityId FOREIGN KEY (ProductTraceabilityId) REFERENCES public.ProductTraceabilities(Id)
);
CREATE TABLE public.Badges (
  Id uuid NOT NULL,
  Type character varying NOT NULL,
  Name character varying NOT NULL,
  Description character varying NOT NULL,
  IconUrl character varying,
  DistrictId uuid,
  FestivalName character varying,
  RequiredPurchaseCount integer,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Badges_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Badges_Districts_DistrictId FOREIGN KEY (DistrictId) REFERENCES public.Districts(Id)
);
CREATE TABLE public.UserBadges (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  BadgeId uuid NOT NULL,
  EarnedAt timestamp with time zone NOT NULL,
  CONSTRAINT UserBadges_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_UserBadges_Badges_BadgeId FOREIGN KEY (BadgeId) REFERENCES public.Badges(Id),
  CONSTRAINT FK_UserBadges_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Achievements (
  Id uuid NOT NULL,
  Name character varying NOT NULL,
  Description character varying NOT NULL,
  IconUrl character varying,
  RequiredXp integer NOT NULL,
  XpReward integer NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Achievements_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.XpTransactions (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  Amount integer NOT NULL,
  Reason character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT XpTransactions_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_XpTransactions_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.UserAchievements (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  AchievementId uuid NOT NULL,
  UnlockedAt timestamp with time zone NOT NULL,
  CONSTRAINT UserAchievements_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_UserAchievements_Achievements_AchievementId FOREIGN KEY (AchievementId) REFERENCES public.Achievements(Id),
  CONSTRAINT FK_UserAchievements_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ProducerHeritageIdentities (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  HeritageIdNumber character varying NOT NULL,
  PrimaryCraft character varying NOT NULL,
  YearsOfExperience integer NOT NULL,
  WorkshopName character varying NOT NULL,
  WorkshopDescription character varying NOT NULL,
  WorkshopAddress character varying,
  EstablishedYear integer,
  DistrictId uuid,
  VerificationStatus character varying NOT NULL,
  VerifiedByUserId uuid,
  VerificationNotes character varying,
  VerifiedAt timestamp with time zone,
  LegacyScore integer NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ProducerHeritageIdentities_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProducerHeritageIdentities_Districts_DistrictId FOREIGN KEY (DistrictId) REFERENCES public.Districts(Id),
  CONSTRAINT FK_ProducerHeritageIdentities_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_ProducerHeritageIdentities_Users_VerifiedByUserId FOREIGN KEY (VerifiedByUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.FamilyHeritageMembers (
  Id uuid NOT NULL,
  ProducerHeritageIdentityId uuid NOT NULL,
  FullName text NOT NULL,
  Relation text NOT NULL,
  Generation integer NOT NULL,
  Role text,
  ActiveYearsRange text,
  Story text,
  DisplayOrder integer NOT NULL,
  CONSTRAINT FamilyHeritageMembers_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_FamilyHeritageMembers_ProducerHeritageIdentities_ProducerHe~ FOREIGN KEY (ProducerHeritageIdentityId) REFERENCES public.ProducerHeritageIdentities(Id)
);
CREATE TABLE public.HeritageAwards (
  Id uuid NOT NULL,
  ProducerHeritageIdentityId uuid NOT NULL,
  Title text NOT NULL,
  IssuingOrganization text NOT NULL,
  Year integer NOT NULL,
  Description text,
  ImageUrl text,
  DisplayOrder integer NOT NULL,
  CONSTRAINT HeritageAwards_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_HeritageAwards_ProducerHeritageIdentities_ProducerHeritageI~ FOREIGN KEY (ProducerHeritageIdentityId) REFERENCES public.ProducerHeritageIdentities(Id)
);
CREATE TABLE public.HeritageCertifications (
  Id uuid NOT NULL,
  ProducerHeritageIdentityId uuid NOT NULL,
  Name text NOT NULL,
  IssuingBody text NOT NULL,
  IssuedYear integer NOT NULL,
  ExpiryYear integer,
  CertificateNumber text,
  CertificateUrl text,
  DisplayOrder integer NOT NULL,
  CONSTRAINT HeritageCertifications_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_HeritageCertifications_ProducerHeritageIdentities_ProducerH~ FOREIGN KEY (ProducerHeritageIdentityId) REFERENCES public.ProducerHeritageIdentities(Id)
);
CREATE TABLE public.SkillTimelineEntries (
  Id uuid NOT NULL,
  ProducerHeritageIdentityId uuid NOT NULL,
  Title text NOT NULL,
  Description text NOT NULL,
  Year integer NOT NULL,
  DisplayOrder integer NOT NULL,
  CONSTRAINT SkillTimelineEntries_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SkillTimelineEntries_ProducerHeritageIdentities_ProducerHer~ FOREIGN KEY (ProducerHeritageIdentityId) REFERENCES public.ProducerHeritageIdentities(Id)
);
CREATE TABLE public.StoryArchiveEntries (
  Id uuid NOT NULL,
  ProducerHeritageIdentityId uuid NOT NULL,
  Title text NOT NULL,
  Content text NOT NULL,
  Year integer,
  DisplayOrder integer NOT NULL,
  CONSTRAINT StoryArchiveEntries_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_StoryArchiveEntries_ProducerHeritageIdentities_ProducerHeri~ FOREIGN KEY (ProducerHeritageIdentityId) REFERENCES public.ProducerHeritageIdentities(Id)
);
CREATE TABLE public.CustomOrderRequests (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  CustomerId uuid NOT NULL,
  ProductId uuid,
  Title character varying NOT NULL,
  Specifications character varying NOT NULL,
  Budget numeric,
  Deadline timestamp with time zone,
  Status character varying NOT NULL,
  QuotedPrice numeric,
  ProducerResponse character varying,
  RespondedAt timestamp with time zone,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT CustomOrderRequests_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CustomOrderRequests_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_CustomOrderRequests_Users_CustomerId FOREIGN KEY (CustomerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_CustomOrderRequests_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.InventoryTransactions (
  Id uuid NOT NULL,
  ProductId uuid NOT NULL,
  ProductVariantId uuid,
  ChangeAmount integer NOT NULL,
  Reason character varying NOT NULL,
  PreviousStock integer NOT NULL,
  NewStock integer NOT NULL,
  CreatedByUserId uuid NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT InventoryTransactions_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_InventoryTransactions_ProductVariants_ProductVariantId FOREIGN KEY (ProductVariantId) REFERENCES public.ProductVariants(Id),
  CONSTRAINT FK_InventoryTransactions_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_InventoryTransactions_Users_CreatedByUserId FOREIGN KEY (CreatedByUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ProductVideos (
  Id uuid NOT NULL,
  VideoUrl character varying NOT NULL,
  Title character varying,
  DisplayOrder integer NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  ProductId uuid NOT NULL,
  CONSTRAINT ProductVideos_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProductVideos_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.MentorProfiles (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  Bio character varying NOT NULL,
  Expertise character varying NOT NULL,
  YearsOfExperience integer NOT NULL,
  IsActive boolean NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT MentorProfiles_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_MentorProfiles_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.Courses (
  Id uuid NOT NULL,
  MentorId uuid NOT NULL,
  Title character varying NOT NULL,
  Description character varying NOT NULL,
  Category character varying NOT NULL,
  Status character varying NOT NULL,
  MaxApprentices integer,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  PublishedAt timestamp with time zone,
  CONSTRAINT Courses_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Courses_MentorProfiles_MentorId FOREIGN KEY (MentorId) REFERENCES public.MentorProfiles(Id)
);
CREATE TABLE public.CourseEnrollments (
  Id uuid NOT NULL,
  CourseId uuid NOT NULL,
  ApprenticeId uuid NOT NULL,
  Status character varying NOT NULL,
  EnrolledAt timestamp with time zone NOT NULL,
  CompletedAt timestamp with time zone,
  CONSTRAINT CourseEnrollments_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CourseEnrollments_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES public.Courses(Id),
  CONSTRAINT FK_CourseEnrollments_Users_ApprenticeId FOREIGN KEY (ApprenticeId) REFERENCES public.Users(Id)
);
CREATE TABLE public.CourseLessons (
  Id uuid NOT NULL,
  CourseId uuid NOT NULL,
  Title character varying NOT NULL,
  Content character varying NOT NULL,
  VideoUrl character varying,
  DisplayOrder integer NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT CourseLessons_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CourseLessons_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES public.Courses(Id)
);
CREATE TABLE public.TrainingCertificates (
  Id uuid NOT NULL,
  EnrollmentId uuid NOT NULL,
  CertificateNumber character varying NOT NULL,
  CourseTitle character varying NOT NULL,
  ApprenticeName character varying NOT NULL,
  MentorName character varying NOT NULL,
  IsRevoked boolean NOT NULL,
  RevokedAt timestamp with time zone,
  IssuedAt timestamp with time zone NOT NULL,
  CONSTRAINT TrainingCertificates_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_TrainingCertificates_CourseEnrollments_EnrollmentId FOREIGN KEY (EnrollmentId) REFERENCES public.CourseEnrollments(Id)
);
CREATE TABLE public.LessonProgress (
  Id uuid NOT NULL,
  EnrollmentId uuid NOT NULL,
  LessonId uuid NOT NULL,
  IsCompleted boolean NOT NULL,
  CompletedAt timestamp with time zone,
  CONSTRAINT LessonProgress_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_LessonProgress_CourseEnrollments_EnrollmentId FOREIGN KEY (EnrollmentId) REFERENCES public.CourseEnrollments(Id),
  CONSTRAINT FK_LessonProgress_CourseLessons_LessonId FOREIGN KEY (LessonId) REFERENCES public.CourseLessons(Id)
);
CREATE TABLE public.SustainabilityProfiles (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  EcoScore numeric NOT NULL,
  BadgeLevel character varying NOT NULL,
  TotalCarbonSavingsKg numeric NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  LastCalculatedAt timestamp with time zone,
  CONSTRAINT SustainabilityProfiles_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SustainabilityProfiles_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.SustainableMaterialCertifications (
  Id uuid NOT NULL,
  SustainabilityProfileId uuid NOT NULL,
  MaterialName character varying NOT NULL,
  CertifyingBody character varying NOT NULL,
  CertificateReference character varying NOT NULL,
  IssuedAt timestamp with time zone NOT NULL,
  ExpiresAt timestamp with time zone,
  IsVerified boolean NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT SustainableMaterialCertifications_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SustainableMaterialCertifications_SustainabilityProfiles_Su~ FOREIGN KEY (SustainabilityProfileId) REFERENCES public.SustainabilityProfiles(Id)
);
CREATE TABLE public.SustainableMaterialRecords (
  Id uuid NOT NULL,
  SustainabilityProfileId uuid NOT NULL,
  ProductId uuid,
  MaterialName character varying NOT NULL,
  QuantityUsed numeric NOT NULL,
  Unit character varying NOT NULL,
  IsRecycled boolean NOT NULL,
  IsRenewable boolean NOT NULL,
  IsLocallySourced boolean NOT NULL,
  IsBiodegradable boolean NOT NULL,
  CarbonSavingsPerUnitKg numeric NOT NULL,
  RecordedAt timestamp with time zone NOT NULL,
  CONSTRAINT SustainableMaterialRecords_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SustainableMaterialRecords_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_SustainableMaterialRecords_SustainabilityProfiles_Sustainab~ FOREIGN KEY (SustainabilityProfileId) REFERENCES public.SustainabilityProfiles(Id)
);
CREATE TABLE public.HeritageScoreHistory (
  Id uuid NOT NULL,
  ProducerHeritageIdentityId uuid NOT NULL,
  Score integer NOT NULL,
  YearsOfExperiencePoints integer NOT NULL,
  VerificationPoints integer NOT NULL,
  AwardsPoints integer NOT NULL,
  CertificationsPoints integer NOT NULL,
  ProductsPoints integer NOT NULL,
  ReviewsPoints integer NOT NULL,
  ApprenticesTrainedPoints integer NOT NULL,
  CoursesPublishedPoints integer NOT NULL,
  CulturalContributionPoints integer NOT NULL,
  CalculatedAt timestamp with time zone NOT NULL,
  CONSTRAINT HeritageScoreHistory_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_HeritageScoreHistory_ProducerHeritageIdentities_ProducerHer~ FOREIGN KEY (ProducerHeritageIdentityId) REFERENCES public.ProducerHeritageIdentities(Id)
);
CREATE TABLE public.BusinessPartnerProfiles (
  Id uuid NOT NULL,
  UserId uuid NOT NULL,
  BusinessType character varying NOT NULL,
  CompanyName character varying NOT NULL,
  RegistrationNumber character varying NOT NULL,
  TaxIdentificationNumber character varying,
  YearEstablished integer,
  Industry character varying NOT NULL,
  BusinessSize character varying NOT NULL,
  EmployeeCount integer,
  Website character varying,
  CompanyDescription character varying NOT NULL,
  AddressLine character varying NOT NULL,
  City character varying NOT NULL,
  DistrictId uuid,
  PostalCode character varying,
  Country character varying NOT NULL,
  ContactPersonName character varying NOT NULL,
  ContactPersonDesignation character varying,
  ContactPhone character varying NOT NULL,
  ContactEmail character varying NOT NULL,
  MinimumOrderQuantity integer,
  MaxBudgetPerOrder numeric,
  PreferredOrderFrequency character varying,
  PreferredPaymentTerms character varying,
  VerificationStatus character varying NOT NULL,
  VerifiedByUserId uuid,
  VerificationNotes character varying,
  VerifiedAt timestamp with time zone,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT BusinessPartnerProfiles_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_BusinessPartnerProfiles_Districts_DistrictId FOREIGN KEY (DistrictId) REFERENCES public.Districts(Id),
  CONSTRAINT FK_BusinessPartnerProfiles_Users_UserId FOREIGN KEY (UserId) REFERENCES public.Users(Id),
  CONSTRAINT FK_BusinessPartnerProfiles_Users_VerifiedByUserId FOREIGN KEY (VerifiedByUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.BusinessDocuments (
  Id uuid NOT NULL,
  BusinessPartnerProfileId uuid NOT NULL,
  DocumentType character varying NOT NULL,
  DocumentName character varying NOT NULL,
  FileUrl character varying NOT NULL,
  DocumentNumber character varying,
  IssuedDate timestamp with time zone,
  ExpiryDate timestamp with time zone,
  UploadedAt timestamp with time zone NOT NULL,
  CONSTRAINT BusinessDocuments_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_BusinessDocuments_BusinessPartnerProfiles_BusinessPartnerPr~ FOREIGN KEY (BusinessPartnerProfileId) REFERENCES public.BusinessPartnerProfiles(Id)
);
CREATE TABLE public.BusinessPartnerPreferredCategories (
  Id uuid NOT NULL,
  BusinessPartnerProfileId uuid NOT NULL,
  CategoryId uuid NOT NULL,
  CONSTRAINT BusinessPartnerPreferredCategories_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_BusinessPartnerPreferredCategories_BusinessPartnerProfiles_~ FOREIGN KEY (BusinessPartnerProfileId) REFERENCES public.BusinessPartnerProfiles(Id),
  CONSTRAINT FK_BusinessPartnerPreferredCategories_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES public.Categories(Id)
);
CREATE TABLE public.QuotationRequests (
  Id uuid NOT NULL,
  BusinessPartnerId uuid NOT NULL,
  ReferenceNumber character varying NOT NULL,
  Title character varying NOT NULL,
  Requirements character varying,
  RequiredDeliveryDate timestamp with time zone NOT NULL,
  Status character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT QuotationRequests_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QuotationRequests_Users_BusinessPartnerId FOREIGN KEY (BusinessPartnerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.QuotationRequestItems (
  Id uuid NOT NULL,
  QuotationRequestId uuid NOT NULL,
  ProductId uuid,
  ProductName character varying NOT NULL,
  CategoryId uuid,
  Quantity integer NOT NULL,
  TargetPrice numeric,
  Specifications character varying,
  CONSTRAINT QuotationRequestItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QuotationRequestItems_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES public.Categories(Id),
  CONSTRAINT FK_QuotationRequestItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id),
  CONSTRAINT FK_QuotationRequestItems_QuotationRequests_QuotationRequestId FOREIGN KEY (QuotationRequestId) REFERENCES public.QuotationRequests(Id)
);
CREATE TABLE public.QuotationRequestProducers (
  Id uuid NOT NULL,
  QuotationRequestId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  Status character varying NOT NULL,
  InvitedAt timestamp with time zone NOT NULL,
  ViewedAt timestamp with time zone,
  RespondedAt timestamp with time zone,
  CONSTRAINT QuotationRequestProducers_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QuotationRequestProducers_QuotationRequests_QuotationReques~ FOREIGN KEY (QuotationRequestId) REFERENCES public.QuotationRequests(Id),
  CONSTRAINT FK_QuotationRequestProducers_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.QuotationStatusEvents (
  Id uuid NOT NULL,
  QuotationRequestId uuid NOT NULL,
  Status character varying NOT NULL,
  Note character varying,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT QuotationStatusEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QuotationStatusEvents_QuotationRequests_QuotationRequestId FOREIGN KEY (QuotationRequestId) REFERENCES public.QuotationRequests(Id)
);
CREATE TABLE public.QuotationResponses (
  Id uuid NOT NULL,
  QuotationRequestProducerId uuid NOT NULL,
  TotalPrice numeric NOT NULL,
  EstimatedDeliveryDate timestamp with time zone,
  Notes character varying,
  Status character varying NOT NULL,
  DecidedAt timestamp with time zone,
  DecisionNotes character varying,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT QuotationResponses_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QuotationResponses_QuotationRequestProducers_QuotationReque~ FOREIGN KEY (QuotationRequestProducerId) REFERENCES public.QuotationRequestProducers(Id)
);
CREATE TABLE public.QuotationResponseItems (
  Id uuid NOT NULL,
  QuotationResponseId uuid NOT NULL,
  QuotationRequestItemId uuid NOT NULL,
  QuotedUnitPrice numeric NOT NULL,
  QuotedQuantity integer NOT NULL,
  Notes character varying,
  CONSTRAINT QuotationResponseItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_QuotationResponseItems_QuotationRequestItems_QuotationReque~ FOREIGN KEY (QuotationRequestItemId) REFERENCES public.QuotationRequestItems(Id),
  CONSTRAINT FK_QuotationResponseItems_QuotationResponses_QuotationResponse~ FOREIGN KEY (QuotationResponseId) REFERENCES public.QuotationResponses(Id)
);
CREATE TABLE public.ProcurementRequests (
  Id uuid NOT NULL,
  BusinessPartnerId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  ReferenceNumber character varying NOT NULL,
  Title character varying NOT NULL,
  Budget numeric,
  DeliveryDeadline timestamp with time zone NOT NULL,
  Status character varying NOT NULL,
  QuotationRequestId uuid,
  QuotationResponseId uuid,
  OrderId uuid,
  ApprovedByUserId uuid,
  ApprovedAt timestamp with time zone,
  ApprovalNotes character varying,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ProcurementRequests_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProcurementRequests_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES public.Orders(Id),
  CONSTRAINT FK_ProcurementRequests_QuotationRequests_QuotationRequestId FOREIGN KEY (QuotationRequestId) REFERENCES public.QuotationRequests(Id),
  CONSTRAINT FK_ProcurementRequests_QuotationResponses_QuotationResponseId FOREIGN KEY (QuotationResponseId) REFERENCES public.QuotationResponses(Id),
  CONSTRAINT FK_ProcurementRequests_Users_ApprovedByUserId FOREIGN KEY (ApprovedByUserId) REFERENCES public.Users(Id),
  CONSTRAINT FK_ProcurementRequests_Users_BusinessPartnerId FOREIGN KEY (BusinessPartnerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_ProcurementRequests_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ProcurementItems (
  Id uuid NOT NULL,
  ProcurementRequestId uuid NOT NULL,
  ProductId uuid NOT NULL,
  ProductName character varying NOT NULL,
  Quantity integer NOT NULL,
  UnitPrice numeric NOT NULL,
  Specifications character varying,
  CONSTRAINT ProcurementItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProcurementItems_ProcurementRequests_ProcurementRequestId FOREIGN KEY (ProcurementRequestId) REFERENCES public.ProcurementRequests(Id),
  CONSTRAINT FK_ProcurementItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.ProcurementStatusEvents (
  Id uuid NOT NULL,
  ProcurementRequestId uuid NOT NULL,
  Status character varying NOT NULL,
  Note character varying,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ProcurementStatusEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProcurementStatusEvents_ProcurementRequests_ProcurementRequ~ FOREIGN KEY (ProcurementRequestId) REFERENCES public.ProcurementRequests(Id)
);
CREATE TABLE public.Contracts (
  Id uuid NOT NULL,
  BusinessPartnerId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  ReferenceNumber character varying NOT NULL,
  Title character varying NOT NULL,
  Terms character varying NOT NULL,
  StartDate timestamp with time zone NOT NULL,
  EndDate timestamp with time zone NOT NULL,
  AutoRenew boolean NOT NULL,
  RenewalTermMonths integer,
  Status character varying NOT NULL,
  PreviousContractId uuid,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT Contracts_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Contracts_Contracts_PreviousContractId FOREIGN KEY (PreviousContractId) REFERENCES public.Contracts(Id),
  CONSTRAINT FK_Contracts_Users_BusinessPartnerId FOREIGN KEY (BusinessPartnerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_Contracts_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ContractDeliverySchedules (
  Id uuid NOT NULL,
  ContractId uuid NOT NULL,
  ScheduledDate timestamp with time zone NOT NULL,
  Quantity integer NOT NULL,
  Status character varying NOT NULL,
  ActualDeliveryDate timestamp with time zone,
  Notes character varying,
  CONSTRAINT ContractDeliverySchedules_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ContractDeliverySchedules_Contracts_ContractId FOREIGN KEY (ContractId) REFERENCES public.Contracts(Id)
);
CREATE TABLE public.ContractDocuments (
  Id uuid NOT NULL,
  ContractId uuid NOT NULL,
  DocumentName character varying NOT NULL,
  DocumentType character varying NOT NULL,
  FileUrl character varying NOT NULL,
  UploadedAt timestamp with time zone NOT NULL,
  CONSTRAINT ContractDocuments_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ContractDocuments_Contracts_ContractId FOREIGN KEY (ContractId) REFERENCES public.Contracts(Id)
);
CREATE TABLE public.ContractItems (
  Id uuid NOT NULL,
  ContractId uuid NOT NULL,
  ProductId uuid NOT NULL,
  ProductName character varying NOT NULL,
  Quantity integer NOT NULL,
  UnitPrice numeric NOT NULL,
  Specifications character varying,
  CONSTRAINT ContractItems_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ContractItems_Contracts_ContractId FOREIGN KEY (ContractId) REFERENCES public.Contracts(Id),
  CONSTRAINT FK_ContractItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES public.Products(Id)
);
CREATE TABLE public.ContractStatusEvents (
  Id uuid NOT NULL,
  ContractId uuid NOT NULL,
  Status character varying NOT NULL,
  Note character varying,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ContractStatusEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ContractStatusEvents_Contracts_ContractId FOREIGN KEY (ContractId) REFERENCES public.Contracts(Id)
);
CREATE TABLE public.ManufacturingPartnerships (
  Id uuid NOT NULL,
  BusinessPartnerId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  ReferenceNumber character varying NOT NULL,
  Title character varying NOT NULL,
  ProductRequirements character varying NOT NULL,
  ManufacturingSpecifications character varying NOT NULL,
  Quantity integer NOT NULL,
  TargetUnitPrice numeric,
  TimelineStartDate timestamp with time zone NOT NULL,
  TimelineEndDate timestamp with time zone NOT NULL,
  Status character varying NOT NULL,
  ProducerResponseNotes character varying,
  RespondedAt timestamp with time zone,
  CompletedAt timestamp with time zone,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT ManufacturingPartnerships_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ManufacturingPartnerships_Users_BusinessPartnerId FOREIGN KEY (BusinessPartnerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_ManufacturingPartnerships_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.ManufacturingMilestones (
  Id uuid NOT NULL,
  PartnershipId uuid NOT NULL,
  Title character varying NOT NULL,
  Description character varying,
  DueDate timestamp with time zone NOT NULL,
  Status character varying NOT NULL,
  CompletedAt timestamp with time zone,
  DisplayOrder integer NOT NULL,
  CONSTRAINT ManufacturingMilestones_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ManufacturingMilestones_ManufacturingPartnerships_Partnersh~ FOREIGN KEY (PartnershipId) REFERENCES public.ManufacturingPartnerships(Id)
);
CREATE TABLE public.PartnershipStatusEvents (
  Id uuid NOT NULL,
  PartnershipId uuid NOT NULL,
  Status character varying NOT NULL,
  Note character varying,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT PartnershipStatusEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_PartnershipStatusEvents_ManufacturingPartnerships_Partnersh~ FOREIGN KEY (PartnershipId) REFERENCES public.ManufacturingPartnerships(Id)
);
CREATE TABLE public.DesignCollaborationProjects (
  Id uuid NOT NULL,
  BusinessPartnerId uuid NOT NULL,
  ProducerId uuid NOT NULL,
  ReferenceNumber character varying NOT NULL,
  Title character varying NOT NULL,
  DesignRequirements character varying NOT NULL,
  Status character varying NOT NULL,
  RespondedAt timestamp with time zone,
  CompletedAt timestamp with time zone,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT DesignCollaborationProjects_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_DesignCollaborationProjects_Users_BusinessPartnerId FOREIGN KEY (BusinessPartnerId) REFERENCES public.Users(Id),
  CONSTRAINT FK_DesignCollaborationProjects_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.CollaborationStatusEvents (
  Id uuid NOT NULL,
  ProjectId uuid NOT NULL,
  Status character varying NOT NULL,
  Note character varying,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT CollaborationStatusEvents_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_CollaborationStatusEvents_DesignCollaborationProjects_Proje~ FOREIGN KEY (ProjectId) REFERENCES public.DesignCollaborationProjects(Id)
);
CREATE TABLE public.DesignComments (
  Id uuid NOT NULL,
  ProjectId uuid NOT NULL,
  AuthorUserId uuid NOT NULL,
  Content character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT DesignComments_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_DesignComments_DesignCollaborationProjects_ProjectId FOREIGN KEY (ProjectId) REFERENCES public.DesignCollaborationProjects(Id),
  CONSTRAINT FK_DesignComments_Users_AuthorUserId FOREIGN KEY (AuthorUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.DesignRevisions (
  Id uuid NOT NULL,
  ProjectId uuid NOT NULL,
  RevisionNumber integer NOT NULL,
  Description character varying NOT NULL,
  Status character varying NOT NULL,
  SubmittedByUserId uuid NOT NULL,
  SubmittedAt timestamp with time zone NOT NULL,
  DecidedAt timestamp with time zone,
  DecisionNotes character varying,
  CONSTRAINT DesignRevisions_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_DesignRevisions_DesignCollaborationProjects_ProjectId FOREIGN KEY (ProjectId) REFERENCES public.DesignCollaborationProjects(Id),
  CONSTRAINT FK_DesignRevisions_Users_SubmittedByUserId FOREIGN KEY (SubmittedByUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.DesignFiles (
  Id uuid NOT NULL,
  ProjectId uuid NOT NULL,
  RevisionId uuid,
  FileName character varying NOT NULL,
  FileUrl character varying NOT NULL,
  FileType character varying NOT NULL,
  UploadedByUserId uuid NOT NULL,
  UploadedAt timestamp with time zone NOT NULL,
  CONSTRAINT DesignFiles_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_DesignFiles_DesignCollaborationProjects_ProjectId FOREIGN KEY (ProjectId) REFERENCES public.DesignCollaborationProjects(Id),
  CONSTRAINT FK_DesignFiles_DesignRevisions_RevisionId FOREIGN KEY (RevisionId) REFERENCES public.DesignRevisions(Id),
  CONSTRAINT FK_DesignFiles_Users_UploadedByUserId FOREIGN KEY (UploadedByUserId) REFERENCES public.Users(Id)
);
CREATE TABLE public.SponsorshipOpportunities (
  Id uuid NOT NULL,
  ProducerId uuid NOT NULL,
  Title character varying NOT NULL,
  Description character varying NOT NULL,
  FundingGoal numeric NOT NULL,
  Status character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT SponsorshipOpportunities_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SponsorshipOpportunities_Users_ProducerId FOREIGN KEY (ProducerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.SponsorshipProposals (
  Id uuid NOT NULL,
  OpportunityId uuid NOT NULL,
  BusinessPartnerId uuid NOT NULL,
  FundingAmount numeric NOT NULL,
  ProposalMessage character varying,
  Status character varying NOT NULL,
  SubmittedAt timestamp with time zone NOT NULL,
  DecidedAt timestamp with time zone,
  DecisionNotes character varying,
  CompletedAt timestamp with time zone,
  CreatedAt timestamp with time zone NOT NULL,
  UpdatedAt timestamp with time zone NOT NULL,
  CONSTRAINT SponsorshipProposals_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SponsorshipProposals_SponsorshipOpportunities_OpportunityId FOREIGN KEY (OpportunityId) REFERENCES public.SponsorshipOpportunities(Id),
  CONSTRAINT FK_SponsorshipProposals_Users_BusinessPartnerId FOREIGN KEY (BusinessPartnerId) REFERENCES public.Users(Id)
);
CREATE TABLE public.SponsorshipImpactRecords (
  Id uuid NOT NULL,
  ProposalId uuid NOT NULL,
  Description character varying NOT NULL,
  Metric character varying NOT NULL,
  Value numeric NOT NULL,
  RecordedAt timestamp with time zone NOT NULL,
  CONSTRAINT SponsorshipImpactRecords_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SponsorshipImpactRecords_SponsorshipProposals_ProposalId FOREIGN KEY (ProposalId) REFERENCES public.SponsorshipProposals(Id)
);
CREATE TABLE public.SponsorshipMilestones (
  Id uuid NOT NULL,
  ProposalId uuid NOT NULL,
  Title character varying NOT NULL,
  Description character varying,
  DueDate timestamp with time zone NOT NULL,
  Status character varying NOT NULL,
  CompletedAt timestamp with time zone,
  DisplayOrder integer NOT NULL,
  CONSTRAINT SponsorshipMilestones_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SponsorshipMilestones_SponsorshipProposals_ProposalId FOREIGN KEY (ProposalId) REFERENCES public.SponsorshipProposals(Id)
);
CREATE TABLE public.SponsorshipProgressUpdates (
  Id uuid NOT NULL,
  ProposalId uuid NOT NULL,
  AuthorUserId uuid NOT NULL,
  Content character varying NOT NULL,
  CreatedAt timestamp with time zone NOT NULL,
  CONSTRAINT SponsorshipProgressUpdates_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_SponsorshipProgressUpdates_SponsorshipProposals_ProposalId FOREIGN KEY (ProposalId) REFERENCES public.SponsorshipProposals(Id),
  CONSTRAINT FK_SponsorshipProgressUpdates_Users_AuthorUserId FOREIGN KEY (AuthorUserId) REFERENCES public.Users(Id)
);
