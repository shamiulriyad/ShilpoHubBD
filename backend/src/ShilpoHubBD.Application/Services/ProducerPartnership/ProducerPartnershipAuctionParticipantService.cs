using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Services.ProducerPartnership;

public class ProducerPartnershipAuctionParticipantService : IProducerPartnershipAuctionParticipantService
{
    private readonly IProducerPartnershipAuctionRepository _auctionRepository;
    private readonly IProducerPartnershipAuctionParticipantRepository _participantRepository;

    public ProducerPartnershipAuctionParticipantService(
        IProducerPartnershipAuctionRepository auctionRepository, IProducerPartnershipAuctionParticipantRepository participantRepository)
    {
        _auctionRepository = auctionRepository;
        _participantRepository = participantRepository;
    }

    public async Task<ProducerPartnershipAuctionParticipantDto> ApplyAsync(
        Guid auctionId, Guid businessPartnerId, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken)
            ?? throw new NotFoundException("Producer partnership auction not found.");

        if (auction.Status != ProducerPartnershipAuctionStatus.RegistrationOpen)
        {
            throw new ConflictException("Registration is not open for this auction.");
        }

        var now = DateTime.UtcNow;
        if (auction.RegistrationClosesAt.HasValue && now > auction.RegistrationClosesAt.Value)
        {
            throw new ConflictException("Registration has closed for this auction.");
        }

        var existing = await _participantRepository.GetForBusinessPartnerAsync(auctionId, businessPartnerId, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("You have already applied to participate in this auction.");
        }

        var participant = new ProducerPartnershipAuctionParticipant
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            BusinessPartnerId = businessPartnerId,
            Status = ProducerPartnershipAuctionParticipantStatus.Applied,
            AppliedAt = now,
        };

        await _participantRepository.AddAsync(participant, cancellationToken);
        await _participantRepository.SaveChangesAsync(cancellationToken);

        var saved = await _participantRepository.GetByIdAsync(participant.Id, cancellationToken)
            ?? throw new NotFoundException("Auction participant not found.");
        return ToDto(saved);
    }

    public async Task<ProducerPartnershipAuctionParticipantDto> DecideAsync(
        Guid auctionId, Guid participantId, Guid decidedByUserId, DecideProducerPartnershipAuctionParticipantRequest request, CancellationToken cancellationToken)
    {
        var participant = await _participantRepository.GetByIdAsync(participantId, cancellationToken);
        if (participant is null || participant.AuctionId != auctionId)
        {
            throw new NotFoundException("Auction participant not found.");
        }

        if (participant.Status != ProducerPartnershipAuctionParticipantStatus.Applied)
        {
            throw new ConflictException("This application has already been decided.");
        }

        participant.Status = request.Approve
            ? ProducerPartnershipAuctionParticipantStatus.Approved
            : ProducerPartnershipAuctionParticipantStatus.Rejected;
        participant.DecidedAt = DateTime.UtcNow;
        participant.DecidedByUserId = decidedByUserId;
        participant.DecisionNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _participantRepository.SaveChangesAsync(cancellationToken);
        return ToDto(participant);
    }

    public async Task<List<ProducerPartnershipAuctionParticipantDto>> GetForAuctionAsync(Guid auctionId, CancellationToken cancellationToken)
    {
        var participants = await _participantRepository.GetForAuctionAsync(auctionId, cancellationToken);
        return participants.Select(ToDto).ToList();
    }

    public async Task<ProducerPartnershipAuctionParticipantDto?> GetMineAsync(Guid auctionId, Guid businessPartnerId, CancellationToken cancellationToken)
    {
        var participant = await _participantRepository.GetForBusinessPartnerAsync(auctionId, businessPartnerId, cancellationToken);
        return participant is null ? null : ToDto(participant);
    }

    private static ProducerPartnershipAuctionParticipantDto ToDto(ProducerPartnershipAuctionParticipant participant) => new()
    {
        Id = participant.Id,
        AuctionId = participant.AuctionId,
        BusinessPartnerId = participant.BusinessPartnerId,
        BusinessPartnerName = participant.BusinessPartner?.FullName ?? string.Empty,
        Status = participant.Status,
        AppliedAt = participant.AppliedAt,
        DecidedAt = participant.DecidedAt,
        DecisionNotes = participant.DecisionNotes,
    };
}
