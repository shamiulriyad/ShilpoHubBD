namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

public enum ProducerPartnershipAuctionLotStatus
{
    /// <summary>Entered by the admin; bidding not open yet (auction is still Draft/Scheduled/RegistrationOpen).</summary>
    Pending,

    /// <summary>Auction is Live and this lot accepts bids.</summary>
    Open,

    /// <summary>Admin withdrew this producer before the auction went live.</summary>
    Withdrawn,

    /// <summary>Auction ended with at least one bid on this lot and a winner was assigned.</summary>
    Awarded,

    /// <summary>Auction ended with no bids (or no eligible winner) on this lot.</summary>
    Unsold,
}
