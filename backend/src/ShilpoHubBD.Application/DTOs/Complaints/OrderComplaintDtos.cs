namespace ShilpoHubBD.Application.DTOs.Complaints;

public class CreateOrderComplaintRequest
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class RespondToOrderComplaintRequest
{
    public string Message { get; set; } = string.Empty;
}

public class CustomerComplaintNoteRequest
{
    public string? Note { get; set; }
}

public class OrderComplaintDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProducerResponse { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? CustomerNote { get; set; }

    // True once the customer may rate the producer and product for this complaint's item.
    public bool CanRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
