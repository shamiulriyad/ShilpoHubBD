namespace ShilpoHubBD.Domain.Entities.Community;

public class VillageFavorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public Guid VillageId { get; set; }
    public Village Village { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
