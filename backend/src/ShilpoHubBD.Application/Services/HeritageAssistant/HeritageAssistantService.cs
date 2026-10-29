using Microsoft.Extensions.Logging;
using ShilpoHubBD.Application.DTOs.HeritageAssistant;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.DTOs.PerformingArts;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.PerformingArts;

namespace ShilpoHubBD.Application.Services.HeritageAssistant;

public class HeritageAssistantService : IHeritageAssistantService
{
    private readonly IDistrictRepository _districtRepository;
    private readonly IHeritageFestivalRepository _festivalRepository;
    private readonly IUnescoRecordRepository _unescoRepository;
    private readonly IPerformingArtRepository _performingArtRepository;
    private readonly IHeritageAssistantProvider _provider;
    private readonly ILogger<HeritageAssistantService> _logger;

    public HeritageAssistantService(
        IDistrictRepository districtRepository,
        IHeritageFestivalRepository festivalRepository,
        IUnescoRecordRepository unescoRepository,
        IPerformingArtRepository performingArtRepository,
        IHeritageAssistantProvider provider,
        ILogger<HeritageAssistantService> logger)
    {
        _districtRepository = districtRepository;
        _festivalRepository = festivalRepository;
        _unescoRepository = unescoRepository;
        _performingArtRepository = performingArtRepository;
        _provider = provider;
        _logger = logger;
    }

    public async Task<HeritageAssistantAnswerDto> AskAsync(AskHeritageAssistantRequest request, CancellationToken cancellationToken)
    {
        // Performing-arts questions are answered straight from reviewed, Published records (with their citations and
        // detail-page links); the RAG service only knows the craft datasets, so it would have nothing to ground them on.
        // A failure here (e.g. the performing-arts tables not migrated yet) must never take the RAG path down with it.
        try
        {
            var published = await _performingArtRepository.ListAsync(
                PerformingArtRules.BuildFilter(new PerformingArtQueryParameters(), publishedOnly: true), cancellationToken);
            var matches = PerformingArtMatcher.Rank(request.Question, published);
            if (matches.Count > 0)
            {
                return PerformingArtAnswerComposer.Compose(request.Question, matches);
            }
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _logger.LogWarning(exc, "Performing-arts lookup failed; answering through the RAG provider instead.");
        }

        var districts = await _districtRepository.GetAllAsync(false, cancellationToken);
        var (festivals, _) = await _festivalRepository.GetPagedAsync(
            new HeritageFestivalQueryParameters { ActiveOnly = true, Page = 1, PageSize = 200 }, cancellationToken);
        var unescoRecords = await _unescoRepository.GetAllAsync(false, cancellationToken);

        var context = new HeritageAssistantContext
        {
            Question = request.Question.Trim(),
            Districts = districts.Select(d => new HeritageDistrictFact(d.Name, d.Division)).ToList(),
            Festivals = festivals.Select(f => new HeritageFestivalFact(f.Name, f.Description, f.District.Name)).ToList(),
            UnescoRecords = unescoRecords.Select(u => new HeritageUnescoFact(u.Title, u.Description, u.InscribedYear)).ToList(),
        };

        return await _provider.AnswerAsync(context, cancellationToken);
    }
}
