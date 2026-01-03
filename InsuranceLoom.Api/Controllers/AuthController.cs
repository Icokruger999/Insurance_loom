using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("broker/register")]
    public async Task<ActionResult<BrokerDto>> RegisterBroker([FromBody] CreateBrokerRequest request)
    {
        try
        {
            var broker = await _authService.RegisterBrokerAsync(request);
            return Created($"/api/broker/{broker.Id}", broker);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
        }
    }

    [HttpPost("broker/login")]
    public async Task<ActionResult<LoginResponse>> BrokerLogin([FromBody] BrokerLoginRequest request)
    {
        try
        {
            var response = await _authService.BrokerLoginAsync(request);
            if (response == null)
                return Unauthorized(new { message = "Invalid agent number or password" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    [HttpPost("policyholder/login")]
    public async Task<ActionResult<LoginResponse>> PolicyHolderLogin([FromBody] PolicyHolderLoginRequest request)
    {
        try
        {
            var response = await _authService.PolicyHolderLoginAsync(request);
            if (response == null)
                return Unauthorized(new { message = "Invalid policy number or password" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    [HttpPost("manager/login")]
    public async Task<ActionResult<LoginResponse>> ManagerLogin([FromBody] ManagerLoginRequest request)
    {
        try
        {
            var response = await _authService.ManagerLoginAsync(request);
            if (response == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }
}

