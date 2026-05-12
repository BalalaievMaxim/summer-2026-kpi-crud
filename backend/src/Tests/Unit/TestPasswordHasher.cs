using GymManagement.Domain.Ports;

namespace GymManagement.Tests.Unit;

public sealed class TestPasswordHasher : IPasswordHasher
{
    public string Hash(string raw) => $"test-hash:{raw}";

    public bool Verify(string raw, string hash) => hash == Hash(raw);
}
