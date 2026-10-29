using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken)
    {
        var from = string.IsNullOrWhiteSpace(_options.FromAddress) ? _options.UserName : _options.FromAddress;
        using var message = new MailMessage
        {
            From = new MailAddress(from, _options.FromName),
            Subject = "Reset your ShilpoHub BD password",
            Body = "We received a request to reset your ShilpoHub BD password.\n\n"
                + $"Open this link to choose a new password:\n{resetLink}\n\n"
                + "If you did not ask for this, you can ignore this email.",
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_options.UserName)
                ? null
                : new NetworkCredential(_options.UserName, _options.Password),
        };

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (SmtpException ex)
        {
            // Forgot-password must answer the same way whether or not the address exists,
            // so a mail-server failure is logged instead of surfacing as a 500.
            _logger.LogError(ex, "Password reset email to {Email} could not be sent.", toEmail);
        }
    }
}
