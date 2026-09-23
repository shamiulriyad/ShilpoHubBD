using ShilpoHubBD.Application.DTOs.HeritageAssistant;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.HeritageAssistant;

public class HeritageAssistantService : IHeritageAssistantService
{
    private readonly IDistrictRepository _districtRepository;
    private readonly IHeritageFestivalRepository _festivalRepository;
    private readonly IUnescoRecordRepository _unescoRepository;
    private readonly IHeritageAssistantProvider _provider;

    public HeritageAssistantService(
        IDistrictRepository districtRepository,
        IHeritageFestivalRepository festivalRepository,
        IUnescoRecordRepository unescoRepository,
        IHeritageAssistantProvider provider)
    {
        _districtRepository = districtRepository;
        _festivalRepository = festivalRepository;
        _unescoRepository = unescoRepository;
        _provider = provider;
    }

    public async Task<HeritageAssistantAnswerDto> AskAsync(AskHeritageAssistantRequest request, CancellationToken cancellationToken)
    {
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
