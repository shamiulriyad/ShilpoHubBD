namespace ShilpoHubBD.Infrastructure.Options;

// Infrastructure is a plain class library (no ASP.NET Core hosting reference), so it cannot resolve
// IWebHostEnvironment itself. Program.cs (which does have it) fills this in at startup instead.
public class ImageStorageOptions
{
    public string WebRootPath { get; set; } = string.Empty;
}
