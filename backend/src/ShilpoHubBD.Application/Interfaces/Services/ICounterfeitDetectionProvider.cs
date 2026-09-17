using ShilpoHubBD.Application.DTOs.CounterfeitDetection;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICounterfeitDetectionProvider
{
    (decimal RiskScore, string RiskLevel, List<string> Signals) Check(CounterfeitCheckContext context);
}
