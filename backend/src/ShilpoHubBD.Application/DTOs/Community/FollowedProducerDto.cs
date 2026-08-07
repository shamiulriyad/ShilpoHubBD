namespace ShilpoHubBD.Application.DTOs.Community;

public class FollowedProducerDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public DateTime FollowedAt { get; set; }
}
