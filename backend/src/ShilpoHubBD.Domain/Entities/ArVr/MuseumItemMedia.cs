namespace ShilpoHubBD.Domain.Entities.ArVr;

public class MuseumItemMedia
{
    public Guid Id { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public ArVrMediaType MediaType { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }

    public Guid MuseumItemId { get; set; }
    public MuseumItem MuseumItem { get; set; } = null!;
}
