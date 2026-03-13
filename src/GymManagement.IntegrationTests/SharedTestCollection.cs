using Xunit;

namespace GymManagement.IntegrationTests;

[CollectionDefinition("Integration")]
public class SharedTestCollection : ICollectionFixture<GymApiFactory>
{
   
}