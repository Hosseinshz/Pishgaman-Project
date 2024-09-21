using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pishgaman_Project.Models;
using Pishgaman_Project.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pishgaman_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly DBTestProject _dbcontext;
        private readonly IConfiguration _configuration;

        public AccountController(DBTestProject context, IConfiguration configuration)
        {
            _dbcontext = context;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(SignInViewModel model)
        {
            // Fetch user by phone number
            var user = _dbcontext.UserInfo.FirstOrDefault(u => u.PhoneNumber == model.PhoneNumber);
            if (user == null)
                return Unauthorized("User not found.");

            // Verify the password using PasswordHasher
            var passwordHasher = new PasswordHasher<UserInfo>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (result != PasswordVerificationResult.Success)
                return Unauthorized("Invalid password.");

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Return the token in the response (in this case, as a simple text response)
            return Ok(new { token });
        }

        private string GenerateJwtToken(UserInfo user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.Role, user.Role) 
        }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenTimeoutMinutes"])),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


    }
}
