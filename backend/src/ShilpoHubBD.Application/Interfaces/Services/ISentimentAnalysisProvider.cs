using ShilpoHubBD.Application.DTOs.SentimentAnalysis;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISentimentAnalysisProvider
{
    SentimentResultDto Analyze(string text);
}
