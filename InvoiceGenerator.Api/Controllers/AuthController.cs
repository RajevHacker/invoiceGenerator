using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(IUserService userService, IOptions<JwtSettings> jwtOptions)
    {
        _userService = userService;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        if (!await _userService.ValidateUserAsync(login.partnerName, login.Username, login.Password))
            return Unauthorized("Invalid username or password");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, login.Username) }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return Ok(new { Token = jwtToken });
    }
}

