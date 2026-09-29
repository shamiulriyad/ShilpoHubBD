using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Admin;
using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Admin.Services;

[Trait("Feature", "Admin")]
[Trait("Layer", "Service")]
public class IdentityVerificationServiceTests
{
    private readonly IIdentityVerificationRepository _repository = Substitute.For<IIdentityVerificationRepository>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private IdentityVerificationService CreateService() => new(_repository);

    private static SubmitIdentityVerificationRequest SubmitRequest() => new()
    {
        Type = "NationalId", DocumentNumber = "  1234567890  ", FrontImageUrl = "  front.jpg  ",
        BackImageUrl = "  ", SelfieImageUrl = null, ApplicantNote = "   ",
    };

    private void ReturnsAfterAdd(IdentityVerificationRequest stored)
        => _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(stored);

    // ---------- SubmitAsync ----------

    [Fact]
    public async Task SubmitAsync_ValidRequest_StoresATrimmedPendingRequest()
    {
        var userId = Guid.NewGuid();
        IdentityVerificationRequest? saved = null;
        var before = DateTime.UtcNow;

        var request = SubmitRequest();
        await _repository.AddAsync(Arg.Do<IdentityVerificationRequest>(v => { saved = v; ReturnsAfterAdd(v); }), Arg.Any<CancellationToken>());

        var dto = await CreateService().SubmitAsync(userId, request, Ct);

        Assert.NotNull(saved);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(IdentityVerificationType.NationalId, saved.Type);
        Assert.Equal(IdentityVerificationStatus.Pending, saved.Status);
        Assert.Equal("1234567890", saved.DocumentNumber);
        Assert.Equal("front.jpg", saved.FrontImageUrl);
        Assert.Null(saved.BackImageUrl);
        Assert.Null(saved.SelfieImageUrl);
        Assert.Null(saved.ApplicantNote);
        Assert.InRange(saved.SubmittedAt, before, DateTime.UtcNow);
        Assert.Equal(saved.SubmittedAt, saved.CreatedAt);
        Assert.Equal(saved.CreatedAt, saved.UpdatedAt);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(saved.Id, dto.Id);
    }

    [Fact]
    public async Task SubmitAsync_OptionalFieldsProvided_AreTrimmedAndKept()
    {
        var request = SubmitRequest();
        request.BackImageUrl = "  back.jpg  ";
        request.SelfieImageUrl = "  selfie.jpg  ";
        request.ApplicantNote = "  Please review quickly.  ";
        IdentityVerificationRequest? saved = null;
        _repository.AddAsync(Arg.Do<IdentityVerificationRequest>(v => { saved = v; ReturnsAfterAdd(v); }), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await CreateService().SubmitAsync(Guid.NewGuid(), request, Ct);

        Assert.NotNull(saved);
        Assert.Equal("back.jpg", saved.BackImageUrl);
        Assert.Equal("selfie.jpg", saved.SelfieImageUrl);
        Assert.Equal("Please review quickly.", saved.ApplicantNote);
    }

    [Fact]
    public async Task SubmitAsync_AlreadyHasAPendingRequest_ThrowsConflictAndSavesNothing()
    {
        _repository.HasPendingRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().SubmitAsync(Guid.NewGuid(), SubmitRequest(), Ct));

        Assert.Equal("You already have a pending identity verification request.", error.Message);
        await _repository.DidNotReceive().AddAsync(Arg.Any<IdentityVerificationRequest>(), Arg.Any<CancellationToken>());
    }

    // ---------- GetMineAsync ----------

    [Fact]
    public async Task GetMineAsync_ReturnsTheUsersOwnRequestsMapped()
    {
        var userId = Guid.NewGuid();
        var request = new IdentityVerificationRequest { Id = Guid.NewGuid(), UserId = userId, DocumentNumber = "X", FrontImageUrl = "f" };
        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<IdentityVerificationRequest> { request });

        var result = await CreateService().GetMineAsync(userId, Ct);

        Assert.Equal(request.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetMineAsync_NoRequests_ReturnsEmpty()
    {
        _repository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new List<IdentityVerificationRequest>());

        Assert.Empty(await CreateService().GetMineAsync(Guid.NewGuid(), Ct));
    }

