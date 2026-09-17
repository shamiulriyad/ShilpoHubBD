using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Services.Cms;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _repository;

    public AnnouncementService(IAnnouncementRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AnnouncementDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var announcements = await _repository.GetAllAsync(activeOnly, cancellationToken);
        return announcements.Select(ToDto).ToList();
    }

    public async Task<AnnouncementDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var announcement = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Announcement not found.");
        return ToDto(announcement);
    }

    public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        if (request.EndsAt.HasValue && request.StartsAt.HasValue && request.EndsAt < request.StartsAt)
        {
            throw new ConflictException("End time cannot be earlier than start time.");
        }

        var now = DateTime.UtcNow;
        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Severity = request.Severity,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(announcement, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(announcement);
    }

    public async Task<AnnouncementDto> UpdateAsync(Guid id, UpdateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var announcement = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Announcement not found.");

        if (request.EndsAt.HasValue && request.StartsAt.HasValue && request.EndsAt < request.StartsAt)
        {
            throw new ConflictException("End time cannot be earlier than start time.");
        }

        announcement.Title = request.Title.Trim();
        announcement.Message = request.Message.Trim();
        announcement.Severity = request.Severity;
        announcement.StartsAt = request.StartsAt;
        announcement.EndsAt = request.EndsAt;
        announcement.IsActive = request.IsActive;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(announcement);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var announcement = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Announcement not found.");

        _repository.Remove(announcement);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static AnnouncementDto ToDto(Announcement announcement) => new()
    {
        Id = announcement.Id,
        Title = announcement.Title,
        Message = announcement.Message,
        Severity = announcement.Severity,
        StartsAt = announcement.StartsAt,
        EndsAt = announcement.EndsAt,
        IsActive = announcement.IsActive,
        CreatedAt = announcement.CreatedAt,
        UpdatedAt = announcement.UpdatedAt,
    };
}
