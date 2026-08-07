using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Marketplace;

public class ProducerStoryService : IProducerStoryService
{
    private readonly IProducerStoryRepository _producerStoryRepository;
    private readonly IUserRepository _userRepository;

    public ProducerStoryService(IProducerStoryRepository producerStoryRepository, IUserRepository userRepository)
    {
        _producerStoryRepository = producerStoryRepository;
        _userRepository = userRepository;
    }

    public async Task<ProducerStoryDto> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var story = await _producerStoryRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer story not found.");

        var producer = await _userRepository.GetByIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        return ToDto(story, producer.FullName);
    }

    public async Task<ProducerStoryDto> UpsertAsync(Guid producerId, Guid currentUserId, bool isAdmin, UpsertProducerStoryRequest request, CancellationToken cancellationToken)
    {
        if (!isAdmin && producerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this producer's story.");
        }

        var producer = await _userRepository.GetByIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        var story = await _producerStoryRepository.GetByProducerIdAsync(producerId, cancellationToken);
        var now = DateTime.UtcNow;

        if (story is null)
        {
            story = new ProducerStory
            {
                Id = Guid.NewGuid(),
                ProducerId = producerId,
                HeritageId = await GenerateUniqueHeritageIdAsync(cancellationToken),
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _producerStoryRepository.AddAsync(story, cancellationToken);
        }

        story.Generations = request.Generations;
        story.FoundingYear = request.FoundingYear;
        story.Quote = request.Quote.Trim();
        story.UpdatedAt = now;

        story.Chapters.Clear();
        AttachChapters(story, request.Chapters);

        await _producerStoryRepository.SaveChangesAsync(cancellationToken);

        return ToDto(story, producer.FullName);
    }

    public async Task DeleteAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var story = await _producerStoryRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer story not found.");

        _producerStoryRepository.Remove(story);
        await _producerStoryRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueHeritageIdAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string heritageId;

        do
        {
            heritageId = $"SH-HID-{year}{Random.Shared.Next(1000, 9999)}";
        }
        while (await _producerStoryRepository.ExistsByHeritageIdAsync(heritageId, cancellationToken));

        return heritageId;
    }

    private static void AttachChapters(ProducerStory story, List<StoryChapterInput> chapters)
    {
        for (var i = 0; i < chapters.Count; i++)
        {
            story.Chapters.Add(new ProducerStoryChapter
            {
                Id = Guid.NewGuid(),
                Heading = chapters[i].Heading.Trim(),
                Body = chapters[i].Body.Trim(),
                DisplayOrder = i,
            });
        }
    }

    private static ProducerStoryDto ToDto(ProducerStory story, string producerName) => new()
    {
        Id = story.Id,
        ProducerId = story.ProducerId,
        ProducerName = producerName,
        HeritageId = story.HeritageId,
        Generations = story.Generations,
        FoundingYear = story.FoundingYear,
        Quote = story.Quote,
        Chapters = story.Chapters
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new StoryChapterDto { Heading = c.Heading, Body = c.Body, DisplayOrder = c.DisplayOrder })
            .ToList(),
    };
}
