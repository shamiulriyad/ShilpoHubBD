namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken);
}
