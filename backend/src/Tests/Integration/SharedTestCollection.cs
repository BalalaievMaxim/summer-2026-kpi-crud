using Xunit;

namespace GymManagement.Tests.Integration;

[CollectionDefinition("Integration")]
public class SharedTestCollection : ICollectionFixture<GymApiFactory>
{
   
}