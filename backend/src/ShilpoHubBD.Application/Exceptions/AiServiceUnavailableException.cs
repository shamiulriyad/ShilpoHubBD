namespace ShilpoHubBD.Application.Exceptions;

// Thrown when an AI-backed feature has no real result to give (missing config, upstream failure,
// timeout) and there is no honest non-AI fallback to fall back to. Callers must surface this as a
// clear failure -- never substitute fabricated output for a real answer.
public class AiServiceUnavailableException : Exception
{
    public AiServiceUnavailableException(string message) : base(message)
    {
    }
}
