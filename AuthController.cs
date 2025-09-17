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
    /// <summary>
    /// Handles user authentication - login and JWT token generation
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// User login endpoint - validates credentials and returns JWT token
        /// </summary>
        /// <param name="request">Login credentials (email and password)</param>
        /// <returns>JWT token if login successful, error message if failed</returns>
        [AllowAnonymous] // This endpoint doesn't require authentication
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validate input - check if email and password are provided
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required");
            }

            // Connect to database
            using var db = new ECommerceContext();

            // Find user by email address
            var user = db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // Check if password matches (simple comparison for learning purposes)
            if (user.Password != request.Password)
            {
                return Unauthorized("Invalid password");
            }

            // Create JWT token for successful login
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345superSecretKey@345superSecretKey@345"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            
            // Add user information to token claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Uid), // User ID
                new Claim(JwtRegisteredClaimNames.Email, user.Email) // User email
            };
            
            // Configure token settings
            var tokenOptions = new JwtSecurityToken(
                issuer: "https://localhost:7077", // Who issued this token
                audience: "https://localhost:7077", // Who can use this token
                claims: claims, // User data in token
                expires: DateTime.UtcNow.AddDays(1), // Token valid for 1 day
                signingCredentials: signinCredentials // Secret key to sign token
            );
            
            // Generate the actual token string
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            
            // Return token and expiration time to frontend
            return Ok(new AuthenticatedResponse { Token = tokenString, ExpiresAtUtc = tokenOptions.ValidTo });
        }
    }
}
