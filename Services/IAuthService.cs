using BackendOfReactProject.DTO;
using BackendOfReactProject.Models;

namespace BackendOfReactProject.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync (RegisterDto registerDto);
        Task<string> LoginAsync (LoginDTOs loginDto);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync (string email);

        string GenerateJwtToken(User user);
        bool VerifyPassword(string password,string hash);
        string HashPassword(string password);

    }
}
