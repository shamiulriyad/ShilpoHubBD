namespace ShilpoHubBD.Application.DTOs.Governance;

public class ComplaintDto
{
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid? ComplainantUserId { get; set; }
    public string? ComplainantName { get; set; }
    public string? ComplainantContact { get; set; }

    public string AgainstType { get; set; } = string.Empty;
    public Guid? AgainstId { get; set; }
    public string? AgainstLabel { get; set; }
    public Guid? RelatedOrderId { get; set; }
    public Guid? MonitoringFlagId { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }

    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ComplaintUpdateDto> Updates { get; set; } = new();
}

public class ComplaintUpdateDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ComplaintListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? AgainstLabel { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
}
