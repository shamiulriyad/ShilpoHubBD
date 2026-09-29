namespace ShilpoHubBD.Application.DTOs.Reviews;

// ---- Review index feed (consumed by the Python sync worker; carries no database access) ----------------------

public class ReviewIndexItemDto
{
    public Guid ReviewId { get; set; }

    /// <summary>State version this item was built from; echoed back in the ack.</summary>
    public int Version { get; set; }

    /// <summary>upsert = re-embed and store; payload = only refresh the filterable snapshot; delete = remove the vectors.</summary>
    public string Action { get; set; } = "upsert";
    public string? TextHash { get; set; }
    public string Text { get; set; } = string.Empty;
    public Dictionary<string, object?> Payload { get; set; } = new();
}

public class ReviewIndexBatchDto
{
    public List<ReviewIndexItemDto> Items { get; set; } = new();
    public int PendingTotal { get; set; }
}

public class ReviewIndexAckItem
{
    public Guid ReviewId { get; set; }
    public int Version { get; set; }
    public bool Success { get; set; }
    public string? TextHash { get; set; }
    public string? EmbeddingModel { get; set; }
    public string? Error { get; set; }
}

public class ReviewIndexAckRequest
{
    public List<ReviewIndexAckItem> Items { get; set; } = new();
}
