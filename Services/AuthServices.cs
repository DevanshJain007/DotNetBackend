using BackendOfReactProject.Data;
using BackendOfReactProject.DTO;
using BackendOfReactProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
namespace BackendOfReactProject.Services
{
    public class AuthServices : IAuthService
    {
        private readonly BlogContext _context;
        private readonly IConfiguration _configuration;
        public AuthServices(BlogContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<string> RegisterAsync (RegisterDto registerDto)
        {
            //Check if the email already exists if does then throw an error
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email)){

                throw new InvalidOperationException("This email is already exists");
            }
            //Check if the username is already taken
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username)) {
                throw new InvalidOperationException("The Username is ALready taken");
                    }
            var newUser=new User { 
                Username = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PasswordHash=HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return GenerateJwtToken(newUser);
        }

        //this is the section of the LoginAsync 
        public async Task<string> LoginAsync(LoginDTOs loginDto)
        {
            var user = await GetUserByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive || !VerifyPassword(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            return GenerateJwtToken(user);
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
        //end of the login concept 
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }
        //now 
        
        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var key=Encoding.ASCII.GetBytes(jwtKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("firstName", user.FirstName ?? ""),
                new Claim("lastName", user.LastName ?? "")
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
