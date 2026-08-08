using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Validators.HeritageIdentity;

public class UpsertHeritageIdentityRequestValidator : AbstractValidator<UpsertHeritageIdentityRequest>
{
    public UpsertHeritageIdentityRequestValidator()
    {
        RuleFor(x => x.PrimaryCraft).NotEmpty().MaximumLength(100);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 100);
        RuleFor(x => x.WorkshopName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WorkshopDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.WorkshopAddress).MaximumLength(300);
        RuleFor(x => x.EstablishedYear).InclusiveBetween(1700, 2100).When(x => x.EstablishedYear.HasValue);
        RuleFor(x => x.DistrictId).NotEmpty().When(x => x.DistrictId.HasValue);

        RuleForEach(x => x.FamilyMembers).SetValidator(new FamilyHeritageMemberInputValidator());
        RuleForEach(x => x.SkillTimeline).SetValidator(new SkillTimelineEntryInputValidator());
        RuleForEach(x => x.Awards).SetValidator(new HeritageAwardInputValidator());
        RuleForEach(x => x.Certifications).SetValidator(new HeritageCertificationInputValidator());
        RuleForEach(x => x.StoryArchive).SetValidator(new StoryArchiveEntryInputValidator());
    }
}
