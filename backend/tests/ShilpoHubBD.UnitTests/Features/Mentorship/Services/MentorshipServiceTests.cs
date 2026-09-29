using ShilpoHubBD.Application.DTOs.Mentorship;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.Mentorship;
using ShilpoHubBD.UnitTests.Common;
using MentorshipService = ShilpoHubBD.Application.Services.Mentorship.MentorshipService;

namespace ShilpoHubBD.UnitTests.Features.Mentorship.Services;

[Trait("Feature", "Mentorship")]
[Trait("Layer", "Service")]
public class MentorshipServiceTests
{
    private readonly IMentorshipRequestRepository _repository = Substitute.For<IMentorshipRequestRepository>();
    private readonly IMentorRepository _mentorRepository = Substitute.For<IMentorRepository>();
    private readonly IHeritageSkillRepository _heritageSkillRepository = Substitute.For<IHeritageSkillRepository>();
    private readonly MentorshipService _service;

    public MentorshipServiceTests()
    {
        _service = new MentorshipService(_repository, _mentorRepository, _heritageSkillRepository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static MentorProfile MakeMentor(bool isActive = true)
    {
        var user = TestUsers.Create(fullName: "Mentor");
        return new MentorProfile { Id = Guid.NewGuid(), UserId = user.Id, User = user, IsActive = isActive };
    }

    private static MentorshipRequest MakeRequest(MentorProfile mentor, Guid learnerUserId, MentorshipRequestStatus status = MentorshipRequestStatus.Pending)
    {
        var learner = TestUsers.Create(fullName: "Learner");
        learner.Id = learnerUserId;
        return new MentorshipRequest
        {
            Id = Guid.NewGuid(), MentorProfileId = mentor.Id, MentorProfile = mentor,
            LearnerUserId = learnerUserId, Learner = learner, Message = "Please teach me", Status = status, RequestedAt = DateTime.UtcNow,
        };
    }

    [Fact]
    public async Task CreateRequestAsync_UnknownMentor_ThrowsNotFound()
    {
        _mentorRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MentorProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateRequestAsync(Guid.NewGuid(), new CreateMentorshipRequestRequest { MentorProfileId = Guid.NewGuid(), Message = "x" }, Ct));
    }

    [Fact]
    public async Task CreateRequestAsync_InactiveMentor_ThrowsConflict()
    {
        var mentor = MakeMentor(isActive: false);
        _mentorRepository.GetByIdAsync(mentor.Id, Arg.Any<CancellationToken>()).Returns(mentor);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateRequestAsync(Guid.NewGuid(), new CreateMentorshipRequestRequest { MentorProfileId = mentor.Id, Message = "x" }, Ct));
    }

