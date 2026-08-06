namespace ShilpoHubBD.Domain.Constants;

public static class RoleNames
{
    public const string Customer = "Customer";
    public const string Producer = "Producer";
    public const string BusinessPartner = "BusinessPartner";
    public const string Tourist = "Tourist";
    public const string HeritageAcademyMember = "HeritageAcademyMember";
    public const string HeritageInnovationHub = "HeritageInnovationHub";
    public const string GovernmentNGO = "GovernmentNGO";
    public const string LogisticsPartner = "LogisticsPartner";
    public const string SuperAdmin = "SuperAdmin";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Customer,
        Producer,
        BusinessPartner,
        Tourist,
        HeritageAcademyMember,
        HeritageInnovationHub,
        GovernmentNGO,
        LogisticsPartner,
        SuperAdmin,
    };

    public static readonly IReadOnlyList<string> SelfRegisterableRoles = new[]
    {
        Customer,
        Producer,
        BusinessPartner,
        Tourist,
        HeritageAcademyMember,
        HeritageInnovationHub,
        GovernmentNGO,
        LogisticsPartner,
    };
}
