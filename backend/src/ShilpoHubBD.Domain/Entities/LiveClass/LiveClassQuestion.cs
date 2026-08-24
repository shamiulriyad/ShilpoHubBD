using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.LiveClass;

public class LiveClassQuestion
{
    public Guid Id { get; set; }

    public Guid LiveClassId { get; set; }
    public LiveClass LiveClass { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
    public string? AnswerBody { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
}