    [Fact]
    public async Task CreateRequestAsync_RequestingFromYourself_ThrowsConflict()
    {
        var mentor = MakeMentor();
        _mentorRepository.GetByIdAsync(mentor.Id, Arg.Any<CancellationToken>()).Returns(mentor);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateRequestAsync(mentor.UserId, new CreateMentorshipRequestRequest { MentorProfileId = mentor.Id, Message = "x" }, Ct));
    }

    [Fact]
    public async Task CreateRequestAsync_UnknownHeritageSkill_ThrowsNotFound()
    {
        var mentor = MakeMentor();
        _mentorRepository.GetByIdAsync(mentor.Id, Arg.Any<CancellationToken>()).Returns(mentor);
        _heritageSkillRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HeritageSkill?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateRequestAsync(
            Guid.NewGuid(), new CreateMentorshipRequestRequest { MentorProfileId = mentor.Id, HeritageSkillId = Guid.NewGuid(), Message = "x" }, Ct));
    }

    [Fact]
    public async Task CreateRequestAsync_AlreadyHasAnOpenRequest_ThrowsConflict()
    {
        var mentor = MakeMentor();
        var learnerId = Guid.NewGuid();
        _mentorRepository.GetByIdAsync(mentor.Id, Arg.Any<CancellationToken>()).Returns(mentor);
        _repository.HasOpenRequestAsync(mentor.Id, learnerId, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateRequestAsync(learnerId, new CreateMentorshipRequestRequest { MentorProfileId = mentor.Id, Message = "x" }, Ct));
    }

    [Fact]
    public async Task CreateRequestAsync_Valid_SavesAndReturnsTheCreatedRequest()
    {
        var mentor = MakeMentor();
        var learnerId = Guid.NewGuid();
        _mentorRepository.GetByIdAsync(mentor.Id, Arg.Any<CancellationToken>()).Returns(mentor);
        _repository.HasOpenRequestAsync(mentor.Id, learnerId, Arg.Any<CancellationToken>()).Returns(false);
        var created = MakeRequest(mentor, learnerId);
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(created);

        var request = new CreateMentorshipRequestRequest { MentorProfileId = mentor.Id, Message = "  Please teach me  " };
        var result = await _service.CreateRequestAsync(learnerId, request, Ct);

        await _repository.Received(1).AddAsync(Arg.Is<MentorshipRequest>(r =>
            r.MentorProfileId == mentor.Id && r.LearnerUserId == learnerId && r.Message == "Please teach me"
            && r.Status == MentorshipRequestStatus.Pending), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task AcceptAsync_UnknownRequest_ThrowsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MentorshipRequest?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AcceptAsync(Guid.NewGuid(), Guid.NewGuid(), new RespondMentorshipRequestRequest(), Ct));
    }

    [Fact]
    public async Task AcceptAsync_NotTheMentor_ThrowsUnauthorized()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid());
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.AcceptAsync(Guid.NewGuid(), mentorshipRequest.Id, new RespondMentorshipRequestRequest(), Ct));
    }

    [Fact]
    public async Task AcceptAsync_NotPending_ThrowsConflict()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid(), MentorshipRequestStatus.Accepted);
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.AcceptAsync(mentor.UserId, mentorshipRequest.Id, new RespondMentorshipRequestRequest(), Ct));
    }

    [Fact]
    public async Task AcceptAsync_Valid_SetsAcceptedAndRecordsTheResponse()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid());
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        var result = await _service.AcceptAsync(mentor.UserId, mentorshipRequest.Id, new RespondMentorshipRequestRequest { ResponseMessage = "  Welcome!  " }, Ct);

        Assert.Equal("Accepted", result.Status);
        Assert.Equal("Welcome!", result.ResponseMessage);
        Assert.NotNull(result.RespondedAt);
    }

    [Fact]
    public async Task RejectAsync_NotPending_ThrowsConflict()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid(), MentorshipRequestStatus.Rejected);
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RejectAsync(mentor.UserId, mentorshipRequest.Id, new RespondMentorshipRequestRequest(), Ct));
    }

    [Fact]
    public async Task RejectAsync_Valid_SetsRejected()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid());
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        var result = await _service.RejectAsync(mentor.UserId, mentorshipRequest.Id, new RespondMentorshipRequestRequest(), Ct);

        Assert.Equal("Rejected", result.Status);
    }

    [Fact]
    public async Task CompleteAsync_NotAccepted_ThrowsConflict()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid(), MentorshipRequestStatus.Pending);
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CompleteAsync(mentor.UserId, mentorshipRequest.Id, Ct));
    }

    [Fact]
    public async Task CompleteAsync_Valid_SetsCompletedWithATimestamp()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid(), MentorshipRequestStatus.Accepted);
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        var result = await _service.CompleteAsync(mentor.UserId, mentorshipRequest.Id, Ct);

        Assert.Equal("Completed", result.Status);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownRequest_ThrowsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MentorshipRequest?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdAsync_NeitherLearnerNorMentor_ThrowsUnauthorized()
    {
        var mentor = MakeMentor();
        var learnerId = Guid.NewGuid();
        var mentorshipRequest = MakeRequest(mentor, learnerId);
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetByIdAsync(Guid.NewGuid(), mentorshipRequest.Id, Ct));
    }

    [Fact]
    public async Task GetByIdAsync_TheLearner_CanView()
    {
        var mentor = MakeMentor();
        var learnerId = Guid.NewGuid();
        var mentorshipRequest = MakeRequest(mentor, learnerId);
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        var result = await _service.GetByIdAsync(learnerId, mentorshipRequest.Id, Ct);

        Assert.Equal(mentorshipRequest.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_TheMentor_CanView()
    {
        var mentor = MakeMentor();
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid());
        _repository.GetByIdAsync(mentorshipRequest.Id, Arg.Any<CancellationToken>()).Returns(mentorshipRequest);

        var result = await _service.GetByIdAsync(mentor.UserId, mentorshipRequest.Id, Ct);

        Assert.Equal(mentorshipRequest.Id, result.Id);
    }

    [Fact]
    public async Task GetMyRequestsAsLearnerAsync_ReturnsMappedList()
    {
        var learnerId = Guid.NewGuid();
        var mentorshipRequest = MakeRequest(MakeMentor(), learnerId);
        _repository.GetByLearnerAsync(learnerId, Arg.Any<CancellationToken>()).Returns(new List<MentorshipRequest> { mentorshipRequest });

        var result = await _service.GetMyRequestsAsLearnerAsync(learnerId, Ct);

        Assert.Equal(mentorshipRequest.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetMyRequestsAsMentorAsync_UnknownMentorProfile_ThrowsNotFound()
    {
        _mentorRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MentorProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetMyRequestsAsMentorAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetMyRequestsAsMentorAsync_Valid_ReturnsMappedList()
    {
        var mentor = MakeMentor();
        _mentorRepository.GetByUserIdAsync(mentor.UserId, Arg.Any<CancellationToken>()).Returns(mentor);
        var mentorshipRequest = MakeRequest(mentor, Guid.NewGuid());
        _repository.GetByMentorProfileAsync(mentor.Id, Arg.Any<CancellationToken>()).Returns(new List<MentorshipRequest> { mentorshipRequest });

        var result = await _service.GetMyRequestsAsMentorAsync(mentor.UserId, Ct);

        Assert.Equal(mentorshipRequest.Id, Assert.Single(result).Id);
    }
}
