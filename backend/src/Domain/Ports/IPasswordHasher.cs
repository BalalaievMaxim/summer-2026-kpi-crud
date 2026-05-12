namespace GymManagement.Domain.Ports;

public interface IPasswordHasher
{
    string Hash(string raw);
    bool Verify(string raw, string hash);
}