    // ---------- GetPagedAsync ----------

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(2, 0, 2, 20)]
    [InlineData(2, 101, 2, 20)]
    public async Task GetPagedAsync_KeepsPageAndSizeWithinBounds(int page, int pageSize, int expectedPage, int expectedSize)
    {
        _repository.GetPagedAsync(Arg.Any<IdentityVerificationQueryParameters>(), Arg.Any<CancellationToken>())
            .Returns((new List<IdentityVerificationRequest>(), 0));

        var result = await CreateService().GetPagedAsync(new IdentityVerificationQueryParameters { Page = page, PageSize = pageSize }, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedItemsAndTotalCount()
    {
        var request = new IdentityVerificationRequest { Id = Guid.NewGuid(), DocumentNumber = "X", FrontImageUrl = "f" };
        _repository.GetPagedAsync(Arg.Any<IdentityVerificationQueryParameters>(), Arg.Any<CancellationToken>())
            .Returns((new List<IdentityVerificationRequest> { request }, 9));

        var result = await CreateService().GetPagedAsync(new IdentityVerificationQueryParameters(), Ct);

        Assert.Equal(9, result.TotalCount);
        Assert.Equal(request.Id, Assert.Single(result.Items).Id);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ExistingRequest_ReturnsIt()
    {
        var request = new IdentityVerificationRequest { Id = Guid.NewGuid(), DocumentNumber = "X", FrontImageUrl = "f" };
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>()).Returns(request);

        Assert.Equal(request.Id, (await CreateService().GetByIdAsync(request.Id, Ct)).Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownRequest_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Identity verification request not found.", error.Message);
    }

    // ---------- ApproveAsync ----------

    private IdentityVerificationRequest PendingRequest(IdentityVerificationStatus status = IdentityVerificationStatus.Pending)
    {
        var request = new IdentityVerificationRequest
        {
            Id = Guid.NewGuid(), DocumentNumber = "X", FrontImageUrl = "f", Status = status, RejectionReason = "old reason",
        };
        _repository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>()).Returns(request);
        return request;
    }

    [Fact]
    public async Task ApproveAsync_PendingRequest_ApprovesItAndClearsAnyRejectionReason()
    {
        var request = PendingRequest();
        var reviewer = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var dto = await CreateService().ApproveAsync(request.Id, reviewer, Ct);

        Assert.Equal(IdentityVerificationStatus.Approved, request.Status);
        Assert.Equal(reviewer, request.ReviewedByUserId);
        Assert.InRange(request.ReviewedAt!.Value, before, DateTime.UtcNow);
        Assert.Null(request.RejectionReason);
        Assert.Equal(request.UpdatedAt, request.ReviewedAt!.Value, TimeSpan.FromSeconds(1));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("Approved", dto.Status);
    }

    [Theory]
    [InlineData(IdentityVerificationStatus.Approved)]
    [InlineData(IdentityVerificationStatus.Rejected)]
    public async Task ApproveAsync_AlreadyReviewedRequest_ThrowsConflictAndSavesNothing(IdentityVerificationStatus status)
    {
        var request = PendingRequest(status);

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().ApproveAsync(request.Id, Guid.NewGuid(), Ct));

        Assert.Equal($"This request has already been {status}.", error.Message);
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_UnknownRequest_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().ApproveAsync(Guid.NewGuid(), Guid.NewGuid(), Ct));

        Assert.Equal("Identity verification request not found.", error.Message);
    }

    // ---------- RejectAsync ----------

    [Fact]
    public async Task RejectAsync_PendingRequest_RejectsItWithATrimmedReason()
    {
        var request = PendingRequest();
        var reviewer = Guid.NewGuid();

        var dto = await CreateService().RejectAsync(request.Id, reviewer, new RejectIdentityVerificationRequest { RejectionReason = "  Blurry photo.  " }, Ct);

        Assert.Equal(IdentityVerificationStatus.Rejected, request.Status);
        Assert.Equal(reviewer, request.ReviewedByUserId);
        Assert.Equal("Blurry photo.", request.RejectionReason);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("Rejected", dto.Status);
    }

    [Fact]
    public async Task RejectAsync_AlreadyApprovedRequest_ThrowsConflictAndSavesNothing()
    {
        var request = PendingRequest(IdentityVerificationStatus.Approved);

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().RejectAsync(request.Id, Guid.NewGuid(), new RejectIdentityVerificationRequest { RejectionReason = "x" }, Ct));

        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectAsync_UnknownRequest_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().RejectAsync(Guid.NewGuid(), Guid.NewGuid(), new RejectIdentityVerificationRequest { RejectionReason = "x" }, Ct));

        Assert.Equal("Identity verification request not found.", error.Message);
    }
}
