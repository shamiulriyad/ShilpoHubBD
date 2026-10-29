namespace ShilpoHubBD.Infrastructure.Options;

public class SmtpOptions
{
    /// <summary>When empty, password-reset links are only written to the log (development).</summary>
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "ShilpoHub BD";
}
