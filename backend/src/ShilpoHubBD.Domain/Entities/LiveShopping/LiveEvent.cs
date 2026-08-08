using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.LiveShopping;

public class LiveEvent
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public LiveEventStatus Status { get; set; }

    public DateTime ScheduledStartAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<LiveEventComment> Comments { get; set; } = new List<LiveEventComment>();
    public ICollection<LiveEventReaction> Reactions { get; set; } = new List<LiveEventReaction>();
    public ICollection<LiveEventPurchase> Purchases { get; set; } = new List<LiveEventPurchase>();
}
