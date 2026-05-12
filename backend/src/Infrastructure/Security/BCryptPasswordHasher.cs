using GymManagement.Domain.Ports;

namespace GymManagement.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string raw) => BCrypt.Net.BCrypt.HashPassword(raw);

    public bool Verify(string raw, string hash)
        => hash.StartsWith("$2", StringComparison.Ordinal) &&
           BCrypt.Net.BCrypt.Verify(raw, hash);
}
