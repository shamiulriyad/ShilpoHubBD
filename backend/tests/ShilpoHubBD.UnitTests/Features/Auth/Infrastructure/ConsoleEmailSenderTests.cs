using Microsoft.Extensions.Logging;
using ShilpoHubBD.Infrastructure.Email;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Auth.Infrastructure;

[Trait("Feature", "Auth")]
[Trait("Layer", "Infrastructure")]
public class ConsoleEmailSenderTests
{
    private readonly CapturingLogger<ConsoleEmailSender> _logger = new();

    [Fact]
    public async Task SendPasswordResetEmailAsync_LogsTheRecipientAndResetLinkAtInformationLevel()
    {
        var sender = new ConsoleEmailSender(_logger);

        await sender.SendPasswordResetEmailAsync(
            "artisan@example.com", "https://shilpohub.example/reset?token=abc", TestContext.Current.CancellationToken);

        var (level, message) = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Information, level);
        Assert.Contains("artisan@example.com", message);
        Assert.Contains("https://shilpohub.example/reset?token=abc", message);
    }

    [Fact]
    public void SendPasswordResetEmailAsync_CompletesImmediately()
    {
        var sender = new ConsoleEmailSender(_logger);

        var task = sender.SendPasswordResetEmailAsync("artisan@example.com", "link", TestContext.Current.CancellationToken);

        Assert.True(task.IsCompletedSuccessfully);
    }
}
