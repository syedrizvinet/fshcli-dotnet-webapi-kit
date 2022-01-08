using DN.WebApi.Application.Identity.Tokens;
using DN.WebApi.Application.Wrapper;
using DN.WebApi.Infrastructure.Swagger;
using DN.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace DN.WebApi.Host.Controllers.Identity;

public sealed class TokensController : VersionNeutralApiController
{
    private readonly ITokenService _tokenService;

    public TokensController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost]
    [AllowAnonymous]
    [SwaggerHeader(MultitenancyConstants.TenantHeaderKey, "Input your tenant Id to access this API", "", true)]
    [OpenApiOperation("Submit Credentials with Tenant Key to generate valid Access Token.", "")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<Result<TokenResponse>>> GetTokenAsync(TokenRequest request)
    {
        var token = await _tokenService.GetTokenAsync(request, GenerateIpAddress());
        return Ok(token);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [SwaggerHeader(MultitenancyConstants.TenantHeaderKey, "Input your tenant Id to access this API", "", true)]
    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<Result<TokenResponse>>> RefreshAsync(RefreshTokenRequest request)
    {
        var response = await _tokenService.RefreshTokenAsync(request, GenerateIpAddress());
        return Ok(response);
    }

    private string GenerateIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"];
        }
        else
        {
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
        }
    }
}