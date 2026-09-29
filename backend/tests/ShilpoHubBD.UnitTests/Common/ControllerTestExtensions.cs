using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShilpoHubBD.UnitTests.Common;

public static class ControllerTestExtensions
{
    /// <summary>Signs the request in as <paramref name="userId"/>, using the JWT "sub" claim the API issues.</summary>
    public static T WithUser<T>(this T controller, Guid userId, params string[] roles) where T : ControllerBase
        => controller.WithClaims(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), roles);

    /// <summary>Signs the request in with only the NameIdentifier claim (how JwtBearer maps "sub" by default).</summary>
    public static T WithNameIdentifierUser<T>(this T controller, Guid userId, params string[] roles) where T : ControllerBase
        => controller.WithClaims(new Claim(ClaimTypes.NameIdentifier, userId.ToString()), roles);

    public static T WithRemoteIp<T>(this T controller, string ipAddress) where T : ControllerBase
    {
        controller.EnsureHttpContext().Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        return controller;
    }

    public static T WithAnonymousRequest<T>(this T controller) where T : ControllerBase
    {
        controller.EnsureHttpContext();
        return controller;
    }

    private static T WithClaims<T>(this T controller, Claim idClaim, string[] roles) where T : ControllerBase
    {
        var claims = new List<Claim> { idClaim };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        controller.EnsureHttpContext().User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        return controller;
    }

    private static HttpContext EnsureHttpContext(this ControllerBase controller)
    {
        controller.ControllerContext.HttpContext ??= new DefaultHttpContext();
        return controller.ControllerContext.HttpContext;
    }

    /// <summary>Reads a property from an anonymous response body such as <c>new { message = "..." }</c>.</summary>
    public static string? JsonProperty(this ObjectResult result, string name)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
        return document.RootElement.TryGetProperty(name, out var value) ? value.GetString() : null;
    }
}

/// <summary>Reads the access rules declared on a controller, so tests can lock them down.</summary>
public static class AccessRules
{
    public static string? ClassRoles(Type controller)
        => controller.GetCustomAttribute<AuthorizeAttribute>(inherit: true)?.Roles;

    public static bool ClassRequiresSignIn(Type controller)
        => controller.GetCustomAttribute<AuthorizeAttribute>(inherit: true) is not null;

    public static bool ActionRequiresSignIn(Type controller, string action)
        => Action(controller, action).GetCustomAttribute<AuthorizeAttribute>() is not null;

    public static bool ActionAllowsAnonymous(Type controller, string action)
        => Action(controller, action).GetCustomAttribute<AllowAnonymousAttribute>() is not null;

    public static IReadOnlyList<string> PublicActions(Type controller)
        => controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name)
            .ToList();

    public static string? ControllerRoute(Type controller)
        => controller.GetCustomAttribute<RouteAttribute>()?.Template;

    /// <summary>The HTTP verb and route template of an action, e.g. ("POST", "login").</summary>
    public static (string Method, string? Template) ActionRoute(Type controller, string action)
    {
        var attribute = Action(controller, action).GetCustomAttribute<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
            ?? throw new InvalidOperationException($"{controller.Name}.{action} has no HTTP method attribute.");
        return (attribute.HttpMethods.Single(), attribute.Template);
    }

    private static MethodInfo Action(Type controller, string action)
        => controller.GetMethod(action, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"{controller.Name} has no action {action}.");
}
