using Tests.TestClasses;

namespace Tests;

[CollectionDefinition("Integration tests")]
public class IntegrationTestCollection : ICollectionFixture<TestDatabaseFixture>
{
}