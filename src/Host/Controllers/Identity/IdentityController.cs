using DN.WebApi.Application.Identity.Interfaces;
using DN.WebApi.Application.Wrapper;
using DN.WebApi.Domain.Constants;
using DN.WebApi.Infrastructure.Identity.Permissions;
using DN.WebApi.Shared.DTOs.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DN.WebApi.Host.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
[ApiVersionNeutral]
[ApiConventionType(typeof(FSHApiConventions))]
public sealed class IdentityController : ControllerBase
{
    private readonly ICurrentUser _user;
    private readonly IIdentityService _identityService;
    private readonly IUserService _userService;

    public IdentityController(IIdentityService identityService, ICurrentUser user, IUserService userService)
    {
        _identityService = identityService;
        _user = user;
        _userService = userService;
    }

    [HttpPost("register")]
    [MustHavePermission(PermissionConstants.Identity.Register)]
    public async Task<ActionResult<Result<string>>> RegisterAsync(RegisterRequest request)
    {
        string origin = GenerateOrigin();
        return Ok(await _identityService.RegisterAsync(request, origin));
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ErrorResult<string>))]
    public async Task<ActionResult<Result<string>>> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code, [FromQuery] string tenant)
    {
        return Ok(await _identityService.ConfirmEmailAsync(userId, code, tenant));
    }

    [HttpGet("confirm-phone-number")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ErrorResult<string>))]
    public async Task<ActionResult<Result<string>>> ConfirmPhoneNumberAsync([FromQuery] string userId, [FromQuery] string code)
    {
        return Ok(await _identityService.ConfirmPhoneNumberAsync(userId, code));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ErrorResult<string>))]
    public async Task<ActionResult<Result>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        string origin = GenerateOrigin();
        return Ok(await _identityService.ForgotPasswordAsync(request, origin));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesDefaultResponseType(typeof(ErrorResult<string>))]
    public async Task<ActionResult<Result>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return Ok(await _identityService.ResetPasswordAsync(request));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<Result>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        return Ok(await _identityService.UpdateProfileAsync(request, _user.GetUserId().ToString()));
    }

    [HttpGet("profile")]
    public async Task<ActionResult<Result<UserDetailsDto>>> GetProfileDetailsAsync()
    {
        return Ok(await _userService.GetAsync(_user.GetUserId().ToString()));
    }

    private string GenerateOrigin()
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
        return string.IsNullOrEmpty(Request.Headers["origin"].ToString()) ? baseUrl : Request.Headers["origin"].ToString();
    }
}