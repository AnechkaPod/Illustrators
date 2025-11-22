using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace ImageService.Attributes;

public class RequireAuthenticationAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var httpClientFactory = context.HttpContext.RequestServices
            .GetRequiredService<IHttpClientFactory>();
        var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<RequireAuthenticationAttribute>>();

        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Authentication required"
            });
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            var authServiceUrl = configuration["AuthService:BaseUrl"]
                ?? throw new InvalidOperationException("AuthService:BaseUrl not configured");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{authServiceUrl}/api/auth/validate");
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    error = "Invalid or expired token"
                });
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var validationResult = JsonSerializer.Deserialize<TokenValidationResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (validationResult == null || !validationResult.Success)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    error = "Invalid token"
                });
                return;
            }

            // Store user info
            context.HttpContext.Items["UserId"] = validationResult.User?.Id;
            context.HttpContext.Items["Email"] = validationResult.User?.Email;
            context.HttpContext.Items["Role"] = validationResult.User?.Role;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating token");
            context.Result = new ObjectResult(new
            {
                error = "Authentication service error"
            })
            {
                StatusCode = 503
            };
            return;
        }

        await next();
    }
}

public class TokenValidationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}