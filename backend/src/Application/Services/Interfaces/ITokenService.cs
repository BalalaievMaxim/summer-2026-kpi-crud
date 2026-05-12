namespace GymManagement.Application.Services.Interfaces;

public interface ITokenService
{
    string CreateToken(int userId, string email, string role);
}
