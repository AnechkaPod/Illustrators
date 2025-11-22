using System.Text;
using System.Text.Json;

namespace IllustratorService.Middleweare;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            await _next(context);
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var authServiceUrl = _configuration["AuthService:BaseUrl"] 
                ?? throw new InvalidOperationException("AuthService:BaseUrl not configured");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{authServiceUrl}/api/auth/validate");
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { token }),
                Encoding.UTF8,
                "application/json"
            );

            //var request = new HttpRequestMessage(HttpMethod.Post, $"{authServiceUrl}/api/auth/validate");
            request.Headers.Add("Authorization", $"Bearer {token}");  // ← Send in HEADER, not body
                                                                      // Remove the request.Content line completely!

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token validation failed with status {StatusCode}", response.StatusCode);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                return;
            }

            var validationResult = await response.Content.ReadFromJsonAsync<TokenValidationResponse>();

            if (validationResult == null || !validationResult.Success)
            {
                _logger.LogWarning("Token validation returned invalid result");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
                return;
            }

            context.Items["UserId"] = validationResult.User.Id ;
            context.Items["Email"] = validationResult.User.Email;
            context.Items["Role"] = validationResult.User.Role;

            _logger.LogDebug("Token validated successfully for user {UserId}", validationResult.User.Id);

            await _next(context);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error connecting to Auth Service");
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { error = "Authentication service unavailable" });
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Token validation error" });
            return;
        }

        await _next(context);
    }
}

public class TokenValidationResponse
{
    public bool Success { get; set; }  // ← Changed from IsValid to Success
    public string Message { get; set; } = string.Empty;
    public UserInfo? User { get; set; }  // ← Nested user object
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
