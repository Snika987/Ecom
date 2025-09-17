using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce_Project.Models;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required");
            }

            using var db = new ECommerceContext();

            var user = db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // Debug: Check if password is hashed or plain text
            var isHashed = user.Password.Contains(':') && user.Password.Split(':').Length == 3;
            
            bool passwordOk;
            if (isHashed)
            {
                passwordOk = SimplePasswordHasher.VerifyPassword(request.Password, user.Password);
            }
            else
            {
                // For existing plain text passwords (backward compatibility)
                passwordOk = user.Password == request.Password;
            }
            
            if (!passwordOk)
            {
                return Unauthorized("Invalid password");
            }

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345superSecretKey@345superSecretKey@345"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Uid),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };
            var tokenOptions = new JwtSecurityToken(
                issuer: "https://localhost:7077",
                audience: "https://localhost:7077",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: signinCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return Ok(new AuthenticatedResponse { Token = tokenString, ExpiresAtUtc = tokenOptions.ValidTo });
        }

    }
}
