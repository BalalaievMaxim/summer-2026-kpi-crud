namespace GymManagement.Domain.Ports;

public interface ITokenService
{
    string CreateToken(int userId, string email, string role);
}
