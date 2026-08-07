namespace ShilpoHubBD.Domain.Entities.Reviews;

public class ReviewImage
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;
}
