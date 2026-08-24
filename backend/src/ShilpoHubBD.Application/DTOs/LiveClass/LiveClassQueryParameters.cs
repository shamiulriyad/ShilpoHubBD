using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Application.DTOs.LiveClass;

public class LiveClassQueryParameters
{
    public Guid? InstructorUserId { get; set; }
    public LiveClassStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
