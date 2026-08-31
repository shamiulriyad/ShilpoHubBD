namespace ShilpoHubBD.Application.DTOs.Governance;

public class CreateComplaintRequest
{
    /// <summary>ProductQuality, Fraud, Counterfeit, Delivery, Payment, Conduct, HeritageMisrepresentation or Other.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Low, Medium, High or Urgent. Defaults to Medium.</summary>
    public string Priority { get; set; } = "Medium";

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Optional — leave blank to record the complaint filed on someone's behalf.</summary>
    public Guid? ComplainantUserId { get; set; }
    public string? ComplainantName { get; set; }
    public string? ComplainantContact { get; set; }

    /// <summary>Producer, Product, Order, Payment, QrCode, Review, Village, District or Other.</summary>
    public string AgainstType { get; set; } = "Other";
    public Guid? AgainstId { get; set; }
    public string? AgainstLabel { get; set; }
    public Guid? RelatedOrderId { get; set; }
}

public class UpdateComplaintRequest
{
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AgainstLabel { get; set; }
}

public class AddComplaintUpdateRequest
{
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; }

    /// <summary>Optional status transition to apply with this update.</summary>
    public string? NewStatus { get; set; }
}

public class AssignComplaintRequest
{
    public Guid AssigneeUserId { get; set; }
    public string? Note { get; set; }
}

public class ResolveComplaintRequest
{
    public string Resolution { get; set; } = string.Empty;

    /// <summary>Resolved (default) or Rejected.</summary>
    public string Outcome { get; set; } = "Resolved";
}

public class LinkComplaintFlagRequest
{
    public Guid MonitoringFlagId { get; set; }
}

public class ComplaintQueryParameters
{
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ComplainantUserId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
