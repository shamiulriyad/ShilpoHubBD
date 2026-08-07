namespace ShilpoHubBD.Application.DTOs.Reviews;

public class UpdateReviewRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
}
