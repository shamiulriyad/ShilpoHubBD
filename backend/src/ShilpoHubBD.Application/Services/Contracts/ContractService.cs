using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Contracts;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Application.Services.Contracts;

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public ContractService(
        IContractRepository contractRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _contractRepository = contractRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public async Task<ContractDto> CreateAsync(Guid businessPartnerId, CreateContractRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(request.ProducerId, cancellationToken);
        if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        var now = DateTime.UtcNow;
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = businessPartnerId,
            ProducerId = request.ProducerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            Terms = request.Terms.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AutoRenew = request.AutoRenew,
            RenewalTermMonths = request.RenewalTermMonths,
            Status = ContractStatus.PendingApproval,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await AttachItemsAsync(contract, request.Items, cancellationToken);
        AttachDeliverySchedules(contract, request.DeliverySchedules);

        contract.StatusHistory.Add(new ContractStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.PendingApproval,
            Note = "Contract proposed to producer.",
            CreatedAt = now,
        });

        await _contractRepository.AddAsync(contract, cancellationToken);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        var created = await _contractRepository.GetByIdWithDetailsAsync(contract.Id, cancellationToken)
            ?? throw new NotFoundException("Contract not found.");
        return ToDto(created);
    }

    public async Task<PagedResult<ContractListItemDto>> GetForBusinessPartnerAsync(
        Guid businessPartnerId, bool isAdmin, ContractQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = isAdmin
            ? await _contractRepository.GetPagedAllAsync(parameters, cancellationToken)
            : await _contractRepository.GetPagedForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);

        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<ContractListItemDto>> GetForProducerAsync(
        Guid producerId, ContractQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _contractRepository.GetPagedForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<ContractDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var contract = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);
        return ToDto(contract);
    }

    public async Task<ContractDto> AcceptAsync(Guid id, Guid producerId, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract not found.");

        if (contract.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to accept this contract.");
        }

        if (contract.Status != ContractStatus.PendingApproval)
        {
            throw new ConflictException("Only a pending contract can be accepted.");
        }

        var now = DateTime.UtcNow;
        contract.Status = ContractStatus.Active;
        contract.UpdatedAt = now;
        contract.StatusHistory.Add(new ContractStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.Active,
            Note = "Accepted by producer.",
            CreatedAt = now,
        });

        await _contractRepository.SaveChangesAsync(cancellationToken);
        return ToDto(contract);
    }

    public async Task<ContractDto> RejectAsync(Guid id, Guid producerId, ContractDecisionRequest request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract not found.");

        if (contract.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to reject this contract.");
        }

        if (contract.Status != ContractStatus.PendingApproval)
        {
            throw new ConflictException("Only a pending contract can be rejected.");
        }

        var now = DateTime.UtcNow;
        contract.Status = ContractStatus.Rejected;
        contract.UpdatedAt = now;
        contract.StatusHistory.Add(new ContractStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.Rejected,
            Note = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = now,
        });

        await _contractRepository.SaveChangesAsync(cancellationToken);
        return ToDto(contract);
    }

    public async Task<ContractDto> TerminateAsync(Guid id, Guid currentUserId, bool isAdmin, ContractDecisionRequest request, CancellationToken cancellationToken)
    {
        var contract = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (contract.Status != ContractStatus.Active)
        {
            throw new ConflictException("Only an active contract can be terminated.");
        }

        var now = DateTime.UtcNow;
        contract.Status = ContractStatus.Terminated;
        contract.UpdatedAt = now;
        contract.StatusHistory.Add(new ContractStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.Terminated,
            Note = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = now,
        });

        await _contractRepository.SaveChangesAsync(cancellationToken);
        return ToDto(contract);
    }

    public async Task<ContractDto> RenewAsync(Guid id, Guid businessPartnerId, bool isAdmin, RenewContractRequest request, CancellationToken cancellationToken)
    {
        var previous = await _contractRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract not found.");

        if (!isAdmin && previous.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to renew this contract.");
        }

        if (previous.Status is not (ContractStatus.Active or ContractStatus.Expired))
        {
            throw new ConflictException("Only an active or expired contract can be renewed.");
        }

        var now = DateTime.UtcNow;
        var renewed = new Contract
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = previous.BusinessPartnerId,
            ProducerId = previous.ProducerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = previous.Title,
            Terms = previous.Terms,
            StartDate = previous.EndDate,
            EndDate = request.NewEndDate,
            AutoRenew = previous.AutoRenew,
            RenewalTermMonths = previous.RenewalTermMonths,
            Status = ContractStatus.PendingApproval,
            PreviousContractId = previous.Id,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var itemInputs = request.Items ?? previous.Items.Select(i => new ContractItemInput
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Specifications = i.Specifications,
        }).ToList();
        await AttachItemsAsync(renewed, itemInputs, cancellationToken);

        var scheduleInputs = request.DeliverySchedules ?? new List<ContractDeliveryScheduleInput>();
        AttachDeliverySchedules(renewed, scheduleInputs);

        renewed.StatusHistory.Add(new ContractStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.PendingApproval,
            Note = $"Renewal of contract '{previous.ReferenceNumber}'.",
            CreatedAt = now,
        });

        await _contractRepository.AddAsync(renewed, cancellationToken);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        var created = await _contractRepository.GetByIdWithDetailsAsync(renewed.Id, cancellationToken)
            ?? throw new NotFoundException("Contract not found.");
        return ToDto(created);
    }

    public async Task<ContractDocumentDto> AddDocumentAsync(
        Guid id, Guid currentUserId, bool isAdmin, AddContractDocumentRequest request, CancellationToken cancellationToken)
    {
        var contract = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        var now = DateTime.UtcNow;
        var document = new ContractDocument
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            DocumentName = request.DocumentName.Trim(),
            DocumentType = request.DocumentType.Trim(),
            FileUrl = request.FileUrl.Trim(),
            UploadedAt = now,
        };

        contract.Documents.Add(document);
        contract.UpdatedAt = now;

        await _contractRepository.SaveChangesAsync(cancellationToken);

        return new ContractDocumentDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            DocumentType = document.DocumentType,
            FileUrl = document.FileUrl,
            UploadedAt = document.UploadedAt,
        };
    }

    public async Task<ContractDeliveryScheduleDto> UpdateDeliveryStatusAsync(
        Guid id, Guid scheduleId, Guid currentUserId, bool isAdmin, UpdateDeliveryStatusRequest request, CancellationToken cancellationToken)
    {
        var schedule = await _contractRepository.GetDeliveryScheduleAsync(id, scheduleId, cancellationToken)
            ?? throw new NotFoundException("Delivery schedule entry not found.");

        if (!isAdmin && schedule.Contract.BusinessPartnerId != currentUserId && schedule.Contract.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this delivery schedule.");
        }

        schedule.Status = request.Status;
        schedule.ActualDeliveryDate = request.ActualDeliveryDate;
        schedule.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _contractRepository.SaveChangesAsync(cancellationToken);

        return new ContractDeliveryScheduleDto
        {
            Id = schedule.Id,
            ScheduledDate = schedule.ScheduledDate,
            Quantity = schedule.Quantity,
            Status = schedule.Status,
            ActualDeliveryDate = schedule.ActualDeliveryDate,
            Notes = schedule.Notes,
        };
    }

    private async Task<Contract> GetPartyAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract not found.");

        if (!isAdmin && contract.BusinessPartnerId != currentUserId && contract.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this contract.");
        }

        return contract;
    }

    private async Task AttachItemsAsync(Contract contract, List<ContractItemInput> items, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException($"Product '{item.ProductId}' not found.");

            contract.Items.Add(new ContractItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Specifications = string.IsNullOrWhiteSpace(item.Specifications) ? null : item.Specifications.Trim(),
            });
        }
    }

    private static void AttachDeliverySchedules(Contract contract, List<ContractDeliveryScheduleInput> schedules)
    {
        foreach (var schedule in schedules)
        {
            contract.DeliverySchedules.Add(new ContractDeliverySchedule
            {
                Id = Guid.NewGuid(),
                ScheduledDate = schedule.ScheduledDate,
                Quantity = schedule.Quantity,
                Status = ContractDeliveryStatus.Pending,
                Notes = string.IsNullOrWhiteSpace(schedule.Notes) ? null : schedule.Notes.Trim(),
            });
        }
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string referenceNumber;

        do
        {
            referenceNumber = $"CON-{year}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _contractRepository.ExistsByReferenceNumberAsync(referenceNumber, cancellationToken));

        return referenceNumber;
    }

    private static PagedResult<ContractListItemDto> ToPagedListDto(List<Contract> items, int totalCount, ContractQueryParameters parameters)
    {
        return new PagedResult<ContractListItemDto>
        {
            Items = items.Select(c => new ContractListItemDto
            {
                Id = c.Id,
                ReferenceNumber = c.ReferenceNumber,
                Title = c.Title,
                ProducerName = c.Producer.FullName,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ContractValue = c.Items.Sum(i => i.UnitPrice * i.Quantity),
                Status = c.Status,
                CreatedAt = c.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static ContractDto ToDto(Contract contract) => new()
    {
        Id = contract.Id,
        ReferenceNumber = contract.ReferenceNumber,
        BusinessPartnerId = contract.BusinessPartnerId,
        BusinessPartnerName = contract.BusinessPartner.FullName,
        ProducerId = contract.ProducerId,
        ProducerName = contract.Producer.FullName,
        Title = contract.Title,
        Terms = contract.Terms,
        StartDate = contract.StartDate,
        EndDate = contract.EndDate,
        IsExpired = contract.Status == ContractStatus.Active && contract.EndDate < DateTime.UtcNow,
        AutoRenew = contract.AutoRenew,
        RenewalTermMonths = contract.RenewalTermMonths,
        Status = contract.Status,
        PreviousContractId = contract.PreviousContractId,
        ContractValue = contract.Items.Sum(i => i.UnitPrice * i.Quantity),
        Items = contract.Items.Select(i => new ContractItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Specifications = i.Specifications,
        }).ToList(),
        DeliverySchedules = contract.DeliverySchedules
            .OrderBy(s => s.ScheduledDate)
            .Select(s => new ContractDeliveryScheduleDto
            {
                Id = s.Id,
                ScheduledDate = s.ScheduledDate,
                Quantity = s.Quantity,
                Status = s.Status,
                ActualDeliveryDate = s.ActualDeliveryDate,
                Notes = s.Notes,
            }).ToList(),
        Documents = contract.Documents.Select(d => new ContractDocumentDto
        {
            Id = d.Id,
            DocumentName = d.DocumentName,
            DocumentType = d.DocumentType,
            FileUrl = d.FileUrl,
            UploadedAt = d.UploadedAt,
        }).ToList(),
        StatusHistory = contract.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new ContractStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = contract.CreatedAt,
        UpdatedAt = contract.UpdatedAt,
    };
}
